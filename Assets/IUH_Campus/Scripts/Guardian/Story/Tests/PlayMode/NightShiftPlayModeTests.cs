using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class NightShiftPlayModeTests
{
    [UnityTest]
    public IEnumerator FacadeNightGlobalTracksTransitionsAndResetsOnDisable()
    {
        float previous = Shader.GetGlobalFloat("_IUH_NightAmount");
        var originalSkybox = RenderSettings.skybox;
        var go = new GameObject("FacadeNightGlobalTest");
        var type = Type.GetType("IUHCampus.Guardian.Story.IUHStoryNightShift, Assembly-CSharp", true);
        var shift = (Behaviour)go.AddComponent(type);
        try
        {
            Shader.SetGlobalFloat("_IUH_NightAmount", .5f);
            type.GetMethod("Initialize").Invoke(shift, new object[] { "UnusedStoryLed" });
            Assert.That(Shader.GetGlobalFloat("_IUH_NightAmount"), Is.Zero);
            type.GetMethod("ToNight").Invoke(shift, new object[] { 0f });
            float deadline = Time.realtimeSinceStartup + 4;
            while (Shader.GetGlobalFloat("_IUH_NightAmount") < 1 && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(Shader.GetGlobalFloat("_IUH_NightAmount"), Is.EqualTo(1));
            type.GetMethod("ToDay").Invoke(shift, new object[] { 0f });
            Assert.That(Shader.GetGlobalFloat("_IUH_NightAmount"), Is.Zero);
            type.GetMethod("SetNightImmediate").Invoke(shift, null);
            Assert.That(Shader.GetGlobalFloat("_IUH_NightAmount"), Is.EqualTo(1));
            type.GetMethod("ToNight").Invoke(shift, new object[] { 1f });
            shift.enabled = false;
            Assert.That(Shader.GetGlobalFloat("_IUH_NightAmount"), Is.Zero);
            yield return new WaitForSecondsRealtime(1.3f);
            Assert.That(Shader.GetGlobalFloat("_IUH_NightAmount"), Is.Zero);
        }
        finally
        {
            type.GetMethod("ToDay").Invoke(shift, new object[] { 0f });
            UnityEngine.Object.DestroyImmediate(go);
            RenderSettings.skybox = originalSkybox;
            Shader.SetGlobalFloat("_IUH_NightAmount", previous);
        }
    }

    [UnityTest]
    public IEnumerator FogTransitionsToNightAndRestoresDay()
    {
        var originalFog = RenderSettings.fogColor;
        var dayFog = new Color(.7f, .8f, .9f);
        var go = new GameObject("NightShiftFogTest");
        var type = Type.GetType("IUHCampus.Guardian.Story.IUHStoryNightShift, Assembly-CSharp", true);
        var shift = go.AddComponent(type);
        try
        {
            RenderSettings.fogColor = dayFog;
            type.GetMethod("Initialize").Invoke(shift, new object[] { "UnusedStoryLed" });
            type.GetMethod("ToNight").Invoke(shift, new object[] { 1f });
            var deadline = Time.realtimeSinceStartup + 3f;
            while (RenderSettings.fogColor == dayFog && Time.realtimeSinceStartup < deadline)
                yield return null;
            var transitionFog = RenderSettings.fogColor;
            Assert.That(transitionFog, Is.Not.EqualTo(dayFog));

            type.GetMethod("SetNightImmediate").Invoke(shift, null);
            var nightFog = RenderSettings.fogColor;
            Assert.That(nightFog, Is.Not.EqualTo(transitionFog));
            Assert.That(nightFog.grayscale, Is.LessThan(dayFog.grayscale));
            Assert.That(nightFog.b, Is.GreaterThan(nightFog.g));

            type.GetMethod("ToDay").Invoke(shift, new object[] { 0f });
            Assert.That(RenderSettings.fogColor, Is.EqualTo(dayFog));
        }
        finally
        {
            type.GetMethod("ToDay").Invoke(shift, new object[] { 0f });
            UnityEngine.Object.DestroyImmediate(go);
            RenderSettings.fogColor = originalFog;
        }
    }

    [UnityTest]
    public IEnumerator NightUsesPrivateSkyboxAndDayRestoresOriginal()
    {
        var original = RenderSettings.skybox;
        Assert.IsNotNull(original, "Test scene must provide a skybox material");
        Assert.IsTrue(original.HasProperty("_Exposure") || original.HasProperty("_Tint"));
        float exposure = original.HasProperty("_Exposure") ? original.GetFloat("_Exposure") : 0;
        Color tint = original.HasProperty("_Tint") ? original.GetColor("_Tint") : Color.white;
        var go = new GameObject("NightShiftTest");
        var type = Type.GetType("IUHCampus.Guardian.Story.IUHStoryNightShift, Assembly-CSharp", true);
        var shift = go.AddComponent(type);
        try
        {
            type.GetMethod("Initialize").Invoke(shift, new object[] { "UnusedStoryLed" });
            type.GetMethod("SetNightImmediate").Invoke(shift, null);
            Assert.AreNotSame(original, RenderSettings.skybox);
            if (original.HasProperty("_Exposure"))
            {
                Assert.That(RenderSettings.skybox.GetFloat("_Exposure"), Is.EqualTo(exposure * .25f).Within(.001f));
                Assert.AreEqual(exposure, original.GetFloat("_Exposure"));
            }
            else
            {
                Assert.AreNotEqual(tint, RenderSettings.skybox.GetColor("_Tint"));
                Assert.AreEqual(tint, original.GetColor("_Tint"));
            }
            type.GetMethod("ToDay").Invoke(shift, new object[] { 0f });
            yield return null;
            Assert.AreSame(original, RenderSettings.skybox);
            type.GetMethod("SetNightImmediate").Invoke(shift, null);
            UnityEngine.Object.DestroyImmediate(go);
            Assert.AreSame(original, RenderSettings.skybox, "Destroy must release the private skybox");
        }
        finally
        {
            if (go != null) UnityEngine.Object.DestroyImmediate(go);
            RenderSettings.skybox = original;
        }
    }
}
