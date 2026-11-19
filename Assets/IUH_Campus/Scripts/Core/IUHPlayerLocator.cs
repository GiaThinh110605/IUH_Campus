using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IUHCampus.Core
{
    public static class IUHPlayerLocator
    {
        public const float RetryInterval = 0.5f;
        public static Func<float> Clock = ReadClock;

        private static Transform s_Player;
        private static float s_NextSearch = float.NegativeInfinity;

        public static Transform Player
        {
            get
            {
                if (s_Player != null) return s_Player;

                float now = Clock();
                if (now < s_NextSearch) return null;

                s_NextSearch = now + RetryInterval;
                var player = GameObject.Find("Campus Student") ?? GameObject.FindGameObjectWithTag("Player");
                s_Player = player != null ? player.transform : null;
                return s_Player;
            }
        }

        public static void Reset()
        {
            s_Player = null;
            s_NextSearch = float.NegativeInfinity;
        }

        private static float ReadClock() => Time.unscaledTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnLoad()
        {
            Clock = ReadClock;
            Reset();
            SceneManager.activeSceneChanged -= OnSceneChanged;
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private static void OnSceneChanged(Scene previous, Scene current) => Reset();
    }
}
