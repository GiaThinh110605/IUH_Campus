using UnityEngine;

namespace IUHCampus.Core
{
    public static class IUHSoundSynthesizer
    {
        private const int SampleRate = 44100;
        private const float TwoPi = 2f * Mathf.PI;

        private static AudioClip s_EngineClip;
        private static AudioClip s_HornClip;
        private static AudioClip s_PickupClip;
        private static AudioClip s_VictoryClip;
        private static AudioClip s_DefeatClip;
        private static AudioClip s_GhostWhooshClip;
        private static AudioClip s_CoinClip;
        private static AudioClip s_DamageClip;
        private static AudioClip s_ClickClip;
        private static AudioClip s_HeartbeatClip;
        private static AudioClip s_ShutterClip;
        private static AudioClip s_PhotoSuccessClip;
        private static AudioClip s_PhotoWarningClip;
        private static AudioClip s_FlashlightClickClip;
        private static AudioClip s_BatSwingClip;
        private static AudioClip s_BatHitClip;
        private static AudioClip s_FlareHissClip;
        private static AudioClip s_TalismanChantClip;
        private static AudioClip s_ItemPickupClip;
        private static AudioClip s_HealClip;
        private static AudioClip s_GhostMoanClip;

        private static AudioClip GetOrSynthesize(
            ref AudioClip cache,
            string clipName,
            float duration,
            System.Func<float, float> sampleFunc)
        {
            if (cache != null) return cache;

            int totalSamples = Mathf.RoundToInt(SampleRate * duration);
            float[] samples = new float[totalSamples];
            for (int i = 0; i < totalSamples; i++)
            {
                samples[i] = sampleFunc((float)i / SampleRate);
            }

            cache = AudioClip.Create(clipName, totalSamples, 1, SampleRate, false);
            cache.SetData(samples, 0);
            return cache;
        }

        public static AudioClip GetEngineSound()
        {
            const float baseFreq = 48.0f;
            return GetOrSynthesize(ref s_EngineClip, "Procedural_Motorbike_Engine", 1.0f, t =>
            {
                float s1 = Mathf.Sin(TwoPi * baseFreq * t);
                float s2 = Mathf.Sin(TwoPi * (baseFreq * 2.05f) * t) * 0.6f;
                float s3 = Mathf.Sin(TwoPi * (baseFreq * 3.01f) * t) * 0.35f;
                float s4 = Mathf.Sin(TwoPi * (baseFreq * 4.02f) * t) * 0.2f;
                float stroke = Mathf.Pow(Mathf.Sin(TwoPi * baseFreq * 0.5f * t), 4f);
                float noise = (Random.value * 2f - 1f) * 0.08f;
                return (s1 + s2 + s3 + s4 + noise) * stroke * 0.7f;
            });
        }

        public static AudioClip GetHornSound()
        {
            const float duration = 0.35f;
            return GetOrSynthesize(ref s_HornClip, "Procedural_Horn", duration, t =>
            {
                float env = 1f;
                if (t < 0.02f) env = t / 0.02f;
                else if (t > duration - 0.05f) env = (duration - t) / 0.05f;

                float tone1 = Mathf.Sin(TwoPi * 440f * t);
                float tone2 = Mathf.Sin(TwoPi * 554.37f * t);
                float tone3 = Mathf.Sin(TwoPi * 880f * t) * 0.25f;
                return ((tone1 + tone2 + tone3) / 2.25f) * env * 0.75f;
            });
        }

        public static AudioClip GetPickupSound()
        {
            return GetOrSynthesize(ref s_PickupClip, "Procedural_Pickup", 0.45f, t =>
            {
                float freq = t < 0.18f ? 587.33f : 880.0f;
                float noteT = t < 0.18f ? t : (t - 0.18f);
                float env = Mathf.Exp(-noteT * 9.0f);
                float wave = Mathf.Sin(TwoPi * freq * t) + 0.3f * Mathf.Sin(TwoPi * freq * 2f * t);
                return wave * env * 0.6f;
            });
        }

        public static AudioClip GetVictoryFanfare()
        {
            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.50f };
            const float noteDuration = 0.25f;
            return GetOrSynthesize(ref s_VictoryClip, "Procedural_Victory", 1.3f, t =>
            {
                int noteIdx = Mathf.Clamp(Mathf.FloorToInt(t / noteDuration), 0, notes.Length - 1);
                float freq = notes[noteIdx];
                float noteT = t - (noteIdx * noteDuration);
                float decayRate = (noteIdx == notes.Length - 1) ? 2.5f : 6.0f;
                float env = Mathf.Exp(-noteT * decayRate);
                float wave = Mathf.Sin(TwoPi * freq * t) + 0.35f * Mathf.Sin(TwoPi * freq * 2f * t);
                return wave * env * 0.65f;
            });
        }

        public static AudioClip GetDefeatSound()
        {
            const float duration = 0.8f;
            return GetOrSynthesize(ref s_DefeatClip, "Procedural_Defeat", duration, t =>
            {
                float freq = Mathf.Lerp(280f, 90f, t / duration);
                float env = Mathf.Clamp01(1f - (t / duration));
                float square = Mathf.Sign(Mathf.Sin(TwoPi * freq * t));
                return square * env * 0.35f;
            });
        }

        public static AudioClip GetGhostWhooshSound()
        {
            const float duration = 1.0f;
            return GetOrSynthesize(ref s_GhostWhooshClip, "Procedural_GhostWhoosh", duration, t =>
            {
                float env = Mathf.Sin(Mathf.PI * (t / duration));
                float f1 = 220f + Mathf.Sin(t * 15f) * 60f;
                float f2 = 330f + Mathf.Cos(t * 11f) * 80f;
                float sineWave = Mathf.Sin(TwoPi * f1 * t) + Mathf.Sin(TwoPi * f2 * t);
                float noise = (Random.value * 2f - 1f) * 0.4f;
                return (sineWave * 0.4f + noise * 0.6f) * env * 0.5f;
            });
        }

        public static AudioClip GetCoinClinkSound()
        {
            return GetOrSynthesize(ref s_CoinClip, "Procedural_Coin", 0.35f, t =>
            {
                float env = Mathf.Exp(-t * 14f);
                float wave = Mathf.Sin(TwoPi * 1864f * t) + 0.5f * Mathf.Sin(TwoPi * 2793f * t);
                return wave * env * 0.5f;
            });
        }

        public static AudioClip GetDamageSound()
        {
            return GetOrSynthesize(ref s_DamageClip, "Procedural_Damage", 0.35f, t =>
            {
                float env = Mathf.Exp(-t * 12f);
                float lowImpact = Mathf.Sin(TwoPi * 75f * t);
                float noise = Random.value * 2f - 1f;
                return (lowImpact * 0.6f + noise * 0.4f) * env * 0.7f;
            });
        }

        public static AudioClip GetClickSound()
        {
            return GetOrSynthesize(ref s_ClickClip, "Procedural_Click", 0.08f, t =>
            {
                float env = Mathf.Exp(-t * 50f);
                return Mathf.Sin(TwoPi * 1200f * t) * env * 0.4f;
            });
        }

        public static AudioClip GetHeartbeatSound()
        {
            return GetOrSynthesize(ref s_HeartbeatClip, "Procedural_Heartbeat", 0.45f, t =>
            {
                float env = Mathf.Exp(-t * 18f);
                return Mathf.Sin(TwoPi * 52f * t) * env * 0.85f;
            });
        }

        public static AudioClip GetCameraShutterSound()
        {
            return GetOrSynthesize(ref s_ShutterClip, "Procedural_Camera_Shutter", 0.22f, t =>
            {
                float env1 = Mathf.Exp(-t * 45f);
                float env2 = t > 0.08f ? Mathf.Exp(-(t - 0.08f) * 40f) : 0f;
                float click1 = Mathf.Sin(TwoPi * 2200f * t) * env1;
                float click2 = Mathf.Sin(TwoPi * 1400f * t) * env2;
                float noise = (Random.value * 2f - 1f) * (env1 * 0.5f + env2 * 0.4f);
                return Mathf.Clamp((click1 + click2 + noise) * 0.7f, -1f, 1f);
            });
        }

        public static AudioClip GetPhotoSuccessSound()
        {
            return GetOrSynthesize(ref s_PhotoSuccessClip, "Procedural_Photo_Success", 0.55f, t =>
            {
                float env = Mathf.Exp(-t * 6f);
                float c6 = Mathf.Sin(TwoPi * 1046.5f * t);
                float e6 = Mathf.Sin(TwoPi * 1318.5f * t);
                float g6 = Mathf.Sin(TwoPi * 1567.98f * t);
                return (c6 * 0.4f + e6 * 0.35f + g6 * 0.35f) * env * 0.6f;
            });
        }

        public static AudioClip GetPhotoWarningSound()
        {
            return GetOrSynthesize(ref s_PhotoWarningClip, "Procedural_Photo_Warning", 0.16f, t =>
            {
                float env = Mathf.Exp(-t * 18f);
                return Mathf.Sin(TwoPi * 330f * t) * env * 0.5f;
            });
        }

        public static AudioClip GetFlashlightClickSound()
        {
            return GetOrSynthesize(ref s_FlashlightClickClip, "Procedural_Flashlight_Click", 0.08f, t =>
            {
                float env = Mathf.Exp(-t * 90f);
                float tone1 = Mathf.Sin(TwoPi * 3200f * t);
                float tone2 = Mathf.Sin(TwoPi * 1800f * t);
                float noise = (Random.value * 2f - 1f) * 0.4f;
                return (tone1 * 0.5f + tone2 * 0.3f + noise) * env * 0.75f;
            });
        }

        public static AudioClip GetBatSwingSound()
        {
            const float duration = 0.25f;
            return GetOrSynthesize(ref s_BatSwingClip, "Procedural_Bat_Swing", duration, t =>
            {
                float env = Mathf.Sin(Mathf.PI * (t / duration));
                float freq = Mathf.Lerp(160f, 480f, t / duration);
                float tone = Mathf.Sin(TwoPi * freq * t);
                float noise = (Random.value * 2f - 1f) * 0.65f;
                return (tone * 0.4f + noise * 0.6f) * env * 0.85f;
            });
        }

        public static AudioClip GetBatHitSound()
        {
            const float duration = 0.35f;
            return GetOrSynthesize(ref s_BatHitClip, "Procedural_Bat_Hit", duration, t =>
            {
                float env = Mathf.Exp(-t * 22f);
                float thud = Mathf.Sin(TwoPi * Mathf.Lerp(120f, 40f, t / duration) * t);
                float crack = Mathf.Sin(TwoPi * 1200f * t) * Mathf.Exp(-t * 60f);
                float noise = (Random.value * 2f - 1f) * Mathf.Exp(-t * 40f);
                return Mathf.Clamp((thud * 0.8f + crack * 0.5f + noise * 0.5f) * env, -1f, 1f);
            });
        }

        public static AudioClip GetFlareHissSound()
        {
            return GetOrSynthesize(ref s_FlareHissClip, "Procedural_Flare_Hiss", 1.2f, t =>
            {
                float sizzle1 = Mathf.Sin(TwoPi * 3400f * t);
                float sizzle2 = Mathf.Sin(TwoPi * 4800f * t);
                float whiteNoise = Random.value * 2f - 1f;
                float crackle = (Random.value > 0.96f) ? (Random.value * 2f - 1f) * 1.5f : 0f;
                return (sizzle1 * 0.2f + sizzle2 * 0.15f + whiteNoise * 0.5f + crackle) * 0.5f;
            });
        }

        public static AudioClip GetTalismanChantSound()
        {
            return GetOrSynthesize(ref s_TalismanChantClip, "Procedural_Talisman_Chant", 0.95f, t =>
            {
                float env = Mathf.Exp(-t * 3.8f);
                float b1 = Mathf.Sin(TwoPi * 523.25f * t);
                float b2 = Mathf.Sin(TwoPi * 659.25f * t);
                float b3 = Mathf.Sin(TwoPi * 783.99f * t);
                float b4 = Mathf.Sin(TwoPi * 1046.50f * t);
                float shimmer = Mathf.Sin(TwoPi * 14f * t);
                return (b1 * 0.35f + b2 * 0.25f + b3 * 0.25f + b4 * 0.2f) * env * (0.8f + shimmer * 0.2f) * 0.8f;
            });
        }

        public static AudioClip GetItemPickupSound()
        {
            return GetOrSynthesize(ref s_ItemPickupClip, "Procedural_Item_Pickup", 0.4f, t =>
            {
                float env = Mathf.Exp(-t * 7f);
                float f = (t < 0.15f) ? 880f : 1318.5f;
                float tone = Mathf.Sin(TwoPi * f * t);
                float overtone = Mathf.Sin(TwoPi * f * 2f * t) * 0.3f;
                return (tone + overtone) * env * 0.65f;
            });
        }

        public static AudioClip GetHealSound()
        {
            const float duration = 0.6f;
            return GetOrSynthesize(ref s_HealClip, "Procedural_Heal", duration, t =>
            {
                float env = Mathf.Sin(Mathf.PI * (t / duration));
                float freq = Mathf.Lerp(440f, 880f, t / duration);
                return Mathf.Sin(TwoPi * freq * t) * env * 0.6f;
            });
        }

        public static AudioClip GetGhostMoanSound()
        {
            const float duration = 1.4f;
            return GetOrSynthesize(ref s_GhostMoanClip, "Procedural_Ghost_Moan", duration, t =>
            {
                float env = Mathf.Sin(Mathf.PI * (t / duration));
                float vibrato = Mathf.Sin(TwoPi * 5.5f * t) * 35f;
                float tone = Mathf.Sin(TwoPi * (210f + vibrato) * t);
                float breath = (Random.value * 2f - 1f) * 0.25f;
                return (tone * 0.75f + breath) * env * 0.55f;
            });
        }
    }
}
