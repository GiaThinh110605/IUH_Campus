#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace IUHCampus.Editor
{
    public static partial class IUHReferenceCampusBuilder
    {
        const string ArtFolder="Assets/IUH_Campus/CourtyardGraphics";
        [MenuItem("IUH Campus/Graphics/Upgrade Central Courtyard")]
        public static void UpgradeCourtyard()
        {
            var scene=EditorSceneManager.GetActiveScene();
            if(Application.isPlaying||scene.path!=ScenePath)throw new InvalidOperationException("Open main campus in Edit Mode.");
            root=GameObject.Find("IUH_REFERENCE_CAMPUS")?.transform;
            if(root==null||root.Find("Photo_Forecourt")==null)throw new InvalidOperationException("Photo forecourt required.");
            if(root.Find("COURTYARD_GRAPHICS_R3")!=null)throw new InvalidOperationException("Courtyard upgrade already exists.");
            Directory.CreateDirectory("Docs/Backups");
            EditorSceneManager.SaveScene(scene,"Docs/Backups/IUH_Campus_Main_before_courtyard_graphics.unity",true);
            Directory.CreateDirectory(ArtFolder+"/Meshes");AssetDatabase.Refresh();
            var art=Group("COURTYARD_GRAPHICS_R3",root,Vector3.zero);
            Undo.RegisterCreatedObjectUndo(art.gameObject,"Upgrade courtyard graphics");
            meshFolderOverride=ArtFolder+"/Meshes";CacheMeshes();pieces.Clear();
            try
            {
                var road=Surface("Fine asphalt",new Color(.35f,.37f,.36f),0);
                var granite=Surface("Granite",new Color(.65f,.66f,.61f),1);
                var timber=Surface("Warm timber",new Color(.36f,.19f,.085f),2);
                var soil=Surface("Planting soil",new Color(.17f,.12f,.065f),0);
                var greens=Surface("Garden foliage",new Color(.19f,.34f,.075f),0);
                var flower=Surface("Golden flowers",new Color(.94f,.58f,.10f),0);
                var court=root.Find("Photo_Forecourt");
                foreach(var r in court.GetComponentsInChildren<MeshRenderer>())
                {
                    Undo.RecordObject(r,"Courtyard material detail");
                    r.sharedMaterials=r.sharedMaterials.Select(m=>m!=null&&m.name=="Forecourt_Asphalt"?road:m).ToArray();
                }
                // Visual dressing stays within existing planters and seating, with no added blockers.
                foreach(float x in new[]{370f,396f})
                {
                    Cy(new Vector3(x,.82f,x==370?-78:-80),new Vector3(4.1f,.015f,4.1f),soil);
                    float z=x==370?-78:-80;
                    for(int i=0;i<44;i++)
                    {
                        float a=i*Mathf.PI*2/44;
                        var p=new Vector3(x+Mathf.Cos(a)*1.85f,.95f,z+Mathf.Sin(a)*1.85f);
                        S(p,new Vector3(.34f,.27f,.32f),greens);
                        if(i%2==0)S(p+Vector3.up*.14f,new Vector3(.13f,.10f,.13f),flower);
                    }
                }
                for(float x=370;x<=395;x+=5)
                {
                    for(int slat=0;slat<5;slat++)Q(new Vector3(x,1.057f,-69.55f+slat*.125f),new Vector3(3.38f,.035f,.105f),timber);
                    for(int slat=0;slat<4;slat++)Q(new Vector3(x,1.12f+slat*.16f,-69.075f),new Vector3(3.38f,.115f,.035f),timber);
                }
                // Thin stone gutters mark the parking edge; center circulation stays asphalt.
                Q(new Vector3(382,.566f,-70.25f),new Vector3(38,.012f,.38f),granite);
                for(float x=364;x<401;x+=1.2f)Q(new Vector3(x,.574f,-70.25f),new Vector3(.035f,.004f,.38f),soil);
                Flush(art);
                RefineCourtyardFoliage();
                var sun=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).FirstOrDefault(l=>l.type==LightType.Directional);
                if(sun==null)sun=Group("Courtyard daylight",art,Vector3.zero).gameObject.AddComponent<Light>();
                Undo.RecordObject(sun,"Courtyard sunlight");sun.type=LightType.Directional;sun.color=new Color(1,.95f,.84f);
                sun.intensity=1.6f;sun.shadows=LightShadows.Soft;sun.shadowStrength=.78f;sun.shadowBias=.035f;sun.shadowNormalBias=.3f;
                Undo.RecordObject(sun.transform,"Sun angle");sun.transform.rotation=Quaternion.Euler(48,145,0);RenderSettings.sun=sun;
                RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.48f,.61f,.75f);
                RenderSettings.ambientEquatorColor=new Color(.39f,.43f,.46f);RenderSettings.ambientGroundColor=new Color(.22f,.24f,.20f);
                var sky=new Material(Shader.Find("Skybox/Procedural"));sky.name="Courtyard daylight sky";
                sky.SetFloat("_SunSize",.035f);sky.SetFloat("_AtmosphereThickness",1.05f);sky.SetFloat("_Exposure",1.1f);
                AssetDatabase.CreateAsset(sky,ArtFolder+"/DaySky.mat");RenderSettings.skybox=sky;
                var volume=Group("Courtyard color and contrast",art,new Vector3(382,8,-80)).gameObject;
                var area=volume.AddComponent<BoxCollider>();area.isTrigger=true;area.size=new Vector3(55,30,42);
                var v=volume.AddComponent<Volume>();v.isGlobal=false;v.blendDistance=8;v.priority=30;
                var profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,ArtFolder+"/CourtyardVolume.asset");v.sharedProfile=profile;
                var tone=profile.Add<Tonemapping>();tone.mode.Override(TonemappingMode.ACES);
                var color=profile.Add<ColorAdjustments>();color.contrast.Override(12);color.saturation.Override(6);
                var bloom=profile.Add<Bloom>();bloom.intensity.Override(.12f);bloom.threshold.Override(1.3f);
                foreach(var component in profile.components)AssetDatabase.AddObjectToAsset(component,profile);
                foreach(var camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
                {var data=camera.GetComponent<UniversalAdditionalCameraData>();if(data!=null){Undo.RecordObject(data,"Enable courtyard grading");data.renderPostProcessing=true;}}
                AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
                Selection.activeGameObject=art.gameObject;
                Debug.Log("Courtyard R3 applied: textured surfaces, timber seating, planted tree rings, daylight and local grading.");
            }
            finally{meshFolderOverride=null;pieces.Clear();}
        }
        static Material Surface(string name,Color color,int pattern)
        {
            const int n=256;var texture=new Texture2D(n,n,TextureFormat.RGB24,true);texture.name=name;
            var pixels=new Color[n*n];
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                float noise=Mathf.PerlinNoise(x*.37f,y*.37f)*.20f+.87f;
                if(pattern==1)noise*=x%128<2||y%128<2?.65f:1;
                if(pattern==2)noise*=.8f+.2f*Mathf.Sin(y*.6f+Mathf.PerlinNoise(x*.04f,y*.025f)*6);
                pixels[y*n+x]=color*noise;
            }
            texture.SetPixels(pixels);texture.Apply();
            string path=ArtFolder+"/"+name+".png";File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.wrapMode=TextureWrapMode.Repeat;importer.anisoLevel=4;importer.SaveAndReimport();
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.name=name;m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            m.mainTextureScale= name=="Fine asphalt"?new Vector2(32,16):new Vector2(2,2);
            m.SetFloat("_Smoothness",pattern==1?.28f:.12f);m.enableInstancing=true;AssetDatabase.CreateAsset(m,ArtFolder+"/"+name+".mat");return m;
        }

        [MenuItem("IUH Campus/Graphics/Refine Courtyard Foliage")]
        public static void RefineCourtyardFoliage()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Edit Mode required.");
            var court=GameObject.Find("IUH_REFERENCE_CAMPUS/Photo_Forecourt");
            if(court==null)throw new InvalidOperationException("Courtyard missing.");
            var random=new System.Random(7361);
            float R()=> (float)random.NextDouble();
            foreach(var renderer in court.GetComponentsInChildren<MeshRenderer>())
            {
                if(!renderer.name.StartsWith("Large shade tree")||renderer.transform.Find("Detailed leaf canopy")!=null)continue;
                var bounds=renderer.bounds;var filter=renderer.GetComponent<MeshFilter>();
                var trunk=UnityEngine.Object.Instantiate(filter.sharedMesh);trunk.name=renderer.name+" branches";
                // Preserve the source's bark submesh; replace its polygonal foliage with individual leaves.
                trunk.SetTriangles(Array.Empty<int>(),1);AssetDatabase.CreateAsset(trunk,ArtFolder+"/Meshes/"+trunk.name+".asset");
                Undo.RecordObject(filter,"Refine tree canopy");filter.sharedMesh=trunk;
                var vertices=new List<Vector3>();var normals=new List<Vector3>();var triangles=new List<int>();
                for(int i=0;i<9000;i++)
                {
                    float angle=R()*Mathf.PI*2;float radius=Mathf.Sqrt(R())*9.5f;
                    var p=new Vector3(bounds.center.x+Mathf.Cos(angle)*radius,6.5f+2.7f*(1-radius/12)+R()*1.1f,bounds.center.z+Mathf.Sin(angle)*radius);
                    var rotation=Quaternion.Euler(R()*100-50,R()*360,R()*90-45);
                    float size=.22f+R()*.22f;int index=vertices.Count;
                    foreach(var v in new[]{new Vector3(-size,0,0),new Vector3(0,.025f,size*.48f),new Vector3(size,0,0),new Vector3(0,-.025f,-size*.48f)})
                    {vertices.Add(p+rotation*v);normals.Add(rotation*Vector3.up);}
                    triangles.AddRange(new[]{index,index+1,index+2,index,index+2,index+3,index+2,index+1,index,index+3,index+2,index});
                }
                var mesh=new Mesh();mesh.name=renderer.name+" leaf canopy";mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();
                AssetDatabase.CreateAsset(mesh,ArtFolder+"/Meshes/"+mesh.name+".asset");
                var g=new GameObject("Detailed leaf canopy");Undo.RegisterCreatedObjectUndo(g,"Detailed canopy");
                g.transform.SetParent(court.transform,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;
                var leaves=g.AddComponent<MeshRenderer>();leaves.sharedMaterial=renderer.sharedMaterials[1];g.isStatic=true;
                // Marker under source prevents duplicate generation while retaining world-space mesh coordinates.
                new GameObject("Detailed leaf canopy").transform.SetParent(renderer.transform,false);
            }
            AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
    }
}
#endif
