using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace IUHCampus.Guardian.Visual.Tests
{
    public sealed class VisualFeetGrounderTests
    {
        [TestCase("Idle", false)] [TestCase("Walk", false)] [TestCase("Idle", true)]
        public void ActualNpcFeetFollowVisibleSurfaceWithoutMovingPhysics(string motion, bool playerModel)
        {
            var grounderType = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("IUHCampus.Guardian.Visual.IUHVisualFeetGrounder")).FirstOrDefault(t => t != null);
            Assert.That(grounderType, Is.Not.Null, "Visual feet grounder must exist");
            var dataType = grounderType.Assembly.GetType("IUHCampus.Guardian.Visual.IUHVisualGroundSurface");
            Assert.That(dataType, Is.Not.Null);
            var data = ScriptableObject.CreateInstance(dataType);
            var triangles = new[] { new Vector3(-4,.24f,-4), new Vector3(4,.24f,4), new Vector3(4,.24f,-4), new Vector3(-4,.24f,-4), new Vector3(-4,.24f,4), new Vector3(4,.24f,4) };
            dataType.GetMethod("SetTriangles").Invoke(data, new object[] { triangles });
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(playerModel
                ? "Assets/IUH_Campus/Prefabs/Player/PF_Student_VRoid.prefab"
                : "Assets/IUH_Campus/Prefabs/NPC/PF_NPC_VRoid_01.prefab");
            var npc = UnityEngine.Object.Instantiate(prefab);
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0,-.5f,0);
            floor.transform.localScale = new Vector3(10,1,10);
            var baked = new Mesh();
            try
            {
                float rootY = playerModel ? 1.2f : .43f;
                npc.transform.position = new Vector3(0,rootY,0);
                var animator = npc.GetComponent<Animator>();
                if (playerModel)
                    animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                        "Assets/IUH_Campus/Prefabs/NPC/AC_AmbientStudent.controller");
                var clip = animator.runtimeAnimatorController.animationClips.First(c => c.name.IndexOf(motion, StringComparison.OrdinalIgnoreCase) >= 0);
                var renderer = npc.GetComponentsInChildren<SkinnedMeshRenderer>().First(r => r.name == "Body");
                var component = npc.AddComponent(grounderType);
                grounderType.GetMethod("Configure").Invoke(component, new object[] { data });
                var align = grounderType.GetMethod("AlignNow");
                var originalColliderBounds = floor.GetComponent<Collider>().bounds;
                for (int sample = 0; sample < 7; sample++)
                {
                    clip.SampleAnimation(npc, clip.length * sample / 7f);
                    align.Invoke(component, null);
                    renderer.BakeMesh(baked);
                    float min = baked.vertices.Min(v => renderer.transform.TransformPoint(v).y);
                    Assert.That(min, Is.EqualTo(.24f).Within(.03f), motion + " sample " + sample);
                    Assert.That(npc.transform.position.y, Is.EqualTo(rootY).Within(.0001f));
                    Assert.That(floor.GetComponent<Collider>().bounds, Is.EqualTo(originalColliderBounds));
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(npc);
                UnityEngine.Object.DestroyImmediate(floor);
                UnityEngine.Object.DestroyImmediate(baked);
                UnityEngine.Object.DestroyImmediate(data);
            }
        }
    }
}
