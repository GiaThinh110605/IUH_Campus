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
    }
}
