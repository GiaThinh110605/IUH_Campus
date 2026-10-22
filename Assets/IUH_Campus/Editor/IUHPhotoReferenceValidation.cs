#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using StarterAssets;
using IUHCampus.World;

namespace IUHCampus.Editor
{
    public static class IUHPhotoReferenceValidation
    {
        const string Evidence="Docs/Evidence/PhotoReference";
        [Serializable] public class Report {public int passed,failed,physicsSteps;public List<string> checks=new List<string>();}
        static void Check(Report r,bool pass,string description){if(pass)r.passed++;else r.failed++;r.checks.Add((pass?"PASS ":"FAIL ")+description);}
        public static Report Audit()
        {
            var r=new Report();var root=GameObject.Find(IUHReferenceLayout.RootName).transform;
            foreach(var entry in new[]{("A",4),("B",4),("A7",7),("H",11)})
            {
                var t=root.Find("Block_"+entry.Item1);var info=t.GetComponent<IUHBuildingExteriorInfo>();
                Check(r,info!=null&&info.floorCount==entry.Item2,entry.Item1+" has "+entry.Item2+" authored facade storeys");
                float top=t.GetComponentsInChildren<BoxCollider>().Max(c=>c.bounds.max.y);
                Check(r,Mathf.Abs(top-(t.position.y+info.roofHeight))<.1f,entry.Item1+" collision envelope matches its new height");
            }
            Check(r,root.Find("Block_H").GetComponent<IUHBuildingExteriorInfo>().roofHeight>root.Find("Block_A7").GetComponent<IUHBuildingExteriorInfo>().roofHeight,"H is taller than A7");
            Check(r,root.Find("Photo_Forecourt").GetComponentsInChildren<MeshRenderer>().Count(m=>m.name.StartsWith("Large shade tree"))==2,"Two broad-canopy shade trees exist");
            var deck=root.Find("Central_Parking/Low bicycle terrace").GetComponent<BoxCollider>();
            Check(r,deck.bounds.max.y<3,"Parking is a low podium rather than a second tall building");
            Check(r,root.Find("Central_Parking/West access ramp")!=null&&root.Find("Central_Parking/East access ramp")!=null,"Both parking access ramps exist");
            Check(r,root.Find("Block_E").GetComponentsInChildren<Transform>().Any(t=>t.name=="Signage_Panel_IUH"),"E has its photographic curtain-wall facade and sign");
            Directory.CreateDirectory(Evidence);File.WriteAllText(Evidence+"/photo-reference-audit.json",JsonUtility.ToJson(r,true));return r;
        }
        // Exercise both ramps with the actual player collision solver; no transform writes between endpoints.
        public static Report TestRampPhysics()
        {
            if(!Application.isPlaying)throw new InvalidOperationException("Run ramp collision verification in Play Mode.");
            var r=new Report();var player=UnityEngine.Object.FindFirstObjectByType<ThirdPersonController>();var cc=player.GetComponent<CharacterController>();
            var old=player.transform.position;var rotation=player.transform.rotation;bool enabled=player.enabled;player.enabled=false;
            try
            {
                foreach(float x in new[]{360f,404f})
                {
                    cc.enabled=false;player.transform.position=new Vector3(x,.7f,-87);cc.enabled=true;player.ResetMotion();Physics.SyncTransforms();
                    for(int i=0;i<220;i++){cc.Move(new Vector3(0,-.04f,.1f));r.physicsSteps++;}
                    Check(r,player.transform.position.z>-65.4f,"Ramp "+x+" can be ascended without a wall stop");
                    Check(r,player.transform.position.y>2.35f&&player.transform.position.y<2.9f,"Ramp "+x+" reaches the low deck height");
                    float direction=x<382?1:-1;
                    for(int i=0;i<70;i++){cc.Move(new Vector3(direction*.1f,-.04f,0));r.physicsSteps++;}
                    Check(r,player.transform.position.y>2.35f,"Ramp "+x+" landing connects to the parking terrace");
                }
            }
            finally{cc.enabled=false;player.transform.SetPositionAndRotation(old,rotation);cc.enabled=true;player.ResetMotion();player.enabled=enabled;}
            Directory.CreateDirectory(Evidence);File.WriteAllText(Evidence+"/parking-ramp-physics.json",JsonUtility.ToJson(r,true));return r;
        }
        public static void Capture()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Capture in Edit Mode.");
            Directory.CreateDirectory(Evidence);
            var method=typeof(IUHReferenceLayoutValidation).GetMethod("Shot",BindingFlags.Static|BindingFlags.NonPublic);
            void Shot(string name,Vector3 p,Vector3 look,int w,int h,float ortho=0)=>method.Invoke(null,new object[]{"../PhotoReference/"+name,p,look,w,h,ortho});
            Shot("e-front.png",new Vector3(385,2.1f,-71),new Vector3(385,9,-101),1600,1000);
            Shot("parking-from-e.png",new Vector3(373,2.1f,-88),new Vector3(378,2,-65),1600,1000);
            Shot("h-a7-heights.png",new Vector3(386,2.1f,-84),new Vector3(424,16,-83),1600,1100);
            Shot("overview.png",new Vector3(373,260,100),new Vector3(373,0,-98),1800,1800,110);
        }
    }
}
#endif
