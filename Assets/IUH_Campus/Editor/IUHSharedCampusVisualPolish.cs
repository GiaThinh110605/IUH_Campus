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
    public static partial class IUHSharedCampusConceptBuilder
    {
        const string Polish=Folder+"/VisualPolish";
        static Material polishedHedge,flowerOrange,flowerWhite,flowerPink;
        [MenuItem("IUH Campus/Concept/Apply Detail and Sharpness Pass")]
        public static void ApplyVisualPolish()
        {
            var scene=EditorSceneManager.GetActiveScene();
            if(Application.isPlaying||scene.path!=ScenePath)throw new InvalidOperationException("Open concept scene in Edit Mode.");
            root=GameObject.Find("CAMPUS_ADVENTURE_SHARED_WORLD").transform;
            if(root.Find("VISUAL_POLISH_V2")!=null)throw new InvalidOperationException("Visual polish already applied.");
            Directory.CreateDirectory("Docs/Backups");const string backup="Docs/Backups/IUH_SharedCampus_before_visual_polish.unity";
            if(!File.Exists(backup)&&!EditorSceneManager.SaveScene(scene,backup,true))throw new IOException("Snapshot failed.");
            Directory.CreateDirectory(Polish+"/Textures");Directory.CreateDirectory(Polish+"/Materials");AssetDatabase.Refresh();
            cube=Primitive(PrimitiveType.Cube);cylinder=Primitive(PrimitiveType.Cylinder);sphere=Primitive(PrimitiveType.Sphere);groupId=400;parts.Clear();
            Material Load(string name)=>AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/"+name+".mat");
            cream=Load("Limestone facade");stone=Load("Warm granite");paving=Load("Sandstone paving");pavingLight=Load("Paving pale");brick=Load("Terracotta brick");wood=Load("Teak timber");
            glass=Load("Blue green glazing");metal=Load("Dark bronze");lawn=Load("Lawn");bark=Load("Tree bark");hedge=Load("Hedge");leaf=Load("Leaves shadow");leafLight=Load("Leaves sunlight");white=Load("Ivory sculpture");gold=Load("Antique brass");blue=Load("IUH blue");purple=Load("Festival violet");
            var swaps=new Dictionary<Material,Material>{
                {cream,SurfaceSet("Dressed limestone",new Color(.82f,.79f,.69f),2,.22f)},
                {stone,SurfaceSet("Speckled granite",new Color(.61f,.60f,.54f),2,.30f)},
                {paving,SurfaceSet("Courtyard sandstone",new Color(.69f,.61f,.49f),0,.27f)},
                {pavingLight,SurfaceSet("Pale paving slabs",new Color(.82f,.76f,.64f),0,.24f)},
                {brick,SurfaceSet("Weathered terracotta",new Color(.58f,.30f,.20f),1,.16f)},
                {wood,SurfaceSet("Timber grain",new Color(.39f,.24f,.11f),3,.24f)},
                {lawn,SurfaceSet("Detailed lawn",new Color(.25f,.37f,.10f),4,.08f)},
                {bark,SurfaceSet("Bark grooves",new Color(.26f,.19f,.12f),3,.12f)}
            };
            foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>())
            {
                Undo.RecordObject(renderer,"Detailed surface materials");renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>m!=null&&swaps.TryGetValue(m,out var replacement)?replacement:m).ToArray();
            }
            cream=swaps[cream];stone=swaps[stone];paving=swaps[paving];pavingLight=swaps[pavingLight];brick=swaps[brick];wood=swaps[wood];lawn=swaps[lawn];bark=swaps[bark];
            polishedHedge=SurfaceSet("Trimmed hedge foliage",new Color(.14f,.29f,.07f),5,.13f);
            flowerOrange=PolishMat("Marigold",new Color(.96f,.43f,.075f));flowerWhite=PolishMat("White flowers",new Color(.93f,.91f,.79f));flowerPink=PolishMat("Pink flowers",new Color(.77f,.15f,.29f));
            FacadeDetails();WindowVariation();GardenDetails();RefineTrees();Pipeline();
            var probeObject=new GameObject("Courtyard reflections");probeObject.transform.SetParent(root,false);probeObject.transform.position=new Vector3(0,8,9);
            var probe=probeObject.AddComponent<ReflectionProbe>();probe.mode=ReflectionProbeMode.Realtime;probe.refreshMode=ReflectionProbeRefreshMode.OnAwake;probe.timeSlicingMode=ReflectionProbeTimeSlicingMode.AllFacesAtOnce;probe.resolution=256;probe.size=new Vector3(150,80,165);probe.boxProjection=true;probe.intensity=1;
            var sun=RenderSettings.sun;if(sun!=null){Undo.RecordObject(sun,"Natural daylight");sun.intensity=2.05f;sun.color=new Color(1,.95f,.85f);sun.shadowStrength=.83f;}
            RenderSettings.ambientSkyColor=new Color(.57f,.69f,.83f);RenderSettings.ambientEquatorColor=new Color(.46f,.50f,.52f);
            foreach(var text in root.GetComponentsInChildren<TextMesh>())
            {
                if(text.text.Contains("ĐẠI HỌC")||text.text=="IUH")text.fontStyle=FontStyle.Bold;
                if(text.text.Contains("ĐẠI HỌC")){text.transform.localPosition=new Vector3(0,21.6f,58.04f);text.characterSize=1.55f/9;}
            }
            var photographicSky=AssetDatabase.LoadAssetAtPath<Material>(Polish+"/Materials/Photographic campus sky.mat");
            if(photographicSky!=null)RenderSettings.skybox=photographicSky;
            else if(RenderSettings.skybox!=null){RenderSettings.skybox.SetColor("_Zenith",new Color(.14f,.42f,.80f));RenderSettings.skybox.SetColor("_Horizon",new Color(.70f,.84f,.94f));EditorUtility.SetDirty(RenderSettings.skybox);}
            new GameObject("VISUAL_POLISH_V2").transform.SetParent(root,false);
            AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);Selection.activeGameObject=root.gameObject;
            Debug.Log("Visual polish applied: detailed PBR surfaces, facade depth, trimmed hedges, flowers, curved leaves, reflection probe and a scene-owned desktop pipeline.");
        }
        static Material PolishMat(string name,Color color,float smooth=.2f)
        {
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name,color=color,enableInstancing=true};m.SetFloat("_Smoothness",smooth);AssetDatabase.CreateAsset(m,Polish+"/Materials/"+name+".mat");return m;
        }
        static float RandomCell(int x,int y){unchecked{uint n=(uint)(x*374761393+y*668265263);n=(n^(n>>13))*1274126177;return (n&65535)/65535f;}}
        static Material SurfaceSet(string name,Color tint,int pattern,float smooth)
        {
            const int n=1024;var colorPixels=new Color[n*n];var heights=new float[n*n];
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                float fine=RandomCell(x,y),macro=Mathf.PerlinNoise(x*.015f,y*.015f),value=.86f+macro*.18f+(fine-.5f)*.1f,h=.5f;
                if(pattern==0)
                {
                    bool grout=x%512<5||y%512<5;value*=grout?.58f:.96f+RandomCell(x/512,y/512)*.10f;h=grout?.16f:.56f+macro*.018f;
                }
                else if(pattern==1)
                {
                    int row=y/32,xx=(x+(row%2)*64)%128;bool mortar=xx<4||y%32<3;value*=mortar?1.14f:.73f+RandomCell((x+(row%2)*64)/128,row)*.34f;h=mortar?.12f:.55f+(fine-.5f)*.07f;
                }
                else if(pattern==2){value=.89f+(macro-.5f)*.08f+(fine-.5f)*.20f;h=.5f+(fine-.5f)*.04f;}
                else if(pattern==3){float grain=Mathf.Sin(x*.22f+Mathf.PerlinNoise(x*.005f,y*.025f)*13);value*=.9f+grain*.17f;h=.5f+grain*.12f;}
                else if(pattern==4){float blade=RandomCell(x/2,y/15);value=.76f+blade*.38f+(macro-.5f)*.19f;h=.4f+blade*.16f;}
                else {float xx=(x%20-10)/10f,yy=(y%13-6.5f)/6.5f;float oval=Mathf.Clamp01(1-xx*xx-yy*yy);value=.70f+oval*.36f+(macro-.5f)*.2f;h=.35f+oval*.22f;}
                colorPixels[y*n+x]=new Color(tint.r*value,tint.g*value,tint.b*value);heights[y*n+x]=h;
            }
            var normalPixels=new Color[n*n];
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                float dx=heights[y*n+(x+1)%n]-heights[y*n+(x+n-1)%n],dy=heights[((y+1)%n)*n+x]-heights[((y+n-1)%n)*n+x];
                var normal=new Vector3(-dx*2,-dy*2,1).normalized;normalPixels[y*n+x]=new Color(normal.x*.5f+.5f,normal.y*.5f+.5f,normal.z*.5f+.5f);
            }
            string albedo=Polish+"/Textures/"+name+"_Albedo.png",normalPath=Polish+"/Textures/"+name+"_Normal.png";
            WriteTexture(albedo,colorPixels,n,false);WriteTexture(normalPath,normalPixels,n,true);
            var material=PolishMat(name,Color.white,smooth);material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(albedo);material.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath));material.SetFloat("_BumpScale",pattern==1?.65f:.35f);material.EnableKeyword("_NORMALMAP");return material;
        }
        static void WriteTexture(string path,Color[] pixels,int size,bool normal)
        {
            var texture=new Texture2D(size,size,TextureFormat.RGB24,false,normal);texture.SetPixels(pixels);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;importer.sRGBTexture=!normal;importer.wrapMode=TextureWrapMode.Repeat;importer.anisoLevel=8;importer.maxTextureSize=1024;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();
        }
        static void FacadeDetails()
        {
            var atrium=Group("Detailed central facade",Vector3.zero);
            Q(new Vector3(0,12.8f,58.7f),new Vector3(14.8f,12.8f,.22f),glass);
            foreach(float x in new[]{-8f,-4f,0f,4f,8f})Q(new Vector3(x,13.8f,58.35f),new Vector3(.32f,15,.40f),white);
            foreach(float y in new[]{7f,11.3f,15.6f,19.9f})
            {
                Q(new Vector3(0,y,57.95f),new Vector3(17,.28f,1.6f),cream);
                Q(new Vector3(0,y+.65f,57.25f),new Vector3(16.5f,.065f,.065f),metal);
                for(float x=-8;x<=8;x+=.75f)Q(new Vector3(x,y+.35f,57.25f),new Vector3(.045f,.65f,.045f),metal);
            }
            Q(new Vector3(0,21.6f,58.3f),new Vector3(19,1.1f,.35f),white);Flush(atrium);
            // Repeated recessed panels, balcony frames and stone joints break up the plain side walls.
            var specs=new[]{new{p=new Vector3(-47,0,43),w=48f,d=13f,h=21f,yaw=-72f},new{p=new Vector3(49,0,39),w=52f,d=14f,h=25f,yaw=74f}};
            foreach(var spec in specs)
            {
                var t=Group("Facade relief",spec.p,Quaternion.Euler(0,spec.yaw,0));
                for(float y=4.2f;y<spec.h;y+=4.2f)
                {
                    foreach(int end in new[]{-1,1})Q(new Vector3(end*(spec.w/2+.04f),y,0),new Vector3(.18f,.18f,spec.d+.4f),cream);
                    for(float x=-spec.w/2+4;x<spec.w/2-3;x+=7.6f)
                    {
                        Q(new Vector3(x,y-.35f,-spec.d/2-1),new Vector3(3.25f,.16f,1.65f),cream);
                        Q(new Vector3(x,y+.45f,-spec.d/2-1.75f),new Vector3(3,.07f,.07f),metal);
                        for(int k=-2;k<=2;k++)Q(new Vector3(x+k*.6f,y+.05f,-spec.d/2-1.75f),new Vector3(.045f,.8f,.045f),metal);
                    }
                }
                Q(new Vector3(0,spec.h+1.1f,-spec.d/2+.3f),new Vector3(spec.w,.7f,.22f),cream);Flush(t);
            }
            var flags=Group("University flags",Vector3.zero);
            foreach(float x in new[]{-10f,10f})C(new Vector3(x,4.7f,52),.055f,9.4f,metal);
            Flush(flags);
            var flagTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/IUH_Campus/Textures/T_Flag_Vietnam.png");
            var flag=PolishMat("Vietnam flag fabric",Color.white);flag.mainTexture=flagTexture;flag.SetFloat("_Cull",0);
            var quad=Primitive(PrimitiveType.Quad);var f=Group("Entrance flags",Vector3.zero);
            foreach(float x in new[]{-10f,10f})Add(flag,quad,new Vector3(x+1.1f,8.2f,51.9f),new Vector3(2.2f,1.45f,1));Flush(f);
        }
        static Mesh RoundedBox(Vector3 size,float radius)
        {
            var v=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();int grid=5;
            var axes=new[]{Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back};var inner=size*.5f-Vector3.one*radius;
            foreach(var normal in axes)
            {
                var tangent=Mathf.Abs(normal.y)>.5f?Vector3.right:Vector3.Cross(Vector3.up,normal);var bitangent=Vector3.Cross(normal,tangent);int start=v.Count;
                for(int y=0;y<=grid;y++)for(int x=0;x<=grid;x++)
                {
                    var point=Vector3.Scale(normal*.5f+tangent*(x/(float)grid-.5f)+bitangent*(y/(float)grid-.5f),size);
                    var closest=new Vector3(Mathf.Clamp(point.x,-inner.x,inner.x),Mathf.Clamp(point.y,-inner.y,inner.y),Mathf.Clamp(point.z,-inner.z,inner.z));var n=(point-closest).normalized;
                    v.Add(closest+n*radius);normals.Add(n);uv.Add(new Vector2(point.x+point.z,point.y)*3);
                }
                for(int y=0;y<grid;y++)for(int x=0;x<grid;x++){int k=start+y*(grid+1)+x;triangles.AddRange(new[]{k,k+1,k+grid+2,k,k+grid+2,k+grid+1});}
            }
            var mesh=new Mesh();mesh.SetVertices(v);mesh.SetNormals(normals);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();return mesh;
        }
        static void GardenDetails()
        {
            foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>())
                if(renderer.sharedMaterial==hedge&&(renderer.transform.parent.name.StartsWith("Garden beds")||renderer.transform.parent.name.StartsWith("Curved fountain")))
                {Undo.RecordObject(renderer,"Replace bead hedges");renderer.enabled=false;}
            var t=Group("Layered planting and clipped hedges",Vector3.zero);
            foreach(int side in new[]{-1,1})foreach(float z in new[]{-29f,-1f,29f})foreach(int edge in new[]{-1,1})
                Add(polishedHedge,RoundedBox(new Vector3(15.5f,.65f,.85f),.2f),new Vector3(side*20,.65f,z+edge*5.6f),Vector3.one);
            foreach(float start in new[]{-36f,144f})for(int i=0;i<=24;i++)
            {
                float a=(start+i*3)*Mathf.Deg2Rad;
                Add(polishedHedge,RoundedBox(new Vector3(.8f,.6f,.85f),.2f),new Vector3(Mathf.Cos(a)*11.6f,.55f,10+Mathf.Sin(a)*11.6f),Vector3.one,Quaternion.Euler(0,-a*Mathf.Rad2Deg,0));
            }
            foreach(int side in new[]{-1,1})foreach(float z in new[]{-33f,-19f,-6f,26f,39f})
            {
                var center=new Vector3(side*7.15f,0,z);C(center+Vector3.up*.24f,.48f,.48f,white);C(center+Vector3.up*.49f,.42f,.05f,polishedHedge);
                for(int petal=0;petal<22;petal++)
                {
                    float a=petal*2.4f,r=.33f*Mathf.Sqrt(petal/22f);var p=center+new Vector3(Mathf.Cos(a)*r,.65f+(petal%3)*.035f,Mathf.Sin(a)*r);
                    S(p,new Vector3(.17f,.11f,.17f),petal%3==0?flowerWhite:flowerOrange);
                }
            }
            // Dense flower drifts sit inside lawns, leaving the tested central paths clear.
            var random=new System.Random(74);
            foreach(int side in new[]{-1,1})foreach(float z in new[]{-29f,-1f,29f})for(int i=0;i<95;i++)
            {
                float x=side*20+(float)(random.NextDouble()*12-6),zz=z+(i%2==0?-4.5f:4.5f)+(float)(random.NextDouble()*.7-.35);
                S(new Vector3(x,.51f,zz),new Vector3(.22f,.22f,.22f),polishedHedge);S(new Vector3(x,.66f,zz),new Vector3(.15f,.10f,.15f),i%4==0?flowerWhite:i%3==0?flowerPink:flowerOrange);
            }
            Flush(t);
        }
        static void RefineTrees()
        {
            for(int variant=0;variant<3;variant++)for(int level=0;level<3;level++)
            {
                string path=Folder+"/Meshes/Broadleaf_"+variant+"_LOD"+level+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing==null)continue;
                int count=level==0?4200:level==1?1400:350;var random=new System.Random(170+variant);float R()=>(float)random.NextDouble();
                var v=new List<Vector3>();var normals=new List<Vector3>();var triangles=new[]{new List<int>(),new List<int>()};
                for(int i=0;i<count;i++)
                {
                    int clump=i%9;float angle=clump*Mathf.PI*2/9;var center=new Vector3(Mathf.Cos(angle)*2.3f,6.4f+(clump%3)*.55f,Mathf.Sin(angle)*2.3f);
                    var point=new Vector3(R()*2-1,R()*2-1,R()*2-1);if(point.sqrMagnitude>1){i--;continue;}point=center+Vector3.Scale(point,new Vector3(2.9f,1.75f,2.9f));
                    var rotation=Quaternion.Euler(R()*100-50,R()*360,R()*60-30);float size=(.16f+R()*.16f)*(level==0?1:level==1?1.55f:2.8f);int k=v.Count;
                    var normal=(rotation*Vector3.up+Vector3.up*.65f).normalized;
                    var leafPoints=new[]{new Vector3(-size,0,0),new Vector3(-size*.45f,.025f,size*.39f),new Vector3(size*.45f,.025f,size*.39f),new Vector3(size,0,0),new Vector3(size*.45f,-.02f,-size*.39f),new Vector3(-size*.45f,-.02f,-size*.39f)};
                    foreach(var p in leafPoints){v.Add(point+rotation*p);normals.Add(normal);}
                    for(int j=1;j<5;j++)triangles[i%2].AddRange(new[]{k,k+j,k+j+1,k+j+1,k+j,k});
                }
                var mesh=new Mesh{indexFormat=IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetNormals(normals);mesh.subMeshCount=2;mesh.SetTriangles(triangles[0],0);mesh.SetTriangles(triangles[1],1);mesh.RecalculateBounds();
                EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);existing.UploadMeshData(false);EditorUtility.SetDirty(existing);
            }
            Undo.RecordObject(leaf,"Soften canopy shade");Undo.RecordObject(leafLight,"Natural canopy variation");leaf.color=new Color(.20f,.34f,.09f);leafLight.color=new Color(.36f,.49f,.15f);
        }
        static void Pipeline()
        {
            string pipelinePath=Polish+"/Campus HQ Pipeline.asset",rendererPath=Polish+"/Campus HQ Renderer.asset";
            AssetDatabase.CopyAsset("Assets/Settings/PC_Renderer.asset",rendererPath);AssetDatabase.CopyAsset("Assets/Settings/PC_RPAsset.asset",pipelinePath);
            var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);var rendererSo=new SerializedObject(renderer);rendererSo.FindProperty("m_RenderingMode").intValue=0;rendererSo.ApplyModifiedProperties();
            foreach(var feature in renderer.rendererFeatures)
            {
                if(!(feature is ScreenSpaceAmbientOcclusion))continue;
                var settings=new SerializedObject(feature);settings.FindProperty("m_Settings.Intensity").floatValue=.75f;settings.FindProperty("m_Settings.Radius").floatValue=.35f;settings.FindProperty("m_Settings.Samples").enumValueIndex=0;settings.FindProperty("m_Settings.Downsample").boolValue=false;settings.ApplyModifiedProperties();EditorUtility.SetDirty(feature);
            }
            var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);var so=new SerializedObject(pipeline);
            so.FindProperty("m_RendererDataList").GetArrayElementAtIndex(0).objectReferenceValue=renderer;so.FindProperty("m_MSAA").intValue=4;so.FindProperty("m_RenderScale").floatValue=1.2f;so.FindProperty("m_MainLightShadowmapResolution").intValue=4096;so.ApplyModifiedProperties();
            var quality=root.gameObject.AddComponent<IUHCampus.World.IUHConceptRenderQuality>();quality.desktopPipeline=pipeline;
        }
        static void WindowVariation()
        {
            var cool=PolishMat("Sky blue window reflections",new Color(.27f,.42f,.51f),.88f);cool.SetFloat("_Metallic",.4f);
            var warm=PolishMat("Warm window reflections",new Color(.42f,.39f,.29f),.84f);warm.SetFloat("_Metallic",.3f);
            var random=new System.Random(512);
            var specs=new[]{new{p=new Vector3(0,0,66),w=54f,d=13f,h=23f,floors=5,yaw=0f},new{p=new Vector3(-47,0,43),w=48f,d=13f,h=21f,floors=5,yaw=-72f},new{p=new Vector3(49,0,39),w=52f,d=14f,h=25f,floors=6,yaw=74f}};
            foreach(var spec in specs)
            {
                var t=Group("Individual window reflections",spec.p,Quaternion.Euler(0,spec.yaw,0));
                for(int floor=0;floor<spec.floors;floor++)for(float x=-spec.w/2+2.2f;x<spec.w/2-1;x+=3.8f)
                {
                    if(random.NextDouble()>.28||(spec.yaw==0&&Mathf.Abs(x)<9))continue;
                    float y=floor*spec.h/spec.floors+2.12f,z=-spec.d/2-.38f;var material=random.NextDouble()>.4?cool:warm;
                    Q(new Vector3(x,y,z),new Vector3(2.10f,2.35f,.035f),material);
                    Q(new Vector3(x,y,z-.03f),new Vector3(.075f,2.4f,.04f),white);Q(new Vector3(x,y,z-.035f),new Vector3(2.15f,.075f,.04f),white);
                }
                Flush(t);
            }
            new GameObject("WINDOW_VARIATION_APPLIED").transform.SetParent(root,false);
        }
        [MenuItem("IUH Campus/Concept/Add Window Reflection Variation")]
        public static void AddWindowVariation()
        {
            if(Application.isPlaying||EditorSceneManager.GetActiveScene().path!=ScenePath)throw new InvalidOperationException("Concept Edit Mode required.");
            root=GameObject.Find("CAMPUS_ADVENTURE_SHARED_WORLD").transform;if(root.Find("WINDOW_VARIATION_APPLIED")!=null)throw new InvalidOperationException("Variation already applied.");
            cube=Primitive(PrimitiveType.Cube);white=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/Ivory sculpture.mat");groupId=600;
            WindowVariation();AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
    }
}
#endif
