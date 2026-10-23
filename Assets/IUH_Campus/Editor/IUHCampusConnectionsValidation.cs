#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using StarterAssets;

namespace IUHCampus.Editor
{
    public static class IUHCampusConnectionsValidation
    {
        const string Evidence="Docs/Evidence/CampusConnections";
        [Serializable] public class Report
        {public int passed,failed,sampledFrames;public bool running;public float minY=999,maxY=-999;public List<string> checks=new List<string>();}
        public static Report Result {get;private set;}=new Report();
        static readonly Vector3[][] Paths={
            new[]{P(425,-11),P(425,-64),P(425,-70),P(425,-99),P(425,-105),P(425,-153)},
            new[]{P(443,-11),P(443,-54),P(443,-40),P(425,-40)},
            new[]{P(425,-149),P(405,-149),P(381,-149),P(377,-149),P(340,-149)},
            new[]{P(376,-178),P(376,-171),P(376,-139),P(376,-132)}
        };
        static readonly string[] Names={"A-H-B continuous ground hall","Clinic-A7-A connected hall","B-V-X transverse hall","Rear-to-front X through passage"};
        static Vector3 P(float x,float z)=>new Vector3(x,.5f,z);
        static void Check(bool condition,string description)
        {if(condition)Result.passed++;else Result.failed++;Result.checks.Add((condition?"PASS ":"FAIL ")+description);}
        static void Save(string name)
        {Directory.CreateDirectory(Evidence);File.WriteAllText(Evidence+"/"+name,JsonUtility.ToJson(Result,true));}

        [MenuItem("IUH Campus/Reference Layout/Verify Connected Halls")]
        public static void Audit()
        {
            Result=new Report();Physics.SyncTransforms();
            var root=GameObject.Find("IUH_REFERENCE_CAMPUS").transform;
            Check(root.Find("Block_A_Clinic")!=null,"Dedicated attached medical wing exists");
            Check(root.Find("Block_A_Link")==null&&root.Find("Block_B_Wing")==null,"Former separated filler blocks archived");
            Check(root.Find("ARCHIVE_BEFORE_CONNECTED_BLOCKS")!=null&&!root.Find("ARCHIVE_BEFORE_CONNECTED_BLOCKS").gameObject.activeSelf,"Previous revision retained inactive");
            foreach(int route in Enumerable.Range(0,Paths.Length))
                for(int i=1;i<Paths[route].Length;i++)
                {
                    var a=Paths[route][i-1];var delta=Paths[route][i]-a;
                    var hits=Physics.CapsuleCastAll(new Vector3(a.x,.88f,a.z),new Vector3(a.x,2.05f,a.z),.28f,delta.normalized,delta.magnitude,~0,QueryTriggerInteraction.Ignore)
                        .Where(h=>h.collider.GetComponentInParent<CharacterController>()==null).ToArray();
                    Check(hits.Length==0,Names[route]+" segment "+i+" clears a standing capsule"+(hits.Length>0?": "+string.Join(",",hits.Select(h=>h.collider.name)):""));
                    var b=Paths[route][i];
                    bool floor=Physics.Raycast(b+Vector3.up*.3f,Vector3.down,out var hit,1f,~0,QueryTriggerInteraction.Ignore);
                    Check(floor&&Mathf.Abs(hit.point.y-.49f)<.06f,Names[route]+" segment "+i+" keeps a level floor");
                }
            Save("connected-halls-audit.json");
        }

        static CharacterController cc;static ThirdPersonController player;
        static int routeIndex,pointIndex,lastFrame;static float stalled;
        static Vector3 oldPosition;static Quaternion oldRotation;static bool oldController,oldBackground;

        [MenuItem("IUH Campus/Reference Layout/Walk Connected Halls (Play Mode)")]
        public static void Walk()
        {
            if(!Application.isPlaying||Result.running)throw new InvalidOperationException("Run in Play Mode after other tests finish.");
            IUHCampus.Minigames.GreenCampus.IUHGreenCampusManager.Instance.CancelMission();
            IUHCampus.Minigames.PhotoHunter.IUHPhotoHunterManager.Instance.CancelMission();
            player=UnityEngine.Object.FindFirstObjectByType<ThirdPersonController>();cc=player.GetComponent<CharacterController>();
            oldPosition=player.transform.position;oldRotation=player.transform.rotation;oldController=player.enabled;
            oldBackground=Application.runInBackground;Application.runInBackground=true;player.enabled=false;
            Result=new Report{running=true};routeIndex=0;lastFrame=-1;StartRoute();EditorApplication.update+=Tick;
        }
        static void StartRoute()
        {
            cc.enabled=false;player.transform.position=Paths[routeIndex][0]+Vector3.up*.05f;cc.enabled=true;
            player.ResetMotion();pointIndex=1;stalled=0;Physics.SyncTransforms();
        }
        static void Tick()
        {
            try
            {
                if(!Application.isPlaying)throw new InvalidOperationException("Play Mode interrupted.");
                if(Time.frameCount==lastFrame)return;lastFrame=Time.frameCount;Result.sampledFrames++;
                var delta=Paths[routeIndex][pointIndex]-player.transform.position;delta.y=0;
                if(delta.magnitude<.12f)
                {
                    Check(true,Names[routeIndex]+" reached waypoint "+pointIndex);
                    pointIndex++;
                    if(pointIndex==Paths[routeIndex].Length)
                    {
                        routeIndex++;if(routeIndex==Paths.Length){Finish();return;}StartRoute();
                    }
                    return;
                }
                float dt=Mathf.Min(Time.deltaTime,.04f);var before=player.transform.position;
                // Move the actual player's CharacterController through its collision solver, without per-frame warps.
                cc.Move(delta.normalized*Mathf.Min(5.3f*dt,delta.magnitude)+Vector3.down*2*dt);
                player.transform.rotation=Quaternion.LookRotation(delta);player.SetCameraOrientation(delta);
                float y=player.transform.position.y;Result.minY=Mathf.Min(Result.minY,y);Result.maxY=Mathf.Max(Result.maxY,y);
                float moved=Vector2.Distance(new Vector2(before.x,before.z),new Vector2(player.transform.position.x,player.transform.position.z));
                stalled=moved<.001f?stalled+dt:0;
                if(stalled>1.5f){Check(false,Names[routeIndex]+" is blocked at "+player.transform.position);Finish();}
            }
            catch(Exception ex){Check(false,ex.ToString());Finish();}
        }
        static void Finish()
        {
            EditorApplication.update-=Tick;
            Check(Result.minY>.35f&&Result.maxY<1,"Character remains grounded through all connections");
            if(Application.isPlaying&&player!=null)
            {
                cc.enabled=false;player.transform.SetPositionAndRotation(oldPosition,oldRotation);cc.enabled=true;
                player.ResetMotion();player.enabled=oldController;
            }
            Application.runInBackground=oldBackground;Result.running=false;Save("connected-halls-walk-playmode.json");
        }

        [MenuItem("IUH Campus/Reference Layout/Capture Connected Blocks")]
        public static void Capture()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Capture in Edit Mode.");
            Directory.CreateDirectory(Evidence);
            var method=typeof(IUHReferenceLayoutValidation).GetMethod("Shot",BindingFlags.Static|BindingFlags.NonPublic);
            void Shot(string name,Vector3 p,Vector3 look,int w,int h,float ortho=0)
            {method.Invoke(null,new object[]{"../CampusConnections/"+name,p,look,w,h,ortho});}
            Shot("overview.png",new Vector3(373,240,80),new Vector3(373,0,-98),1800,1800,107);
            Shot("b-v-x.png",new Vector3(397,125,-85),new Vector3(397,0,-140),1500,1100,57);
            Shot("a-a7-clinic.png",new Vector3(432,110,5),new Vector3(432,0,-45),1000,1200,42);
            Shot("a-h-hall.png",new Vector3(425,2.1f,-61),new Vector3(425,2,-88),1400,800);
            Shot("b-v-x-hall.png",new Vector3(415,2.1f,-149),new Vector3(378,2,-149),1400,800);
        }
    }
}
#endif
