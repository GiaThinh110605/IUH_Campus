#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace IUHCampus.Guardian.Visual.Tests
{
    public sealed class ActualPlayerStableGroundingPlayModeTests
    {
        [UnityTest]
        public IEnumerator ActualPlayerRunsThreeSecondsJumpsThreeTimesAndIgnoresHighPlanter()
        {
            Func<string,Type> find = n => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(n)).First(t => t != null);
            Type grounderType=find("IUHCampus.Guardian.Visual.IUHVisualFeetGrounder");
            Type surfaceType=find("IUHCampus.Guardian.Visual.IUHVisualGroundSurface");
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/IUH_Campus/Prefabs/Player/PF_Student_VRoid.prefab");
            var player=UnityEngine.Object.Instantiate(asset);
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);
            var surface=ScriptableObject.CreateInstance(surfaceType);
            var baked=new Mesh();
            try
            {
                player.transform.position=new Vector3(10000,.445f,10000);
                floor.transform.position=new Vector3(10000,-.5f,10000);
                floor.transform.localScale=new Vector3(40,1,40);
                var tpc=(Behaviour)player.AddComponent(find("StarterAssets.ThirdPersonController"));
                tpc.enabled=false; // Root trajectory is deterministic; production controller stays untouched.
                var cc=player.GetComponent<CharacterController>(); cc.center=Vector3.up; cc.height=2f; cc.radius=.28f;
                var animator=player.GetComponent<Animator>();
                animator.runtimeAnimatorController=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/IUH_Campus/Prefabs/NPC/AC_AmbientStudent.controller");
                var idle=animator.runtimeAnimatorController.animationClips.First(c=>c.name.IndexOf("Idle",StringComparison.OrdinalIgnoreCase)>=0);
                var walk=animator.runtimeAnimatorController.animationClips.First(c=>c.name.IndexOf("Walk",StringComparison.OrdinalIgnoreCase)>=0);
                var renderer=player.GetComponentsInChildren<SkinnedMeshRenderer>().First(r=>r.name=="Body");
                var grounder=player.GetComponent(grounderType)??player.AddComponent(grounderType);
                Action<float> visibleHeight=y=>surfaceType.GetMethod("SetTriangles").Invoke(surface,new object[]{new[]{new Vector3(9980,y,9980),new Vector3(10020,y,10020),new Vector3(10020,y,9980),new Vector3(9980,y,9980),new Vector3(9980,y,10020),new Vector3(10020,y,10020)}});
                visibleHeight(.24f);
                grounderType.GetMethod("Configure").Invoke(grounder,new object[]{surface});
                Physics.SyncTransforms();
                idle.SampleAnimation(player,0f);
                grounderType.GetMethod("AlignNow").Invoke(grounder,null);
                yield return null;
                yield return new WaitForEndOfFrame();
                renderer.BakeMesh(baked);
                float baseline=baked.vertices.Min(v=>renderer.transform.TransformPoint(v).y)-player.transform.position.y;
                float previous=baseline;
                for(float elapsed=0;elapsed<3f;elapsed+=Time.deltaTime)
                {
                    player.transform.position=new Vector3(10000+elapsed,.445f,10000);
                    walk.SampleAnimation(player,elapsed%walk.length);
                    yield return new WaitForEndOfFrame();
                    renderer.BakeMesh(baked);
                    float relative=baked.vertices.Min(v=>renderer.transform.TransformPoint(v).y)-player.transform.position.y;
                    Assert.That(Mathf.Abs(relative-previous),Is.LessThanOrEqualTo(.02f),"Run sole-relative discontinuity");
                    previous=relative;
                    yield return null;
                }
                for(int jump=0;jump<3;jump++)
                {
                    for(float elapsed=0;elapsed<2f;elapsed+=Time.deltaTime)
                    {
                        float lift=1.45f*Mathf.Sin(Mathf.PI*Mathf.Clamp01(elapsed/1.5f));
                        player.transform.position=new Vector3(10003,.445f+lift,10000);
                        idle.SampleAnimation(player,elapsed%idle.length);
                        yield return new WaitForEndOfFrame();
                        renderer.BakeMesh(baked);
                        float relative=baked.vertices.Min(v=>renderer.transform.TransformPoint(v).y)-player.transform.position.y;
                        Assert.That(relative,Is.EqualTo(baseline).Within(.08f),"Jump must preserve calibrated visual offset even if TPC Grounded remains true");
                        Assert.That(renderer.enabled,Is.True);
                        yield return null;
                    }
                }
                player.transform.position=new Vector3(10003,.445f,10000);
                // A high visible planter above a flat collider must not lift the avatar.
                visibleHeight(.8f);
                for(int i=0;i<30;i++) { idle.SampleAnimation(player,0f); yield return null; }
                yield return new WaitForEndOfFrame();
                renderer.BakeMesh(baked);
                Assert.That(baked.vertices.Min(v=>renderer.transform.TransformPoint(v).y)-player.transform.position.y,Is.EqualTo(baseline).Within(.03f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(player); UnityEngine.Object.DestroyImmediate(floor);
                UnityEngine.Object.DestroyImmediate(surface); UnityEngine.Object.DestroyImmediate(baked);
            }
        }
    }
}
#endif
