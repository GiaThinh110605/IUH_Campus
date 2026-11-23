#if UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class FarNpcPoseTests
{
    [UnityTest]
    public IEnumerator InitiallyDistantNpcSamplesIdlePoseBeforeAnimatorIsDisabled()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/IUH_Campus/Prefabs/NPC/PF_NPC_VRoid_01.prefab");
        Assert.IsNotNull(prefab, "The regression must exercise the imported NPC prefab");
        GameObject cameraObject = null;
        GameObject instance = null;
        try
        {
            cameraObject = new GameObject("Far NPC pose regression camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(1100f, 2f, 1000f);
            var camera = cameraObject.AddComponent<Camera>();
            instance = UnityEngine.Object.Instantiate(prefab, new Vector3(1000f, 0f, 1000f), Quaternion.identity);
            var studentType = Type.GetType("IUHCampus.Guardian.Ambient.IUHAmbientStudent, Assembly-CSharp", true);
            var student = instance.GetComponent(studentType);
            Assert.IsNotNull(student, "Actual prefab must contain the ambient behavior");
            studentType.GetField("viewCamera", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(student, camera);
            var animator = instance.GetComponent<Animator>();
            Assert.IsNotNull(animator);
            Assert.IsNotNull(animator.runtimeAnimatorController);
            Assert.IsTrue(animator.isHuman);
            yield return null;
            yield return null;
            Assert.IsFalse(animator.enabled, "Initially distant NPC should stop animator updates");
            var rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            var chest = animator.GetBoneTransform(HumanBodyBones.Chest);
            Assert.IsNotNull(rightHand);
            Assert.IsNotNull(chest);
            Assert.Less(rightHand.position.y, chest.position.y - 0.15f,
                "Frozen pose must have relaxed Idle arms rather than the imported T-pose");
        }
        finally
        {
            if (instance != null) UnityEngine.Object.DestroyImmediate(instance);
            if (cameraObject != null) UnityEngine.Object.DestroyImmediate(cameraObject);
        }
    }
}
#endif
