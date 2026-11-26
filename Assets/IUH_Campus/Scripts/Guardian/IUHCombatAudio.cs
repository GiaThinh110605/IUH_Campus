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
        private static AudioClip s_FireworkWhistleClip;
        private static AudioClip s_FireworkBurstClip;

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
                if (Application.isPlaying) DontDestroyOnLoad(go);
            }
        }

        private const int SampleRate = 44100;
        private const float TwoPi = 2.0f * Mathf.PI;

        public static void PlaySword() => PlayClip(ref s_SwordClip, GenerateSwordSlash, 0.95f, 1.15f, 0.9f);

        public static void PlayShield() => PlayClip(ref s_ShieldClip, GenerateShieldBlock, 0.98f, 1.08f, 0.85f);

        public static void PlayHeal() => PlayClip(ref s_HealClip, GenerateHealChime, 1.0f, 1.0f, 0.8f);

        public static void PlayEnergy() => PlayClip(ref s_EnergyClip, GenerateEnergyLaser, 0.95f, 1.05f, 0.75f);

        public static void PlayLaserShoot() => PlayEnergy();

        public static void PlayFireworkWhistle(float volume = 0.85f) =>
            PlayClip(ref s_FireworkWhistleClip, GenerateFireworkWhistle, 0.95f, 1.1f, volume);

        public static void PlayFireworkBurst(float volume = 1.0f) =>
            PlayClip(ref s_FireworkBurstClip, GenerateFireworkBurst, 0.92f, 1.08f, volume);

        public static void PlaySkillBurst() => PlayClip(ref s_SkillClip, GenerateSkillExplosion, 1.0f, 1.0f, 1.0f);

        public static void PlayCombo(int comboCount)
        {
            float pitch = 1.0f + Mathf.Clamp(comboCount * 0.12f, 0f, 0.8f);
            PlayClip(ref s_ComboClip, GenerateComboChime, pitch, pitch, 0.9f);
        }

        public static void PlayBossHit() => PlayClip(ref s_BossHitClip, GenerateBossHit, 0.85f, 1.05f, 0.9f);

        public static void PlayBossRoar() => PlayClip(ref s_BossRoarClip, GenerateBossRoar, 0.9f, 1.05f, 0.95f);

        public static void PlayCoin() => PlayClip(ref s_CoinClip, GenerateCoinDing, 0.98f, 1.05f, 0.85f);

        public static void PlayPunchWhoosh() => PlayClip(ref s_PunchWhooshClip, GeneratePunchWhoosh, 0.95f, 1.2f, 0.85f);

        public static void PlayPunchHit() => PlayClip(ref s_PunchHitClip, GeneratePunchHit, 0.92f, 1.12f, 1.0f);

        private static void PlayClip(ref AudioClip cache, System.Func<AudioClip> generator, float minPitch, float maxPitch, float volume)
        {
            EnsureInstance();
            if (cache == null) cache = generator();
            Instance.m_AudioSource.pitch = minPitch == maxPitch ? minPitch : Random.Range(minPitch, maxPitch);
            Instance.m_AudioSource.PlayOneShot(cache, volume);
        }

        // ================= PROCEDURAL SYNTHESIS ================= //

        private static AudioClip SynthesizeClip(string clipName, float duration, System.Func<float, float, float> sampleFunc)
        {
            int samples = Mathf.RoundToInt(SampleRate * duration);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                data[i] = sampleFunc(t, t / duration);
            }
            AudioClip clip = AudioClip.Create(clipName, samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip GenerateSwordSlash() => SynthesizeClip("SFX_SwordSlash", 0.22f, (t, progress) =>
        {
            float freq = Mathf.Lerp(1600f, 220f, progress);
            float env = Mathf.Pow(1.0f - progress, 1.8f);
            float noise = (Random.value * 2.0f - 1.0f) * 0.45f;
            float tone = Mathf.Sin(TwoPi * freq * t) * 0.55f;
            return (tone + noise) * env;
        });

        private static AudioClip GenerateShieldBlock() => SynthesizeClip("SFX_ShieldBlock", 0.28f, (t, progress) =>
        {
            float env = Mathf.Exp(-12.0f * progress);
            float tone1 = Mathf.Sin(TwoPi * 480f * t);
            float tone2 = Mathf.Sin(TwoPi * 920f * t) * 0.45f;
            float tone3 = Mathf.Sin(TwoPi * 1840f * t) * 0.25f;
            return ((tone1 + tone2 + tone3) / 1.7f) * env;
        });

        private static AudioClip GenerateHealChime() => SynthesizeClip("SFX_HealChime", 0.38f, (t, progress) =>
        {
            float env = Mathf.Sin(progress * Mathf.PI) * Mathf.Exp(-3.0f * progress);
            float toneA = Mathf.Sin(TwoPi * 523.25f * t);
            float toneB = Mathf.Sin(TwoPi * 659.25f * t) * 0.7f;
            float toneC = Mathf.Sin(TwoPi * 783.99f * t) * 0.5f;
            return ((toneA + toneB + toneC) / 2.2f) * env;
        });

        private static AudioClip GenerateEnergyLaser() => SynthesizeClip("SFX_EnergyCharge", 0.25f, (t, progress) =>
        {
            float freq = Mathf.Lerp(300f, 1400f, progress * progress);
            float env = Mathf.Pow(1.0f - progress, 1.2f);
            return Mathf.Sin(TwoPi * freq * t) * env;
        });

        private static AudioClip GenerateSkillExplosion() => SynthesizeClip("SFX_SkillBurst", 0.55f, (t, progress) =>
        {
            float env = Mathf.Pow(1.0f - progress, 1.5f);
            float bassFreq = Mathf.Lerp(120f, 35f, progress);
            float subBass = Mathf.Sin(TwoPi * bassFreq * t);
            float noise = (Random.value * 2.0f - 1.0f) * 0.65f;
            return (subBass * 0.6f + noise * 0.4f) * env;
        });

        private static AudioClip GenerateBossHit() => SynthesizeClip("SFX_BossHit", 0.26f, (t, progress) =>
        {
            float env = Mathf.Exp(-14.0f * progress);
            float bass = Mathf.Sin(TwoPi * 75f * t);
            float punch = (Random.value * 2.0f - 1.0f) * 0.5f;
            return (bass * 0.7f + punch * 0.3f) * env;
        });

        private static AudioClip GenerateBossRoar() => SynthesizeClip("SFX_BossRoar", 0.45f, (t, progress) =>
        {
            float env = Mathf.Sin(progress * Mathf.PI) * Mathf.Pow(1.0f - progress, 0.8f);
            float mod = Mathf.Sin(TwoPi * 18f * t);
            float growl = Mathf.Sin(TwoPi * (90f + mod * 25f) * t);
            float breath = (Random.value * 2.0f - 1.0f) * 0.35f;
            return (growl * 0.7f + breath * 0.3f) * env;
        });

        private static AudioClip GenerateComboChime() => SynthesizeClip("SFX_ComboChime", 0.3f, (t, progress) =>
        {
            float env = Mathf.Exp(-8.0f * progress);
            float tone1 = Mathf.Sin(TwoPi * 659.25f * t);
            float tone2 = Mathf.Sin(TwoPi * 1318.5f * t) * 0.35f;
            return (tone1 + tone2) * env * 0.85f;
        });

        private static AudioClip GenerateCoinDing() => SynthesizeClip("SFX_CoinDing", 0.32f, (t, progress) =>
        {
            float env = Mathf.Exp(-9.0f * progress);
            float tone = Mathf.Sin(TwoPi * 987.77f * t);
            float overtone = Mathf.Sin(TwoPi * 1975.53f * t) * 0.4f;
            return (tone + overtone) * env * 0.85f;
        });

        private static AudioClip GeneratePunchWhoosh() => SynthesizeClip("SFX_PunchWhoosh", 0.16f, (t, progress) =>
        {
            float freq = Mathf.Lerp(800f, 120f, progress);
            float env = Mathf.Sin(progress * Mathf.PI);
            float noise = (Random.value * 2f - 1f) * 0.6f;
            float tone = Mathf.Sin(TwoPi * freq * t) * 0.4f;
            return (tone + noise) * env * 0.9f;
        });

        private static AudioClip GeneratePunchHit() => SynthesizeClip("SFX_PunchHit", 0.22f, (t, progress) =>
        {
            float env = Mathf.Exp(-12f * progress);
            float thud = Mathf.Sin(TwoPi * Mathf.Lerp(120f, 45f, progress) * t) * 0.7f;
            float snap = (Random.value * 2f - 1f) * Mathf.Exp(-35f * progress) * 0.6f;
            return Mathf.Clamp(thud + snap, -1f, 1f) * env;
        });

        private static AudioClip GenerateFireworkWhistle() => SynthesizeClip("SFX_FireworkWhistle", 0.55f, (t, progress) =>
        {
            float freq = Mathf.Lerp(400f, 1600f, progress * progress);
            float env = Mathf.Sin(progress * Mathf.PI * 0.9f);
            float whistle = Mathf.Sin(TwoPi * freq * t) * 0.65f;
            float airyNoise = (Random.value * 2f - 1f) * 0.25f;
            return Mathf.Clamp((whistle + airyNoise) * env, -1f, 1f) * 0.8f;
        });

        private static AudioClip GenerateFireworkBurst() => SynthesizeClip("SFX_FireworkBurst", 1.1f, (t, progress) =>
        {
            float boomEnv = Mathf.Exp(-7.0f * progress);
            float boom = Mathf.Sin(TwoPi * Mathf.Lerp(110f, 40f, progress) * t) * boomEnv * 0.9f;
            float crackleEnv = Mathf.Exp(-3.5f * progress) * (progress > 0.1f ? 1f : progress * 10f);
            float crackle = (Random.value > 0.92f ? (Random.value * 2f - 1f) * 0.75f : 0f) * crackleEnv;
            return Mathf.Clamp(boom + crackle, -1f, 1f);
        });
    }
}

