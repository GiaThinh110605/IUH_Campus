#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace IUHCampus.Editor {
public static partial class IUHSharedCampusConceptBuilder {
const string AE="Docs/Evidence/AmbientCG_20261031";
public static void AmbientCapture(string phase) {
 Directory.CreateDirectory(AE);
 var positions=new[]{new Vector3(13,9,-56),new Vector3(-15,4,-40),new Vector3(12,4,-37),new Vector3(3,2,-26),new Vector3(5,5,4),new Vector3(-19,2,-12)};
 var targets=new[]{new Vector3(6,5,25),new Vector3(-28,1.4f,-19),new Vector3(29,1.5f,-19),new Vector3(3,0,-20),new Vector3(5,5,12),new Vector3(-19,0,-6)};
 var names=new[]{"overview","canteen","stage","paving-closeup","facade-closeup","grass-closeup"};
 var g=new GameObject("AmbientEvidenceCamera");g.hideFlags=HideFlags.HideAndDontSave;
 var cam=g.AddComponent<Camera>();cam.enabled=false;cam.fieldOfView=60;cam.farClipPlane=500;cam.clearFlags=CameraClearFlags.Skybox;
 var data=cam.GetUniversalAdditionalCameraData();data.renderPostProcessing=true;data.volumeLayerMask=~0;
 var rt=new RenderTexture(1280,720,24);var previous=RenderTexture.active;
 try { for(int i=0;i<names.Length;i++){g.transform.position=positions[i];g.transform.LookAt(targets[i]);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes(AE+"/"+phase+"-"+names[i]+".png",tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);} }
 finally {cam.targetTexture=null;RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(g);}
}
[MenuItem("IUH Campus/Concept/Apply ambientCG PBR Texture Pass")]
public static void ApplyAmbientCG() {
 var scene=EditorSceneManager.GetActiveScene();
 if(Application.isPlaying||scene.path!=ScenePath||scene.isDirty)throw new InvalidOperationException("Saved target scene in Edit Mode required.");
 root=scene.GetRootGameObjects().Single(g=>g.name=="CAMPUS_ADVENTURE_SHARED_WORLD").transform;
 if(root.Find("AMBIENTCG_PASS_APPLIED")!=null)throw new InvalidOperationException("Pass already applied.");
 var sets=new[]{"Bricks051","Bricks104","Grass005","PavingStones151","Plaster001","WoodFloor051","WoodFloor062"};
 var textureFolder=Folder+"/Textures/ambientCG";var materialFolder=Folder+"/Materials/ambientCG";
 Directory.CreateDirectory(materialFolder);
 foreach(var set in sets){var dest=textureFolder+"/"+set;Directory.CreateDirectory(dest);foreach(var suffix in new[]{"Color","NormalGL","Roughness","AmbientOcclusion"}){var file=set+"_2K-JPG_"+suffix+".jpg";var src="D:/IUH/CongNgheMoi/CK/Texture/"+set+"_2K-JPG/"+file;if(File.Exists(src))File.Copy(src,dest+"/"+file,false);}}
 AssetDatabase.Refresh();
 foreach(var file in Directory.GetFiles(textureFolder,"*.jpg",SearchOption.AllDirectories)){var path=file.Replace('\\','/');var ti=(TextureImporter)AssetImporter.GetAtPath(path);bool normal=path.Contains("NormalGL"),color=path.Contains("_Color");ti.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;ti.sRGBTexture=color;ti.maxTextureSize=2048;ti.mipmapEnabled=true;ti.anisoLevel=(color||normal)?8:4;ti.wrapMode=TextureWrapMode.Repeat;ti.filterMode=FilterMode.Bilinear;ti.npotScale=TextureImporterNPOTScale.ToNearest;ti.textureCompression=TextureImporterCompression.CompressedHQ;ti.SaveAndReimport();}
 var oldNames=new[]{"Sandstone paving","Paving pale","Lawn","Limestone facade","Terracotta brick","Teak timber","Unused Bricks104","Unused WoodFloor062"};
 var useSets=new[]{"PavingStones151","PavingStones151","Grass005","Plaster001","Bricks051","WoodFloor051","Bricks104","WoodFloor062"};
 var tiling=new[]{new Vector2(18,18),new Vector2(18,18),new Vector2(26,26),new Vector2(5,3),new Vector2(6,4),new Vector2(4,4),new Vector2(6,4),new Vector2(4,4)};
 var smooth=new[]{.18f,.22f,.08f,.16f,.14f,.28f,.14f,.28f};var bump=new[]{.9f,.9f,.7f,.5f,1f,.6f,1f,.6f};
 var map=new Dictionary<Material,Material>();var log=new List<string>();
 for(int i=0;i<oldNames.Length;i++){var name="ambientCG_"+oldNames[i].Replace(' ','_');var path=materialFolder+"/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}mat.enableInstancing=true;mat.SetColor("_BaseColor",i==3?new Color(1,.97f,.90f):Color.white);mat.SetFloat("_Metallic",0);mat.SetFloat("_Smoothness",smooth[i]);mat.SetFloat("_BumpScale",bump[i]);mat.EnableKeyword("_NORMALMAP");var prefix=textureFolder+"/"+useSets[i]+"/"+useSets[i]+"_2K-JPG_";mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(prefix+"Color.jpg"));mat.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(prefix+"NormalGL.jpg"));var ao=AssetDatabase.LoadAssetAtPath<Texture2D>(prefix+"AmbientOcclusion.jpg");if(ao!=null){mat.SetTexture("_OcclusionMap",ao);mat.SetFloat("_OcclusionStrength",1);mat.EnableKeyword("_OCCLUSIONMAP");}mat.SetTextureScale("_BaseMap",tiling[i]);mat.SetTextureScale("_BumpMap",tiling[i]);mat.SetTextureScale("_OcclusionMap",tiling[i]);EditorUtility.SetDirty(mat);var old=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/"+oldNames[i]+".mat");if(old!=null)map.Add(old,mat);log.Add(oldNames[i]+" -> "+path+" tiling="+tiling[i]+" smoothness="+smooth[i]+" bump="+bump[i]);}
 var aliases=new[]{"Courtyard sandstone","Pale paving slabs","Detailed lawn","Dressed limestone","Weathered terracotta","Timber grain"};for(int i=0;i<aliases.Length;i++){var alias=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/VisualPolish/Materials/"+aliases[i]+".mat");if(alias!=null)map[alias]=AssetDatabase.LoadAssetAtPath<Material>(materialFolder+"/ambientCG_"+oldNames[i].Replace(' ','_')+".mat");}
 int changed=0;foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>(true)){if(renderer.GetComponentsInParent<Transform>(true).Any(t=>t.name=="IMPORTED_URBAN_PROPS"))continue;var mats=renderer.sharedMaterials;bool dirty=false;for(int j=0;j<mats.Length;j++){Material replacement;if(mats[j]!=null&&map.TryGetValue(mats[j],out replacement)){mats[j]=replacement;dirty=true;changed++;}}if(dirty){Undo.RecordObject(renderer,"Apply ambientCG");renderer.sharedMaterials=mats;}}
 var volume=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Volume>(true)).Single(v=>v.sharedProfile!=null);var profile=volume.sharedProfile;var vp=AssetDatabase.GetAssetPath(profile);File.Copy(vp,"Docs/Backups/Campus_volume_before_ambientcg_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".asset",false);
 ColorAdjustments ca;if(!profile.TryGet(out ca))ca=profile.Add<ColorAdjustments>();ca.postExposure.Override(.15f);ca.contrast.Override(12);ca.saturation.Override(10);ca.colorFilter.Override(new Color(1,1.01f,1.03f));Bloom bl;if(!profile.TryGet(out bl))bl=profile.Add<Bloom>();bl.threshold.Override(1.15f);bl.intensity.Override(.35f);bl.scatter.Override(.72f);bl.tint.Override(new Color(1,.97f,.93f));Vignette vi;if(!profile.TryGet(out vi))vi=profile.Add<Vignette>();vi.intensity.Override(.22f);vi.smoothness.Override(.45f);vi.color.Override(new Color(.05f,.06f,.10f));foreach(var component in profile.components)EditorUtility.SetDirty(component);EditorUtility.SetDirty(profile);
 var marker=new GameObject("AMBIENTCG_PASS_APPLIED");marker.transform.SetParent(root,false);
 File.WriteAllLines(AE+"/material-mapping.txt",log);File.AppendAllText(AE+"/material-mapping.txt","\nReplaced material slots="+changed+"\nVolume="+vp);
 AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
}
}
}
#endif

