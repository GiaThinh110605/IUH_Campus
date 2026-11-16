using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class NightShiftPlayModeTests
{
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
