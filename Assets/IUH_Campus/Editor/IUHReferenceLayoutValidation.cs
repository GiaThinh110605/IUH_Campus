#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using StarterAssets;
using IUHCampus.World;

namespace IUHCampus.Editor
{
    public static class IUHReferenceLayoutValidation
    {
        const string Evidence="Docs/Evidence/CampusLayout";
        [Serializable] public class Report
        { public int passed,failed;public bool running;public List<string> checks=new List<string>(); }
        public static Report Current {get;private set;}=new Report();
        static void Check(bool condition,string description)
        {if(condition)Current.passed++;else Current.failed++;Current.checks.Add((condition?"PASS ":"FAIL ")+description);}
        static void Save(string filename)
        {Directory.CreateDirectory(Evidence);File.WriteAllText(Evidence+"/"+filename,JsonUtility.ToJson(Current,true));}

        [MenuItem("IUH Campus/Reference Layout/Verify Exterior Routes")]
        public static void Audit()
        {
            Current=new Report();Physics.SyncTransforms();
            var root=GameObject.Find(IUHReferenceLayout.RootName);
            if(root==null)throw new InvalidOperationException("Reference layout missing.");
            foreach(string id in new[]{"A","H","B","V","X","E","D","F","C","I","G","T","K"})
                Check(root.transform.Find("Block_"+id)!=null,"Building "+id+" exists");
            var old=Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g=>g.name=="campus_P0_progress"&&g.scene.IsValid());
            Check(old!=null&&!old.activeSelf,"Legacy campus retained inactive");
            if(old!=null)foreach(int floor in new[]{1,2,3,4})
            {
                var rail=old.transform.Find("Block_D_StairMidRail_Left_Fl"+floor);
                Check(rail!=null && rail.GetComponent<BoxCollider>()!=null && !rail.GetComponent<BoxCollider>().enabled,"Legacy empty stair rail "+floor+" remains disabled");
            }

            var oversized=root.GetComponentsInChildren<BoxCollider>().Where(c=>c.bounds.size.y>80).ToArray();
            Check(oversized.Length==0,"No oversized vertical collider in exterior");

            Route("Gate 1 to E plaza",new[]{P(409,-3),P(409,-87),P(390,-87),P(390,-84),P(385,-84)});
            Route("Gate 2 to E plaza",new[]{P(357,-3),P(357,-89),P(375,-89),P(375,-83)});
            Route("Gate 3 to right courtyard",new[]{P(294,-3),P(295,-68),P(357,-68),P(358,-90),P(365,-90)});
            Route("Gate 4 from rear to E",new[]{P(337,-185),P(337,-178),P(376,-178),P(376,-131),P(362,-131),P(362,-89),P(375,-89),P(375,-86)});
            Route("Gate 5 from rear to E",new[]{P(385,-185),P(385,-178),P(376,-178),P(376,-131),P(409,-131),P(407,-123),P(407,-111),P(409,-105),P(409,-89),P(399,-89),P(399,-85)});
            Check(Clear(IUHReferenceLayout.Entrance),"Entrance player spawn has full standing clearance");
            Check(Clear(IUHReferenceLayout.GreenSpawn),"Green spawn has full standing clearance");
            Save("exterior-audit.json");Debug.Log("[Layout Audit] "+Current.passed+" passed, "+Current.failed+" failed");
        }
        static Vector3 P(float x,float z)=>new Vector3(x,.50f,z);
        static bool Relevant(Collider c)=>!c.isTrigger && c.gameObject.activeInHierarchy && !c.CompareTag("Player") && c.GetComponentInParent<CharacterController>()==null;
        static bool Clear(Vector3 p)
        {return !Physics.OverlapCapsule(new Vector3(p.x,.88f,p.z),new Vector3(p.x,2.05f,p.z),.28f,~0,QueryTriggerInteraction.Ignore).Any(Relevant);}
        static void Route(string name,Vector3[] points)
        {
            var blockers=new HashSet<string>();
            for(int i=1;i<points.Length;i++)
            {
                var a=points[i-1];var delta=points[i]-a;
                foreach(var hit in Physics.CapsuleCastAll(new Vector3(a.x,.88f,a.z),new Vector3(a.x,2.05f,a.z),.28f,delta.normalized,delta.magnitude,~0,QueryTriggerInteraction.Ignore))
                    if(Relevant(hit.collider))blockers.Add(hit.collider.name+" in "+hit.collider.transform.parent.name);
                if(!Clear(points[i]))blockers.Add("standing clearance at "+points[i]);
            }
            Check(blockers.Count==0,name+(blockers.Count>0?": "+string.Join("; ",blockers):" has continuous capsule clearance"));
        }

        [MenuItem("IUH Campus/Reference Layout/Capture")]
        public static void Capture()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Capture in Edit Mode for deterministic render state.");
            Directory.CreateDirectory(Evidence);
            Shot("overview.png",new Vector3(373,240,80),new Vector3(373,0,-98),1800,1800,107);
            Shot("walkway.png",new Vector3(409,2.1f,-23),new Vector3(403,2,-80),1400,900);
            Shot("plaza.png",new Vector3(391,2.1f,-73),new Vector3(385,5,-105),1400,900);
            Shot("front-gates.png",new Vector3(372,15,32),new Vector3(373,4,-24),1600,900);
        }
        static void Shot(string filename,Vector3 position,Vector3 look,int w,int h,float ortho=0)
        {
            var obj=new GameObject("Layout review camera (temporary)");var cam=obj.AddComponent<Camera>();
            RenderTexture rt=null;Texture2D tex=null;var previous=RenderTexture.active;
            try
            {
                cam.transform.position=position;cam.transform.LookAt(look);cam.fieldOfView=65;
                cam.nearClipPlane=.1f;cam.farClipPlane=700;cam.useOcclusionCulling=false;
                cam.orthographic=ortho>0;cam.orthographicSize=ortho;
                if(ortho>0){cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.80f,.84f,.82f);}
                rt=new RenderTexture(w,h,24);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
                tex=new Texture2D(w,h,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();
                File.WriteAllBytes(Evidence+"/"+filename,tex.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active=previous;cam.targetTexture=null;
                if(rt!=null)UnityEngine.Object.DestroyImmediate(rt);if(tex!=null)UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(obj);
            }
        }
    }
}
#endif
