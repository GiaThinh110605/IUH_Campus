#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
namespace IUHCampus.Editor {
public static partial class IUHSharedCampusConceptBuilder {
const string HE="Docs/Evidence/AmbientCG_20261031/Hybrid";
[MenuItem("IUH Campus/Concept/Apply ambientCG Hybrid Surface Pass")]
public static void ApplyAmbientCGHybridSurfacePass() {
 var scene=EditorSceneManager.GetActiveScene();
 if(Application.isPlaying||scene.path!=ScenePath)throw new InvalidOperationException("Target scene in Edit Mode required.");
 var world=scene.GetRootGameObjects().Single(g=>g.name=="CAMPUS_ADVENTURE_SHARED_WORLD");
 if(world.transform.Find("AMBIENTCG_PASS_APPLIED")==null)throw new InvalidOperationException("Existing pass marker required.");
 var props=scene.GetRootGameObjects().Single(g=>g.name=="IMPORTED_URBAN_PROPS");
 if(props.transform.childCount!=6)throw new InvalidOperationException("Six props required.");
 var sources=new[]{"Sandstone_paving","Paving_pale","Lawn"};var targets=new[]{"Courtyard sandstone","Pale paving slabs","Detailed lawn"};
 var map=new Dictionary<Material,Material>();
 for(int i=0;i<3;i++){var a=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/ambientCG/ambientCG_"+sources[i]+".mat");var b=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/VisualPolish/Materials/"+targets[i]+".mat");if(a==null||b==null)throw new InvalidOperationException("Missing material");map.Add(a,b);}
 var backup=Directory.GetFiles("Docs/Backups","Campus_volume_before_ambientcg_*.asset").Where(p=>System.Text.RegularExpressions.Regex.IsMatch(Path.GetFileName(p),@"^Campus_volume_before_ambientcg_\d{8}_\d{6}\.asset$")).OrderBy(p=>p,StringComparer.Ordinal).FirstOrDefault();
 if(backup==null)throw new InvalidOperationException("Original volume backup required.");
 Directory.CreateDirectory(HE);var stamp=DateTime.Now.ToString("yyyyMMdd_HHmmss");var sceneBackup="Docs/Backups/IUH_SharedCampus_Concept_before_ambientcg_hybrid_"+stamp+".unity";if(File.Exists(sceneBackup))throw new InvalidOperationException("Backup already exists.");
 if(!EditorSceneManager.SaveScene(scene,sceneBackup,true))throw new InvalidOperationException("Scene backup failed.");
 var vp=Folder+"/Campus volume.asset";var volumeBackup="Docs/Backups/Campus_volume_before_ambientcg_hybrid_"+stamp+".asset";File.Copy(vp,volumeBackup,false);
 var lines=new List<string>();int slots=0,renderers=0;
 foreach(var r in world.GetComponentsInChildren<MeshRenderer>(true)){if(r.transform.IsChildOf(props.transform))continue;var mats=r.sharedMaterials;bool changed=false;for(int i=0;i<mats.Length;i++){Material replacement;if(mats[i]!=null&&map.TryGetValue(mats[i],out replacement)){lines.Add(r.name+" slot "+i+": "+mats[i].name+" -> "+replacement.name);mats[i]=replacement;changed=true;slots++;}}if(changed){Undo.RecordObject(r,"Hybrid surface pass");r.sharedMaterials=mats;renderers++;}}
 var vertical=new[]{"Limestone_facade","Terracotta_brick","Teak_timber"};var scale=new[]{new Vector2(1.5f,1),new Vector2(2,1.25f),new Vector2(1.5f,1.5f)};var smooth=new[]{.16f,.14f,.28f};var bump=new[]{.5f,.75f,.6f};
 for(int i=0;i<3;i++){var m=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/ambientCG/ambientCG_"+vertical[i]+".mat");if(m==null)throw new InvalidOperationException("Missing retained material");Undo.RecordObject(m,"Hybrid vertical material");foreach(var slot in new[]{"_BaseMap","_BumpMap","_OcclusionMap"})m.SetTextureScale(slot,scale[i]);m.SetFloat("_Smoothness",smooth[i]);m.SetFloat("_BumpScale",bump[i]);EditorUtility.SetDirty(m);lines.Add(m.name+" scale="+scale[i]+" smoothness="+smooth[i]+" bump="+bump[i]+" color="+m.GetColor("_BaseColor"));}
 AssetDatabase.SaveAssets();File.Copy(backup,vp,true);AssetDatabase.ImportAsset(vp,ImportAssetOptions.ForceUpdate);
 var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(vp);var v=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Volume>(true)).Single(g=>g.sharedProfile!=null);if(v.sharedProfile!=profile)throw new InvalidOperationException("Volume reference mismatch.");
 EditorSceneManager.MarkSceneDirty(scene);if(!EditorSceneManager.SaveScene(scene))throw new InvalidOperationException("Save failed.");
 lines.Add("Renderers="+renderers+" Slots="+slots);File.WriteAllLines(HE+"/material-mapping.txt",lines);File.WriteAllText(HE+"/volume-mapping.txt","Restored exact file="+backup+"\nCurrent backup="+volumeBackup+"\nScene backup="+sceneBackup+"\nProfile="+vp+"\nComponents="+string.Join(",",profile.components.Select(c=>c.GetType().Name).ToArray()));
}
}
}
#endif
