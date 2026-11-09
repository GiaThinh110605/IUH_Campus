using System;
using System.Collections.Generic;
using IUHCampus.Guardian.Backend;
using IUHCampus.Guardian.Match3;
using UnityEngine;
using UnityEngine.UI;

namespace IUHCampus.Guardian
{
    public sealed class IUHGuardianBackendReporter : MonoBehaviour
    {
        [SerializeField] private GuardianBackendService m_Service;
        [SerializeField] private Match3CombatManager m_CombatManager;
        [SerializeField] private string m_QuestId = "campus";
        [SerializeField] private int m_FontSize = 20;
        private const int OverlayOrder = 1000;
        private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);
        private static readonly Vector2 StatusSize = new Vector2(640f, 40f);
        private static readonly Vector2 StatusInset = new Vector2(-24f, -18f);
        private readonly HashSet<IUHMonsterEncounter> m_WarnedEncounters = new HashSet<IUHMonsterEncounter>();
        private readonly HashSet<string> m_AcceptedEvents = new HashSet<string>();
        private GuardianEventOutbox m_Outbox;
        private Text m_StatusText;
        private bool m_ErrorLogged;

        private void OnEnable()
        {
            if (m_CombatManager != null) m_CombatManager.CombatFinished += HandleCombatFinished;
            EnsureStatusOverlay();
            BindOutbox();
        }

        private void Start() => BindOutbox();

        private void OnDisable()
        {
            if (m_CombatManager != null) m_CombatManager.CombatFinished -= HandleCombatFinished;
            if (m_Outbox != null) m_Outbox.StatusChanged -= HandleStatusChanged;
            m_Outbox = null;
        }

        private void BindOutbox()
        {
            if (m_Outbox != null || m_Service == null || m_Service.Outbox == null) return;
            m_Outbox = m_Service.Outbox;
            m_Outbox.StatusChanged += HandleStatusChanged;
            RefreshStatus();
        }

        private void HandleCombatFinished(IUHMonsterEncounter encounter, bool won)
        {
            try
            {
                var value = GuardianCombatReport.Create(won, m_QuestId, encounter != null ? encounter.MilestoneId : null);
                if (value == null)
                {
                    if (m_WarnedEncounters.Add(encounter))
                        Debug.LogWarning("[IUH Guardian] Combat report skipped: invalid quest/milestone identifier.", encounter);
                    return;
                }
                BindOutbox();
                if (m_Outbox == null) throw new InvalidOperationException("Backend outbox is unavailable.");
                m_Outbox.Enqueue(value);
            }
            catch (Exception error)
            {
                if (m_ErrorLogged) return;
                m_ErrorLogged = true;
                Debug.LogError("[IUH Guardian] Combat report could not be queued: " + error.GetType().Name, this);
            }
        }

        private void HandleStatusChanged(string eventId, GuardianSyncStatus status)
        {
            if (status == GuardianSyncStatus.Synced) m_AcceptedEvents.Add(eventId);
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            m_StatusText.text = $"Máy chủ: chờ {m_Outbox.PendingCount} · đã nhận {m_AcceptedEvents.Count} · lỗi {m_Outbox.RejectedCount}";
        }

        private void EnsureStatusOverlay()
        {
            if (m_StatusText != null) return;
            var root = new GameObject("GuardianCloudStatus", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            root.transform.SetParent(transform, false);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = OverlayOrder;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;
            CreateStatusText(root.transform);
        }

        private void CreateStatusText(Transform parent)
        {
            var label = new GameObject("Status", typeof(RectTransform), typeof(Text), typeof(Outline));
            label.transform.SetParent(parent, false);
            m_StatusText = label.GetComponent<Text>();
            m_StatusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_StatusText.fontSize = m_FontSize;
            m_StatusText.alignment = TextAnchor.UpperRight;
            m_StatusText.raycastTarget = false;
            m_StatusText.text = "Máy chủ: đang khởi tạo";
            var rect = m_StatusText.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one;
            rect.sizeDelta = StatusSize;
            rect.anchoredPosition = StatusInset;
        }
    }
}
