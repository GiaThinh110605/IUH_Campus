using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IUHCampus.Guardian.Story
{
    public sealed class IUHStoryNightShift : MonoBehaviour
    {
        const float NightIntensity = .25f;
        static readonly Color NightColor = new Color(106f / 255, 90f / 255, 205f / 255);
        static readonly Color LedColor = new Color(176f / 255, 0, 1) * 4;
        readonly List<Renderer> leds = new List<Renderer>();
        Light sun;
        float dayIntensity;
        Color dayColor;
        Color dayAmbient;
        Coroutine transition;
        public void Initialize(string festivalLedMaterialName)
        {
            foreach (var light in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (light.type != LightType.Directional)
                    continue;
                sun = light;
                dayIntensity = light.intensity;
                dayColor = light.color;
                break;
            }

            dayAmbient = RenderSettings.ambientLight;
            foreach (var renderer in FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (renderer.sharedMaterial != null && renderer.sharedMaterial.name.StartsWith(festivalLedMaterialName, System.StringComparison.Ordinal))
                    leds.Add(renderer);
            }
        }

        public void ToNight(float seconds) => Begin(true, seconds);
        public void ToDay(float seconds) => Begin(false, seconds);
        void Begin(bool night, float seconds)
        {
            if (transition != null)
                StopCoroutine(transition);
            transition = StartCoroutine(Shift(night, seconds));
        }

        public void SetNightImmediate()
        {
            if (transition != null)
                StopCoroutine(transition);
            Apply(1);
            SetLeds(true);
        }

        void Apply(float fraction)
        {
            if (sun != null)
            {
                sun.intensity = Mathf.Lerp(dayIntensity, dayIntensity * NightIntensity, fraction);
                sun.color = Color.Lerp(dayColor, NightColor, fraction);
            }

            RenderSettings.ambientLight = Color.Lerp(dayAmbient, dayAmbient * NightIntensity, fraction);
        }

        void SetLeds(bool night)
        {
            var block = new MaterialPropertyBlock();
            if (night)
                block.SetColor("_EmissionColor", LedColor);
            foreach (var renderer in leds)
            {
                if (renderer != null)
                    renderer.SetPropertyBlock(night ? block : null);
            }
        }

        IEnumerator Shift(bool night, float seconds)
        {
            if (night && sun != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    sun.intensity = 0;
                    yield return new WaitForSecondsRealtime(.15f);
                    sun.intensity = dayIntensity;
                    yield return new WaitForSecondsRealtime(.15f);
                }
            }

            SetLeds(night);
            for (float time = 0; time < seconds; time += Time.unscaledDeltaTime)
            {
                float fraction = time / seconds;
                Apply(night ? fraction : 1 - fraction);
                yield return null;
            }

            Apply(night ? 1 : 0);
            transition = null;
        }
    }
}
