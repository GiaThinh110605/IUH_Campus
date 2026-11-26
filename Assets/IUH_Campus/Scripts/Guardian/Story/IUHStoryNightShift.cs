using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IUHCampus.Guardian.Story
{
    public sealed class IUHStoryNightShift : MonoBehaviour
    {
        const float NightIntensity = .25f;
        const float NightSunIntensity = .14f;
        const float NightAmbientIntensity = .30f;
        const float NightReflectionIntensity = .30f;
        static readonly Color NightColor = new Color(120f / 255, 138f / 255, 185f / 255);
        static readonly Color NightAmbientSky = new Color(.09f, .11f, .17f);
        static readonly Color NightAmbientEquator = new Color(.07f, .09f, .14f);
        static readonly Color NightAmbientGround = new Color(.045f, .055f, .08f);
        static readonly Color NightFog = new Color(.07f, .08f, .13f);
        static readonly Color LedColor = new Color(176f / 255, 0, 1) * 4;
        readonly List<Renderer> leds = new List<Renderer>();
        Light sun;
        float dayIntensity;
        float dayAmbientIntensity = 1f;
        float dayReflectionIntensity = 1f;
        Color dayColor;
        Color dayAmbient;
        Color dayAmbientEquator;
        Color dayAmbientGround;
        Color dayFog;
        Coroutine transition;
        Material originalSkybox;
        Material nightSkybox;
        Material customNightSkybox;
        float dayExposure;
        float customNightExposure = 1f;
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
            dayAmbientEquator = RenderSettings.ambientEquatorColor;
            dayAmbientGround = RenderSettings.ambientGroundColor;
            dayAmbientIntensity = RenderSettings.ambientIntensity;
            dayReflectionIntensity = RenderSettings.reflectionIntensity;
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

        public void SetNightSkyboxMaterial(Material nightSkyboxOverride)
        {
            if (customNightSkybox != null)
            {
                Destroy(customNightSkybox);
                customNightSkybox = null;
            }
            if (nightSkyboxOverride == null) return;
            customNightSkybox = new Material(nightSkyboxOverride);
            customNightSkybox.name = nightSkyboxOverride.name + " (Story Night Runtime)";
            if (nightSkyboxOverride.HasProperty("_Exposure"))
                customNightExposure = nightSkyboxOverride.GetFloat("_Exposure");
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
            Shader.SetGlobalFloat("_IUH_NightAmount", isActiveAndEnabled ? fraction : 0);
            if (sun != null)
            {
                sun.intensity = Mathf.Lerp(dayIntensity, dayIntensity * NightSunIntensity, fraction);
                sun.color = Color.Lerp(dayColor, NightColor, fraction);
            }

            RenderSettings.ambientLight = Color.Lerp(dayAmbient, NightAmbientSky, fraction);
            RenderSettings.ambientSkyColor = Color.Lerp(dayAmbient, NightAmbientSky, fraction);
            RenderSettings.ambientEquatorColor = Color.Lerp(dayAmbientEquator, NightAmbientEquator, fraction);
            RenderSettings.ambientGroundColor = Color.Lerp(dayAmbientGround, NightAmbientGround, fraction);
            RenderSettings.ambientIntensity = Mathf.Lerp(dayAmbientIntensity, NightAmbientIntensity, fraction);
            RenderSettings.reflectionIntensity = Mathf.Lerp(dayReflectionIntensity, NightReflectionIntensity, fraction);
            RenderSettings.fogColor = Color.Lerp(dayFog, NightFog, fraction);
            ApplySkybox(fraction);
        }

        void ApplySkybox(float fraction)
        {
            if (fraction <= 0)
            {
                if (originalSkybox != null) RenderSettings.skybox = originalSkybox;
                return;
            }
            if (customNightSkybox != null)
            {
                if (fraction < 0.5f && nightSkybox != null)
                {
                    float fadeOut = fraction * 2f;
                    if (nightSkybox.HasProperty("_Exposure"))
                        nightSkybox.SetFloat("_Exposure", Mathf.Lerp(dayExposure, 0.08f, fadeOut));
                    RenderSettings.skybox = nightSkybox;
                }
                else
                {
                    float fadeIn = Mathf.Clamp01((fraction - 0.5f) * 2f);
                    if (customNightSkybox.HasProperty("_Exposure"))
                        customNightSkybox.SetFloat("_Exposure", Mathf.Lerp(0.15f, customNightExposure, fadeIn));
                    RenderSettings.skybox = customNightSkybox;
                }
                return;
            }
            if (nightSkybox == null) return;
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

        void OnDisable()
        {
            Shader.SetGlobalFloat("_IUH_NightAmount", 0);
            if (dayAmbient != default)
            {
                RenderSettings.ambientLight = dayAmbient;
                RenderSettings.ambientSkyColor = dayAmbient;
                RenderSettings.ambientEquatorColor = dayAmbientEquator;
                RenderSettings.ambientGroundColor = dayAmbientGround;
            }
            RenderSettings.ambientIntensity = dayAmbientIntensity;
            RenderSettings.reflectionIntensity = dayReflectionIntensity;
        }

        void OnDestroy()
        {
            if (RenderSettings.skybox == nightSkybox || RenderSettings.skybox == customNightSkybox)
                RenderSettings.skybox = originalSkybox;
            if (nightSkybox != null) Destroy(nightSkybox);
            if (customNightSkybox != null) Destroy(customNightSkybox);
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
