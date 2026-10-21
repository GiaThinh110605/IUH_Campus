using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IUHCampus.Minigames.PhotoHunter
{
    /// <summary>
    /// Master UI Controller for Photo Hunter Minigame.
    /// Manages Briefing Modal, HUD checklist, Viewfinder Overlay with camera reticle,
    /// flash shutter animation, toast notifications, and Victory Gallery dialog.
    /// Handles texture memory ownership and deallocation cleanly.
    /// </summary>
    public class IUHPhotoHunterUI : MonoBehaviour
    {
        [Header("Root Canvas")]
        [SerializeField] private Canvas m_ParentCanvas;

        [Header("1. Briefing Dialog")]
        [SerializeField] private GameObject m_BriefingPanel;
        [SerializeField] private Text m_TxtBriefingStatus;
        [SerializeField] private Button m_BtnStartMission;
        [SerializeField] private Button m_BtnStartAndTeleportGate1;
        [SerializeField] private Button m_BtnViewCompletedAlbum;
        [SerializeField] private Button m_BtnCloseBriefing;

        [Header("2. Exploration HUD")]
        [SerializeField] private GameObject m_HudPanel;
        [SerializeField] private Text m_TxtProgress;
        [SerializeField] private Text m_TxtScore;
        [SerializeField] private Text m_TxtTargetList;
        [SerializeField] private Button m_BtnOpenViewfinder;
        [SerializeField] private Button m_BtnHudViewAlbum;
        [SerializeField] private Button m_BtnCancelMission;

        [Header("3. Camera Viewfinder Overlay")]
        [SerializeField] private GameObject m_ViewfinderPanel;
        [SerializeField] private Text m_TxtCameraSpecs;
        [SerializeField] private Text m_TxtToastMessage;
        [SerializeField] private Button m_BtnCapturePhoto;
        [SerializeField] private Button m_BtnCloseViewfinder;
        [SerializeField] private Image m_FlashOverlay;

        [Header("4. Victory Gallery Dialog")]
        [SerializeField] private GameObject m_VictoryDialog;
        [SerializeField] private Text m_TxtVictoryTitle;
        [SerializeField] private Text m_TxtVictoryScore;
        [SerializeField] private RawImage[] m_PhotoThumbnails;
        [SerializeField] private Text[] m_PhotoLabels;
        [SerializeField] private Button m_BtnRestartMission;
        [SerializeField] private Button m_BtnExitToCampus;

        private Coroutine m_ToastCoroutine;
        private Coroutine m_FlashCoroutine;

        // Texture memory ownership: tracks textures loaded specifically for the gallery display
        private List<Texture2D> m_LoadedGalleryTextures = new List<Texture2D>();

        public int LoadedTextureCount => m_LoadedGalleryTextures != null ? m_LoadedGalleryTextures.Count : 0;
        public Canvas ParentCanvas => m_ParentCanvas != null ? m_ParentCanvas : GetComponentInParent<Canvas>();

        private void Awake()
        {
            if (m_ParentCanvas == null) m_ParentCanvas = GetComponentInParent<Canvas>();
            WireButtonListeners();
        }

        private void OnDestroy()
        {
            ClearLoadedGalleryTextures();
        }

        private void WireButtonListeners()
        {
            // Briefing buttons
            if (m_BtnStartMission != null)
            {
                m_BtnStartMission.onClick.RemoveAllListeners();
                m_BtnStartMission.onClick.AddListener(() => IUHPhotoHunterManager.Instance?.StartMission(false));
            }

            if (m_BtnStartAndTeleportGate1 != null)
            {
                m_BtnStartAndTeleportGate1.onClick.RemoveAllListeners();
                m_BtnStartAndTeleportGate1.onClick.AddListener(() => IUHPhotoHunterManager.Instance?.StartMission(true));
            }

            if (m_BtnViewCompletedAlbum != null)
            {
                m_BtnViewCompletedAlbum.onClick.RemoveAllListeners();
                m_BtnViewCompletedAlbum.onClick.AddListener(() => IUHPhotoHunterManager.Instance?.OpenAlbumFromBriefing());
            }

            if (m_BtnCloseBriefing != null)
            {
                m_BtnCloseBriefing.onClick.RemoveAllListeners();
                m_BtnCloseBriefing.onClick.AddListener(() => IUHPhotoHunterManager.Instance?.CancelMission());
            }

            // HUD buttons
            if (m_BtnOpenViewfinder != null)
            {
                m_BtnOpenViewfinder.onClick.RemoveAllListeners();
                m_BtnOpenViewfinder.onClick.AddListener(() => IUHPhotoHunterManager.Instance?.EnterViewfinderMode());
            }

            if (m_BtnHudViewAlbum != null)
            {
                m_BtnHudViewAlbum.onClick.RemoveAllListeners();
                m_BtnHudViewAlbum.onClick.AddListener(() => IUHPhotoHunterManager.Instance?.OpenAlbumFromBriefing());
            }

            if (m_BtnCancelMission != null)
            {
                m_BtnCancelMission.onClick.RemoveAllListeners();
                m_BtnCancelMission.onClick.AddListener(() => IUHPhotoHunterManager.Instance?.CancelMission());
            }

            // Viewfinder buttons
            if (m_BtnCapturePhoto != null)
            {
                m_BtnCapturePhoto.onClick.RemoveAllListeners();
                m_BtnCapturePhoto.onClick.AddListener(() => IUHPhotoHunterManager.Instance?.TriggerCapturePhoto());
            }

            if (m_BtnCloseViewfinder != null)
            {
                m_BtnCloseViewfinder.onClick.RemoveAllListeners();
                m_BtnCloseViewfinder.onClick.AddListener(() => IUHPhotoHunterManager.Instance?.ExitViewfinderMode());
            }

            // Victory buttons
            if (m_BtnRestartMission != null)
            {
                m_BtnRestartMission.onClick.RemoveAllListeners();
                m_BtnRestartMission.onClick.AddListener(() => IUHPhotoHunterManager.Instance?.RestartMission());
            }

            if (m_BtnExitToCampus != null)
            {
                m_BtnExitToCampus.onClick.RemoveAllListeners();
                m_BtnExitToCampus.onClick.AddListener(() => {
                    ClearLoadedGalleryTextures();
                    IUHPhotoHunterManager.Instance?.CancelMission();
                });
            }
        }

        // ================= STATE TOGGLES =================

        public void ShowBriefing(bool show, bool isAlreadyCompleted = false)
        {
            if (m_BriefingPanel != null)
            {
                m_BriefingPanel.SetActive(show);

                if (m_BtnViewCompletedAlbum != null)
                {
                    m_BtnViewCompletedAlbum.gameObject.SetActive(isAlreadyCompleted);
                }

                if (m_TxtBriefingStatus != null)
                {
                    if (isAlreadyCompleted)
                    {
                        m_TxtBriefingStatus.text = "<color=#10b981><b>✓ BẠN ĐÃ HOÀN THÀNH BỘ ẢNH (3/3 ĐỊA ĐIỂM - 300 ĐIỂM)</b></color>\nBạn có thể mở Album xem lại ảnh hoặc bắt đầu chụp lại.";
                    }
                    else
                    {
                        m_TxtBriefingStatus.text = "Chào mừng bạn đến với thử thách khám phá và săn ảnh đẹp khuôn viên trường!";
                    }
                }
            }

            if (show)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void ShowHUD(bool show, bool isCompleted = false)
        {
            if (m_HudPanel != null)
            {
                m_HudPanel.SetActive(show);
                if (m_BtnHudViewAlbum != null)
                {
                    m_BtnHudViewAlbum.gameObject.SetActive(isCompleted);
                }
            }
        }

        public void ShowViewfinder(bool show)
        {
            if (m_ViewfinderPanel != null) m_ViewfinderPanel.SetActive(show);
            if (show)
            {
                ShowToast("Rê chuột để ngắm góc máy. Nhấn [Space] / Click chuột để chụp.", 3.0f, false);
            }
        }

        public void ShowVictory(bool show)
        {
            if (m_VictoryDialog != null) m_VictoryDialog.SetActive(show);
            if (show)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                ClearLoadedGalleryTextures();
            }
        }

        public void HideAllDialogs()
        {
            if (m_BriefingPanel != null) m_BriefingPanel.SetActive(false);
            if (m_HudPanel != null) m_HudPanel.SetActive(false);
            if (m_ViewfinderPanel != null) m_ViewfinderPanel.SetActive(false);
            if (m_VictoryDialog != null) m_VictoryDialog.SetActive(false);
            if (m_FlashOverlay != null) m_FlashOverlay.gameObject.SetActive(false);
            ClearLoadedGalleryTextures();
        }

        // ================= DATA UPDATES =================

        public void UpdateHUD(int completedCount, int totalTargets, int currentScore, List<IUHPhotoTarget> targets)
        {
            if (m_TxtProgress != null)
            {
                m_TxtProgress.text = $"Tiến độ: {completedCount}/{totalTargets} địa điểm";
            }

            if (m_TxtScore != null)
            {
                m_TxtScore.text = $"Điểm: {currentScore:N0} đ";
            }

            if (m_BtnHudViewAlbum != null)
            {
                m_BtnHudViewAlbum.gameObject.SetActive(completedCount >= totalTargets && totalTargets > 0);
            }

            if (m_TxtTargetList != null && targets != null)
            {
                string listStr = "";
                for (int i = 0; i < targets.Count; i++)
                {
                    var t = targets[i];
                    string statusIcon = t.IsCompleted ? "<color=#10b981>[✓ Đã Chụp]</color>" : "<color=#f59e0b>[○ Chưa Chụp]</color>";
                    listStr += $"{i + 1}. {t.DisplayName} {statusIcon}\n";
                }
                m_TxtTargetList.text = listStr.TrimEnd();
            }
        }

        public void ShowToast(string message, float duration = 2.5f, bool isSuccess = false)
        {
            if (m_TxtToastMessage == null) return;

            if (m_ToastCoroutine != null) StopCoroutine(m_ToastCoroutine);
            m_ToastCoroutine = StartCoroutine(ToastRoutine(message, duration, isSuccess));
        }

        private IEnumerator ToastRoutine(string message, float duration, bool isSuccess)
        {
            m_TxtToastMessage.gameObject.SetActive(true);
            string prefix = isSuccess ? "<color=#10b981><b>✓ " : "<color=#ef4444><b>⚠ ";
            string suffix = "</b></color>";
            m_TxtToastMessage.text = prefix + message + suffix;

            yield return new WaitForSeconds(duration);

            m_TxtToastMessage.gameObject.SetActive(false);
            m_ToastCoroutine = null;
        }

        public void TriggerFlashShutter()
        {
            if (m_FlashOverlay == null) return;
            if (m_FlashCoroutine != null) StopCoroutine(m_FlashCoroutine);
            m_FlashCoroutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            m_FlashOverlay.gameObject.SetActive(true);
            Color col = Color.white;
            col.a = 0.85f;
            m_FlashOverlay.color = col;

            float elapsed = 0f;
            float duration = 0.28f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                col.a = Mathf.Lerp(0.85f, 0f, elapsed / duration);
                m_FlashOverlay.color = col;
                yield return null;
            }

            m_FlashOverlay.gameObject.SetActive(false);
            m_FlashCoroutine = null;
        }

        /// <summary>
        /// Populates the victory / album gallery with captured photos from disk.
        /// Reliably destroys any previous textures to prevent memory buildup.
        /// </summary>
        public void PopulateVictoryDialog(int totalScore, List<PhotoRecord> records)
        {
            // Clear any previously loaded gallery textures first
            ClearLoadedGalleryTextures();

            if (m_TxtVictoryScore != null)
            {
                m_TxtVictoryScore.text = $"TỔNG ĐIỂM: {totalScore:N0} ĐIỂM (Hoàn thành 3/3 địa điểm)";
            }

            if (m_PhotoThumbnails != null)
            {
                for (int i = 0; i < m_PhotoThumbnails.Length; i++)
                {
                    if (records != null && i < records.Count)
                    {
                        var rec = records[i];
                        Texture2D tex = IUHPhotoCaptureService.LoadTextureFromFile(rec.filePath);

                        if (tex != null)
                        {
                            m_LoadedGalleryTextures.Add(tex);
                            m_PhotoThumbnails[i].texture = tex;
                            m_PhotoThumbnails[i].color = Color.white;
                            m_PhotoThumbnails[i].gameObject.SetActive(true);

                            if (m_PhotoLabels != null && i < m_PhotoLabels.Length && m_PhotoLabels[i] != null)
                            {
                                m_PhotoLabels[i].text = $"{rec.displayName}\n<size=11><color=#94a3b8>{rec.timestamp}</color></size>";
                            }
                        }
                        else
                        {
                            // Missing / corrupted photo placeholder
                            m_PhotoThumbnails[i].texture = null;
                            m_PhotoThumbnails[i].color = new Color(0.15f, 0.15f, 0.15f, 1f);
                            m_PhotoThumbnails[i].gameObject.SetActive(true);

                            if (m_PhotoLabels != null && i < m_PhotoLabels.Length && m_PhotoLabels[i] != null)
                            {
                                m_PhotoLabels[i].text = $"{rec.displayName}\n<size=11><color=#ef4444>[Ảnh không tìm thấy trên đĩa]</color></size>";
                            }
                        }
                    }
                    else
                    {
                        if (m_PhotoThumbnails[i] != null)
                        {
                            m_PhotoThumbnails[i].texture = null;
                            m_PhotoThumbnails[i].gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

        private void ClearLoadedGalleryTextures()
        {
            if (m_PhotoThumbnails != null)
            {
                for (int i = 0; i < m_PhotoThumbnails.Length; i++)
                {
                    if (m_PhotoThumbnails[i] != null)
                    {
                        m_PhotoThumbnails[i].texture = null;
                    }
                }
            }

            if (m_LoadedGalleryTextures != null)
            {
                for (int i = 0; i < m_LoadedGalleryTextures.Count; i++)
                {
                    if (m_LoadedGalleryTextures[i] != null)
                    {
                        if (Application.isPlaying) Destroy(m_LoadedGalleryTextures[i]);
                        else DestroyImmediate(m_LoadedGalleryTextures[i]);
                    }
                }
                m_LoadedGalleryTextures.Clear();
            }
        }
    }
}
