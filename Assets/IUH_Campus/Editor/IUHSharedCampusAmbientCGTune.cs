#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace IUHCampus.Editor {
public static partial class IUHSharedCampusConceptBuilder {
[MenuItem("IUH Campus/Concept/Tune ambientCG Surface Scale")]
public static void TuneAmbientCGSurfaceScale() {
 var scene=EditorSceneManager.GetActiveScene();
 if(Application.isPlaying || scene.path!=ScenePath)throw new InvalidOperationException("Target scene in Edit Mode required.");
 var world=scene.GetRootGameObjects().Single(g=>g.name=="CAMPUS_ADVENTURE_SHARED_WORLD");
 if(world.transform.Find("AMBIENTCG_PASS_APPLIED")==null)throw new InvalidOperationException("Existing ambientCG pass required.");
 var names=new[]{"Sandstone_paving","Paving_pale","Lawn","Limestone_facade","Terracotta_brick","Teak_timber","Unused_Bricks104","Unused_WoodFloor062"};
 var scales=new[]{new Vector2(2,2),new Vector2(2,2),new Vector2(3,3),new Vector2(1.5f,1),new Vector2(2.5f,1.5f),new Vector2(1.5f,1.5f),new Vector2(2.5f,1.5f),new Vector2(1.5f,1.5f)};
 var colors=new[]{new Color(.82f,.79f,.70f,1),new Color(.82f,.79f,.70f,1),new Color(.62f,.72f,.48f,1),new Color(.88f,.84f,.75f,1),new Color(.78f,.55f,.42f,1),new Color(.72f,.55f,.38f,1)};
 var mats=names.Select(n=>AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/ambientCG/ambientCG_"+n+".mat")).ToArray();
 if(mats.Any(m=>m==null))throw new InvalidOperationException("All eight existing materials required.");
 var volume=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Volume>(true)).Single(v=>v.sharedProfile!=null);
 var profile=volume.sharedProfile;var path=AssetDatabase.GetAssetPath(profile);
 if(path!=Folder+"/Campus volume.asset")throw new InvalidOperationException("Unexpected volume profile.");
 var backup="Docs/Backups/Campus_volume_before_ambientcg_tune_"+DateTime.Now.ToString("yyyyMMdd_HHmmss_fff")+".asset";
 File.Copy(path,backup,false);
 const string evidence="Docs/Evidence/AmbientCG_20261031/Tuned";Directory.CreateDirectory(evidence);
 var mapping=new System.Collections.Generic.List<string>();
 for(int i=0;i<mats.Length;i++) {var m=mats[i];Undo.RecordObject(m,"Tune ambientCG Surface Scale");var old=m.GetTextureScale("_BaseMap");var oldColor=m.GetColor("_BaseColor");foreach(var slot in new[]{"_BaseMap","_BumpMap","_OcclusionMap"})m.SetTextureScale(slot,scales[i]);if(i<colors.Length)m.SetColor("_BaseColor",colors[i]);EditorUtility.SetDirty(m);mapping.Add(m.name+": "+old+" -> "+scales[i]+" all three maps; BaseColor "+oldColor+" -> "+m.GetColor("_BaseColor"));}
 ColorAdjustments ca;Bloom bloom;Vignette vig;
 if(!profile.TryGet(out ca)||!profile.TryGet(out bloom))throw new InvalidOperationException("Expected color and bloom overrides.");
 if(!profile.TryGet(out vig)){vig=profile.Add<Vignette>();AssetDatabase.AddObjectToAsset(vig,profile);}
 File.WriteAllText(evidence+"/volume-mapping.txt","Profile="+path+"\nBackup="+backup+"\nContrast "+ca.contrast.value+" -> 8\nSaturation "+ca.saturation.value+" -> 4\nExposure "+ca.postExposure.value+" -> .05\nFilter "+ca.colorFilter.value+" -> white\nBloom threshold "+bloom.threshold.value+" -> 1.4\nBloom intensity "+bloom.intensity.value+" -> .15\nBloom scatter "+bloom.scatter.value+" -> .7\nVignette "+vig.intensity.value+" -> .12\nVignette smoothness "+vig.smoothness.value+" -> .45\n");
 Undo.RecordObjects(new UnityEngine.Object[]{ca,bloom,vig},"Tune ambientCG volume");
 ca.contrast.Override(8);ca.saturation.Override(4);ca.postExposure.Override(.05f);ca.colorFilter.Override(Color.white);
 bloom.threshold.Override(1.4f);bloom.intensity.Override(.15f);bloom.scatter.Override(.7f);vig.intensity.Override(.12f);vig.smoothness.Override(.45f);
 EditorUtility.SetDirty(ca);EditorUtility.SetDirty(bloom);EditorUtility.SetDirty(vig);EditorUtility.SetDirty(profile);
 AssetDatabase.SaveAssets();File.WriteAllLines(evidence+"/material-mapping.txt",mapping);
 Debug.Log("ambientCG surface scale tuned. Volume backup: "+backup);
}
}
}
#endif


