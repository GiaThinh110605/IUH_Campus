#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using IUHCampus.World;
using IUHCampus.Minigames.GreenCampus;
using IUHCampus.Minigames.PhotoHunter;

namespace IUHCampus.Editor
{
    public static partial class IUHReferenceCampusBuilder
    {
        [MenuItem("IUH Campus/Reference Layout/Apply Real Photo Forecourt and Heights")]
        public static void RefineFromPhotographs()
        {
            var scene=EditorSceneManager.GetActiveScene();
            if(Application.isPlaying||scene.path!=ScenePath)throw new InvalidOperationException("Open main campus in Edit Mode.");
            root=GameObject.Find(IUHReferenceLayout.RootName)?.transform;
            if(root==null||root.Find("CONNECTED_BLOCKS_R1")==null)throw new InvalidOperationException("Connected campus required.");
            if(root.Find("PHOTO_REFERENCE_R2")!=null)throw new InvalidOperationException("Photo revision exists; edit the current objects in place.");
            if(GameObject.Find("_REWORK_V2")!=null)throw new InvalidOperationException("Existing rework root is active; preserve it before using the E builder.");
            string[] names={"Block_A","Block_A7","Block_A_Clinic","Block_H","Block_B","Block_V","Block_X","Block_E","Central_Parking","Gardens_Trees_Plaza"};
            foreach(var name in names)if(root.Find(name)==null)throw new InvalidOperationException("Missing "+name);
            Directory.CreateDirectory("Docs/Backups");
            const string backup="Docs/Backups/IUH_Campus_Main_before_photo_refinement_20261022.unity";
            if(!File.Exists(backup)&&!EditorSceneManager.SaveScene(scene,backup,true))throw new IOException("Snapshot failed.");
            meshFolderOverride=Folder+"/PhotoReference_R2/Meshes";Directory.CreateDirectory(meshFolderOverride);AssetDatabase.Refresh();
            CacheMeshes();Materials();pieces.Clear();
            var archive=Group("ARCHIVE_BEFORE_PHOTO_REFINEMENT",root,Vector3.zero);
            Undo.RegisterCreatedObjectUndo(archive.gameObject,"Preserve pre-photo campus");
            foreach(var name in names)Undo.SetTransformParent(root.Find(name),archive,"Archive exterior revision");
            archive.gameObject.SetActive(false);
            try
            {
                var specs=ConnectedSpecs();foreach(var spec in specs)BuildConnectedBlock(spec,specs);
                AddHGlassBands(root.Find("Block_H"));
                BuildPhotographicE();BuildLowParking();
                photoCourtyard=true;Landscape();photoCourtyard=false;
                BuildPhotoForecourt();
                UpdatePhotoLighting();
                Group("PHOTO_REFERENCE_R2",root,Vector3.zero);
                Place("GAMEPLAY_GRAB_CAMPUS/NIGHT_FOOD_SHOPS/Shop_02_FishBall_Cart_79",IUHReferenceLayout.Pickups[1]+new Vector3(-3,0,1.3f));
                var green=UnityEngine.Object.FindFirstObjectByType<IUHGreenCampusManager>(FindObjectsInactive.Include);
                var item=green.items.First(i=>i.itemId=="green_item_0_0");
                Undo.RecordObject(item.transform,"Keep litter outside shade-tree planter");item.transform.position=new Vector3(372.6f,.65f,-81.7f);
                foreach(var target in UnityEngine.Object.FindObjectsByType<IUHPhotoTarget>(FindObjectsInactive.Include))
                {
                    if(target.TargetId.Contains("block_a"))target.SetStructureReferences(root.Find("Block_A"),null);
                    if(target.TargetId.Contains("central_courtyard"))target.SetStructureReferences(root.Find("Block_E"),null);
                    EditorUtility.SetDirty(target);
                }
                AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
                Selection.activeGameObject=root.Find("Block_E").gameObject;
            }
            finally{photoCourtyard=false;meshFolderOverride=null;pieces.Clear();}
        }

        static void AddHGlassBands(Transform h)
        {
            var g=Group("Courtyard green glazing",h,Vector3.zero);
            var greenGlass=Mat("H_Green_Glass",new Color(.21f,.40f,.36f),.65f);
            const float height=41.25f;
            for(int fl=0;fl<11;fl++)
            {
                float y0=fl==0?0:3.5f+(fl-1)*(height-3.5f)/10;
                float fh=fl==0?3.5f:(height-3.5f)/10;
                Q(new Vector3(-15.2f,y0+fh*.51f,0),new Vector3(.16f,fh*.71f,33.5f),greenGlass);
                Q(new Vector3(-15.36f,y0+.15f,0),new Vector3(.22f,.3f,35.5f),ivory);
                for(float z=-16;z<=16;z+=3.2f)
                    Q(new Vector3(-15.42f,y0+fh*.5f,z),new Vector3(.22f,fh,.12f),ivory);
            }
            Flush(g);
        }

        static void BuildPhotographicE()
        {
            var temporary=new GameObject("_REWORK_V2");
            new GameObject("02_BUILDINGS").transform.SetParent(temporary.transform,false);
            try
            {
                // Reuse the project's existing E facade authoring: curtain glass, vertical fins and shallow lobby.
                IUHBuildingEBuilder.BuildBuildingE();
                var e=temporary.transform.Find("02_BUILDINGS/Building_E_Admin");
                if(e==null)throw new InvalidOperationException("E facade generation failed.");
                e.SetParent(root,false);e.name="Block_E";
                e.SetPositionAndRotation(new Vector3(385,.49f,-87.5f),Quaternion.Euler(0,180,0));
                e.localScale=new Vector3(1.25f,1,1.5f);
                var panel=e.Find("02_Facade_CurtainWall/Signage_Panel_IUH");
                if(panel==null)panel=e.GetComponentsInChildren<Transform>().First(t=>t.name=="Signage_Panel_IUH");
                panel.localScale=new Vector3(18,6,1);panel.GetComponent<MeshRenderer>().sharedMaterial=white;
                panel.localPosition=new Vector3(0,10,2.30f);
                var backing=panel.Find("Signage_Backing_Frame");
                backing.localPosition=new Vector3(0,0,.09f);backing.localScale=new Vector3(1.03f,1.06f,.08f);
                Text(e,"IUH wordmark","IUH",new Vector3(-4.5f,10,2.22f),2.2f,Quaternion.identity,blue.color);
                Text(e,"University name","ĐẠI HỌC CÔNG NGHIỆP\nTHÀNH PHỐ HỒ CHÍ MINH",new Vector3(3.2f,10,2.21f),.53f,Quaternion.identity,blue.color);
                var monument=e.GetComponentsInChildren<Transform>().First(t=>t.name=="Monument_WhiteStone");
                monument.parent.localPosition=new Vector3(-6.5f,0,-1.2f);
                foreach(var renderer in monument.GetComponentsInChildren<MeshRenderer>())renderer.sharedMaterial=white;
                Text(e,"Monument legend","IUH",new Vector3(-6.5f,1.55f,-1.91f),1.35f,Quaternion.identity,Color.white);
                var canopy=e.GetComponentsInChildren<Transform>().First(t=>t.name=="Entrance_Canopy");canopy.gameObject.SetActive(false);
                var newCanopy=Group("Wide elliptical entrance canopy",e,Vector3.zero);
                S(new Vector3(0,3.8f,1.6f),new Vector3(17,.24f,3.8f),dark);
                for(float x=-7.6f;x<=7.6f;x+=1.3f)Q(new Vector3(x,3.79f,1.6f),new Vector3(.05f,.05f,3.3f),stone);
                foreach(float x in new[]{-7.5f,7.5f})Cy(new Vector3(x,1.9f,.3f),new Vector3(.10f,1.9f,.10f),dark);
                Flush(newCanopy);
                // Ground-level planting bowls flank the entry, leaving the photo/circulation axis open.
                var pots=Group("Entrance flower bowls",e,Vector3.zero);
                foreach(float x in new[]{-10f,-8f,-6f,6f,8f,10f})
                {
                    Cy(new Vector3(x,.22f,.3f),new Vector3(.85f,.22f,.85f),white);
                    S(new Vector3(x,.52f,.3f),new Vector3(.85f,.5f,.85f),leafLight);
                    for(int i=0;i<5;i++)S(new Vector3(x+Mathf.Sin(i*1.26f)*.25f,.72f,.3f+Mathf.Cos(i*1.26f)*.25f),new Vector3(.18f,.13f,.18f),gold);
                }
                Flush(pots);
                // Dynamic combined meshes from the older builder need persistent asset references in the saved scene.
                int index=0;
                foreach(var filter in e.GetComponentsInChildren<MeshFilter>(true))
                {
                    var mesh=filter.sharedMesh;if(mesh==null||AssetDatabase.Contains(mesh))continue;
                    mesh.name="PhotoE_"+(index++)+"_"+filter.name;
                    AssetDatabase.CreateAsset(mesh,meshFolderOverride+"/"+mesh.name+".asset");
                }
                // Keep a compact roof identifier for the overview; storeys here follow the facade image, not a survey.
                Text(e,"Roof ID","TÒA E",new Vector3(0,20,11),2,Quaternion.Euler(90,0,0),blue.color);
            }
            finally{UnityEngine.Object.DestroyImmediate(temporary);}
        }

        static void BuildLowParking()
        {
            var p=Group("Central_Parking",root,new Vector3(382,.49f,-40));
            Q(new Vector3(0,.08f,0),new Vector3(39,.16f,56),stone);
            Q(new Vector3(0,2.05f,0),new Vector3(40,.26f,57),stone);
            Q(new Vector3(0,2.195f,0),new Vector3(39.8f,.025f,56.8f),paving);
            foreach(int sign in new[]{-1,1})
            {
                for(float z=-26;z<=26;z+=6.5f)
                {
                    Q(new Vector3(sign*18.5f,1,z),new Vector3(.45f,2,.45f),ivory);
                    Solid(p,"Parking support",new Vector3(sign*18.5f,1,z),new Vector3(.45f,2,.45f));
                }
                Rail(p,new Vector3(0,2.2f,sign*28),new Vector3(38,0,0),teal);
                Rail(p,new Vector3(sign*19.5f,2.2f,0),new Vector3(0,0,54),teal);
            }
            for(float x=-17;x<18;x+=3.3f)for(float z=-24;z<25;z+=8)
            {
                Q(new Vector3(x,2.23f,z),new Vector3(.08f,.02f,4.8f),gold);
                Q(new Vector3(x,.75f,z),new Vector3(.45f,.85f,1.65f),dark);
            }
            // The forecourt sees the low deck's long rear face, a ventilation riser and benches below it.
            Q(new Vector3(0,1.0f,-28.15f),new Vector3(38,1.8f,.18f),stone);
            Q(new Vector3(0,1.1f,-28.26f),new Vector3(35,.65f,.07f),dark);
            Q(new Vector3(0,2.0f,-28.45f),new Vector3(1.2f,3.6f,.8f),roof);
            Flush(p);
            Solid(p,"Low bicycle terrace",new Vector3(0,2.05f,0),new Vector3(40,.26f,57));
            // Paired ramps sit outside the court; their lower ends face E and meet short top landings.
            for(int side=-1;side<=1;side+=2)
            {
                float x=side*22;
                var r=Group(side<0?"West access ramp":"East access ramp",p,new Vector3(x,1.08f,-35.5f));
                r.localRotation=Quaternion.Euler(-Mathf.Atan2(2.16f,20)*Mathf.Rad2Deg,0,0);
                Q(Vector3.zero,new Vector3(3.6f,.16f,20.12f),paving);
                foreach(int edge in new[]{-1,1})
                    Rail(r,new Vector3(edge*1.75f,.10f,0),new Vector3(0,0,20.1f),teal);
                Flush(r);Solid(r,"Sloped ramp surface",Vector3.zero,new Vector3(3.6f,.16f,20.12f));
                Box(p,side<0?"West deck landing":"East deck landing",new Vector3(side*20.5f,2.05f,-25.5f),new Vector3(6,.26f,4),paving,true);
            }
            Text(p,"Parking ID","SÂN GIỮ XE",new Vector3(0,2.25f,-3),1.9f,Quaternion.Euler(90,180,0),blue.color);
        }
        static void Rail(Transform parent,Vector3 center,Vector3 span,Material material)
        {
            bool alongX=Mathf.Abs(span.x)>Mathf.Abs(span.z);float length=span.magnitude;
            Vector3 direction=alongX?Vector3.right:Vector3.forward;
            for(float d=-length/2;d<=length/2+.01f;d+=2)
                Q(center+direction*d+Vector3.up*.55f,new Vector3(.09f,1.1f,.09f),material);
            foreach(float y in new[]{.48f,1.05f})
                Q(center+Vector3.up*y,alongX?new Vector3(length,.09f,.09f):new Vector3(.09f,.09f,length),material);
        }

        static void BuildPhotoForecourt()
        {
            var court=Group("Photo_Forecourt",root,Vector3.zero);
            var grey=Mat("Forecourt_Asphalt",new Color(.37f,.39f,.37f));
            Q(new Vector3(382,.54f,-80),new Vector3(42,.04f,19),grey);Flush(court);
            Solid(court,"Forecourt walking surface",new Vector3(382,.54f,-80),new Vector3(42,.04f,19));
            // Real imported broadleaf meshes supply branching silhouettes, rather than spherical stand-ins.
            var old=Resources.FindObjectsOfTypeAll<GameObject>().First(g=>g.name=="campus_P0_progress"&&g.scene.IsValid());
            var source=old.transform.Find("CayBang_Courtyard_NE");
            foreach(var point in new[]{new Vector2(370,-78),new Vector2(396,-80)})
            {
                var tree=UnityEngine.Object.Instantiate(source.gameObject);tree.name="Large shade tree "+point.x;
                tree.GetComponent<MeshRenderer>().sharedMaterials=new[]{bark,leafLight};
                tree.transform.SetParent(court,true);tree.SetActive(true);
                tree.transform.localScale=source.lossyScale*1.3f;tree.transform.rotation=source.rotation;
                tree.transform.position=new Vector3(point.x,.54f,point.y);
                foreach(var collider in tree.GetComponentsInChildren<Collider>(true))UnityEngine.Object.DestroyImmediate(collider);
                var bounds=tree.GetComponent<MeshRenderer>().bounds;tree.transform.position+=Vector3.up*(.54f-bounds.min.y);
                Solid(court,"Shade tree trunk "+point.x,new Vector3(point.x,2.5f,point.y),new Vector3(.95f,4,.95f));
                Cy(new Vector3(point.x,.65f,point.y),new Vector3(4.8f,.12f,4.8f),dark);
                Cy(new Vector3(point.x,.79f,point.y),new Vector3(4.3f,.025f,4.3f),grass);
                Solid(court,"Low circular planter "+point.x,new Vector3(point.x,.64f,point.y),new Vector3(4.6f,.2f,4.6f));
            }
            // Long seating row against the rear face of the parking deck.
            for(float x=370;x<=395;x+=5)
            {
                Q(new Vector3(x,.98f,-69.3f),new Vector3(3.5f,.13f,.65f),stone);
                Q(new Vector3(x,1.35f,-69),new Vector3(3.5f,.65f,.12f),stone);
                foreach(int sign in new[]{-1,1})Q(new Vector3(x+sign*1.15f,.72f,-69.3f),new Vector3(.14f,.5f,.5f),dark);
            }
            Flush(Group("Tree planters and parking benches",court,Vector3.zero));
        }

        static void UpdatePhotoLighting()
        {
            var sun=root.Find("Daylight/Campus Sun")?.GetComponent<Light>();
            if(sun!=null){sun.transform.rotation=Quaternion.Euler(50,155,0);sun.intensity=1.15f;sun.color=Color.white;sun.shadowStrength=.45f;}
            RenderSettings.ambientSkyColor=new Color(.67f,.73f,.77f);
            RenderSettings.ambientEquatorColor=new Color(.60f,.64f,.65f);
            RenderSettings.ambientGroundColor=new Color(.42f,.43f,.40f);
        }
    }
}
#endif
