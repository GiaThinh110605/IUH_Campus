using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IUHCampus.UI;
using IUHCampus.World;
using IUHCampus.Core;
using StarterAssets;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IUHCampus.Minigames.GreenCampus
{
    [Serializable]
    public class GreenZoneView
    {
        public string id;
        public string displayName;
        public GameObject dirtyVisual;
        public GameObject cleanVisual;
    }

    public class IUHGreenCampusManager : MonoBehaviour
    {
        public enum GameState { Inactive, Briefing, Playing, Victory, Defeat }
        public static IUHGreenCampusManager Instance { get; private set; }
        public IUHGreenLitter[] items = Array.Empty<IUHGreenLitter>();
        public IUHGreenBin[] bins = Array.Empty<IUHGreenBin>();
        public GreenZoneView[] zones = Array.Empty<GreenZoneView>();
        public GameObject gameWorld;
        public IUHGreenCampusUI ui;
        public Vector3 spawnPoint = new Vector3(375.8f, 0.65f, -40.5f);
        public float interactionDistance = 2.7f;
        private readonly List<GreenItemDefinition> definitions = new List<GreenItemDefinition>();
        private GreenCampusProgress progress = new GreenCampusProgress();
        private GameState state;
        private IUHGreenLitter carried;
        private GameObject player;
        private ThirdPersonController controller;
        private bool hudCursor;
        private bool holdsControl;
        private bool previousControllerEnabled;
        private float nextSave;
        private float nextRefresh;
        private Unity.Cinemachine.CinemachineThirdPersonFollow roundCameraFollow;
        private bool previousCameraAvoidance;
        private string toast;
        private float toastUntil;

        public GameState State => state;
        public bool HasActiveSessionOrUI => state != GameState.Inactive;
        public bool HasActiveModal => state == GameState.Briefing || state == GameState.Victory || state == GameState.Defeat || hudCursor;
        public bool IsMissionActive => state == GameState.Playing;
        public int CurrentScore => progress.Score(definitions);
        public int SortedCount => definitions.Count(d => progress.IsSorted(d.id));
        public int CleanZoneCount => progress.CompletedZones(definitions);
        public int TotalItems => definitions.Count;
        public GreenCampusProgress Progress => progress;
        public IUHGreenLitter CarriedItem => carried;
        public IReadOnlyList<GreenItemDefinition> Definitions => definitions;
        public string Toast => Time.unscaledTime < toastUntil ? toast : "";

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Initialize();
        }
        private void Start() { SyncWorldScore(); RefreshUI(); }
        private void OnDisable() { CancelMission(); }
        private void OnDestroy() { if (Instance == this) Instance = null; }

        public void Initialize()
        {
            definitions.Clear();
            foreach (var item in items.Where(i => i != null))
            {
                if (string.IsNullOrWhiteSpace(item.itemId) || definitions.Any(d => d.id == item.itemId))
                    throw new InvalidOperationException("Green Campus item IDs must be nonempty and unique.");
                definitions.Add(item.Definition);
                item.CacheHome();
            }
            progress = GreenCampusStorage.Load();
            progress.Normalize(definitions);
            FindPlayer();
            SyncWorldScore();
            ApplyWorld();
            RefreshUI();
        }

        private void FindPlayer()
        {
            if (player == null) player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && controller == null) controller = player.GetComponent<ThirdPersonController>();
        }

        public bool ExternalUIOpen() =>
            (IUHMiniGamePortalUI.Instance != null && IUHMiniGamePortalUI.Instance.IsOpen) ||
            (IUHCustomizationUI.Instance != null && IUHCustomizationUI.Instance.IsOpen) ||
            (IUHDeliveryDialogueUI.Instance != null && IUHDeliveryDialogueUI.Instance.IsOpen) ||
            (IUHMainMenuUI.Instance != null && IUHMainMenuUI.Instance.IsOpen);

        private bool IsSitting() => player != null && player.GetComponent<IUHCampus.Player.IUHPlayerSittingController>() != null &&
            player.GetComponent<IUHCampus.Player.IUHPlayerSittingController>().IsSitting;

        private void UpdateControl()
        {
            FindPlayer();
            if (HasActiveModal)
            {
                if (!holdsControl)
                {
                    previousControllerEnabled = controller != null && controller.enabled;
                    holdsControl = true;
                }
                if (controller != null) controller.enabled = false;
                if (!ExternalUIOpen()) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
            }
            else if (holdsControl)
            {
                if (!ExternalUIOpen() && !IsSitting() && previousControllerEnabled && controller != null) controller.enabled = true;
                holdsControl = false;
                if (!ExternalUIOpen()) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
            }
        }

        public void OpenBriefing()
        {
            CancelConflictingGames();
            if (HasActiveSessionOrUI) CancelMission();
            FindPlayer();
            state = GameState.Briefing;
            hudCursor = false;
            UpdateControl();
            ApplyWorld();
            RefreshUI();
        }

        private void CancelConflictingGames()
        {
            var photo = IUHCampus.Minigames.PhotoHunter.IUHPhotoHunterManager.Instance;
            if (photo != null && photo.HasActiveSessionOrUI) photo.CancelMission(false);
            var grab = IUHCampus.Minigames.GrabCampus.IUHGrabCampusManager.Instance;
            if (grab != null) grab.CancelMission();
            if (IUHCustomizationUI.Instance != null && IUHCustomizationUI.Instance.IsOpen)
            {
                IUHCustomizationUI.Instance.ReturnToPortalMenuOnClose = false;
                IUHCustomizationUI.Instance.SetOpen(false);
            }
            if (IUHMainMenuUI.Instance != null && IUHMainMenuUI.Instance.IsOpen) IUHMainMenuUI.Instance.SetOpen(false);
            if (IUHMiniGamePortalUI.Instance != null) IUHMiniGamePortalUI.Instance.SetMenuOpen(false);
        }

        public bool BeginRound(bool restart = false)
        {
            if (definitions.Count == 0) { Notify("Chưa có vật phẩm. Hãy chạy setup Green Campus."); return false; }
            FindPlayer();
            if (IsSitting()) { Notify("Hãy đứng dậy khỏi ghế trước khi bắt đầu."); return false; }
            var candidate = progress.Copy();
            if (restart || !candidate.hasRound) candidate = progress.NewRound();
            if (!Commit(candidate)) return false;
            DropCarried();
            state = progress.IsComplete(definitions) ? GameState.Victory :
                progress.secondsRemaining <= 0 ? GameState.Defeat : GameState.Playing;
            if (state == GameState.Playing && roundCameraFollow == null && Camera.main != null)
            {
                var brain = Camera.main.GetComponent<Unity.Cinemachine.CinemachineBrain>();
                var camera = brain != null ? brain.ActiveVirtualCamera as Unity.Cinemachine.CinemachineCamera : null;
                if (camera != null)
                {
                    roundCameraFollow = camera.GetComponent<Unity.Cinemachine.CinemachineThirdPersonFollow>();
                    if (roundCameraFollow != null)
                    {
                        previousCameraAvoidance = roundCameraFollow.AvoidObstacles.Enabled;
                        roundCameraFollow.AvoidObstacles.Enabled = false;
                        camera.PreviousStateIsValid = false;
                    }
                }
            }
            hudCursor = false;
            UpdateControl();
            // Starting a walking game explicitly releases the previous game's movement lock.
            if (state == GameState.Playing && !ExternalUIOpen() && controller != null)
                controller.enabled = true;
            if (state == GameState.Playing && player != null)
            {
                var cc = player.GetComponent<CharacterController>();
                bool wasEnabled = cc != null && cc.enabled;
                Vector3 delta = spawnPoint - player.transform.position;
                if (cc != null) cc.enabled = false;
                player.transform.position = spawnPoint;
                if (controller != null)
                {
                    var availableBins = bins.Where(b => b != null).ToArray();
                    Vector3 lookPoint = availableBins.Length > 0
                        ? availableBins.Select(b => b.transform.position).Aggregate(Vector3.zero, (a, b) => a + b) / availableBins.Length + Vector3.up * 1.2f
                        : spawnPoint + Vector3.forward * 5f;
                    controller.SetCameraOrientation(lookPoint - (spawnPoint + Vector3.up * 1.4f));
                }
                if (cc != null) cc.enabled = wasEnabled;
                if (controller != null) controller.ResetMotion();
                var brain = Camera.main != null ? Camera.main.GetComponent<Unity.Cinemachine.CinemachineBrain>() : null;
                var camera = brain != null ? brain.ActiveVirtualCamera as Unity.Cinemachine.CinemachineCamera : null;
                if (camera != null && controller != null && controller.CinemachineCameraTarget != null)
                {
                    camera.Follow = controller.CinemachineCameraTarget.transform;
                    camera.OnTargetObjectWarped(camera.Follow, delta);
                    camera.PreviousStateIsValid = false;
                }
            }
            nextSave = Time.unscaledTime + 5f;
            ApplyWorld();
            RefreshUI();
            return true;
        }

        private bool Commit(GreenCampusProgress candidate)
        {
            if (!GreenCampusStorage.TrySave(candidate)) { Notify("Không lưu được. Thao tác chưa được ghi nhận, hãy thử lại."); return false; }
            progress = candidate;
            SyncWorldScore();
            return true;
        }

        private void SyncWorldScore()
        {
            if (WorldState.Instance == null) return;
            WorldState.Instance.SetObjectState("GREEN_CAMPUS_SCORE", CurrentScore.ToString());
            WorldState.Instance.SetObjectState("GREEN_CAMPUS_BEST_SCORE", progress.bestScore.ToString());
            WorldState.Instance.SetObjectState("GREEN_CAMPUS_CLEAN_ZONES", CleanZoneCount.ToString());
        }

        public void CancelMission(bool restoreControl = true)
        {
            if (!HasActiveSessionOrUI && !holdsControl) return;
            if (state == GameState.Playing && progress.hasRound) Commit(progress.Copy());
            DropCarried();
            if (roundCameraFollow != null)
            {
                roundCameraFollow.AvoidObstacles.Enabled = previousCameraAvoidance;
                var camera = roundCameraFollow.GetComponent<Unity.Cinemachine.CinemachineCamera>();
                if (camera != null) camera.PreviousStateIsValid = false;
                roundCameraFollow = null;
            }
            state = GameState.Inactive;
            hudCursor = false;
            if (restoreControl) UpdateControl(); else holdsControl = false;
            ApplyWorld();
            RefreshUI();
        }

        public void AdvanceTime(float seconds)
        {
            if (state != GameState.Playing || ExternalUIOpen() || hudCursor || seconds <= 0) return;
            progress.secondsRemaining = Mathf.Max(0, progress.secondsRemaining - seconds);
            if (progress.secondsRemaining <= 0)
            {
                DropCarried();
                state = GameState.Defeat;
                Commit(progress.Copy());
                UpdateControl();
                RefreshUI();
            }
        }

        private bool InReach(Vector3 position, float radius)
        { FindPlayer(); return player != null && Vector3.Distance(player.transform.position, position) <= radius; }

        private bool ClearLineTo(IUHGreenLitter item)
        {
            var from = player.transform.position + Vector3.up * 1.4f;
            var direction = item.transform.position + Vector3.up * 0.15f - from;
            var mask = LayerMask.GetMask("Default", "Building", "Environment", "Ground");
            foreach (var hit in Physics.RaycastAll(from, direction.normalized, direction.magnitude, mask, QueryTriggerInteraction.Ignore))
                if (!hit.transform.IsChildOf(player.transform) && !hit.transform.IsChildOf(item.transform)) return false;
            return true;
        }

        public bool TryPickup(IUHGreenLitter item)
        {
            if (state != GameState.Playing || ExternalUIOpen() || item == null || !items.Contains(item) ||
                carried != null || progress.IsSorted(item.itemId) || !item.gameObject.activeInHierarchy) return false;
            if (!InReach(item.transform.position, interactionDistance)) { Notify("Đến gần món rác hơn để nhặt."); return false; }
            if (!ClearLineTo(item)) { Notify("Có vật cản. Hãy đến gần từ hướng khác."); return false; }
            carried = item;
            item.Carry(player.transform);
            hudCursor = false;
            UpdateControl();
            Notify("Đã nhặt " + item.displayName + ". Mang về đúng thùng phân loại.");
            return true;
        }

        public bool NearStation => bins.Any(b => b != null && InReach(b.transform.position, interactionDistance + 1f));
        public bool TrySort(WasteCategory category)
        {
            if (state != GameState.Playing || ExternalUIOpen() || carried == null) return false;
            var bin = bins.FirstOrDefault(b => b != null && b.category == category);
            if (bin == null || !InReach(bin.transform.position, interactionDistance + 1f))
            { Notify("Mang rác về gần thùng " + IUHGreenBin.CategoryLabel(category) + "."); return false; }
            var candidate = progress.Copy();
            bool correct;
            if (!candidate.TrySort(carried.itemId, category, definitions, out correct) || !Commit(candidate)) return false;
            if (correct)
            {
                var sorted = carried;
                carried = null;
                sorted.ReturnHome();
                sorted.SetSorted(true);
                Notify("Đúng loại! +20 điểm. Dọn sạch cả khu được thêm 30 điểm.");
                hudCursor = false;
                if (progress.IsComplete(definitions)) state = GameState.Victory;
            }
            else Notify("Chưa đúng thùng: -5 điểm. Bạn vẫn giữ món rác để thử lại.");
            UpdateControl();
            ApplyWorld();
            RefreshUI();
            return correct;
        }

        public void DropCarried()
        { if (carried != null) { carried.ReturnHome(); carried = null; } }

        public void Interact()
        {
            if (state != GameState.Playing || ExternalUIOpen()) return;
            FindPlayer();
            if (player == null) return;
            if (carried == null)
            {
                var nearest = items.Where(i => i != null && i.gameObject.activeInHierarchy && !progress.IsSorted(i.itemId))
                    .OrderBy(i => (i.transform.position - player.transform.position).sqrMagnitude).FirstOrDefault();
                if (nearest == null || !TryPickup(nearest)) Notify("Đứng gần món rác có nhãn, nhấn F để nhặt.");
            }
            else
            {
                var nearest = bins.Where(b => b != null).OrderBy(b => (b.transform.position - player.transform.position).sqrMagnitude).FirstOrDefault();
                if (nearest != null) TrySort(nearest.category);
            }
        }

        public void ToggleHudCursor()
        { if (state == GameState.Playing && !ExternalUIOpen()) { hudCursor = !hudCursor; UpdateControl(); } }
        public void Notify(string message) { toast = message; toastUntil = Time.unscaledTime + 4f; RefreshUI(); }
        public void ApplyWorld()
        {
            if (gameWorld != null) gameWorld.SetActive(HasActiveSessionOrUI);
            foreach (var item in items.Where(i => i != null))
                if (item != carried) item.SetSorted(progress.IsSorted(item.itemId));
            foreach (var zone in zones)
            {
                var group = definitions.Where(d => d.zoneId == zone.id).ToArray();
                bool clean = progress.hasRound && group.Length > 0 && group.All(d => progress.IsSorted(d.id));
                if (zone.cleanVisual != null) zone.cleanVisual.SetActive(clean);
                if (zone.dirtyVisual != null) zone.dirtyVisual.SetActive(!clean);
            }
        }
        public void RefreshUI() { if (ui != null) ui.Refresh(this); }

        private void Update()
        {
            if (state == GameState.Playing && !ExternalUIOpen())
            {
#if ENABLE_INPUT_SYSTEM
                var keyboard = Keyboard.current;
                if (keyboard != null)
                {
                    if (keyboard.tabKey.wasPressedThisFrame) ToggleHudCursor();
                    if (keyboard.fKey.wasPressedThisFrame) Interact();
                    if (keyboard.qKey.wasPressedThisFrame) DropCarried();
                    if (keyboard.digit1Key.wasPressedThisFrame) TrySort(WasteCategory.Recyclable);
                    if (keyboard.digit2Key.wasPressedThisFrame) TrySort(WasteCategory.Organic);
                    if (keyboard.digit3Key.wasPressedThisFrame) TrySort(WasteCategory.Residual);
                }
#else
                if (Input.GetKeyDown(KeyCode.Tab)) ToggleHudCursor();
                if (Input.GetKeyDown(KeyCode.F)) Interact();
                if (Input.GetKeyDown(KeyCode.Q)) DropCarried();
                if (Input.GetKeyDown(KeyCode.Alpha1)) TrySort(WasteCategory.Recyclable);
                if (Input.GetKeyDown(KeyCode.Alpha2)) TrySort(WasteCategory.Organic);
                if (Input.GetKeyDown(KeyCode.Alpha3)) TrySort(WasteCategory.Residual);
#endif
                AdvanceTime(Time.deltaTime);
                if (Time.unscaledTime >= nextSave)
                { Commit(progress.Copy()); nextSave = Time.unscaledTime + 5f; }
            }
            if (HasActiveSessionOrUI && Time.unscaledTime >= nextRefresh)
            { RefreshUI(); nextRefresh = Time.unscaledTime + 0.1f; }
        }
    }
}
