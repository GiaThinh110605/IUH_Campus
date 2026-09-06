using System;
using UnityEngine;
using UnityEngine.UI;

namespace IUHCampus.UI
{
    public class SimplePanelUI : MonoBehaviour
    {
        private static SimplePanelUI s_Instance;
        public static SimplePanelUI Instance
        {
            get
            {
                if (s_Instance == null) s_Instance = FindFirstObjectByType<SimplePanelUI>();
                return s_Instance;
            }
            private set => s_Instance = value;
        }

        public GameObject panelRoot;
        public Text titleText;
        public Text bodyText;
        public Button closeButton;

        public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

        private void Awake()
        {
            s_Instance = this;
            if (panelRoot != null) panelRoot.SetActive(false);
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(HidePanel);
            }
        }

        private void Update()
        {
            if (IsOpen && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E)))
            {
                // Only close on E if we didn't just open it this frame
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    HidePanel();
                }
            }
        }

        public static void ShowPanel(string title, string content)
        {
            if (Instance != null)
            {
                if (Instance.titleText != null) Instance.titleText.text = title;
                if (Instance.bodyText != null) Instance.bodyText.text = content;
                if (Instance.panelRoot != null) Instance.panelRoot.SetActive(true);
            }
        }

        public static void Hide()
        {
            if (Instance != null) Instance.HidePanel();
        }

        public void HidePanel()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }
    }
}
