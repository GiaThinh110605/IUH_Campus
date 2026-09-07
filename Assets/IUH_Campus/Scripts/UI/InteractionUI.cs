using UnityEngine;
using UnityEngine.UI;

namespace IUHCampus.UI
{
    public class InteractionUI : MonoBehaviour
    {
        private static InteractionUI s_Instance;
        public static InteractionUI Instance
        {
            get
            {
                if (s_Instance == null) s_Instance = FindFirstObjectByType<InteractionUI>();
                return s_Instance;
            }
            private set => s_Instance = value;
        }

        public GameObject promptRoot;
        public Text promptText;
        public Text debugText;
        public Text locationText;

        private void Awake()
        {
            s_Instance = this;
            if (promptRoot != null) promptRoot.SetActive(false);
        }

        private void OnEnable()
        {
            Core.GameEvents.OnLocationChanged += HandleLocationChanged;
        }

        private void OnDisable()
        {
            Core.GameEvents.OnLocationChanged -= HandleLocationChanged;
        }

        private void HandleLocationChanged(string loc)
        {
            SetLocationText(loc);
        }

        public static void SetLocationText(string loc)
        {
            if (Instance != null && Instance.locationText != null)
            {
                Instance.locationText.text = loc;
            }
        }

        public static void SetPrompt(string text, bool show)
        {
            if (Instance == null) return;

            if (Instance.promptRoot != null)
            {
                Instance.promptRoot.SetActive(show);
            }

            if (Instance.promptText != null)
            {
                Instance.promptText.text = text;
            }
        }

        public static void SetDebugMessage(string msg)
        {
            if (Instance != null && Instance.debugText != null)
            {
                Instance.debugText.text = msg;
            }
        }
    }
}
