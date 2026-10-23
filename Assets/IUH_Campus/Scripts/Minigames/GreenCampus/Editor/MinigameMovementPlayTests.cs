using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using StarterAssets;
using IUHCampus.UI;
using IUHCampus.Minigames.PhotoHunter;

namespace IUHCampus.Minigames.GreenCampus.Editor
{
    // Runs the real PlayerInput -> Update -> CharacterController -> Cinemachine loop.
    // Requires the main campus scene in Play Mode and uses isolated save directories.
    public static class MinigameMovementPlayTests
    {
        [Serializable]
        public class Report
        {
            public bool running;
            public int passed, failed, sampledFrames;
            public float greenMinY = float.MaxValue, greenMaxY = float.MinValue;
            public float photoMinY = float.MaxValue, photoMaxY = float.MinValue;
            public List<string> checks = new List<string>();
        }
        public static Report Result { get; private set; } = new Report();
        private static IEnumerator<float> steps;
        private static double nextStep;
        private static float gameWaitUntil;
        private static int lastFrame;
        private static ThirdPersonController controller;
        private static IUHGreenCampusManager green;
        private static IUHPhotoHunterManager photo;
        private static string oldGreenDirectory;
        private static bool trackGreen, trackPhoto;
        private static Key[] heldKeys = Array.Empty<Key>();
        private static Vector2 mouseDelta;
        private static string photoDirectory;
        private static InputSettings.BackgroundBehavior previousBackground;
        private static InputSettings.EditorInputBehaviorInPlayMode previousEditorInput;
        private static bool previousRunInBackground;
        private static bool previousKeyboardEnabled, previousMouseEnabled;
        private const string Evidence = "Docs/Evidence/MovementRegression";

        [MenuItem("IUH Campus/Tests/Run Movement Regression (Play Mode)")]
        public static void Run()
        {
            if (!Application.isPlaying || Keyboard.current == null || Result.running)
                throw new InvalidOperationException("Open the main scene in Play Mode with a keyboard first.");
            controller = UnityEngine.Object.FindFirstObjectByType<ThirdPersonController>(FindObjectsInactive.Include);
            green = IUHGreenCampusManager.Instance;
            photo = IUHPhotoHunterManager.Instance;
            if (controller == null || green == null || photo == null)
                throw new InvalidOperationException("Both games and the player must exist in the main scene.");
            Result = new Report { running = true };
            oldGreenDirectory = GreenCampusStorage.DirectoryOverride;
            var folder = Path.Combine(Application.temporaryCachePath, "IUH_Movement_" + Guid.NewGuid().ToString("N"));
            GreenCampusStorage.DirectoryOverride = Path.Combine(folder, "Green");
            photoDirectory = Path.Combine(folder, "Photo");
            IUHPhotoHunterStorage.SetCustomStorageDirectoryForTest(photoDirectory);
            green.Initialize();
            previousRunInBackground = Application.runInBackground;
            previousKeyboardEnabled = Keyboard.current.enabled;
            previousMouseEnabled = Mouse.current != null && Mouse.current.enabled;
            previousBackground = InputSystem.settings.backgroundBehavior;
            previousEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            Application.runInBackground = true;
            // MCP can run with Codex in front. Route synthetic input to the Game View for this test only.
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            // Changing IgnoreFocus does not reactivate devices already disabled on focus loss.
            if (!Keyboard.current.enabled) InputSystem.EnableDevice(Keyboard.current);
            if (Mouse.current != null && !Mouse.current.enabled) InputSystem.EnableDevice(Mouse.current);
            EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView")).Focus();
            trackGreen = trackPhoto = false;
            lastFrame = -1;
            nextStep = EditorApplication.timeSinceStartup;
            gameWaitUntil = 0;
            steps = Exercise();
            InputSystem.onBeforeUpdate += FeedKeyboard;
            EditorApplication.update += Tick;
        }

        private static void Keys(params Key[] keys)
        { heldKeys = keys; }

        private static void FeedKeyboard()
        {
            // Editor input updates have a separate state buffer and must not consume test events.
            if (InputState.currentUpdateType == InputUpdateType.Dynamic && Keyboard.current != null)
            {
                InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(heldKeys));
                if (Mouse.current != null && mouseDelta != Vector2.zero)
                {
                    InputSystem.QueueDeltaStateEvent(Mouse.current.delta, mouseDelta);
                    mouseDelta = Vector2.zero;
                }
            }
        }

        private static void Check(bool condition, string message)
        {
            if (condition) Result.passed++; else Result.failed++;
            Result.checks.Add((condition ? "PASS " : "FAIL ") + message);
        }

        private static void Button(MonoBehaviour ui, string name)
        {
            ui.GetComponentsInChildren<Button>(true).First(b => b.name == name).onClick.Invoke();
        }

        private static IEnumerator<float> Exercise()
        {
            var portal = IUHMiniGamePortalUI.Instance;
            portal.LaunchGreenCampusGame();
            Keys(Key.Space); yield return 0.15f;
            Button(green.ui, "Begin");
            Keys(); trackGreen = true;
            yield return 2f;
            Check(controller.enabled && controller.Grounded, "Green start releases controller and lands on courtyard");
            Check(Camera.main.transform.position.y < 5f, "Green camera remains at walking height");
            Check(controller.transform.position.y < 1f, "Start button does not launch player into the air");
            Keys(Key.Space); yield return 0.35f;
            Check(controller.transform.position.y > 0.7f, "Space reaches the real player jump action");
            yield return 2.5f;
            Keys(); yield return 0.3f;
            Check(controller.Grounded && controller.transform.position.y < 1f, "Held Space lands without repeated automatic jumps");
            Vector3 before = controller.transform.position;
            Keys(Key.W); yield return 0.8f; Keys(); yield return 0.3f;
            Check(Vector3.Distance(before, controller.transform.position) > 0.5f, "Green WASD moves the character through PlayerInput");
            Keys(Key.S); yield return 0.8f; Keys(); yield return 0.3f;
            Keys(Key.F); yield return 0.15f; Keys(); yield return 0.2f;
            Check(green.CarriedItem != null, "F picks up reachable courtyard litter");
            if (green.CarriedItem != null)
            {
                var key = green.CarriedItem.category == WasteCategory.Recyclable ? Key.Digit1 :
                    green.CarriedItem.category == WasteCategory.Organic ? Key.Digit2 : Key.Digit3;
                Keys(key); yield return 0.15f; Keys(); yield return 0.3f;
                Check(green.SortedCount == 1 && green.CurrentScore == 20, "Number key sorts litter and awards 20 points");
            }
            Check(Result.greenMaxY < 3f && Result.greenMinY > 0f, "Green stays near ground throughout walking and jumping");
            trackGreen = false;
            green.ToggleHudCursor(); // Switching away while a previous game owns a modal lock.
            portal.LaunchPhotoHunterGame();
            photo.RestartMission();
            photo.OpenBriefing();
            var ui = UnityEngine.Object.FindFirstObjectByType<IUHPhotoHunterUI>(FindObjectsInactive.Include);
            Button(ui, "BtnTeleportGate1");
            trackPhoto = true;
            yield return 2f;
            var cam = Camera.main.GetComponent<Unity.Cinemachine.CinemachineBrain>().ActiveVirtualCamera as Unity.Cinemachine.CinemachineCamera;
            Check(controller.enabled && controller.Grounded, "Photo start after Green cursor mode restores walking");
            Check(cam.Follow == controller.CinemachineCameraTarget.transform, "Photo camera follows player, not the motorbike");
            before = controller.transform.position;
            Keys(Key.W); yield return 0.6f; Keys(); yield return 0.3f;
            Check(Vector3.Distance(before, controller.transform.position) > 0.4f, "Photo WASD moves the character through PlayerInput");
            Keys(Key.S); yield return 0.6f; Keys(); yield return 0.3f;
            Keys(Key.Space); yield return 0.35f;
            Check(controller.transform.position.y > 0.7f, "Photo Space jump works during exploration");
            Keys(); yield return 1.5f;
            Keys(Key.C); yield return 0.15f; Keys(); yield return 0.5f;
            Check(photo.CurrentState == IUHPhotoHunterManager.PhotoHunterState.ViewfinderMode, "C opens the viewfinder through keyboard input");
            float yaw = controller.CinemachineCameraTarget.transform.eulerAngles.y;
            // The project's Look binding scales mouse pixels by 0.05 (100 px => 5 degrees).
            mouseDelta = new Vector2(100f, 0f); yield return 0.3f;
            Check(Mathf.Abs(Mathf.DeltaAngle(yaw, controller.CinemachineCameraTarget.transform.eulerAngles.y)) > 1f,
                "Mouse input rotates the photo viewfinder");
            Check(Camera.main.transform.position.y < 5f, "Photo viewfinder stays at eye height");
            var target = UnityEngine.Object.FindObjectsByType<IUHPhotoTarget>(FindObjectsSortMode.None).First(t => t.TargetId.Contains("gate_01"));
            controller.SetCameraOrientation(target.transform.position - (controller.transform.position + Vector3.up * 1.4f));
            yield return 0.5f;
            before = controller.transform.position;
            Keys(Key.W); yield return 0.5f; Keys(); yield return 0.3f;
            Check(Vector3.Distance(before, controller.transform.position) < 0.05f, "Viewfinder blocks walking without locking camera control");
            Keys(Key.Space); yield return 0.15f; Keys(); yield return 2f;
            Check(photo.CompletedCount == 1, "Space validates, captures and saves the gate photo");
            Check(Directory.Exists(photoDirectory) && Directory.GetFiles(photoDirectory, "*.jpg", SearchOption.AllDirectories).Length == 1,
                "Captured gate JPEG exists on disk");
            Keys(Key.Q); yield return 0.15f; Keys(); yield return 0.5f;
            Check(photo.CurrentState == IUHPhotoHunterManager.PhotoHunterState.Exploring && controller.MoveSpeed > 0f,
                "Q exits aiming and restores exploration");
            Check(Result.photoMaxY < 3f && Result.photoMinY > 0f, "Photo stays at ground level instead of a top-down aerial view");
            trackPhoto = false;
            var bike = UnityEngine.Object.FindFirstObjectByType<IUHCampus.Minigames.GrabCampus.IUHGrabMotorbike>();
            photo.CancelMission();
            bike.MountVehicle(controller.gameObject); // Bike may be mounted even with no active Grab mission.
            portal.LaunchGreenCampusGame(); Button(green.ui, "Begin");
            yield return 1f;
            Check(!bike.IsMounted && controller.gameObject.activeInHierarchy && controller.enabled,
                "Starting Green dismounts an idle Grab bike and restores the active player");
            Check(cam.Follow == controller.CinemachineCameraTarget.transform, "Green restores player camera after idle bike");
            portal.LaunchPhotoHunterGame(); Button(ui, "BtnStart"); yield return 1f;
            Check(controller.enabled && controller.Grounded && photo.IsMissionActive,
                "Green to Photo ordinary Start button restores ground movement");
        }

        private static void Tick()
        {
            try
            {
                if (!Application.isPlaying) throw new InvalidOperationException("Play Mode stopped during the regression.");
                if (lastFrame != Time.frameCount)
                {
                    lastFrame = Time.frameCount;
                    Result.sampledFrames++;
                    float y = controller.transform.position.y;
                    if (trackGreen) { Result.greenMinY = Mathf.Min(Result.greenMinY, y); Result.greenMaxY = Mathf.Max(Result.greenMaxY, y); }
                    if (trackPhoto) { Result.photoMinY = Mathf.Min(Result.photoMinY, y); Result.photoMaxY = Mathf.Max(Result.photoMaxY, y); }
                }
                if (EditorApplication.timeSinceStartup < nextStep || Time.time < gameWaitUntil) return;
                if (steps.MoveNext())
                {
                    nextStep = EditorApplication.timeSinceStartup + steps.Current;
                    gameWaitUntil = Time.time + steps.Current;
                }
                else Finish();
            }
            catch (Exception ex) { Check(false, ex.ToString()); Finish(); }
        }

        private static void Finish()
        {
            EditorApplication.update -= Tick;
            InputSystem.onBeforeUpdate -= FeedKeyboard;
            if (Application.isPlaying)
            {
                Keys();
                InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
                green.CancelMission();
                photo.CancelMission();
            }
            GreenCampusStorage.DirectoryOverride = oldGreenDirectory;
            IUHPhotoHunterStorage.SetCustomStorageDirectoryForTest(null);
            InputSystem.settings.backgroundBehavior = previousBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = previousEditorInput;
            Application.runInBackground = previousRunInBackground;
            if (!previousKeyboardEnabled && Keyboard.current != null) InputSystem.DisableDevice(Keyboard.current);
            if (!previousMouseEnabled && Mouse.current != null) InputSystem.DisableDevice(Mouse.current);
            Result.running = false;
            Directory.CreateDirectory(Evidence);
            File.WriteAllText(Path.Combine(Evidence, "playmode.json"), JsonUtility.ToJson(Result, true));
            Debug.Log("[Movement Regression] " + Result.passed + " passed; " + Result.failed + " failed.");
        }
    }
}
