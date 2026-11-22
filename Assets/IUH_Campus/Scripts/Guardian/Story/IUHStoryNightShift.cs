using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IUHCampus.Guardian.Story
{
    public sealed class IUHStoryNightShift : MonoBehaviour
    {
        const float NightIntensity = .25f;
        static readonly Color NightColor = new Color(106f / 255, 90f / 255, 205f / 255);
        static readonly Color NightFog = new Color(.15f, .13f, .25f);
        static readonly Color LedColor = new Color(176f / 255, 0, 1) * 4;
        readonly List<Renderer> leds = new List<Renderer>();
        Light sun;
        float dayIntensity;
        Color dayColor;
        Color dayAmbient;
        Color dayFog;
        Coroutine transition;
        Material originalSkybox;
        Material nightSkybox;
        float dayExposure;
        Color dayTint;
        public void Initialize(string festivalLedMaterialName)
        {
            Shader.SetGlobalFloat("_IUH_NightAmount", 0);
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
            dayFog = RenderSettings.fogColor;
            originalSkybox = RenderSettings.skybox;
            if (originalSkybox != null)
            {
                nightSkybox = new Material(originalSkybox);
                nightSkybox.name = originalSkybox.name + " (Story Runtime)";
                if (originalSkybox.HasProperty("_Exposure"))
                    dayExposure = originalSkybox.GetFloat("_Exposure");
                if (originalSkybox.HasProperty("_Tint"))
                    dayTint = originalSkybox.GetColor("_Tint");
            }
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
            Shader.SetGlobalFloat("_IUH_NightAmount", fraction);
            if (sun != null)
            {
                sun.intensity = Mathf.Lerp(dayIntensity, dayIntensity * NightIntensity, fraction);
                sun.color = Color.Lerp(dayColor, NightColor, fraction);
            }

            RenderSettings.ambientLight = Color.Lerp(dayAmbient, dayAmbient * NightIntensity, fraction);
            RenderSettings.fogColor = Color.Lerp(dayFog, NightFog, fraction);
            ApplySkybox(fraction);
        }

        void ApplySkybox(float fraction)
        {
            if (nightSkybox == null) return;
            if (fraction <= 0)
            {
                RenderSettings.skybox = originalSkybox;
                return;
            }
            if (nightSkybox.HasProperty("_Exposure"))
            {
                nightSkybox.SetFloat("_Exposure", Mathf.Lerp(dayExposure, dayExposure * NightIntensity, fraction));
            }
            else if (nightSkybox.HasProperty("_Tint"))
            {
                var tint = dayTint * NightIntensity;
                tint.a = dayTint.a;
                nightSkybox.SetColor("_Tint", Color.Lerp(dayTint, tint, fraction));
            }
            RenderSettings.skybox = nightSkybox;
        }

        void OnDisable() => Shader.SetGlobalFloat("_IUH_NightAmount", 0);

        void OnDestroy()
        {
            if (nightSkybox == null) return;
            if (RenderSettings.skybox == nightSkybox)
                RenderSettings.skybox = originalSkybox;
            Destroy(nightSkybox);
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
