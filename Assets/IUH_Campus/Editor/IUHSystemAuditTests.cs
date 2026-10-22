#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using StarterAssets;
using IUHCampus.World;
using IUHCampus.UI;
using IUHCampus.Minigames.PhotoHunter;
using IUHCampus.Minigames.GreenCampus;
using IUHCampus.Minigames.GrabCampus;

namespace IUHCampus.Editor
{
    public static class IUHSystemAuditTests
    {
        const string Evidence="Docs/Evidence/SystemAudit";
        [Serializable] public class Report {public int passed,failed;public bool running;public List<string> checks=new List<string>();}
        public static Report Result {get;private set;}=new Report();
        static void Check(Report r,bool pass,string description){if(pass)r.passed++;else r.failed++;r.checks.Add((pass?"PASS ":"FAIL ")+description);}
        static void Save(string name,Report r){Directory.CreateDirectory(Evidence);File.WriteAllText(Evidence+"/"+name,JsonUtility.ToJson(r,true));}

        [MenuItem("IUH Campus/Tests/System Audit/Storage and Resources")]
        static void RunStorageMenu(){var r=StorageAndResources();Debug.Log("[System Audit] Storage/resources "+r.passed+" passed; "+r.failed+" failed.");}
        [MenuItem("IUH Campus/Tests/System Audit/Setup Idempotence")]
        static void RunSetupMenu(){var r=SetupIdempotence();Debug.Log("[System Audit] Setup "+r.passed+" passed; "+r.failed+" failed.");}

        public static Report StorageAndResources()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Run in Edit Mode.");
            var r=new Report();string previous=IUHPhotoHunterStorage.BaseDirectory;
            string folder=Path.Combine(Application.temporaryCachePath,"IUH_Audit_"+Guid.NewGuid().ToString("N"));
            string store=Path.Combine(folder,"Save");Directory.CreateDirectory(store);
            IUHPhotoHunterStorage.SetCustomStorageDirectoryForTest(store);
            var go=new GameObject("Isolated audit transaction manager");var manager=go.AddComponent<IUHPhotoHunterManager>();
            GameObject cameraObject=null,serviceObject=null,keyRoot=null;
            try
            {
                string first=Path.Combine(store,"first.jpg"),second=Path.Combine(store,"second.jpg"),failed=Path.Combine(store,"failed.jpg"),external=Path.Combine(folder,"external.jpg");
                File.WriteAllBytes(first,new byte[]{1,2,3});File.WriteAllBytes(second,new byte[]{4,5,6});
                manager.SetCustomSaveContainerForTest(new PhotoHunterSaveContainer());
                Check(r,manager.ExecutePhotoTransaction("gate","Gate",first,100,out var initial)&&initial==100,"First photo transaction commits 100 points");
                Check(r,manager.ExecutePhotoTransaction("gate","Gate",second,100,out var duplicate)&&duplicate==0&&manager.CurrentScore==100,"Recapture keeps the reward at 100 points");
                Check(r,manager.CurrentSaveContainer.photoRecords.Count==1&&manager.CurrentSaveContainer.photoRecords[0].filePath==second,"Recapture replaces the album image path");
                Check(r,!File.Exists(first)&&File.Exists(second),"Replaced JPEG is removed only after successful save");
                File.WriteAllBytes(failed,new byte[]{7});IUHPhotoHunterStorage.SetSimulateWriteFailureForTest(true);
                Check(r,!manager.ExecutePhotoTransaction("new","New",failed,100,out _)&&!File.Exists(failed)&&File.Exists(second),"Failed transaction removes new JPEG and preserves the committed photo");
                Check(r,!manager.ExecutePhotoTransaction("gate","Gate",second,100,out _)&&File.Exists(second),"Rollback never deletes the already committed image when paths match");
                File.WriteAllBytes(external,new byte[]{8});
                Check(r,!manager.ExecutePhotoTransaction("outside","Outside",external,100,out _)&&File.Exists(external),"Rollback cannot delete a JPEG outside this game's storage");
                var previousState=manager.CurrentState;manager.RestartMission();
                Check(r,manager.CurrentScore==100&&manager.CompletedCount==1&&manager.CurrentState==previousState,"Restart failure preserves manager progress and state");
                Check(r,IUHPhotoHunterStorage.LoadProgress().totalScore==100&&File.Exists(second),"Restart failure preserves save and album on disk");
                IUHPhotoHunterStorage.SetSimulateWriteFailureForTest(false);
                Check(r,IUHPhotoHunterStorage.ResetProgress()&&IUHPhotoHunterStorage.LoadProgress().totalScore==0,"Successful restart writes a durable empty save");
                var empty=new PhotoHunterSaveContainer{completedTargetIds=null,photoRecords=null};
                Check(r,empty.Clone().photoRecords.Count==0&&empty.Clone().completedTargetIds.Count==0,"Null photo lists can be cloned safely");
                var noTargets=new PhotoHunterSaveContainer();noTargets.RecordCompletion("removed","Removed",second,100,out _);noTargets.SanitizeAndValidate(Array.Empty<string>());
                Check(r,noTargets.totalScore==0&&noTargets.completedTargetIds.Count==0,"An empty active target set clears stale completions");
                Check(r,!IUHPhotoHunterStorage.IsManagedJpeg(IUHPhotoHunterStorage.SaveFilePath)&&!IUHPhotoHunterStorage.IsManagedJpeg(external),"Cleanup rejects progress JSON and files outside the save root");
                var uiObject=new GameObject("Audit album UI");uiObject.transform.SetParent(go.transform);var ui=uiObject.AddComponent<IUHPhotoHunterUI>();
                var panelObject=new GameObject("Album");panelObject.transform.SetParent(uiObject.transform);
                var labelObject=new GameObject("Title",typeof(RectTransform),typeof(CanvasRenderer),typeof(UnityEngine.UI.Text));labelObject.transform.SetParent(panelObject.transform);
                var label=labelObject.GetComponent<UnityEngine.UI.Text>();
                typeof(IUHPhotoHunterUI).GetField("m_VictoryDialog",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(ui,panelObject);
                typeof(IUHPhotoHunterUI).GetField("m_TxtVictoryTitle",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(ui,label);
                ui.ShowPersistenceError("Không lưu được lượt mới");
                Check(r,label.text.Contains("Không lưu")&&label.gameObject.activeInHierarchy,"Restart save errors appear on the visible album rather than a hidden toast");
                cameraObject=new GameObject("Temporary audit camera");var camera=cameraObject.AddComponent<Camera>();
                serviceObject=new GameObject("Temporary capture service");var service=serviceObject.AddComponent<IUHPhotoCaptureService>();
                bool completed=false,success=true;
                var routine=(IEnumerator)typeof(IUHPhotoCaptureService).GetMethod("CaptureRoutine",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(service,new object[]{camera,"audit",1,new Func<int,bool>(_=>true),new Action<bool,string>((ok,path)=>{completed=true;success=ok;})});
                routine.MoveNext();UnityEngine.Object.DestroyImmediate(cameraObject);cameraObject=null;routine.MoveNext();
                Check(r,completed&&!success,"Camera disappearing after frame wait completes cleanly without rendering");
                keyRoot=new GameObject("Audit mesh key root");var a=new GameObject("A").transform;var b=new GameObject("B").transform;a.SetParent(keyRoot.transform);b.SetParent(keyRoot.transform);
                var pa=new GameObject("Entrance plaque").transform;var pb=new GameObject("Entrance plaque").transform;pa.SetParent(a);pb.SetParent(b);
                var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/IUH_Campus/ReferenceLayout/Materials/IUH_Blue.mat");
                var key=typeof(IUHReferenceCampusBuilder).GetMethod("MeshAssetKey",BindingFlags.Static|BindingFlags.NonPublic);
                Check(r,(string)key.Invoke(null,new object[]{pa,material})!=(string)key.Invoke(null,new object[]{pb,material}),"Identically named parts in different buildings get different mesh asset keys");
            }
            finally
            {
                IUHPhotoHunterStorage.SetSimulateWriteFailureForTest(false);IUHPhotoHunterStorage.SetCustomStorageDirectoryForTest(previous==Application.persistentDataPath?null:previous);
                foreach(var obj in new[]{go,cameraObject,serviceObject,keyRoot})if(obj!=null)UnityEngine.Object.DestroyImmediate(obj);
                string tempRoot=Path.GetFullPath(Application.temporaryCachePath).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;
                if(Path.GetFullPath(folder).StartsWith(tempRoot,StringComparison.OrdinalIgnoreCase))Directory.Delete(folder,true);
            }
            Save("storage-resources.json",r);return r;
        }

        public static Report SetupIdempotence()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Run in Edit Mode.");
            var r=new Report();var root=GameObject.Find(IUHReferenceLayout.RootName).transform;
            var transforms=root.GetComponentsInChildren<Transform>(true);var matrices=transforms.Select(t=>t.localToWorldMatrix).ToArray();
            var player=GameObject.Find("PlayerArmature");var playerPosition=player.transform.position;
            var green=UnityEngine.Object.FindAnyObjectByType<IUHGreenCampusManager>(FindObjectsInactive.Include);var firstPosition=green.items.First(i=>i.itemId=="green_item_0_0").transform.position;
            for(int run=0;run<2;run++)
            {
                IUHCampus.Minigames.PhotoHunter.Editor.IUHPhotoHunterSetup.SetupPhotoHunter(false);
                IUHCampus.Minigames.GreenCampus.Editor.IUHGreenCampusSetup.Setup();
            }
            Check(r,root.GetComponentsInChildren<Transform>(true).Length==transforms.Length&&transforms.Select((t,i)=>t.localToWorldMatrix==matrices[i]).All(v=>v),"Repeated setup keeps campus object count and every transform unchanged");
            Check(r,player.transform.position==playerPosition,"Repeated setup does not teleport the player");
            Check(r,green.items.First(i=>i.itemId=="green_item_0_0").transform.position==firstPosition,"Repeated setup preserves litter moved for the tree planter");
            var court=UnityEngine.Object.FindObjectsByType<IUHPhotoTarget>(FindObjectsInactive.Include).First(t=>t.TargetId.Contains("central_courtyard"));
            Check(r,court.StructureRoot==root.Find("Block_E"),"Repeated setup keeps the R2 courtyard bound to the new E facade");
            var original=green.gameObject;string oldName=original.name;bool oldActive=original.activeSelf;
            original.name="Audit preserved Green Campus";original.SetActive(false);GameObject rebuilt=null;
            try
            {
                IUHCampus.Minigames.GreenCampus.Editor.IUHGreenCampusSetup.Setup();rebuilt=GameObject.Find("GAMEPLAY_GREEN_CAMPUS");
                var fresh=rebuilt.GetComponent<IUHGreenCampusManager>();
                Check(r,fresh.spawnPoint==IUHReferenceLayout.GreenSpawn,"Fresh Green setup uses the current courtyard spawn");
                Check(r,fresh.items.First(i=>i.itemId=="green_item_0_0").transform.position==new Vector3(372.6f,.65f,-81.7f),"Fresh setup places the first litter outside the R2 planter");
                Check(r,fresh.items.All(i=>i.transform.position.z<-75)&&fresh.bins.All(b=>b.transform.position.z<-75),"Fresh setup moves all litter and bins out of the parking building");
            }
            finally
            {
                if(rebuilt==null)rebuilt=GameObject.Find("GAMEPLAY_GREEN_CAMPUS");
                if(rebuilt!=null&&rebuilt!=original)UnityEngine.Object.DestroyImmediate(rebuilt);
                original.name=oldName;original.SetActive(oldActive);EditorSceneManager.MarkSceneDirty(root.gameObject.scene);EditorSceneManager.SaveScene(root.gameObject.scene);
            }
            Save("setup-idempotence.json",r);return r;
        }

        static IEnumerator<float> steps;static double next;static float nextGame;static string oldGreen,oldPhoto;static bool oldBackground;
        static ThirdPersonController controller;static IUHMiniGamePortalUI portal;static IUHGreenCampusManager greenManager;static IUHPhotoHunterManager photoManager;
        [MenuItem("IUH Campus/Tests/System Audit/Fast Travel (Play Mode)")]
        public static void StartPlayChecks()
        {
            if(!Application.isPlaying||Result.running)throw new InvalidOperationException("Run alone in Play Mode.");
            Result=new Report{running=true};controller=UnityEngine.Object.FindAnyObjectByType<ThirdPersonController>();portal=IUHMiniGamePortalUI.Instance;
            greenManager=IUHGreenCampusManager.Instance;photoManager=IUHPhotoHunterManager.Instance;
            oldGreen=GreenCampusStorage.DirectoryOverride;oldPhoto=IUHPhotoHunterStorage.BaseDirectory;oldBackground=Application.runInBackground;Application.runInBackground=true;
            string folder=Path.Combine(Application.temporaryCachePath,"IUH_Audit_Play_"+Guid.NewGuid().ToString("N"));
            GreenCampusStorage.DirectoryOverride=Path.Combine(folder,"Green");IUHPhotoHunterStorage.SetCustomStorageDirectoryForTest(Path.Combine(folder,"Photo"));
            greenManager.Initialize();photoManager.SetCustomSaveContainerForTest(new PhotoHunterSaveContainer());
            steps=Exercise();next=EditorApplication.timeSinceStartup;nextGame=0;EditorApplication.update+=Tick;
        }
        static bool ClearPlayer()
        {
            var pos=controller.transform.position;
            return !Physics.OverlapCapsule(pos+Vector3.up*.4f,pos+Vector3.up*1.7f,.27f,~0,QueryTriggerInteraction.Ignore).Any(c=>c.GetComponentInParent<CharacterController>()==null);
        }
        static IEnumerator<float> Exercise()
        {
            greenManager.OpenBriefing();greenManager.BeginRound(true);yield return .5f;
            portal.TeleportToKtxDorm();yield return 1;
            Check(Result,greenManager.State==IUHGreenCampusManager.GameState.Inactive&&controller.enabled&&controller.Grounded,"Dorm fast travel exits Green and restores ground movement");
            Check(Result,ClearPlayer()&&Vector3.Distance(controller.transform.position,IUHReferenceLayout.DormEntrance)<.3f,"Dorm fast travel lands outside F/I solids");
            photoManager.StartMission(true);photoManager.EnterViewfinderMode();yield return .3f;
            portal.TeleportToLibrary();yield return 1;
            Check(Result,!photoManager.HasActiveSessionOrUI&&controller.MoveSpeed>0&&controller.GetComponentsInChildren<SkinnedMeshRenderer>().Any(m=>m.enabled),"Library travel exits aiming and restores avatar and walking speed");
            Check(Result,ClearPlayer()&&controller.Grounded&&Vector3.Distance(controller.transform.position,IUHReferenceLayout.LibraryEntrance)<.3f,"Library travel lands at the actual A entry");
            var bike=UnityEngine.Object.FindAnyObjectByType<IUHGrabMotorbike>();bike.MountVehicle(controller.gameObject);portal.TeleportToGate1Parking();yield return 1;
            Check(Result,!bike.IsMounted&&controller.gameObject.activeInHierarchy&&controller.enabled&&controller.Grounded,"Gate travel dismounts an idle bike and restores the active player");
            var camera=Camera.main.GetComponent<Unity.Cinemachine.CinemachineBrain>().ActiveVirtualCamera as Unity.Cinemachine.CinemachineCamera;
            Check(Result,camera.Follow==controller.CinemachineCameraTarget.transform&&ClearPlayer(),"Fast travel returns the camera to the player and avoids parking geometry");
            controller.GetComponent<StarterAssetsInputs>().JumpInput(true);yield return .2f;
            Check(Result,controller.transform.position.y>.7f,"Player is airborne before the fast-travel reset check");
            portal.TeleportToGreenTrashStation();yield return 1;
            Check(Result,controller.Grounded&&controller.transform.position.y<1&&ClearPlayer(),"Courtyard travel during a jump discards stale vertical velocity");
            portal.TeleportToBlockFRoof();yield return 1;
            Check(Result,controller.Grounded&&controller.transform.position.y>13.3f&&controller.transform.position.y<13.8f&&ClearPlayer(),"Roof travel reaches the real F roof height with full standing clearance");
            portal.TeleportToGate1Parking();yield return .7f;var position=controller.transform.position;portal.TeleportToFestivalStage();yield return .2f;
            Check(Result,portal.IsOpen&&Vector3.Distance(position,controller.transform.position)<.05f,"Unimplemented Festival does not teleport into unrelated geometry");
            portal.SetMenuOpen(false);
        }
        static void Tick()
        {
            try
            {
                if(!Application.isPlaying)throw new InvalidOperationException("Play Mode interrupted.");
                if(EditorApplication.timeSinceStartup<next||Time.time<nextGame)return;
                if(steps.MoveNext()){next=EditorApplication.timeSinceStartup+steps.Current;nextGame=Time.time+steps.Current;}else Finish();
            }
            catch(Exception ex){Check(Result,false,ex.ToString());Finish();}
        }
        static void Finish()
        {
            EditorApplication.update-=Tick;
            if(Application.isPlaying){greenManager.CancelMission();photoManager.CancelMission();portal.SetMenuOpen(false);}
            GreenCampusStorage.DirectoryOverride=oldGreen;IUHPhotoHunterStorage.SetCustomStorageDirectoryForTest(oldPhoto==Application.persistentDataPath?null:oldPhoto);
            Application.runInBackground=oldBackground;Result.running=false;Save("fast-travel-playmode.json",Result);
        }
    }
}
#endif
