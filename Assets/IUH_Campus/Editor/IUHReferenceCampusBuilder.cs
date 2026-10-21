#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using IUHCampus.World;
using IUHCampus.Minigames.GreenCampus;
using IUHCampus.Minigames.PhotoHunter;

namespace IUHCampus.Editor
{
    /// <summary>Exterior interpretation of the supplied illustrated plan, not a measured survey.</summary>
    public static partial class IUHReferenceCampusBuilder
    {
        const string Folder = "Assets/IUH_Campus/ReferenceLayout";
        const string ScenePath = "Assets/IUH_Campus/Scenes/IUH_Campus_Main.unity";
        static Mesh cube, sphere, cylinder;
        static Material ivory, stone, blue, teal, roof, glass, dark, paving, asphalt, grass, leaf, leafLight, bark, white, water, gold;
        static Transform root;
        static string meshFolderOverride;
        static readonly List<string> built = new List<string>();
        static readonly Dictionary<Material, List<CombineInstance>> pieces = new Dictionary<Material, List<CombineInstance>>();

        [MenuItem("IUH Campus/Reference Layout/Build Illustrated Exterior")]
        public static void Build()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (Application.isPlaying || scene.path != ScenePath) throw new InvalidOperationException("Open the main scene in Edit Mode.");
            if (GameObject.Find(IUHReferenceLayout.RootName) != null) throw new InvalidOperationException("Layout already exists. Edit its building groups in place; a rebuild is deliberately blocked.");
            Directory.CreateDirectory("Docs/Backups");
            string backup = "Docs/Backups/IUH_Campus_Main_before_layout_20261021.unity";
            if (!File.Exists(backup) && !EditorSceneManager.SaveScene(scene, backup, true)) throw new IOException("Scene backup failed.");
            Directory.CreateDirectory(Folder + "/Meshes");
            Directory.CreateDirectory(Folder + "/Materials");
            AssetDatabase.Refresh();
            CacheMeshes(); Materials(); built.Clear();
            var old = GameObject.Find("campus_P0_progress");
            if (old == null) throw new InvalidOperationException("Original campus missing.");
            Undo.RecordObject(old, "Preserve original campus inactive"); old.SetActive(false);
            foreach (var name in new[] { "Block_F_Stair_MidBox_Fl1", "Block_F_Stair_FloorBox_Fl2" })
            { var obj = GameObject.Find(name); if (obj != null) { Undo.RecordObject(obj, "Archive detached legacy collider"); obj.SetActive(false); } }
            root = new GameObject(IUHReferenceLayout.RootName).transform;
            Undo.RegisterCreatedObjectUndo(root.gameObject, "Build reference campus exterior");
            GroundAndStreets();
            var a = Building("A", 425, -40, 17, 53, 15, 4, blue, "THƯ VIỆN  •  NHÀ A");
            // Two long wings and recessed central roof court distinguish A/A7.
            Box(a, "Roof court", new Vector3(0,15.72f,0), new Vector3(4,.12f,39), dark);
            Building("A7", 443, -40, 10, 43, 12.4f, 3, teal, "HỘI TRƯỜNG A7");
            Building("A_Link", 436, -61, 12, 8, 12.4f, 3, blue, "", false);
            Building("H", 432, -83, 30, 20, 12, 3, teal, "NHÀ H");
            Building("B", 432, -122, 26, 40, 18, 5, blue, "GIẢNG ĐƯỜNG B");
            Building("B_Wing", 420, -145, 24, 15, 18, 5, blue, "", false);
            Building("V", 393, -158, 28, 29, 20, 5, teal, "GIẢNG ĐƯỜNG V");
            Building("X", 355, -155, 37, 36, 19, 5, gold, "GIẢNG ĐƯỜNG X");
            Building("T", 317, -126, 20, 54, 15, 4, blue, "NHÀ T");
            Building("K", 314, -172, 26, 24, 14, 4, teal, "NHÀ K");
            Building("D", 344, -104, 26, 21, 13, 3, teal, "NHÀ VĂN HÓA SINH VIÊN");
            var e = Building("E", 385, -104, 35, 24, 14, 4, blue, "NHÀ ĐIỀU HÀNH E");
            EntranceCanopy(e, 35, 24, true);
            Building("F", 326, -79, 47, 17, 13, 3, blue, "GIẢNG ĐƯỜNG F");
            Building("C", 349, -43, 12, 32, 12, 3, gold, "NHÀ C");
            Building("I", 316, -49, 30, 31, 20, 5, teal, "KÝ TÚC XÁ I");
            Building("G", 324, -18, 50, 19, 17, 4, blue, "KÝ TÚC XÁ G");
            Parking(); Canteen(); Landscape(); GatesAndFence(); Environment(); RelinkGames();
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save the layout scene.");
            Selection.activeGameObject = root.gameObject;
            if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.LookAt(new Vector3(373,0,-94), Quaternion.Euler(57,180,0), 190);
            Debug.Log("[Reference Campus] Saved illustrated exterior, 13 blocks + A7, parking, five gates, four streets; legacy campus preserved inactive.");
        }

        static void CacheMeshes()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); cube = go.GetComponent<MeshFilter>().sharedMesh; UnityEngine.Object.DestroyImmediate(go);
            go = GameObject.CreatePrimitive(PrimitiveType.Sphere); sphere = go.GetComponent<MeshFilter>().sharedMesh; UnityEngine.Object.DestroyImmediate(go);
            go = GameObject.CreatePrimitive(PrimitiveType.Cylinder); cylinder = go.GetComponent<MeshFilter>().sharedMesh; UnityEngine.Object.DestroyImmediate(go);
        }
        static Material Mat(string name, Color color, float smooth = .15f)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("Project URP Lit shader missing.");
            string path = Folder + "/Materials/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, path); }
            mat.color = color; mat.SetFloat("_Smoothness", smooth); EditorUtility.SetDirty(mat); return mat;
        }
        static void Materials()
        {
            ivory=Mat("Ivory",new Color(.84f,.83f,.75f)); stone=Mat("Stone",new Color(.55f,.61f,.61f));
            blue=Mat("IUH_Blue",new Color(.055f,.25f,.43f)); teal=Mat("Teal",new Color(.13f,.42f,.40f));
            roof=Mat("Roof",new Color(.57f,.65f,.68f)); glass=Mat("Glazing",new Color(.09f,.27f,.34f),.55f);
            dark=Mat("DarkFrame",new Color(.07f,.12f,.16f)); paving=Mat("Paving",new Color(.68f,.69f,.64f));
            asphalt=Mat("Asphalt",new Color(.20f,.25f,.29f)); grass=Mat("Lawn",new Color(.30f,.46f,.24f));
            leaf=Mat("Canopy_Dark",new Color(.14f,.35f,.18f)); leafLight=Mat("Canopy_Light",new Color(.30f,.49f,.18f));
            bark=Mat("Trunk",new Color(.29f,.21f,.14f)); white=Mat("White",new Color(.91f,.93f,.89f));
            water=Mat("Water",new Color(.11f,.52f,.65f),.75f); gold=Mat("WarmAccent",new Color(.68f,.45f,.14f));
        }
        static Transform Group(string name, Transform parent, Vector3 position)
        { var g=new GameObject(name); g.transform.SetParent(parent,false); g.transform.localPosition=position; return g.transform; }
        static void Add(Material material, Mesh mesh, Vector3 pos, Vector3 size, Quaternion rotation)
        {
            if (!pieces.TryGetValue(material,out var list)) pieces[material]=list=new List<CombineInstance>();
            list.Add(new CombineInstance { mesh=mesh, transform=Matrix4x4.TRS(pos,rotation,size) });
        }
        static void Q(Vector3 p,Vector3 size,Material mat) { Add(mat,cube,p,size,Quaternion.identity); }
        static void S(Vector3 p,Vector3 size,Material mat) { Add(mat,sphere,p,size,Quaternion.identity); }
        static void Cy(Vector3 p,Vector3 size,Material mat) { Add(mat,cylinder,p,size,Quaternion.identity); }
        static void Flush(Transform parent)
        {
            foreach(var pair in pieces)
            {
                var mesh=new Mesh { name=parent.name+"_"+pair.Key.name,indexFormat=IndexFormat.UInt32 };
                mesh.CombineMeshes(pair.Value.ToArray(),true,true); mesh.RecalculateBounds();
                string path=(meshFolderOverride??Folder+"/Meshes")+"/"+mesh.name+".asset";
                var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(existing==null) AssetDatabase.CreateAsset(mesh,path); else
                {
                    EditorUtility.CopySerialized(mesh,existing); UnityEngine.Object.DestroyImmediate(mesh); mesh=existing;
                    // Refresh native vertex/index buffers after an in-place mesh asset update.
                    mesh.UploadMeshData(false);EditorUtility.SetDirty(mesh);
                }
                var go=new GameObject(pair.Key.name);go.transform.SetParent(parent,false);
                go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=pair.Key;
                go.isStatic=true;
            }
            pieces.Clear();
        }
        static void Solid(Transform parent,string name,Vector3 center,Vector3 size)
        { var t=Group(name,parent,center); var c=t.gameObject.AddComponent<BoxCollider>();c.size=size; t.gameObject.isStatic=true; }
        static void Box(Transform parent,string name,Vector3 p,Vector3 size,Material mat,bool collide=false)
        {
            var t=Group(name,parent,p);Q(Vector3.zero,size,mat);Flush(t);if(collide)Solid(t,"Collision",Vector3.zero,size);
        }
        static TextMesh Text(Transform parent,string name,string value,Vector3 position,float height,Quaternion rotation,Color color)
        {
            var t=Group(name,parent,position);t.localRotation=rotation;
            var text=t.gameObject.AddComponent<TextMesh>();text.text=value;text.fontSize=90;text.characterSize=height/9;
            text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.color=color;
            text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.GetComponent<MeshRenderer>().sharedMaterial=text.font.material;
            text.gameObject.AddComponent<IUHDepthLabel>().BindShader(Shader.Find("IUH/Depth World Text"));return text;
        }
        static Transform Building(string id,float x,float z,float w,float d,float h,int floors,Material accent,string title,bool label=true)
        {
            var b=Group("Block_"+id,root,new Vector3(x,.49f,z)); built.Add(id);
            Q(new Vector3(0,.18f,0),new Vector3(w+.8f,.36f,d+.8f),stone);
            Q(new Vector3(0,h/2,0),new Vector3(w,h,d),ivory);
            Q(new Vector3(0,h+.16f,0),new Vector3(w+.7f,.32f,d+.7f),roof);
            // Roof parapets and cornices keep every silhouette legible in the overview.
            foreach(int sign in new[]{-1,1})
            {
                Q(new Vector3(0,h+.65f,sign*d/2),new Vector3(w+.6f,1.0f,.32f),accent);
                Q(new Vector3(sign*w/2,h+.65f,0),new Vector3(.32f,1,d),accent);
            }
            float fh=h/floors;
            for(int fl=0;fl<floors;fl++)
            {
                float y=fl*fh+fh*.53f;
                foreach(int sign in new[]{-1,1})
                {
                    Q(new Vector3(0,fl*fh+.16f,sign*(d/2+.10f)),new Vector3(w+.35f,.24f,.28f),stone);
                    for(float u=-w/2+2;u<w/2-1;u+=3.3f)
                    {
                        Q(new Vector3(u,y,sign*(d/2+.025f)),new Vector3(2.25f,fh*.48f,.12f),glass);
                        Q(new Vector3(u,y,sign*(d/2+.12f)),new Vector3(.08f,fh*.49f,.17f),ivory);
                        Q(new Vector3(u,y+fh*.28f,sign*(d/2+.40f)),new Vector3(2.65f,.16f,.86f),accent);
                    }
                    Q(new Vector3(sign*(w/2+.10f),fl*fh+.16f,0),new Vector3(.28f,.24f,d+.35f),stone);
                    for(float v=-d/2+2;v<d/2-1;v+=3.3f)
                    {
                        Q(new Vector3(sign*(w/2+.025f),y,v),new Vector3(.12f,fh*.48f,2.25f),glass);
                        Q(new Vector3(sign*(w/2+.40f),y+fh*.28f,v),new Vector3(.86f,.16f,2.65f),accent);
                    }
                }
            }
            for(float u=-w/2+.3f;u<=w/2;u+=w/4)
                foreach(int sign in new[]{-1,1})Q(new Vector3(u,h/2,sign*(d/2+.18f)),new Vector3(.34f,h,.38f),ivory);
            // Recessed glass entry under a light canopy, with paired columns.
            Q(new Vector3(0,1.8f,d/2+.09f),new Vector3(Mathf.Min(6,w*.45f),3.5f,.2f),glass);
            Q(new Vector3(0,3.5f,d/2+1.4f),new Vector3(Mathf.Min(9,w*.65f),.28f,3),accent);
            foreach(int sign in new[]{-1,1})Q(new Vector3(sign*Mathf.Min(3.5f,w*.25f),1.7f,d/2+2.2f),new Vector3(.24f,3.4f,.24f),stone);
            // Rooftop service boxes, separated from the clean central roof panel.
            foreach(int sign in new[]{-1,1})
            {
                Q(new Vector3(sign*(w/2-3),h+.65f,-d/2+3),new Vector3(3.2f,1,2.5f),stone);
                for(int j=0;j<4;j++)Q(new Vector3(sign*(w/2-3),h+1.17f,-d/2+2.15f+j*.5f),new Vector3(2.8f,.05f,.10f),dark);
            }
            if(label)Q(new Vector3(0,h+.40f,0),new Vector3(Mathf.Min(w-3,19),.15f,5.6f),accent);
            Flush(b);
            Solid(b,"Exterior volume",new Vector3(0,h/2,0),new Vector3(w,h,d));
            if(label)
            {
                Text(b,"Roof ID","TÒA "+id,new Vector3(0,h+.51f,0),2.6f,Quaternion.Euler(90,180,0),Color.white);
                Box(b,"Entry plaque",new Vector3(0,4.3f,d/2+.28f),new Vector3(Mathf.Min(w-2,16),1.25f,.15f),accent);
                Text(b,"Entry name",title,new Vector3(0,4.3f,d/2+.38f),.8f,Quaternion.Euler(0,180,0),Color.white);
            }
            return b;
        }
        static void EntranceCanopy(Transform b,float w,float d,bool curved)
        {
            // A shallow fan-shaped porch, a distinctive cue from the reference's E entrance.
            for(int i=0;i<9;i++)
            {
                float angle=(-65+i*16.25f)*Mathf.Deg2Rad;
                Add(blue,cube,new Vector3(Mathf.Sin(angle)*4.6f,4, d/2+Mathf.Cos(angle)*3.5f),new Vector3(1.6f,.23f,3),Quaternion.Euler(0,-65+i*16.25f,0));
            }
            Flush(Group("Fan entrance canopy",b,Vector3.zero));
        }
        static void Parking()
        {
            var p=Group("Central_Parking",root,new Vector3(382,.49f,-40));
            // Open ground floor, wide low podium and long raised bicycle terrace.
            Q(new Vector3(0,.12f,0),new Vector3(39,.24f,56),stone);
            Q(new Vector3(0,3.5f,0),new Vector3(40,.42f,57),roof);
            Q(new Vector3(0,6.7f,-12),new Vector3(40,.42f,33),roof);
            foreach(int s in new[]{-1,1})
            {
                Q(new Vector3(s*19.4f,4.2f,0),new Vector3(.25f,1.1f,56),blue);
                Q(new Vector3(s*19.4f,7.4f,-12),new Vector3(.25f,1.1f,33),blue);
                for(float v=-26;v<=26;v+=6.5f)
                {
                    Q(new Vector3(s*18.5f,1.8f,v),new Vector3(.6f,3.6f,.6f),ivory);
                    Solid(p,"Podium column",new Vector3(s*18.5f,1.8f,v),new Vector3(.6f,3.6f,.6f));
                }
                Q(new Vector3(0,4.2f,s*27.7f),new Vector3(39,1.1f,.25f),blue);
            }
            for(float u=-17;u<=17;u+=3.4f)for(float v=-23;v<=22;v+=8)
            {
                Q(new Vector3(u,3.73f,v),new Vector3(.10f,.03f,4),white);
                Q(new Vector3(u,3.98f,v+1),new Vector3(.34f,.48f,1.5f),dark);
                Q(new Vector3(u,4.3f,v+1),new Vector3(.66f,.12f,.18f),stone);
            }
            Q(new Vector3(0,6.97f,-12),new Vector3(26,.08f,9),blue);
            Flush(p);
            Solid(p,"Lower terrace",new Vector3(0,3.5f,0),new Vector3(40,.42f,57));
            Solid(p,"Upper terrace",new Vector3(0,6.7f,-12),new Vector3(40,.42f,33));
            Text(p,"Roof parking label","SÂN XE ĐẠP\nBÃI XE TRUNG TÂM",new Vector3(0,7.04f,-12),1.9f,Quaternion.Euler(90,180,0),Color.white);
            Box(p,"Front parking plaque",new Vector3(0,2.1f,28.05f),new Vector3(25,1.2f,.18f),blue);
            Text(p,"Parking entry label","BÃI XE TRUNG TÂM",new Vector3(0,2.1f,28.17f),1f,Quaternion.Euler(0,180,0),Color.white);
            // Separate accessible ramp on the rear edge. Ground routes remain unobstructed.
            var ramp=Group("Bicycle terrace ramp",p,new Vector3(13,1.8f,-34));
            ramp.localRotation=Quaternion.Euler(-12,0,0);Q(Vector3.zero,new Vector3(4,.25f,16),stone);Flush(ramp);
            Solid(ramp,"Ramp collision",Vector3.zero,new Vector3(4,.25f,16));
        }
        static void Canteen()
        {
            var c=Group("Canteen_Gym",root,new Vector3(337,.49f,-47));
            Q(new Vector3(0,1.8f,0),new Vector3(6,3.6f,25),ivory);
            Q(new Vector3(0,3.75f,0),new Vector3(7,.3f,26),teal);
            for(float z=-11;z<12;z+=3)Q(new Vector3(3.05f,1.9f,z),new Vector3(.1f,2.1f,2),glass);
            Flush(c);Solid(c,"Exterior volume",new Vector3(0,1.8f,0),new Vector3(6,3.6f,25));
            Text(c,"Roof label","CĂN TIN • GYM",new Vector3(0,4,0),.95f,Quaternion.Euler(90,90,0),Color.white);
            // Covered passage alongside I, visible in the illustrated plan.
            var hall=Group("Dorm covered walkway",root,new Vector3(295,.49f,-49));
            Q(new Vector3(0,4,0),new Vector3(4,.25f,29),teal);
            for(float z=-13;z<=13;z+=4.3f)foreach(int s in new[]{-1,1})Q(new Vector3(s*1.7f,2,z),new Vector3(.15f,4,.15f),stone);
            Flush(hall);
        }
        static void GroundAndStreets()
        {
            var g=Group("Ground_Streets",root,Vector3.zero);
            Q(new Vector3(373,.24f,-96),new Vector3(230,.42f,250),grass);
            Q(new Vector3(373,.465f,-96),new Vector3(166,.05f,184),paving);
            foreach(float z in new[]{6f,-195f})Q(new Vector3(373,.465f,z),new Vector3(200,.03f,12),asphalt);
            foreach(float x in new[]{280f,465f})Q(new Vector3(x,.465f,-95),new Vector3(12,.03f,190),asphalt);
            foreach(float z in new[]{-2f,-187f})Q(new Vector3(373,.485f,z),new Vector3(180,.04f,4),white);
            foreach(float x in new[]{288f,457f})Q(new Vector3(x,.485f,-94),new Vector3(4,.04f,182),white);
            // Broad campus circulation loops and two clear front-to-back paths.
            foreach(float x in new[]{295f,357f,409f,450f})Q(new Vector3(x,.50f,-96),new Vector3(5,.02f,178),stone);
            foreach(float z in new[]{-71f,-90f,-132f,-180f})Q(new Vector3(373,.51f,z),new Vector3(158,.02f,4.8f),stone);
            for(float x=284;x<465;x+=9)foreach(float z in new[]{6f,-195f})Q(new Vector3(x,.492f,z),new Vector3(4,.02f,.15f),white);
            for(float z=-185;z<0;z+=9)foreach(float x in new[]{280f,465f})Q(new Vector3(x,.492f,z),new Vector3(.15f,.02f,4),white);
            foreach(float x in new[]{409f,357f,294f})for(int i=0;i<8;i++)Q(new Vector3(x,.5f,1.6f+i*1.1f),new Vector3(4,.025f,.55f),white);
            Flush(g);Solid(g,"Continuous walkable ground",new Vector3(373,.24f,-96),new Vector3(230,.42f,250));
            // The ground collision surface is at .49 throughout the campus.
            Solid(g,"Campus paving collision",new Vector3(373,.47f,-96),new Vector3(166,.04f,184));
            Text(g,"Nguyen Van Bao","ĐƯỜNG NGUYỄN VĂN BẢO",new Vector3(378,.53f,9.5f),2.1f,Quaternion.Euler(90,180,0),Color.white);
            Text(g,"Nguyen Van Nghi","ĐƯỜNG NGUYỄN VĂN NGHI",new Vector3(375,.53f,-196),2.1f,Quaternion.Euler(90,180,0),Color.white);
            Text(g,"Nguyen Thai Son","ĐƯỜNG NGUYỄN THÁI SƠN",new Vector3(465,.53f,-96),2f,Quaternion.Euler(90,90,0),Color.white);
            Text(g,"Le Loi","ĐƯỜNG LÊ LỢI",new Vector3(280,.53f,-96),2f,Quaternion.Euler(90,270,0),Color.white);
        }
        static void Tree(Transform parent,float x,float z,int index,float scale=1)
        {
            float h=(5.3f+index%3*.6f)*scale;
            Cy(new Vector3(x,.49f+h*.30f,z),new Vector3(.38f,h*.30f,.38f),bark);
            S(new Vector3(x,h*.85f,z),new Vector3(4.6f,4.8f,4.6f)*scale,index%2==0?leaf:leafLight);
            S(new Vector3(x-1*scale,h*.71f,z+.5f*scale),new Vector3(3.5f,3.6f,3.8f)*scale,leaf);
            Cy(new Vector3(x,.56f,z),new Vector3(2.2f,.07f,2.2f),stone);
            Solid(parent,"Tree trunk "+index,new Vector3(x,h*.30f+.49f,z),new Vector3(.38f,h*.60f,.38f));
        }
        static void Landscape()
        {
            var l=Group("Gardens_Trees_Plaza",root,Vector3.zero);int index=0;
            foreach(float x in new[]{290f,454f})for(float z=-177;z<=-9;z+=8)Tree(l,x,z,index++);
            for(float x=300;x<450;x+=9)foreach(float z in new[]{-3f,-184f})
                if(Mathf.Abs(x-409)>7 && Mathf.Abs(x-357)>7 && Mathf.Abs(x-294)>6 && Mathf.Abs(x-337)>7 && Mathf.Abs(x-385)>7
                    && !(z>-10 && Mathf.Abs(x-425)<5))Tree(l,x,z,index++,.85f);
            foreach(float x in new[]{405f,413f})for(float z=-126;z<-10;z+=12)
                if(z < -96 || z > -74) Tree(l,x,z,index++,.65f);
            // Green court in front of E is deliberately open for the two mini games.
            foreach(var pos in new[]{new Vector2(365,-80),new Vector2(399,-82),new Vector2(360,-96),new Vector2(406,-111),new Vector2(360,-118),new Vector2(405,-130),new Vector2(332,-157),new Vector2(338,-139),new Vector2(415,-95)})Tree(l,pos.x,pos.y,index++,.8f);
            foreach(var p in new[]{new Vector2(407,-103),new Vector2(361,-111),new Vector2(413,-79),new Vector2(361,-79),new Vector2(300,-107),new Vector2(335,-161)})
            {
                Q(new Vector3(p.x,.61f,p.y),new Vector3(3,.24f,6),ivory);
                Q(new Vector3(p.x,.76f,p.y),new Vector3(2.6f,.08f,5.6f),grass);
                for(int k=-1;k<=1;k++)S(new Vector3(p.x,1.05f,p.y+k*1.5f),new Vector3(1.8f,.9f,1.7f),leafLight);
            }
            // Fountain beside B/E, matching the reference's courtyard landmark.
            Cy(new Vector3(411,.67f,-116),new Vector3(6,.18f,6),ivory);
            Cy(new Vector3(411,.88f,-116),new Vector3(5.1f,.08f,5.1f),water);
            Cy(new Vector3(411,1.5f,-116),new Vector3(.55f,.7f,.55f),stone);
            Cy(new Vector3(411,2.1f,-116),new Vector3(2.8f,.12f,2.8f),ivory);
            S(new Vector3(411,2.7f,-116),new Vector3(.3f,1.2f,.3f),water);
            Solid(l,"Fountain base",new Vector3(411,.68f,-116),new Vector3(5.7f,.4f,5.7f));
            // Paving inlays provide human scale without expensive individual tile objects.
            for(float x=367;x<=398;x+=3)for(float z=-88;z<=-74;z+=3)Q(new Vector3(x,.535f,z),new Vector3(.045f,.015f,2.9f),white);
            for(float z=-88;z<=-74;z+=3)Q(new Vector3(383,.537f,z),new Vector3(34,.015f,.045f),white);
            foreach(float x in new[]{362f,402f,413f})foreach(float z in new[]{-73f,-91f,-129f})
            {
                Cy(new Vector3(x,2.5f,z),new Vector3(.13f,2,.13f),dark);
                Q(new Vector3(x,4.55f,z),new Vector3(.7f,.14f,.7f),gold);
            }
            // Benches flank circulation instead of occupying it.
            foreach(var p in new[]{new Vector2(400,-73),new Vector2(365,-73),new Vector2(414,-114),new Vector2(359,-128)})
            {
                Q(new Vector3(p.x,1,p.y),new Vector3(2.6f,.15f,.7f),stone);
                Q(new Vector3(p.x,1.38f,p.y-.32f),new Vector3(2.6f,.65f,.12f),stone);
                foreach(int s in new[]{-1,1})Q(new Vector3(p.x+s*.9f,.75f,p.y),new Vector3(.12f,.5f,.6f),dark);
            }
            Flush(l);
        }
        static void GatesAndFence()
        {
            var p=Group("Perimeter_Five_Gates",root,Vector3.zero);
            foreach(float z in new[]{-6f,-181f})
            {
                float[] gates=z>-10?new[]{294f,357f,409f}:new[]{337f,385f};
                float start=290;
                foreach(float x in gates.OrderBy(x=>x)) { FenceSegment(p,start,x-5.8f,z);start=x+5.8f; }
                FenceSegment(p,start,452,z);
            }
            foreach(float x in new[]{291f,452f})
            {
                Q(new Vector3(x,1.1f,-94),new Vector3(.3f,1.2f,174),ivory);
                for(float z=-179;z<-7;z+=2)Q(new Vector3(x,1.9f,z),new Vector3(.10f,1,.10f),dark);
                Q(new Vector3(x,2.4f,-94),new Vector3(.12f,.12f,174),dark);
                Solid(p,"Side boundary",new Vector3(x,1.65f,-94),new Vector3(.3f,2.3f,174));
            }
            Flush(p);
            Gate(1,409,-6);Gate(2,357,-6);Gate(3,294,-6);Gate(4,337,-181);Gate(5,385,-181);
        }
        static void FenceSegment(Transform p,float a,float b,float z)
        {
            if(b<=a)return;float w=b-a,x=(a+b)/2;
            Q(new Vector3(x,1,z),new Vector3(w,1,.3f),ivory);
            Q(new Vector3(x,2.2f,z),new Vector3(w,.12f,.12f),dark);
            for(float u=a;u<b;u+=1.5f)Q(new Vector3(u,1.8f,z),new Vector3(.10f,1.1f,.10f),dark);
            Solid(p,"Boundary segment",new Vector3(x,1.45f,z),new Vector3(w,2,.3f));
        }
        static void Gate(int number,float x,float z)
        {
            var g=Group("Gate_0"+number,root,new Vector3(x,.49f,z));
            foreach(int s in new[]{-1,1})
            {
                Q(new Vector3(s*5.6f,2,0),new Vector3(.8f,4,.8f),ivory);
                Q(new Vector3(s*5.6f,4.2f,0),new Vector3(1.1f,.3f,1.1f),blue);
                Solid(g,"Gate pillar",new Vector3(s*5.6f,2,0),new Vector3(.8f,4,.8f));
            }
            Q(new Vector3(0,4.1f,0),new Vector3(12,.9f,.5f),blue);Flush(g);
            Text(g,"Street gate sign","IUH  •  CỔNG "+number,new Vector3(0,4.15f,.28f),1f,Quaternion.Euler(0,180,0),Color.white);
            Text(g,"Campus gate sign","CỔNG "+number,new Vector3(0,4.15f,-.28f),1f,Quaternion.identity,Color.white);
            Text(g,"Overview gate label","CỔNG "+number,new Vector3(0,4.65f,0),1.3f,Quaternion.Euler(90,180,0),blue.color);
        }
        static void Environment()
        {
            var e=Group("Daylight",root,Vector3.zero);var sun=Group("Campus Sun",e,Vector3.zero).gameObject.AddComponent<Light>();
            sun.type=LightType.Directional;sun.intensity=1.25f;sun.color=new Color(1,.95f,.84f);sun.transform.rotation=Quaternion.Euler(48,-36,0);
            sun.shadows=LightShadows.Soft;sun.shadowStrength=.65f;RenderSettings.sun=sun;
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.60f,.69f,.77f);
            RenderSettings.ambientEquatorColor=new Color(.45f,.49f,.51f);RenderSettings.ambientGroundColor=new Color(.30f,.32f,.29f);
            RenderSettings.ambientIntensity=1;RenderSettings.fog=false;
            var cam=Camera.main;if(cam!=null){Undo.RecordObject(cam,"Campus camera range");cam.farClipPlane=650;cam.useOcclusionCulling=false;}
        }
        static void Place(string path,Vector3 position)
        { var go=GameObject.Find(path);if(go!=null){Undo.RecordObject(go.transform,"Relink campus landmark");go.transform.position=position;} }
        public static void RelinkGames()
        {
            var green=UnityEngine.Object.FindFirstObjectByType<IUHGreenCampusManager>(FindObjectsInactive.Include);
            if(green!=null)
            {
                Undo.RecordObject(green,"Move Green Campus to new courtyard");green.spawnPoint=IUHReferenceLayout.GreenSpawn;
                // Transform the already-authored world only once; keep IDs, references and saves intact.
                if(green.gameWorld.transform.position==Vector3.zero)
                {
                    foreach(var name in new[]{"Round_Objects","Restored_Green_Corners"})
                    {
                        var t=green.transform.Find(name);if(t==null)continue;
                        var ts=t.GetComponentsInChildren<Transform>(true);var positions=ts.Select(v=>v.position).ToArray();
                        for(int i=0;i<ts.Length;i++){Undo.RecordObject(ts[i],"Move Green Campus props");var v=positions[i];v.z=v.z*.72f-49.34f;ts[i].position=v;}
                    }
                }
                EditorUtility.SetDirty(green);
                foreach(var text in green.GetComponentsInChildren<TextMesh>(true))
                {
                    Undo.RecordObject(text,"Fit courtyard activity labels");
                    text.characterSize=text.name=="Station_Title"?.045f:text.name=="Label"?.04f:.035f;
                    var depth=text.GetComponent<IUHDepthLabel>();
                    if(depth==null)depth=Undo.AddComponent<IUHDepthLabel>(text.gameObject);
                    depth.BindShader(Shader.Find("IUH/Depth World Text"));EditorUtility.SetDirty(depth);
                }
            }
            var targets=UnityEngine.Object.FindObjectsByType<IUHPhotoTarget>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            foreach(var target in targets)
            {
                Undo.RecordObject(target,"Relink Photo Hunter target");Undo.RecordObject(target.transform,"Move Photo Hunter target");
                if(target.TargetId.Contains("gate_01"))
                {
                    target.transform.position=new Vector3(409,3.6f,-6);
                    target.Configure(target.TargetId,target.DisplayName,target.Description,IUHReferenceLayout.Entrance,new Vector3(0,1.5f,22),6,44,GameObject.Find(IUHReferenceLayout.RootName+"/Gate_01").transform);
                    target.SetKeyFeatureOffsets(new List<Vector3>{new Vector3(-4.5f,0,0),new Vector3(4.5f,0,0),new Vector3(0,1,0)});
                }
                else if(target.TargetId.Contains("central_courtyard"))
                {
                    target.transform.position=new Vector3(385,1.5f,-85);
                    target.Configure(target.TargetId,target.DisplayName,target.Description,new Vector3(385,.65f,-73),Vector3.back,5,58,GameObject.Find(IUHReferenceLayout.RootName+"/Gardens_Trees_Plaza").transform);
                    target.SetKeyFeatureOffsets(new List<Vector3>{new Vector3(-6,0,0),new Vector3(6,0,0),new Vector3(0,.5f,2)});
                }
                else if(target.TargetId.Contains("block_a"))
                {
                    target.transform.position=new Vector3(425,4.8f,-13.2f);
                    target.Configure(target.TargetId,target.DisplayName,target.Description,new Vector3(425,.65f,5),Vector3.back,5,45,GameObject.Find(IUHReferenceLayout.RootName+"/Block_A").transform);
                    target.SetKeyFeatureOffsets(new List<Vector3>{new Vector3(-5,0,0),new Vector3(5,0,0),new Vector3(0,1.5f,0)});
                }
                target.SetStructureReferences(target.StructureRoot,null);EditorUtility.SetDirty(target);
            }
            Place("PlayerArmature",IUHReferenceLayout.Entrance);
            var player=GameObject.Find("PlayerArmature");if(player!=null)player.transform.rotation=Quaternion.Euler(0,180,0);
            Place("GAMEPLAY_GRAB_CAMPUS/IUH_Grab_Motorbike",new Vector3(412,.6f,-29));
            var pickups=IUHReferenceLayout.Pickups;var drops=IUHReferenceLayout.Dropoffs;
            string gp="GAMEPLAY_GRAB_CAMPUS/";
            var shops=GameObject.Find(gp+"NIGHT_FOOD_SHOPS");
            if(shops!=null)for(int i=0;i<Mathf.Min(3,shops.transform.childCount);i++)Place(gp+"NIGHT_FOOD_SHOPS/"+shops.transform.GetChild(i).name,pickups[i]+new Vector3(3,0,-2));
            var stations=GameObject.Find(gp+"DELIVERY_DROPOFF_STATIONS");
            if(stations!=null)for(int i=0;i<Mathf.Min(3,stations.transform.childCount);i++)Place(gp+"DELIVERY_DROPOFF_STATIONS/"+stations.transform.GetChild(i).name,drops[i]+Vector3.right*2.5f);
            Place(gp+"DELIVERY_WAYPOINTS/PickupWaypoint",pickups[0]);Place(gp+"DELIVERY_WAYPOINTS/DropoffWaypoint",drops[0]);
            var ghosts=GameObject.Find(gp+"GHOST_HAZARDS");if(ghosts!=null)
                for(int i=0;i<ghosts.transform.childCount;i++)Place(gp+"GHOST_HAZARDS/"+ghosts.transform.GetChild(i).name,new Vector3(i%2==0?409:357,1.6f,-40-i*18));
            var grab=GameObject.Find("GAMEPLAY_GRAB_CAMPUS");if(grab!=null)
            {
                var manager=grab.GetComponent<IUHCampus.Minigames.GrabCampus.IUHGrabCampusManager>();
                var so=new SerializedObject(manager);var orders=so.FindProperty("m_Orders");
                for(int i=0;i<Mathf.Min(3,orders.arraySize);i++)
                {var o=orders.GetArrayElementAtIndex(i);o.FindPropertyRelative("pickupPosition").vector3Value=pickups[i];o.FindPropertyRelative("dropoffPosition").vector3Value=drops[i];}
                so.ApplyModifiedProperties();
            }
        }
    }
}
#endif
