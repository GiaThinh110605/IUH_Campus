#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.Cinemachine;

namespace IUHCampus.Editor
{
    // A separate, walkable art scene based on the campus composition supplied by the user.
    public static partial class IUHSharedCampusConceptBuilder
    {
        public const string ScenePath="Assets/IUH_Campus/Scenes/IUH_SharedCampus_Concept.unity";
        const string Folder="Assets/IUH_Campus/SharedCampusConcept";
        static Transform root;
        static Mesh cube,cylinder,sphere;
        static Material cream,stone,paving,pavingLight,brick,glass,metal,wood,lawn,hedge,leaf,leafLight,bark,water,red,purple,white,gold,blue;
        static readonly Dictionary<Material,List<CombineInstance>> parts=new Dictionary<Material,List<CombineInstance>>();
        static readonly Dictionary<string,Mesh> uvMeshes=new Dictionary<string,Mesh>();
        static int groupId;

        [MenuItem("IUH Campus/Concept/Build Shared Campus from Reference")]
        public static void Build()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Exit Play Mode first.");
            if(File.Exists(ScenePath))throw new InvalidOperationException("Concept scene already exists; edit it in place.");
            var previous=EditorSceneManager.GetActiveScene();
            if(previous.isDirty)EditorSceneManager.SaveScene(previous);
            Directory.CreateDirectory(Folder+"/Meshes");Directory.CreateDirectory(Folder+"/Materials");Directory.CreateDirectory(Folder+"/Textures");
            AssetDatabase.Refresh();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            root=new GameObject("CAMPUS_ADVENTURE_SHARED_WORLD").transform;groupId=0;parts.Clear();
            cube=Primitive(PrimitiveType.Cube);cylinder=Primitive(PrimitiveType.Cylinder);sphere=Primitive(PrimitiveType.Sphere);
            Materials();GroundAndPaths();Architecture();Fountain();Canteen();Festival();Gardens();Entrance();Details();FestivalDressing();Lighting();Player();
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,ScenePath);
            var view=SceneView.lastActiveSceneView;if(view!=null)view.LookAt(new Vector3(0,5,13),Quaternion.Euler(22,0,0),75);
            Debug.Log("Shared concept campus built: gate, axial plaza, fountain, library, canteen, dormitory, festival and gardens.");
        }
        static Mesh Primitive(PrimitiveType type){var g=GameObject.CreatePrimitive(type);var m=g.GetComponent<MeshFilter>().sharedMesh;UnityEngine.Object.DestroyImmediate(g);return m;}
        static Material Mat(string name,Color color,float smooth=.2f)
        {
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.name=name;m.color=color;m.SetFloat("_Smoothness",smooth);m.enableInstancing=true;
            AssetDatabase.CreateAsset(m,Folder+"/Materials/"+name+".mat");return m;
        }
        static void Materials()
        {
            cream=Mat("Limestone facade",new Color(.83f,.78f,.65f));stone=Mat("Warm granite",new Color(.66f,.64f,.57f));
            paving=Mat("Sandstone paving",new Color(.69f,.61f,.48f));pavingLight=Mat("Paving pale",new Color(.83f,.77f,.65f));
            brick=Mat("Terracotta brick",new Color(.62f,.32f,.22f));glass=Mat("Blue green glazing",new Color(.18f,.34f,.4f),.86f);glass.SetFloat("_Metallic",.35f);
            metal=Mat("Dark bronze",new Color(.10f,.14f,.15f),.5f);wood=Mat("Teak timber",new Color(.37f,.22f,.10f));
            lawn=Mat("Lawn",new Color(.28f,.40f,.14f));hedge=Mat("Hedge",new Color(.18f,.31f,.075f));
            leaf=Mat("Leaves shadow",new Color(.16f,.29f,.075f));leafLight=Mat("Leaves sunlight",new Color(.33f,.45f,.12f));bark=Mat("Tree bark",new Color(.28f,.22f,.15f));
            white=Mat("Ivory sculpture",new Color(.91f,.90f,.82f),.3f);gold=Mat("Antique brass",new Color(.66f,.44f,.15f),.55f);gold.SetFloat("_Metallic",.6f);
            blue=Mat("IUH blue",new Color(.035f,.19f,.32f));red=Mat("Canteen awning",new Color(.63f,.16f,.11f));purple=Mat("Festival violet",new Color(.29f,.13f,.49f));
            water=Mat("Fountain water",new Color(.12f,.53f,.65f));water.shader=Shader.Find("IUH/Concept Fountain Water");
            Texture(stone,"stone",0);Texture(cream,"limestone",0);Texture(brick,"brick",1);Texture(paving,"paving",2);Texture(pavingLight,"paving pale",2);Texture(wood,"timber",3);Texture(bark,"bark",3);Texture(lawn,"grass",4);
        }
        static void Texture(Material mat,string name,int pattern)
        {
            const int n=256;var t=new Texture2D(n,n,TextureFormat.RGB24,true);var pixels=new Color[n*n];
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                float v=.86f+Mathf.PerlinNoise(x*.24f,y*.24f)*.24f;
                if(pattern==1){int xx=(x+(y/32%2)*32)%64;if(y%32<2||xx<2)v*=.68f;}
                if(pattern==2&&(x%128<2||y%128<2))v*=.65f;
                if(pattern==3)v*=.9f+.13f*Mathf.Sin(x*.7f+Mathf.PerlinNoise(x*.02f,y*.04f)*9);
                if(pattern==4)v=.78f+Mathf.PerlinNoise(x*.12f,y*.12f)*.42f;
                pixels[y*n+x]=new Color(v,v,v);
            }
            t.SetPixels(pixels);t.Apply();string path=Folder+"/Textures/"+name+".png";File.WriteAllBytes(path,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.wrapMode=TextureWrapMode.Repeat;importer.anisoLevel=8;importer.SaveAndReimport();mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        static Transform Group(string name,Vector3 p,Quaternion? rotation=null)
        {var t=new GameObject(name+"_"+(groupId++)).transform;t.SetParent(root,false);t.localPosition=p;t.localRotation=rotation??Quaternion.identity;return t;}
        static void Add(Material material,Mesh mesh,Vector3 p,Vector3 s,Quaternion? r=null)
        {if(!parts.TryGetValue(material,out var list))parts[material]=list=new List<CombineInstance>();list.Add(new CombineInstance{mesh=mesh,transform=Matrix4x4.TRS(p,r??Quaternion.identity,s)});}
        static void Q(Vector3 p,Vector3 s,Material m)
        {
            string key=s.ToString("F3");
            if(!uvMeshes.TryGetValue(key,out var mesh))
            {
                mesh=UnityEngine.Object.Instantiate(cube);var v=mesh.vertices;var normals=mesh.normals;var uv=mesh.uv;
                for(int i=0;i<uv.Length;i++){var a=Vector3.Scale(v[i],s);var n=normals[i];uv[i]=Mathf.Abs(n.y)>.5f?new Vector2(a.x,a.z)*.5f:Mathf.Abs(n.x)>.5f?new Vector2(a.z,a.y)*.5f:new Vector2(a.x,a.y)*.5f;}mesh.uv=uv;uvMeshes[key]=mesh;
            }
            Add(m,mesh,p,s);
        }
        static void C(Vector3 p,float radius,float height,Material m){Add(m,cylinder,p,new Vector3(radius*2,height/2,radius*2));}
        static void S(Vector3 p,Vector3 size,Material m){Add(m,sphere,p,size);}
        static void Solid(Transform t,string name,Vector3 p,Vector3 size)
        {var g=new GameObject(name);g.transform.SetParent(t,false);g.transform.localPosition=p;g.AddComponent<BoxCollider>().size=size;g.isStatic=true;}
        static void Flush(Transform parent)
        {
            foreach(var pair in parts)
            {
                var mesh=new Mesh{name=parent.name+"_"+pair.Key.name,indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(pair.Value.ToArray(),true,true);
                var normals=mesh.normals;for(int i=0;i<normals.Length;i++)if(normals[i].sqrMagnitude<.01f)normals[i]=Vector3.up;mesh.normals=normals;mesh.RecalculateBounds();
                AssetDatabase.CreateAsset(mesh,Folder+"/Meshes/"+mesh.name+".asset");mesh.UploadMeshData(false);var g=new GameObject(pair.Key.name);g.transform.SetParent(parent,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=pair.Key;g.isStatic=true;
            }
            parts.Clear();
        }
        static void Text(Transform parent,string value,Vector3 p,float size,Color color,Quaternion? rot=null)
        {
            var g=new GameObject(value);g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localRotation=rot??Quaternion.identity;
            var t=g.AddComponent<TextMesh>();t.text=value;t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=90;t.characterSize=size/9;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=color;
            g.GetComponent<MeshRenderer>().sharedMaterial=t.font.material;g.AddComponent<IUHCampus.World.IUHDepthLabel>().BindShader(Shader.Find("IUH/Depth World Text"));
        }
        static void GroundAndPaths()
        {
            var t=Group("Plaza paving",Vector3.zero);Q(new Vector3(0,-.20f,18),new Vector3(145,.4f,164),lawn);Solid(t,"Campus ground",new Vector3(0,-.2f,18),new Vector3(145,.4f,164));
            Q(new Vector3(0,.015f,12),new Vector3(16,.03f,137),pavingLight);Q(new Vector3(0,.025f,10),new Vector3(59,.04f,32),paving);
            Q(new Vector3(0,.025f,43),new Vector3(112,.04f,12),pavingLight);
            foreach(int side in new[]{-1,1})
            {
                Q(new Vector3(side*38,.02f,9),new Vector3(8,.04f,91),paving);
                Q(new Vector3(side*20,.025f,-14),new Vector3(30,.04f,7),pavingLight);
                Q(new Vector3(side*20,.025f,26),new Vector3(30,.04f,6),pavingLight);
                Q(new Vector3(side*8.25f,.045f,-11),new Vector3(.25f,.08f,88),stone);
            }
            // Broad repeating stone bands establish the axial perspective.
            for(float z=-51;z<54;z+=5)Q(new Vector3(0,.04f,z),new Vector3(15.8f,.025f,.32f),stone);
            Flush(t);
        }
        static void Architecture()
        {
            Building("Central university",new Vector3(0,0,66),54,13,23,5,0,cream);
            Building("Library west wing",new Vector3(-47,0,43),48,13,21,5,-72,cream);
            Building("Dormitory east wing",new Vector3(49,0,39),52,14,25,6,74,brick);
            Building("Rear west wing",new Vector3(-41,0,69),29,12,21,5,0,cream);
            Building("Rear east wing",new Vector3(41,0,69),29,12,25,6,0,brick);
            var top=Group("Central clock tower",new Vector3(0,23,63));
            Q(new Vector3(0,4,0),new Vector3(10,8,7),cream);Q(new Vector3(0,8.2f,0),new Vector3(11,.5f,8),white);Q(new Vector3(0,8.8f,0),new Vector3(11.8f,.25f,8.6f),stone);
            for(int i=-1;i<=1;i++)Q(new Vector3(i*2.5f,3,-3.6f),new Vector3(1.7f,4.8f,.12f),glass);
            Flush(top);Text(top,"IUH",new Vector3(0,6.8f,-3.7f),1.5f,blue.color);Text(root,"ĐẠI HỌC CÔNG NGHIỆP",new Vector3(0,21.3f,58.7f),1.45f,blue.color);
            Text(root,"THƯ VIỆN",new Vector3(-41,4.2f,31),.9f,blue.color,Quaternion.Euler(0,-72,0));Text(root,"KÝ TÚC XÁ",new Vector3(42,4.2f,31),.9f,blue.color,Quaternion.Euler(0,74,0));
        }
        static void Building(string name,Vector3 position,float width,float depth,float height,int floors,float yaw,Material wall)
        {
            var t=Group(name,position,Quaternion.Euler(0,yaw,0));float front=-depth/2;
            Q(new Vector3(0,height/2,0),new Vector3(width,height,depth),wall);Solid(t,"Building shell",new Vector3(0,height/2,0),new Vector3(width,height,depth));
            Q(new Vector3(0,.20f,front-.7f),new Vector3(width+1,.4f,2),stone);
            for(int floor=0;floor<floors;floor++)
            {
                float y=floor*height/floors;
                Q(new Vector3(0,y+.15f,front-.26f),new Vector3(width+.6f,.30f,.9f),cream);
                for(float x=-width/2+2.2f;x<width/2-1;x+=3.8f)
                {
                    Q(new Vector3(x,y+2.1f,front-.12f),new Vector3(2.38f,2.65f,.16f),metal);
                    Q(new Vector3(x,y+2.12f,front-.23f),new Vector3(2.15f,2.4f,.12f),glass);
                    Q(new Vector3(x,y+2.12f,front-.32f),new Vector3(.075f,2.5f,.08f),cream);
                    Q(new Vector3(x,y+2.1f,front-.33f),new Vector3(2.25f,.075f,.08f),cream);
                    Q(new Vector3(x,y+3.5f,front-.55f),new Vector3(2.7f,.17f,1.15f),white);
                    Q(new Vector3(x,y+.7f,front-.4f),new Vector3(2.7f,.16f,.8f),stone);
                }
            }
            for(float x=-width/2;x<=width/2+.1f;x+=7.6f)Q(new Vector3(x,height/2,front-.34f),new Vector3(.32f,height,.40f),cream);
            Q(new Vector3(0,height+.22f,0),new Vector3(width+1.4f,.44f,depth+1.3f),stone);Q(new Vector3(0,height+.6f,0),new Vector3(width+.9f,.35f,depth+.9f),cream);
            Q(new Vector3(0,height+.85f,0),new Vector3(width+1.6f,.18f,depth+1.5f),white);Flush(t);
        }
        static Mesh Ring(float inner,float outer,float height,int segments=96,float startAngle=0,float sweep=360)
        {
            var vertices=new List<Vector3>();var triangles=new List<int>();
            for(int i=0;i<segments;i++)
            {
                float a=(startAngle+i*sweep/segments)*Mathf.Deg2Rad,b=(startAngle+(i+1)*sweep/segments)*Mathf.Deg2Rad;
                var p=new[]{new Vector3(Mathf.Cos(a)*inner,0,Mathf.Sin(a)*inner),new Vector3(Mathf.Cos(a)*outer,0,Mathf.Sin(a)*outer),new Vector3(Mathf.Cos(b)*outer,0,Mathf.Sin(b)*outer),new Vector3(Mathf.Cos(b)*inner,0,Mathf.Sin(b)*inner)};
                foreach(int face in new[]{0,1,2})
                {
                    int start=vertices.Count;
                    if(face==0){foreach(var v in p)vertices.Add(v+Vector3.up*height);}
                    else{int j=face==1?1:3,k=face==1?2:0;vertices.Add(p[j]);vertices.Add(p[k]);vertices.Add(p[k]+Vector3.up*height);vertices.Add(p[j]+Vector3.up*height);}
                    triangles.AddRange(new[]{start,start+2,start+1,start,start+3,start+2});
                }
            }
            var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();return mesh;
        }
        static void Fountain()
        {
            var t=Group("Central fountain and sculpture",new Vector3(0,0,10));
            C(new Vector3(0,.06f,0),8.5f,.12f,pavingLight);Add(stone,Ring(5.9f,6.5f,.70f),new Vector3(0,.1f,0),Vector3.one);Add(white,Ring(5.75f,6.65f,.17f),new Vector3(0,.78f,0),Vector3.one);
            C(new Vector3(0,.51f,0),5.9f,.05f,water);C(new Vector3(0,.65f,0),1.9f,1.3f,stone);C(new Vector3(0,1.38f,0),2.15f,.22f,white);
            for(int arm=0;arm<3;arm++)
            {
                float a=arm*Mathf.PI*2/3;var vertices=new List<Vector3>();var triangles=new List<int>();
                for(int i=0;i<=30;i++)
                {
                    float f=i/30f,r=.65f+Mathf.Sin(f*Mathf.PI)*1.35f,angle=a+f*1.5f;
                    var center=new Vector3(Mathf.Cos(angle)*r,1.5f+f*6.7f,Mathf.Sin(angle)*r);float width=(1-f)*.55f+.06f;
                    var normal=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));vertices.Add(center-normal*width);vertices.Add(center+normal*width);
                    if(i>0){int k=i*2;triangles.AddRange(new[]{k-2,k,k-1,k-1,k,k+1,k-1,k,k-2,k+1,k,k-1});}
                }
                var ribbon=new Mesh();ribbon.SetVertices(vertices);ribbon.SetTriangles(triangles,0);ribbon.RecalculateNormals();Add(arm==2?gold:white,ribbon,Vector3.zero,Vector3.one);
            }
            // Thin curved streams and droplets animate visually through the rippling water surface.
            for(int jet=0;jet<12;jet++)for(int step=0;step<18;step++)
            {
                float a=jet*Mathf.PI/6,f=step/18f,r=4.6f-f*2.5f;
                S(new Vector3(Mathf.Cos(a)*r,.6f+Mathf.Sin(f*Mathf.PI)*1.4f,Mathf.Sin(a)*r),new Vector3(.055f,.11f,.055f),white);
            }
            Flush(t);var barrier=new GameObject("Fountain basin collision");barrier.transform.SetParent(t,false);barrier.transform.localPosition=new Vector3(0,.45f,0);barrier.transform.localScale=new Vector3(13.1f,.55f,13.1f);barrier.AddComponent<MeshCollider>().sharedMesh=cylinder;barrier.isStatic=true;
        }
        static void Canteen()
        {
            Building("Canteen",new Vector3(-34,0,-10),28,10,5.2f,1,0,brick);
            var t=Group("Canteen terrace",new Vector3(-34,0,-10));
            Q(new Vector3(0,.12f,-11),new Vector3(30,.24f,13),pavingLight);
            for(int i=0;i<10;i++)Add(i%2==0?red:white,cube,new Vector3(-13.5f+i*3,3.3f,-6.9f),new Vector3(3,.14f,4.4f),Quaternion.Euler(-10,0,0));
            for(int i=-1;i<=1;i++)
            {
                float x=i*8;C(new Vector3(x,1,-12),1.3f,.12f,wood);C(new Vector3(x,.5f,-12),.1f,1,metal);
                for(int chair=0;chair<4;chair++){float a=chair*Mathf.PI/2;Q(new Vector3(x+Mathf.Cos(a)*1.8f,.55f,-12+Mathf.Sin(a)*1.8f),new Vector3(.7f,.12f,.7f),wood);}
                Umbrella(new Vector3(x,0,-12),3.2f,3.1f,red);
            }
            Flush(t);Text(t,"CĂN TIN SINH VIÊN",new Vector3(0,4.35f,-5.2f),1.1f,white.color);
        }
        static void Umbrella(Vector3 p,float height,float radius,Material color)
        {
            C(p+Vector3.up*(height/2),.045f,height,metal);
            var mesh=new Mesh();var v=new List<Vector3>();var tr=new List<int>();
            for(int i=0;i<16;i++){float a=i*Mathf.PI/8,b=(i+1)*Mathf.PI/8;int k=v.Count;v.Add(p+Vector3.up*(height+.7f));v.Add(p+new Vector3(Mathf.Cos(a)*radius,height,Mathf.Sin(a)*radius));v.Add(p+new Vector3(Mathf.Cos(b)*radius,height,Mathf.Sin(b)*radius));tr.AddRange(new[]{k,k+2,k+1});}
            mesh.SetVertices(v);mesh.SetTriangles(tr,0);mesh.RecalculateNormals();Add(color,mesh,Vector3.zero,Vector3.one);
        }
        static void Festival()
        {
            var t=Group("Campus festival",new Vector3(34,0,-8));
            Q(new Vector3(0,.35f,0),new Vector3(23,.7f,12),metal);Solid(t,"Stage platform",new Vector3(0,.35f,0),new Vector3(23,.7f,12));
            Q(new Vector3(0,4.3f,4),new Vector3(22,7.8f,.35f),purple);Q(new Vector3(0,4.4f,3.75f),new Vector3(13,5.2f,.08f),blue);
            foreach(int side in new[]{-1,1})
            {
                Q(new Vector3(side*11,4,0),new Vector3(.25f,8,.25f),metal);Q(new Vector3(side*11,4,4),new Vector3(.25f,8,.25f),metal);
                for(int y=0;y<8;y++)Add(stone,cube,new Vector3(side*11,y+.5f,2),new Vector3(.10f,.12f,4.2f),Quaternion.Euler(15,0,0));
                Q(new Vector3(side*8,1.7f,-3),new Vector3(1.5f,2.5f,1.2f),metal);
            }
            Q(new Vector3(0,8.15f,1),new Vector3(24,.35f,8),purple);Q(new Vector3(0,7.5f,-3.2f),new Vector3(18,1,.22f),white);
            for(float x=-8;x<=8;x+=4){C(new Vector3(x,6.4f,-2.4f),.3f,.4f,gold);}
            for(int i=0;i<3;i++)
            {
                var p=new Vector3(0,.12f+i*.18f,-6.8f+i*.48f);var size=new Vector3(8,.24f,.7f);Q(p,size,stone);Solid(t,"Stage access step "+i,p,size);
            }
            Umbrella(new Vector3(-15,0,-6),3.5f,3.4f,purple);Umbrella(new Vector3(15,0,-6),3.5f,3.4f,purple);
            Flush(t);Text(t,"CAMPUS FESTIVAL",new Vector3(0,7.5f,-3.4f),.9f,purple.color);Text(t,"IUH • MUSIC & CULTURE",new Vector3(0,4.5f,3.62f),.6f,white.color);
        }
        static void Entrance()
        {
            var t=Group("University main gate",new Vector3(0,0,-47));
            foreach(int side in new[]{-1,1})
            {
                Q(new Vector3(side*10,2.8f,0),new Vector3(1.4f,5.6f,1.5f),cream);Q(new Vector3(side*10,5.7f,0),new Vector3(1.9f,.32f,2),white);
                Q(new Vector3(side*17,1.2f,0),new Vector3(13,2.4f,.5f),stone);
                for(float x=11.3f;x<29;x+=.5f)Q(new Vector3(side*x,2,0),new Vector3(.075f,2.3f,.075f),metal);
                Solid(t,"Gate pillar",new Vector3(side*10,2.8f,0),new Vector3(1.4f,5.6f,1.5f));
            }
            Q(new Vector3(0,5.2f,0),new Vector3(20,1.05f,1.0f),cream);Q(new Vector3(0,5.22f,-.56f),new Vector3(15.5f,.72f,.12f),blue);
            Flush(t);Text(t,"CỔNG TRƯỜNG",new Vector3(0,5.22f,-.64f),1.05f,white.color);
        }
        static void Gardens()
        {
            var t=Group("Garden beds and furniture",Vector3.zero);
            foreach(int side in new[]{-1,1})foreach(float z in new[]{-29f,-1f,29f})
            {
                float x=side*20;Q(new Vector3(x,.16f,z),new Vector3(17,.32f,13),stone);Q(new Vector3(x,.34f,z),new Vector3(16.4f,.04f,12.4f),lawn);
                foreach(int edge in new[]{-1,1})for(float k=-7;k<=7;k+=.65f)S(new Vector3(x+k,.65f,z+edge*5.6f),new Vector3(.9f,.6f,.85f),hedge);
                Bench(new Vector3(side*11,.05f,z-4),side<0?90:-90);
            }
            foreach(float x in new[]{-11f,11f})foreach(float z in new[]{-38f,-19f,2f,24f,46f})Lamp(new Vector3(x,0,z));
            Flush(t);
            foreach(int side in new[]{-1,1})foreach(float z in new[]{-34f,-4f,29f,52f})Tree(new Vector3(side>0&&z< -30?31:side*23,.32f,z),1.0f+(z>30?.15f:0));
            foreach(int side in new[]{-1,1})foreach(float z in new[]{-42f,-18f,10f,40f})Tree(new Vector3(side*59,0,z),1.25f);
            // Entrance foreground trees frame the composition without blocking the central path.
            Tree(new Vector3(-20,0,-48),1.35f);Tree(new Vector3(32,0,-47),1.3f);
        }
        static void Bench(Vector3 p,float yaw)
        {
            var r=Quaternion.Euler(0,yaw,0);
            for(int slat=0;slat<5;slat++)Add(wood,cube,p+r*new Vector3(0,.55f,-.3f+slat*.15f),new Vector3(2.8f,.08f,.11f),r);
            for(int slat=0;slat<3;slat++)Add(wood,cube,p+r*new Vector3(0,.8f+slat*.17f,.36f),new Vector3(2.8f,.11f,.07f),r);
            foreach(int side in new[]{-1,1})Add(metal,cube,p+r*new Vector3(side*1,.3f,0),new Vector3(.1f,.6f,.65f),r);
        }
        static void Lamp(Vector3 p)
        {C(p+Vector3.up*2.3f,.065f,4.6f,metal);Q(p+Vector3.up*4.65f,new Vector3(.55f,.55f,.55f),gold);Q(p+Vector3.up*5,new Vector3(.8f,.16f,.8f),metal);}
        static readonly List<Mesh[]> treeTemplates=new List<Mesh[]>();
        static void Tree(Vector3 p,float scale)
        {
            int variant=groupId%3;
            while(treeTemplates.Count<=variant)treeTemplates.Add(TreeMeshes(treeTemplates.Count));
            var t=Group("Shade tree",p,Quaternion.Euler(0,groupId*47,0));t.localScale=Vector3.one*scale;
            C(new Vector3(0,2.1f,0),.23f,4.2f,bark);
            for(int arm=0;arm<7;arm++)Add(bark,cylinder,new Vector3(Mathf.Sin(arm)*.9f,4.1f,Mathf.Cos(arm)*.9f),new Vector3(.18f,1.5f,.18f),Quaternion.Euler(arm*9,arm*51,40));
            Flush(t);var trunk=t.gameObject.AddComponent<CapsuleCollider>();trunk.height=5;trunk.radius=.28f;trunk.center=Vector3.up*2.5f;
            var lods=new LOD[3];for(int level=0;level<3;level++)
            {
                var g=new GameObject("Leaves LOD "+level);g.transform.SetParent(t,false);g.AddComponent<MeshFilter>().sharedMesh=treeTemplates[variant][level];var renderer=g.AddComponent<MeshRenderer>();renderer.sharedMaterials=new[]{leaf,leafLight};renderer.shadowCastingMode=level<2?ShadowCastingMode.On:ShadowCastingMode.Off;g.isStatic=true;lods[level]=new LOD(level==0?.24f:level==1?.08f:.015f,new Renderer[]{renderer});
            }
            var group=t.gameObject.AddComponent<LODGroup>();group.SetLODs(lods);group.RecalculateBounds();
        }
        static Mesh[] TreeMeshes(int seed)
        {
            var result=new Mesh[3];
            for(int level=0;level<3;level++)
            {
                var random=new System.Random(941+seed);float R()=>(float)random.NextDouble();
                var vertices=new List<Vector3>();var normals=new List<Vector3>();var triangles=new[]{new List<int>(),new List<int>()};int count=level==0?3400:level==1?1100:260;
                for(int i=0;i<count;i++)
                {
                    int clump=i%7;float ca=clump*Mathf.PI*2/7;var center=new Vector3(Mathf.Cos(ca)*2.2f,6.2f+(clump%3)*.55f,Mathf.Sin(ca)*2.2f);
                    var randomPoint=new Vector3(R()*2-1,R()*2-1,R()*2-1);if(randomPoint.sqrMagnitude>1){i--;continue;}var p=center+Vector3.Scale(randomPoint,new Vector3(3,1.8f,3));
                    var rotation=Quaternion.Euler(R()*150,R()*360,R()*90);float size=(.22f+R()*.2f)*(level==0?1:level==1?1.45f:2.4f);int k=vertices.Count;
                    foreach(var v in new[]{new Vector3(-size,0,0),new Vector3(0,.035f,size*.48f),new Vector3(size,0,0),new Vector3(0,-.035f,-size*.48f)}){vertices.Add(p+rotation*v);normals.Add(rotation*Vector3.up);}
                    triangles[i%2].AddRange(new[]{k,k+1,k+2,k,k+2,k+3,k+2,k+1,k,k+3,k+2,k});
                }
                var mesh=new Mesh{name="Broadleaf_"+seed+"_LOD"+level,indexFormat=IndexFormat.UInt32};mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.subMeshCount=2;mesh.SetTriangles(triangles[0],0);mesh.SetTriangles(triangles[1],1);mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,Folder+"/Meshes/"+mesh.name+".asset");mesh.UploadMeshData(false);result[level]=mesh;
            }
            return result;
        }
        static void Lighting()
        {
            var go=new GameObject("Warm afternoon sun");go.transform.SetParent(root,false);go.transform.rotation=Quaternion.Euler(43,-32,0);var sun=go.AddComponent<Light>();sun.type=LightType.Directional;sun.color=new Color(1,.94f,.81f);sun.intensity=1.8f;sun.shadows=LightShadows.Soft;sun.shadowStrength=.82f;sun.shadowBias=.035f;sun.shadowNormalBias=.28f;RenderSettings.sun=sun;
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.51f,.64f,.78f);RenderSettings.ambientEquatorColor=new Color(.40f,.46f,.48f);RenderSettings.ambientGroundColor=new Color(.26f,.28f,.21f);
            var sky=new Material(Shader.Find("IUH/Concept Cloud Sky"));sky.name="Blue sky with clouds";AssetDatabase.CreateAsset(sky,Folder+"/Materials/Cloud sky.mat");RenderSettings.skybox=sky;
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogColor=new Color(.73f,.82f,.85f);RenderSettings.fogStartDistance=125;RenderSettings.fogEndDistance=270;
            var volume=new GameObject("Campus color grading").AddComponent<Volume>();volume.isGlobal=true;var profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,Folder+"/Campus volume.asset");volume.sharedProfile=profile;
            profile.Add<Tonemapping>().mode.Override(TonemappingMode.ACES);var color=profile.Add<ColorAdjustments>();color.contrast.Override(8);color.saturation.Override(6);var bloom=profile.Add<Bloom>();bloom.intensity.Override(.10f);bloom.threshold.Override(1.4f);foreach(var c in profile.components)AssetDatabase.AddObjectToAsset(c,profile);
        }
        static void Player()
        {
            const string folder="Assets/Starter Assets/Runtime/ThirdPersonController/Prefabs/";
            var player=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(folder+"PlayerArmature.prefab"));player.transform.position=new Vector3(0,.2f,-38);player.name="Campus Student";
            var main=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(folder+"MainCamera.prefab"));
            var follow=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(folder+"PlayerFollowCamera.prefab"));
            var controller=player.GetComponent<StarterAssets.ThirdPersonController>();var camera=follow.GetComponent<CinemachineCamera>();camera.Follow=controller.CinemachineCameraTarget.transform;
            controller.SetCameraOrientation(Vector3.forward);
            var data=main.GetComponent<UniversalAdditionalCameraData>();if(data!=null){data.renderPostProcessing=true;data.antialiasing=AntialiasingMode.FastApproximateAntialiasing;}
            StyleStudent(player);
            var poster=new GameObject("Reference composition camera").AddComponent<Camera>();poster.transform.position=new Vector3(13,9,-56);poster.transform.LookAt(new Vector3(6,5,25));poster.fieldOfView=65;poster.farClipPlane=350;poster.enabled=false;var posterData=poster.gameObject.AddComponent<UniversalAdditionalCameraData>();posterData.renderPostProcessing=true;posterData.antialiasing=AntialiasingMode.FastApproximateAntialiasing;
            player.AddComponent<IUHCampus.World.IUHConceptPreviewController>().overviewCamera=poster;
        }

        static void Details()
        {
            var portico=Group("University entrance portico",new Vector3(0,0,57));
            Q(new Vector3(0,6.5f,0),new Vector3(16,.55f,5.5f),cream);Q(new Vector3(0,7,0),new Vector3(17,.3f,6),white);
            foreach(float x in new[]{-6.5f,-3.3f,3.3f,6.5f})C(new Vector3(x,3,-1.4f),.26f,6,white);
            for(int step=0;step<3;step++)Q(new Vector3(0,.12f+step*.18f,-3.4f+step*.55f),new Vector3(17,.24f,1.2f),stone);
            Q(new Vector3(0,3.5f,2.2f),new Vector3(6.5f,6.5f,.15f),glass);Flush(portico);
            var curves=Group("Curved fountain gardens",new Vector3(0,0,10));
            foreach(float start in new[]{-36f,144f})
            {
                Add(stone,Ring(8.8f,12.2f,.25f,32,start,72),Vector3.zero,Vector3.one);
                Add(lawn,Ring(9.05f,11.95f,.28f,32,start+1,70),Vector3.zero,Vector3.one);
                for(int i=0;i<=30;i++)
                {
                    float a=(start+i*72/30f)*Mathf.Deg2Rad;
                    S(new Vector3(Mathf.Cos(a)*11.6f,.51f,Mathf.Sin(a)*11.6f),new Vector3(.65f,.5f,.6f),hedge);
                    S(new Vector3(Mathf.Cos(a)*9.4f,.42f,Mathf.Sin(a)*9.4f),new Vector3(.18f,.16f,.18f),gold);
                }
            }
            Flush(curves);
            var skin=Mat("Student skin",new Color(.64f,.43f,.28f));
            var people=Group("Students in courtyard",Vector3.zero);
            var points=new[]{new Vector3(-6,0,-9),new Vector3(6,0,32),new Vector3(-28,0,-20),new Vector3(29,0,-22),new Vector3(-35,0,17),new Vector3(15,0,34),new Vector3(-6,0,48)};
            foreach(var p in points)
            {
                Q(p+Vector3.up*1.08f,new Vector3(.46f,.65f,.26f),blue);S(p+Vector3.up*1.62f,new Vector3(.30f,.38f,.30f),skin);
                foreach(int side in new[]{-1,1}){C(p+new Vector3(side*.13f,.42f,0),.075f,.8f,metal);C(p+new Vector3(side*.30f,1.03f,0),.055f,.65f,cream);}
            }
            Flush(people);
            new GameObject("REFERENCE_DETAILS_APPLIED").transform.SetParent(root,false);
        }
        static void StyleStudent(GameObject player)
        {
            var jacket=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/Student jacket.mat")??Mat("Student jacket",new Color(.10f,.23f,.20f));
            foreach(var renderer in player.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var slots=renderer.sharedMaterials;for(int i=0;i<slots.Length;i++)slots[i]=jacket;renderer.sharedMaterials=slots;
            }
            var animator=player.GetComponent<Animator>();var head=animator!=null&&animator.isHuman?animator.GetBoneTransform(HumanBodyBones.Head):null;
            if(head!=null&&head.Find("Student head")==null)
            {
                var skin=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/Student skin.mat");
                var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name="Student head";UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.transform.SetParent(head,false);g.transform.localPosition=new Vector3(0,.09f,0);g.transform.localScale=new Vector3(.28f,.34f,.28f);g.GetComponent<MeshRenderer>().sharedMaterial=skin;
                var hair=GameObject.CreatePrimitive(PrimitiveType.Sphere);hair.name="Student hair";UnityEngine.Object.DestroyImmediate(hair.GetComponent<Collider>());hair.transform.SetParent(head,false);hair.transform.localPosition=new Vector3(0,.20f,-.015f);hair.transform.localScale=new Vector3(.30f,.16f,.29f);hair.GetComponent<MeshRenderer>().sharedMaterial=metal;
            }
            var chest=animator!=null&&animator.isHuman?animator.GetBoneTransform(HumanBodyBones.Chest):null;
            if(chest!=null&&chest.Find("Student backpack")==null)
            {
                var pack=GameObject.CreatePrimitive(PrimitiveType.Cube);pack.name="Student backpack";UnityEngine.Object.DestroyImmediate(pack.GetComponent<Collider>());pack.transform.SetParent(chest,false);pack.transform.localPosition=new Vector3(0,0,-.16f);pack.transform.localScale=new Vector3(.30f,.43f,.19f);pack.GetComponent<MeshRenderer>().sharedMaterial=jacket;
            }
        }
        static void FestivalDressing()
        {
            var t=Group("Festival pavilions and banners",Vector3.zero);
            foreach(float x in new[]{19f,49f})
            {
                var p=new Vector3(x,0,-20);C(p+Vector3.up*1.65f,.045f,3.3f,metal);
                for(int panel=0;panel<8;panel++)
                {
                    float a=panel*Mathf.PI/4,b=(panel+1)*Mathf.PI/4;
                    var mesh=new Mesh();mesh.vertices=new[]{p+Vector3.up*4.5f,p+new Vector3(Mathf.Cos(a)*3.4f,3.1f,Mathf.Sin(a)*3.4f),p+new Vector3(Mathf.Cos(b)*3.4f,3.1f,Mathf.Sin(b)*3.4f)};mesh.triangles=new[]{0,2,1};mesh.RecalculateNormals();
                    Add(panel%2==0?purple:white,mesh,Vector3.zero,Vector3.one);
                }
                Q(p+new Vector3(0,.8f,1),new Vector3(4.5f,1.6f,1.0f),purple);Q(p+new Vector3(0,1.63f,1),new Vector3(4.8f,.12f,1.3f),white);
            }
            foreach(int side in new[]{-1,1})foreach(float z in new[]{-26f,0f,31f})
            {
                C(new Vector3(side*8.5f,2.3f,z),.045f,4.6f,metal);Q(new Vector3(side*8.5f,3.1f,z),new Vector3(.85f,2.1f,.06f),side<0?blue:purple);
                Text(t,"IUH",new Vector3(side*8.5f,3.35f,z-.05f),.32f,white.color);
            }
            var glow=Mat("Festival LED",new Color(.25f,.08f,.42f));glow.EnableKeyword("_EMISSION");glow.SetColor("_EmissionColor",new Color(.35f,.04f,.7f)*2.2f);
            for(int i=0;i<12;i++)
            {
                float h=1.1f+Mathf.Sin(i*1.9f)*.8f;
                Q(new Vector3(28.5f+i,2.2f+h/2,-4.4f),new Vector3(.68f,h,.045f),glow);
            }
            Flush(t);new GameObject("FESTIVAL_DRESSING_APPLIED").transform.SetParent(root,false);
        }
        [MenuItem("IUH Campus/Concept/Add Festival Dressing")]
        public static void AddFestivalDressing()
        {
            if(Application.isPlaying||EditorSceneManager.GetActiveScene().path!=ScenePath)throw new InvalidOperationException("Concept Edit Mode required.");
            root=GameObject.Find("CAMPUS_ADVENTURE_SHARED_WORLD").transform;if(root.Find("FESTIVAL_DRESSING_APPLIED")!=null)throw new InvalidOperationException("Dressing already exists.");
            groupId=200;cube=Primitive(PrimitiveType.Cube);cylinder=Primitive(PrimitiveType.Cylinder);sphere=Primitive(PrimitiveType.Sphere);
            Material M(string name)=>AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/"+name+".mat");
            white=M("Ivory sculpture");metal=M("Dark bronze");purple=M("Festival violet");blue=M("IUH blue");FestivalDressing();AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
        [MenuItem("IUH Campus/Concept/Apply Reference Details")]
        public static void ApplyDetails()
        {
            if(Application.isPlaying||EditorSceneManager.GetActiveScene().path!=ScenePath)throw new InvalidOperationException("Open concept in Edit Mode.");
            root=GameObject.Find("CAMPUS_ADVENTURE_SHARED_WORLD").transform;
            if(root.Find("REFERENCE_DETAILS_APPLIED")!=null)throw new InvalidOperationException("Details already applied.");
            groupId=100;cube=Primitive(PrimitiveType.Cube);cylinder=Primitive(PrimitiveType.Cylinder);sphere=Primitive(PrimitiveType.Sphere);
            Material M(string name)=>AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/"+name+".mat");
            cream=M("Limestone facade");stone=M("Warm granite");white=M("Ivory sculpture");glass=M("Blue green glazing");lawn=M("Lawn");hedge=M("Hedge");gold=M("Antique brass");blue=M("IUH blue");metal=M("Dark bronze");
            Details();StyleStudent(GameObject.Find("Campus Student"));
            var poster=GameObject.Find("Reference composition camera").GetComponent<Camera>();poster.transform.position=new Vector3(13,9,-56);poster.transform.LookAt(new Vector3(0,6,29));
            RenderSettings.skybox=M("Cloud sky");RenderSettings.fog=true;
            foreach(var f in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None))if(f.sharedMesh!=null)f.sharedMesh.UploadMeshData(false);
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
    }
}
#endif
