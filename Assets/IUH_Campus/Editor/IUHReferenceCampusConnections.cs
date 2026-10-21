#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using IUHCampus.Minigames.PhotoHunter;
using IUHCampus.World;

namespace IUHCampus.Editor
{
    public static partial class IUHReferenceCampusBuilder
    {
        const string ConnectionMarker="CONNECTED_BLOCKS_R1";
        const string ConnectionEvidence="Docs/Evidence/CampusConnections";
        class ConnectedBlock
        {
            public string id,title,roofTitle;
            public Rect[] footprint;
            public float height;public int floors;public Material accent;
            public Vector2 label;
            public Transform transform;
            public bool Contains(Vector2 p)=>footprint.Any(r=>r.Contains(p));
        }
        static Rect Foot(float minX,float maxX,float minZ,float maxZ)=>Rect.MinMaxRect(minX,minZ,maxX,maxZ);
        static readonly Rect[] WalkingVoids={
            Foot(423,427,-160,-10), // A-H-B spine
            Foot(441,445,-66.6f,-10), // A7/medical wing
            Foot(414,450,-42,-38), // A to A7 and clinic
            Foot(374,378,-176,-134), // Rear street through X
            Foot(391,395,-176,-139),
            Foot(353,357,-176,-134),
            Foot(333,448,-151,-147) // X-V-B transverse hall
        };
        static bool IsHall(Vector2 p)=>WalkingVoids.Any(r=>r.Contains(p));

        [MenuItem("IUH Campus/Reference Layout/Connect A-H-B and B-V-X")]
        public static void ConnectReferenceBlocks()
        {
            var scene=EditorSceneManager.GetActiveScene();
            if(Application.isPlaying || scene.path!=ScenePath)throw new InvalidOperationException("Open the main scene in Edit Mode.");
            root=GameObject.Find(IUHReferenceLayout.RootName)?.transform;
            if(root==null)throw new InvalidOperationException("Reference campus missing.");
            if(root.Find(ConnectionMarker)!=null)throw new InvalidOperationException("Connected layout already applied; preserve subsequent edits.");
            const string backup="Docs/Backups/IUH_Campus_Main_before_connections_20261022.unity";
            Directory.CreateDirectory("Docs/Backups");
            if(!File.Exists(backup)&&!EditorSceneManager.SaveScene(scene,backup,true))throw new IOException("Could not save connection backup.");
            meshFolderOverride=Folder+"/Connections_R1/Meshes";
            Directory.CreateDirectory(meshFolderOverride);AssetDatabase.Refresh();
            CacheMeshes();Materials();pieces.Clear();
            var specs=new[]{
                new ConnectedBlock{id="A",title="NHÀ A • THƯ VIỆN",roofTitle="TÒA A\nTHƯ VIỆN",height=15,floors=4,accent=blue,footprint=new[]{Foot(416.5f,433.5f,-66.5f,-13.5f)},label=new Vector2(425,-30)},
                new ConnectedBlock{id="A7",title="HỘI TRƯỜNG A7",roofTitle="HỘI TRƯỜNG\nA7",height=15,floors=4,accent=blue,footprint=new[]{Foot(433.5f,448,-66.5f,-41.5f)},label=new Vector2(440.75f,-54)},
                new ConnectedBlock{id="A_Clinic",title="PHÒNG Y TẾ",roofTitle="PHÒNG Y TẾ",height=15,floors=4,accent=blue,footprint=new[]{Foot(433.5f,448,-41.5f,-13.5f)},label=new Vector2(440.75f,-26)},
                new ConnectedBlock{id="H",title="NHÀ H",roofTitle="TÒA H",height=12,floors=3,accent=teal,footprint=new[]{Foot(417,447,-102,-66.5f)},label=new Vector2(432,-83)},
                // A true L: one continuous north-south leg with a rectangular rear arm meeting V.
                new ConnectedBlock{id="B",title="GIẢNG ĐƯỜNG B",roofTitle="TÒA B",height=18,floors=5,accent=blue,footprint=new[]{Foot(419,445,-156,-102),Foot(407,419,-156,-142)},label=new Vector2(432,-122)},
                new ConnectedBlock{id="V",title="GIẢNG ĐƯỜNG V",roofTitle="TÒA V",height=20,floors=5,accent=teal,footprint=new[]{Foot(379,407,-172.5f,-143.5f)},label=new Vector2(393,-160)},
                new ConnectedBlock{id="X",title="GIẢNG ĐƯỜNG X",roofTitle="TÒA X",height=19,floors=5,accent=gold,footprint=new[]{Foot(336.5f,379,-173,-137)},label=new Vector2(355,-158)}
            };
            var archive=Group("ARCHIVE_BEFORE_CONNECTED_BLOCKS",root,Vector3.zero);
            Undo.RegisterCreatedObjectUndo(archive.gameObject,"Preserve previous separate blocks");
            foreach(var name in new[]{"Block_A","Block_A7","Block_A_Link","Block_H","Block_B","Block_B_Wing","Block_V","Block_X"})
            {
                var old=root.Find(name);if(old==null)throw new InvalidOperationException("Expected existing block: "+name);
                Undo.SetTransformParent(old,archive,"Archive separate blocks");
            }
            archive.gameObject.SetActive(false);
            try
            {
                foreach(var spec in specs)BuildConnectedBlock(spec,specs);
                var marker=Group(ConnectionMarker,root,Vector3.zero);
                AddRouteMarkers(marker);
                // Keep the original photo ID and viewpoint, changing only its structure reference.
                foreach(var target in UnityEngine.Object.FindObjectsByType<IUHPhotoTarget>(FindObjectsInactive.Include))
                    if(target.TargetId.Contains("block_a"))
                    {Undo.RecordObject(target,"Bind Photo Hunter to connected A");target.SetStructureReferences(root.Find("Block_A"),null);EditorUtility.SetDirty(target);}
                RemoveHallObstacles();
                AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);
                if(!EditorSceneManager.SaveScene(scene))throw new IOException("Could not save connected campus.");
                Selection.activeGameObject=root.Find("Block_B").gameObject;
                Debug.Log("[Campus Connections] A/A7/clinic joined; A-H-B ground hall and B-V-X cross hall saved.");
            }
            finally{meshFolderOverride=null;pieces.Clear();}
        }

        static void BuildConnectedBlock(ConnectedBlock b,ConnectedBlock[] all)
        {
            float minX=b.footprint.Min(r=>r.xMin),maxX=b.footprint.Max(r=>r.xMax);
            float minZ=b.footprint.Min(r=>r.yMin),maxZ=b.footprint.Max(r=>r.yMax);
            var origin=new Vector2((minX+maxX)/2,(minZ+maxZ)/2);
            var t=Group("Block_"+b.id,root,new Vector3(origin.x,.49f,origin.y));b.transform=t;
            const float clearHeight=3.5f;
            foreach(var r in b.footprint)
            {
                Vector3 center=new Vector3(r.center.x-origin.x,clearHeight+(b.height-clearHeight)/2,r.center.y-origin.y);
                Vector3 size=new Vector3(r.width,b.height-clearHeight,r.height);
                Q(center,size,ivory);Solid(t,"Upper floors",center,size);
                Q(new Vector3(r.center.x-origin.x,b.height+.12f,r.center.y-origin.y),new Vector3(r.width,.24f,r.height),roof);
                Q(new Vector3(r.center.x-origin.x,-.035f,r.center.y-origin.y),new Vector3(r.width,.06f,r.height),stone);
            }
            // Carve the ground floor into solid room cells around genuinely empty hallways.
            var xs=b.footprint.SelectMany(r=>new[]{r.xMin,r.xMax}).Concat(WalkingVoids.SelectMany(r=>new[]{r.xMin,r.xMax})).Where(x=>x>=minX&&x<=maxX).Distinct().OrderBy(x=>x).ToArray();
            var zs=b.footprint.SelectMany(r=>new[]{r.yMin,r.yMax}).Concat(WalkingVoids.SelectMany(r=>new[]{r.yMin,r.yMax})).Where(z=>z>=minZ&&z<=maxZ).Distinct().OrderBy(z=>z).ToArray();
            for(int i=1;i<xs.Length;i++)for(int j=1;j<zs.Length;j++)
            {
                var p=new Vector2((xs[i-1]+xs[i])/2,(zs[j-1]+zs[j])/2);
                if(!b.Contains(p)||IsHall(p))continue;
                var center=new Vector3(p.x-origin.x,clearHeight/2,p.y-origin.y);
                var size=new Vector3(xs[i]-xs[i-1],clearHeight,zs[j]-zs[j-1]);
                Q(center,size,ivory);Solid(t,"Ground rooms",center,size);
            }
            // Split the outline at neighbours' edges so shared walls have no external windows or parapets.
            var edgeXs=all.SelectMany(s=>s.footprint).SelectMany(r=>new[]{r.xMin,r.xMax}).Distinct().OrderBy(x=>x).ToArray();
            var edgeZs=all.SelectMany(s=>s.footprint).SelectMany(r=>new[]{r.yMin,r.yMax}).Distinct().OrderBy(x=>x).ToArray();
            for(int i=1;i<edgeXs.Length;i++)for(int j=1;j<edgeZs.Length;j++)
            {
                float x0=edgeXs[i-1],x1=edgeXs[i],z0=edgeZs[j-1],z1=edgeZs[j];
                if(!b.Contains(new Vector2((x0+x1)/2,(z0+z1)/2)))continue;
                Edge(new Vector2(x0,z0),new Vector2(x1,z0),Vector2.down);
                Edge(new Vector2(x0,z1),new Vector2(x1,z1),Vector2.up);
                Edge(new Vector2(x0,z0),new Vector2(x0,z1),Vector2.left);
                Edge(new Vector2(x1,z0),new Vector2(x1,z1),Vector2.right);
            }
            void Edge(Vector2 a,Vector2 c,Vector2 normal)
            {
                var midpoint=(a+c)/2;var outside=midpoint+normal*.01f;
                if(b.Contains(outside)||all.Any(s=>s!=b&&s.Contains(outside)))return;
                FacadeSegment(b,origin,a,c,normal);
            }
            // Service housings stay at the rear edge, leaving the roof labels unobstructed.
            foreach(float x in new[]{minX+2.5f,maxX-2.5f})
                Q(new Vector3(x-origin.x,b.height+.65f,minZ+2.5f-origin.y),new Vector3(2.6f,1,2.3f),stone);
            var lp=b.label-origin;
            Q(new Vector3(lp.x,b.height+.30f,lp.y),new Vector3(Mathf.Min(17,maxX-minX-2),.15f,b.id=="A"?8:5.6f),b.accent);
            Flush(t);
            Text(t,"Roof ID",b.roofTitle,new Vector3(lp.x,b.height+.40f,lp.y),b.id=="A_Clinic"?1.3f:b.id=="A7"?1.7f:2.3f,Quaternion.Euler(90,180,0),Color.white);
            if(b.id=="A"||b.id=="A_Clinic")
            {
                float entryX=b.id=="A"?425:443;
                var e=new Vector3(entryX-origin.x,4.2f,maxZ-origin.y+.28f);
                Box(t,"Entrance plaque",e,new Vector3(b.id=="A"?12:11,1.1f,.15f),blue);
                Text(t,"Entrance name",b.title,e+Vector3.forward*.10f,.65f,Quaternion.Euler(0,180,0),Color.white);
                Box(t,"Entrance canopy",new Vector3(entryX-origin.x,3.65f,maxZ-origin.y+1),new Vector3(6,.22f,2),blue);
            }
            if(b.id=="A_Clinic")
            {
                var red=Mat("Medical_Red",new Color(.78f,.12f,.13f));
                Q(new Vector3(lp.x,b.height+.42f,lp.y+7),new Vector3(3,.10f,.9f),red);
                Q(new Vector3(lp.x,b.height+.43f,lp.y+7),new Vector3(.9f,.12f,3),red);
                Flush(Group("Medical cross",t,Vector3.zero));
            }
        }
        static void FacadeSegment(ConnectedBlock b,Vector2 origin,Vector2 a,Vector2 c,Vector2 normal)
        {
            float length=Vector2.Distance(a,c);bool horizontal=Mathf.Abs(a.y-c.y)<.001f;
            Vector2 middle=(a+c)/2-origin;Vector2 tangent=(c-a).normalized;
            Vector3 bandSize=horizontal?new Vector3(length,.20f,.28f):new Vector3(.28f,.20f,length);
            Vector3 rimSize=horizontal?new Vector3(length,.75f,.28f):new Vector3(.28f,.75f,length);
            Q(new Vector3(middle.x,b.height+.48f,middle.y),rimSize,b.accent);
            for(int floor=0;floor<b.floors;floor++)
            {
                float y0=floor==0?0:3.5f+(floor-1)*(b.height-3.5f)/(b.floors-1);
                float fh=floor==0?3.5f:(b.height-3.5f)/(b.floors-1);
                if(floor>0)Q(new Vector3(middle.x+normal.x*.12f,y0+.1f,middle.y+normal.y*.12f),bandSize,stone);
                int bays=Mathf.Max(1,Mathf.FloorToInt(length/3.2f));float bay=length/bays;
                for(int k=0;k<bays;k++)
                {
                    var p=a+tangent*bay*(k+.5f);var local=p-origin;
                    bool door=floor==0 && new[]{-1.4f,0,1.4f}.Any(offset=>IsHall(p+tangent*offset-normal*.1f));
                    if(door)continue;
                    var windowSize=horizontal?new Vector3(Mathf.Min(2.2f,bay*.72f),fh*.45f,.10f):new Vector3(.10f,fh*.45f,Mathf.Min(2.2f,bay*.72f));
                    Q(new Vector3(local.x+normal.x*.06f,y0+fh*.52f,local.y+normal.y*.06f),windowSize,glass);
                    var shadeSize=horizontal?new Vector3(Mathf.Min(2.7f,bay*.9f),.14f,.65f):new Vector3(.65f,.14f,Mathf.Min(2.7f,bay*.9f));
                    Q(new Vector3(local.x+normal.x*.32f,y0+fh*.8f,local.y+normal.y*.32f),shadeSize,b.accent);
                }
            }
        }
        static void AddRouteMarkers(Transform marker)
        {
            foreach(var pair in new[]{
                ("A_Start",new Vector3(425,.50f,-20)),("A_H_Seam",new Vector3(425,.50f,-66.5f)),
                ("H_B_Seam",new Vector3(425,.50f,-102)),("B_Cross",new Vector3(425,.50f,-149)),
                ("B_V_Seam",new Vector3(407,.50f,-149)),("V_X_Seam",new Vector3(379,.50f,-149)),
                ("Clinic",new Vector3(443,.50f,-25)),("A7",new Vector3(443,.50f,-54)),
                ("X_Rear",new Vector3(376,.50f,-178)),("X_Front",new Vector3(376,.50f,-132))})
                Group(pair.Item1,marker,pair.Item2);
        }
        static void RemoveHallObstacles()
        {
            // The old V receiving desk stood immediately outside its entrance. Keep it beside the hall.
            var station=GameObject.Find("GAMEPLAY_GRAB_CAMPUS/DELIVERY_DROPOFF_STATIONS/Station_02_Block_V");
            if(station!=null){Undo.RecordObject(station.transform,"Keep V doorway clear");station.transform.position=new Vector3(399,.49f,-134.5f);}
        }
    }
}
#endif
