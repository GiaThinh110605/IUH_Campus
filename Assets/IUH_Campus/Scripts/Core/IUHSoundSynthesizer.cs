using UnityEngine;

namespace IUHCampus.Core
{
    /// <summary>
    /// Procedural Sound Synthesizer for IUH Campus & Minigames.
    /// Generates high quality procedural audio clips at runtime without any external asset files.
    /// </summary>
    public static class IUHSoundSynthesizer
    {
        private static AudioClip s_EngineClip;
        private static AudioClip s_HornClip;
        private static AudioClip s_PickupClip;
        private static AudioClip s_VictoryClip;
        private static AudioClip s_DefeatClip;
        private static AudioClip s_GhostWhooshClip;
        private static AudioClip s_CoinClip;
        private static AudioClip s_DamageClip;
        private static AudioClip s_ClickClip;

        /// <summary>
        /// Looped engine audio for motorcycle (2-stroke / 4-stroke thumping rhythm).
        /// </summary>
        public static AudioClip GetEngineSound()
        {
            if (s_EngineClip != null) return s_EngineClip;

            int sampleRate = 44100;
            float duration = 1.0f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            float baseFreq = 48.0f; // Low thumping motorcycle idle

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                // Harmonic synthesis for motorcycle cylinder combustion sound
                float s1 = Mathf.Sin(2f * Mathf.PI * baseFreq * t);
                float s2 = Mathf.Sin(2f * Mathf.PI * (baseFreq * 2.05f) * t) * 0.6f;
                float s3 = Mathf.Sin(2f * Mathf.PI * (baseFreq * 3.01f) * t) * 0.35f;
                float s4 = Mathf.Sin(2f * Mathf.PI * (baseFreq * 4.02f) * t) * 0.2f;

                // Thumping impulse envelope simulating piston stroke
                float stroke = Mathf.Pow(Mathf.Sin(2f * Mathf.PI * baseFreq * 0.5f * t), 4f);
                float noise = (Random.value * 2f - 1f) * 0.08f;

                samples[i] = (s1 + s2 + s3 + s4 + noise) * stroke * 0.7f;
            }

            s_EngineClip = AudioClip.Create("Procedural_Motorbike_Engine", totalSamples, 1, sampleRate, false);
            s_EngineClip.SetData(samples, 0);
            return s_EngineClip;
        }

        /// <summary>
        /// Dual-tone motorbike horn sound (440Hz + 554Hz musical third).
        /// </summary>
        public static AudioClip GetHornSound()
        {
            if (s_HornClip != null) return s_HornClip;

            int sampleRate = 44100;
            float duration = 0.35f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = 1f;
                if (t < 0.02f) env = t / 0.02f;
                else if (t > duration - 0.05f) env = (duration - t) / 0.05f;

                float tone1 = Mathf.Sin(2f * Mathf.PI * 440f * t);
                float tone2 = Mathf.Sin(2f * Mathf.PI * 554.37f * t);
                float tone3 = Mathf.Sin(2f * Mathf.PI * 880f * t) * 0.25f;

                samples[i] = ((tone1 + tone2 + tone3) / 2.25f) * env * 0.75f;
            }

            s_HornClip = AudioClip.Create("Procedural_Horn", totalSamples, 1, sampleRate, false);
            s_HornClip.SetData(samples, 0);
            return s_HornClip;
        }

        /// <summary>
        /// Crisp 2-tone melodic chime for picking up packages / checkpoints.
        /// </summary>
        public static AudioClip GetPickupSound()
        {
            if (s_PickupClip != null) return s_PickupClip;

            int sampleRate = 44100;
            float duration = 0.45f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float freq = t < 0.18f ? 587.33f : 880.0f; // D5 -> A5
                float noteT = t < 0.18f ? t : (t - 0.18f);
                float env = Mathf.Exp(-noteT * 9.0f);

                float wave = Mathf.Sin(2f * Mathf.PI * freq * t) + 0.3f * Mathf.Sin(2f * Mathf.PI * freq * 2f * t);
                samples[i] = wave * env * 0.6f;
            }

            s_PickupClip = AudioClip.Create("Procedural_Pickup", totalSamples, 1, sampleRate, false);
            s_PickupClip.SetData(samples, 0);
            return s_PickupClip;
        }

        /// <summary>
        /// Triumphant 4-note victory fanfare for delivery completion.
        /// </summary>
        public static AudioClip GetVictoryFanfare()
        {
            if (s_VictoryClip != null) return s_VictoryClip;

            int sampleRate = 44100;
            float duration = 1.3f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.50f }; // C5, E5, G5, C6
            float noteDuration = 0.25f;

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                int noteIdx = Mathf.Clamp(Mathf.FloorToInt(t / noteDuration), 0, notes.Length - 1);
                float freq = notes[noteIdx];
                float noteT = t - (noteIdx * noteDuration);
                float decayRate = (noteIdx == notes.Length - 1) ? 2.5f : 6.0f;
                float env = Mathf.Exp(-noteT * decayRate);

                float wave = Mathf.Sin(2f * Mathf.PI * freq * t) + 0.35f * Mathf.Sin(2f * Mathf.PI * freq * 2f * t);
                samples[i] = wave * env * 0.65f;
            }

            s_VictoryClip = AudioClip.Create("Procedural_Victory", totalSamples, 1, sampleRate, false);
            s_VictoryClip.SetData(samples, 0);
            return s_VictoryClip;
        }

        /// <summary>
        /// Descending fail buzz when time runs out or cargo is destroyed.
        /// </summary>
        public static AudioClip GetDefeatSound()
        {
            if (s_DefeatClip != null) return s_DefeatClip;

            int sampleRate = 44100;
            float duration = 0.8f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float freq = Mathf.Lerp(280f, 90f, t / duration);
                float env = Mathf.Clamp01(1f - (t / duration));
                float square = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t));
                samples[i] = square * env * 0.35f;
            }

            s_DefeatClip = AudioClip.Create("Procedural_Defeat", totalSamples, 1, sampleRate, false);
            s_DefeatClip.SetData(samples, 0);
            return s_DefeatClip;
        }

        /// <summary>
        /// Spooky ghost wind / screech swell when passing or colliding with ghost spirits.
        /// </summary>
        public static AudioClip GetGhostWhooshSound()
        {
            if (s_GhostWhooshClip != null) return s_GhostWhooshClip;

            int sampleRate = 44100;
            float duration = 1.0f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(Mathf.PI * (t / duration));
                float f1 = 220f + Mathf.Sin(t * 15f) * 60f;
                float f2 = 330f + Mathf.Cos(t * 11f) * 80f;

                float sineWave = Mathf.Sin(2f * Mathf.PI * f1 * t) + Mathf.Sin(2f * Mathf.PI * f2 * t);
                float noise = (Random.value * 2f - 1f) * 0.4f;

                samples[i] = (sineWave * 0.4f + noise * 0.6f) * env * 0.5f;
            }

            s_GhostWhooshClip = AudioClip.Create("Procedural_GhostWhoosh", totalSamples, 1, sampleRate, false);
            s_GhostWhooshClip.SetData(samples, 0);
            return s_GhostWhooshClip;
        }

        /// <summary>
        /// Metallic coin ding for earnings and score rewards.
        /// </summary>
        public static AudioClip GetCoinClinkSound()
        {
            if (s_CoinClip != null) return s_CoinClip;

            int sampleRate = 44100;
            float duration = 0.35f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 14f);
                float wave = Mathf.Sin(2f * Mathf.PI * 1864f * t) + 0.5f * Mathf.Sin(2f * Mathf.PI * 2793f * t);
                samples[i] = wave * env * 0.5f;
            }

            s_CoinClip = AudioClip.Create("Procedural_Coin", totalSamples, 1, sampleRate, false);
            s_CoinClip.SetData(samples, 0);
            return s_CoinClip;
        }

        /// <summary>
        /// Impact / crash sound when hitting obstacles.
        /// </summary>
        public static AudioClip GetDamageSound()
        {
            if (s_DamageClip != null) return s_DamageClip;

            int sampleRate = 44100;
            float duration = 0.35f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 12f);
                float lowImpact = Mathf.Sin(2f * Mathf.PI * 75f * t);
                float noise = (Random.value * 2f - 1f);
                samples[i] = (lowImpact * 0.6f + noise * 0.4f) * env * 0.7f;
            }

            s_DamageClip = AudioClip.Create("Procedural_Damage", totalSamples, 1, sampleRate, false);
            s_DamageClip.SetData(samples, 0);
            return s_DamageClip;
        }

        /// <summary>
        /// UI click / tap sound.
        /// </summary>
        public static AudioClip GetClickSound()
        {
            if (s_ClickClip != null) return s_ClickClip;

            int sampleRate = 44100;
            float duration = 0.08f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 50f);
                float wave = Mathf.Sin(2f * Mathf.PI * 1200f * t);
                samples[i] = wave * env * 0.4f;
            }

            s_ClickClip = AudioClip.Create("Procedural_Click", totalSamples, 1, sampleRate, false);
            s_ClickClip.SetData(samples, 0);
            return s_ClickClip;
        }

        private static AudioClip s_HeartbeatClip;

        /// <summary>
        /// Horror thumping heartbeat sound for ghost pursuit.
        /// </summary>
        public static AudioClip GetHeartbeatSound()
        {
            if (s_HeartbeatClip != null) return s_HeartbeatClip;

            int sampleRate = 44100;
            float duration = 0.45f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 18f);
                float wave = Mathf.Sin(2f * Mathf.PI * 52f * t);
                samples[i] = wave * env * 0.85f;
            }

            s_HeartbeatClip = AudioClip.Create("Procedural_Heartbeat", totalSamples, 1, sampleRate, false);
            s_HeartbeatClip.SetData(samples, 0);
            return s_HeartbeatClip;
        }

        private static AudioClip s_ShutterClip;
        private static AudioClip s_PhotoSuccessClip;
        private static AudioClip s_PhotoWarningClip;

        /// <summary>
        /// Mechanical DSLR camera shutter click with mirror flip sound.
        /// </summary>
        public static AudioClip GetCameraShutterSound()
        {
            if (s_ShutterClip != null) return s_ShutterClip;

            int sampleRate = 44100;
            float duration = 0.22f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env1 = Mathf.Exp(-t * 45f);
                float env2 = t > 0.08f ? Mathf.Exp(-(t - 0.08f) * 40f) : 0f;
                float click1 = Mathf.Sin(2f * Mathf.PI * 2200f * t) * env1;
                float click2 = Mathf.Sin(2f * Mathf.PI * 1400f * t) * env2;
                float noise = (Random.value * 2f - 1f) * (env1 * 0.5f + env2 * 0.4f);
                samples[i] = Mathf.Clamp((click1 + click2 + noise) * 0.7f, -1f, 1f);
            }

            s_ShutterClip = AudioClip.Create("Procedural_Camera_Shutter", totalSamples, 1, sampleRate, false);
            s_ShutterClip.SetData(samples, 0);
            return s_ShutterClip;
        }

        /// <summary>
        /// Rewarding photo capture harmonic chime (C6 - E6 - G6 chord).
        /// </summary>
        public static AudioClip GetPhotoSuccessSound()
        {
            if (s_PhotoSuccessClip != null) return s_PhotoSuccessClip;

            int sampleRate = 44100;
            float duration = 0.55f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 6f);
                float c6 = Mathf.Sin(2f * Mathf.PI * 1046.5f * t);
                float e6 = Mathf.Sin(2f * Mathf.PI * 1318.5f * t);
                float g6 = Mathf.Sin(2f * Mathf.PI * 1567.98f * t);
                samples[i] = (c6 * 0.4f + e6 * 0.35f + g6 * 0.35f) * env * 0.6f;
            }

            s_PhotoSuccessClip = AudioClip.Create("Procedural_Photo_Success", totalSamples, 1, sampleRate, false);
            s_PhotoSuccessClip.SetData(samples, 0);
            return s_PhotoSuccessClip;
        }

        /// <summary>
        /// Soft warning beep for invalid photo angles or out of range.
        /// </summary>
        public static AudioClip GetPhotoWarningSound()
        {
            if (s_PhotoWarningClip != null) return s_PhotoWarningClip;

            int sampleRate = 44100;
            float duration = 0.16f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 18f);
                float tone = Mathf.Sin(2f * Mathf.PI * 330f * t); // Low E4 buzz
                samples[i] = tone * env * 0.5f;
            }

            s_PhotoWarningClip = AudioClip.Create("Procedural_Photo_Warning", totalSamples, 1, sampleRate, false);
            s_PhotoWarningClip.SetData(samples, 0);
            return s_PhotoWarningClip;
        }

        private static AudioClip s_FlashlightClickClip;
        private static AudioClip s_BatSwingClip;
        private static AudioClip s_BatHitClip;
        private static AudioClip s_FlareHissClip;
        private static AudioClip s_TalismanChantClip;
        private static AudioClip s_ItemPickupClip;
        private static AudioClip s_HealClip;
        private static AudioClip s_GhostMoanClip;

        /// <summary>
        /// Mechanical click switch for flashlight toggle.
        /// </summary>
        public static AudioClip GetFlashlightClickSound()
        {
            if (s_FlashlightClickClip != null) return s_FlashlightClickClip;

            int sampleRate = 44100;
            float duration = 0.08f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 90f);
                float tone1 = Mathf.Sin(2f * Mathf.PI * 3200f * t);
                float tone2 = Mathf.Sin(2f * Mathf.PI * 1800f * t);
                float noise = (Random.value * 2f - 1f) * 0.4f;
                samples[i] = (tone1 * 0.5f + tone2 * 0.3f + noise) * env * 0.75f;
            }

            s_FlashlightClickClip = AudioClip.Create("Procedural_Flashlight_Click", totalSamples, 1, sampleRate, false);
            s_FlashlightClickClip.SetData(samples, 0);
            return s_FlashlightClickClip;
        }

        /// <summary>
        /// Swoosh sound when swinging the self-defense bat.
        /// </summary>
        public static AudioClip GetBatSwingSound()
        {
            if (s_BatSwingClip != null) return s_BatSwingClip;

            int sampleRate = 44100;
            float duration = 0.25f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(Mathf.PI * (t / duration));
                float freq = Mathf.Lerp(160f, 480f, t / duration);
                float tone = Mathf.Sin(2f * Mathf.PI * freq * t);
                float noise = (Random.value * 2f - 1f) * 0.65f;
                samples[i] = (tone * 0.4f + noise * 0.6f) * env * 0.85f;
            }

            s_BatSwingClip = AudioClip.Create("Procedural_Bat_Swing", totalSamples, 1, sampleRate, false);
            s_BatSwingClip.SetData(samples, 0);
            return s_BatSwingClip;
        }

        /// <summary>
        /// Heavy impact thud when hitting a ghost with the bat.
        /// </summary>
        public static AudioClip GetBatHitSound()
        {
            if (s_BatHitClip != null) return s_BatHitClip;

            int sampleRate = 44100;
            float duration = 0.35f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 22f);
                float thud = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(120f, 40f, t / duration) * t);
                float crack = Mathf.Sin(2f * Mathf.PI * 1200f * t) * Mathf.Exp(-t * 60f);
                float noise = (Random.value * 2f - 1f) * Mathf.Exp(-t * 40f);
                samples[i] = Mathf.Clamp((thud * 0.8f + crack * 0.5f + noise * 0.5f) * env, -1f, 1f);
            }

            s_BatHitClip = AudioClip.Create("Procedural_Bat_Hit", totalSamples, 1, sampleRate, false);
            s_BatHitClip.SetData(samples, 0);
            return s_BatHitClip;
        }

        /// <summary>
        /// Sizzling burn hiss of an emergency roadside flare.
        /// </summary>
        public static AudioClip GetFlareHissSound()
        {
            if (s_FlareHissClip != null) return s_FlareHissClip;

            int sampleRate = 44100;
            float duration = 1.2f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float sizzle1 = Mathf.Sin(2f * Mathf.PI * 3400f * t);
                float sizzle2 = Mathf.Sin(2f * Mathf.PI * 4800f * t);
                float whiteNoise = (Random.value * 2f - 1f);
                float crackle = (Random.value > 0.96f) ? (Random.value * 2f - 1f) * 1.5f : 0f;
                samples[i] = ((sizzle1 * 0.2f + sizzle2 * 0.15f + whiteNoise * 0.5f + crackle) * 0.5f);
            }

            s_FlareHissClip = AudioClip.Create("Procedural_Flare_Hiss", totalSamples, 1, sampleRate, false);
            s_FlareHissClip.SetData(samples, 0);
            return s_FlareHissClip;
        }

        /// <summary>
        /// Harmonic sacred bell chime when talisman ward activates.
        /// </summary>
        public static AudioClip GetTalismanChantSound()
        {
            if (s_TalismanChantClip != null) return s_TalismanChantClip;

            int sampleRate = 44100;
            float duration = 0.95f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 3.8f);
                float b1 = Mathf.Sin(2f * Mathf.PI * 523.25f * t); // C5
                float b2 = Mathf.Sin(2f * Mathf.PI * 659.25f * t); // E5
                float b3 = Mathf.Sin(2f * Mathf.PI * 783.99f * t); // G5
                float b4 = Mathf.Sin(2f * Mathf.PI * 1046.50f * t); // C6
                float shimmer = Mathf.Sin(2f * Mathf.PI * 14f * t);
                samples[i] = (b1 * 0.35f + b2 * 0.25f + b3 * 0.25f + b4 * 0.2f) * env * (0.8f + shimmer * 0.2f) * 0.8f;
            }

            s_TalismanChantClip = AudioClip.Create("Procedural_Talisman_Chant", totalSamples, 1, sampleRate, false);
            s_TalismanChantClip.SetData(samples, 0);
            return s_TalismanChantClip;
        }

        /// <summary>
        /// Crisp bright chime when picking up a survival item.
        /// </summary>
        public static AudioClip GetItemPickupSound()
        {
            if (s_ItemPickupClip != null) return s_ItemPickupClip;

            int sampleRate = 44100;
            float duration = 0.4f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 7f);
                float f = (t < 0.15f) ? 880f : 1318.5f;
                float tone = Mathf.Sin(2f * Mathf.PI * f * t);
                float overtone = Mathf.Sin(2f * Mathf.PI * f * 2f * t) * 0.3f;
                samples[i] = (tone + overtone) * env * 0.65f;
            }

            s_ItemPickupClip = AudioClip.Create("Procedural_Item_Pickup", totalSamples, 1, sampleRate, false);
            s_ItemPickupClip.SetData(samples, 0);
            return s_ItemPickupClip;
        }

        /// <summary>
        /// Soothing ascending arpeggio when using first aid kit.
        /// </summary>
        public static AudioClip GetHealSound()
        {
            if (s_HealClip != null) return s_HealClip;

            int sampleRate = 44100;
            float duration = 0.6f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(Mathf.PI * (t / duration));
                float freq = Mathf.Lerp(440f, 880f, t / duration);
                float tone = Mathf.Sin(2f * Mathf.PI * freq * t);
                samples[i] = tone * env * 0.6f;
            }

            s_HealClip = AudioClip.Create("Procedural_Heal", totalSamples, 1, sampleRate, false);
            s_HealClip.SetData(samples, 0);
            return s_HealClip;
        }

        /// <summary>
        /// Creepy eerie moan of ghost presence.
        /// </summary>
        public static AudioClip GetGhostMoanSound()
        {
            if (s_GhostMoanClip != null) return s_GhostMoanClip;

            int sampleRate = 44100;
            float duration = 1.4f;
            int totalSamples = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(Mathf.PI * (t / duration));
                float vibrato = Mathf.Sin(2f * Mathf.PI * 5.5f * t) * 35f;
                float freq = 210f + vibrato;
                float tone = Mathf.Sin(2f * Mathf.PI * freq * t);
                float breath = (Random.value * 2f - 1f) * 0.25f;
                samples[i] = (tone * 0.75f + breath) * env * 0.55f;
            }

            s_GhostMoanClip = AudioClip.Create("Procedural_Ghost_Moan", totalSamples, 1, sampleRate, false);
            s_GhostMoanClip.SetData(samples, 0);
            return s_GhostMoanClip;
        }
    }
}
