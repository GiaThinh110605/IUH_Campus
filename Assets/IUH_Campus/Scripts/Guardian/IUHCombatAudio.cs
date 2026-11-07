using UnityEngine;

namespace IUHCampus.Guardian
{
    public class IUHCombatAudio : MonoBehaviour
    {
        public static IUHCombatAudio Instance { get; private set; }

        private AudioSource m_AudioSource;

        private static AudioClip s_SwordClip;
        private static AudioClip s_ShieldClip;
        private static AudioClip s_HealClip;
        private static AudioClip s_EnergyClip;
        private static AudioClip s_SkillClip;
        private static AudioClip s_BossHitClip;
        private static AudioClip s_BossRoarClip;
        private static AudioClip s_CoinClip;
        private static AudioClip s_ComboClip;
        private static AudioClip s_PunchWhooshClip;
        private static AudioClip s_PunchHitClip;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            m_AudioSource = GetComponent<AudioSource>();
            if (m_AudioSource == null)
            {
                m_AudioSource = gameObject.AddComponent<AudioSource>();
                m_AudioSource.playOnAwake = false;
                m_AudioSource.spatialBlend = 0f; // 2D clean UI audio
            }
        }

        public static void EnsureInstance()
        {
            if (Instance == null)
            {
                GameObject go = new GameObject("IUH_Combat_Audio");
                Instance = go.AddComponent<IUHCombatAudio>();
                DontDestroyOnLoad(go);
            }
        }

        public static void PlaySword()
        {
            EnsureInstance();
            if (s_SwordClip == null) s_SwordClip = GenerateSwordSlash();
            Instance.m_AudioSource.pitch = Random.Range(0.95f, 1.15f);
            Instance.m_AudioSource.PlayOneShot(s_SwordClip, 0.9f);
        }

        public static void PlayShield()
        {
            EnsureInstance();
            if (s_ShieldClip == null) s_ShieldClip = GenerateShieldBlock();
            Instance.m_AudioSource.pitch = Random.Range(0.98f, 1.08f);
            Instance.m_AudioSource.PlayOneShot(s_ShieldClip, 0.85f);
        }

        public static void PlayHeal()
        {
            EnsureInstance();
            if (s_HealClip == null) s_HealClip = GenerateHealChime();
            Instance.m_AudioSource.pitch = 1.0f;
            Instance.m_AudioSource.PlayOneShot(s_HealClip, 0.8f);
        }

        public static void PlayEnergy()
        {
            EnsureInstance();
            if (s_EnergyClip == null) s_EnergyClip = GenerateEnergyLaser();
            Instance.m_AudioSource.pitch = Random.Range(0.95f, 1.05f);
            Instance.m_AudioSource.PlayOneShot(s_EnergyClip, 0.75f);
        }

        public static void PlaySkillBurst()
        {
            EnsureInstance();
            if (s_SkillClip == null) s_SkillClip = GenerateSkillExplosion();
            Instance.m_AudioSource.pitch = 1.0f;
            Instance.m_AudioSource.PlayOneShot(s_SkillClip, 1.0f);
        }

        public static void PlayCombo(int comboCount)
        {
            EnsureInstance();
            if (s_ComboClip == null) s_ComboClip = GenerateComboChime();
            float pitch = 1.0f + Mathf.Clamp(comboCount * 0.12f, 0f, 0.8f);
            Instance.m_AudioSource.pitch = pitch;
            Instance.m_AudioSource.PlayOneShot(s_ComboClip, 0.9f);
        }

        public static void PlayBossHit()
        {
            EnsureInstance();
            if (s_BossHitClip == null) s_BossHitClip = GenerateBossHit();
            Instance.m_AudioSource.pitch = Random.Range(0.85f, 1.05f);
            Instance.m_AudioSource.PlayOneShot(s_BossHitClip, 0.9f);
        }

        public static void PlayBossRoar()
        {
            EnsureInstance();
            if (s_BossRoarClip == null) s_BossRoarClip = GenerateBossRoar();
            Instance.m_AudioSource.pitch = Random.Range(0.9f, 1.05f);
            Instance.m_AudioSource.PlayOneShot(s_BossRoarClip, 0.95f);
        }

        public static void PlayCoin()
        {
            EnsureInstance();
            if (s_CoinClip == null) s_CoinClip = GenerateCoinDing();
            Instance.m_AudioSource.pitch = Random.Range(0.98f, 1.05f);
            Instance.m_AudioSource.PlayOneShot(s_CoinClip, 0.85f);
        }

        public static void PlayPunchWhoosh()
        {
            EnsureInstance();
            if (s_PunchWhooshClip == null) s_PunchWhooshClip = GeneratePunchWhoosh();
            Instance.m_AudioSource.pitch = Random.Range(0.95f, 1.2f);
            Instance.m_AudioSource.PlayOneShot(s_PunchWhooshClip, 0.85f);
        }

        public static void PlayPunchHit()
        {
            EnsureInstance();
            if (s_PunchHitClip == null) s_PunchHitClip = GeneratePunchHit();
            Instance.m_AudioSource.pitch = Random.Range(0.92f, 1.12f);
            Instance.m_AudioSource.PlayOneShot(s_PunchHitClip, 1.0f);
        }

        // ================= PROCEDURAL SYNTHESIS ================= //

        private static AudioClip GenerateSwordSlash()
        {
            int sampleRate = 44100;
            float duration = 0.22f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;

                // Frequency drops rapidly like a blade cutting air
                float freq = Mathf.Lerp(1600f, 220f, progress);
                float env = Mathf.Pow(1.0f - progress, 1.8f);

                float noise = (Random.value * 2.0f - 1.0f) * 0.45f;
                float tone = Mathf.Sin(2.0f * Mathf.PI * freq * t) * 0.55f;

                data[i] = (tone + noise) * env;
            }

            AudioClip clip = AudioClip.Create("SFX_SwordSlash", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip GenerateShieldBlock()
        {
            int sampleRate = 44100;
            float duration = 0.28f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;

                float env = Mathf.Exp(-12.0f * progress);
                float tone1 = Mathf.Sin(2.0f * Mathf.PI * 480f * t);
                float tone2 = Mathf.Sin(2.0f * Mathf.PI * 920f * t) * 0.45f;
                float tone3 = Mathf.Sin(2.0f * Mathf.PI * 1840f * t) * 0.25f;

                data[i] = ((tone1 + tone2 + tone3) / 1.7f) * env;
            }

            AudioClip clip = AudioClip.Create("SFX_ShieldBlock", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip GenerateHealChime()
        {
            int sampleRate = 44100;
            float duration = 0.38f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;

                float env = Mathf.Sin(progress * Mathf.PI) * Mathf.Exp(-3.0f * progress);
                float toneA = Mathf.Sin(2.0f * Mathf.PI * 523.25f * t); // C5
                float toneB = Mathf.Sin(2.0f * Mathf.PI * 659.25f * t) * 0.7f; // E5
                float toneC = Mathf.Sin(2.0f * Mathf.PI * 783.99f * t) * 0.5f; // G5

                data[i] = ((toneA + toneB + toneC) / 2.2f) * env;
            }

            AudioClip clip = AudioClip.Create("SFX_HealChime", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip GenerateEnergyLaser()
        {
            int sampleRate = 44100;
            float duration = 0.25f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;

                // Rising futuristic pitch
                float freq = Mathf.Lerp(300f, 1400f, progress * progress);
                float env = Mathf.Pow(1.0f - progress, 1.2f);
                float tone = Mathf.Sin(2.0f * Mathf.PI * freq * t);

                data[i] = tone * env;
            }

            AudioClip clip = AudioClip.Create("SFX_EnergyCharge", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip GenerateSkillExplosion()
        {
            int sampleRate = 44100;
            float duration = 0.55f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;

                float env = Mathf.Pow(1.0f - progress, 1.5f);
                float bassFreq = Mathf.Lerp(120f, 35f, progress);
                float subBass = Mathf.Sin(2.0f * Mathf.PI * bassFreq * t);
                float noise = (Random.value * 2.0f - 1.0f) * 0.65f;

                data[i] = (subBass * 0.6f + noise * 0.4f) * env;
            }

            AudioClip clip = AudioClip.Create("SFX_SkillBurst", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip GenerateBossHit()
        {
            int sampleRate = 44100;
            float duration = 0.26f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;

                float env = Mathf.Exp(-14.0f * progress);
                float bass = Mathf.Sin(2.0f * Mathf.PI * 75f * t);
                float punch = (Random.value * 2.0f - 1.0f) * 0.5f;

                data[i] = (bass * 0.7f + punch * 0.3f) * env;
            }

            AudioClip clip = AudioClip.Create("SFX_BossHit", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip GenerateBossRoar()
        {
            int sampleRate = 44100;
            float duration = 0.45f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;

                float env = Mathf.Sin(progress * Mathf.PI) * Mathf.Pow(1.0f - progress, 0.8f);
                float mod = Mathf.Sin(2.0f * Mathf.PI * 18f * t); // growl vibrato
                float growl = Mathf.Sin(2.0f * Mathf.PI * (90f + mod * 25f) * t);
                float breath = (Random.value * 2.0f - 1.0f) * 0.35f;

                data[i] = (growl * 0.7f + breath * 0.3f) * env;
            }

            AudioClip clip = AudioClip.Create("SFX_BossRoar", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip GenerateComboChime()
        {
            int sampleRate = 44100;
            float duration = 0.3f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;

                float env = Mathf.Exp(-8.0f * progress);
                float tone1 = Mathf.Sin(2.0f * Mathf.PI * 659.25f * t); // E5
                float tone2 = Mathf.Sin(2.0f * Mathf.PI * 1318.5f * t) * 0.35f; // E6

                data[i] = (tone1 + tone2) * env * 0.85f;
            }

            AudioClip clip = AudioClip.Create("SFX_ComboChime", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip GenerateCoinDing()
        {
            int sampleRate = 44100;
            float duration = 0.32f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;

                float env = Mathf.Exp(-9.0f * progress);
                float tone = Mathf.Sin(2.0f * Mathf.PI * 987.77f * t); // B5
                float overtone = Mathf.Sin(2.0f * Mathf.PI * 1975.53f * t) * 0.4f;

                data[i] = (tone + overtone) * env * 0.85f;
            }

            AudioClip clip = AudioClip.Create("SFX_CoinDing", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip GeneratePunchWhoosh()
        {
            int sampleRate = 44100;
            float duration = 0.16f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;

                // Swift whoosh: pitch swoops down from 800Hz to 120Hz
                float freq = Mathf.Lerp(800f, 120f, progress);
                float env = Mathf.Sin(progress * Mathf.PI);
                float noise = (Random.value * 2f - 1f) * 0.6f;
                float tone = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.4f;

                data[i] = (tone + noise) * env * 0.9f;
            }

            AudioClip clip = AudioClip.Create("SFX_PunchWhoosh", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip GeneratePunchHit()
        {
            int sampleRate = 44100;
            float duration = 0.22f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;

                // Heavy impact thud: 120Hz down to 45Hz + crunchy crack noise
                float env = Mathf.Exp(-12f * progress);
                float thud = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(120f, 45f, progress) * t) * 0.7f;
                float snap = (Random.value * 2f - 1f) * Mathf.Exp(-35f * progress) * 0.6f;

                data[i] = Mathf.Clamp(thud + snap, -1f, 1f) * env;
            }

            AudioClip clip = AudioClip.Create("SFX_PunchHit", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}

