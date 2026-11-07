using System;
using UnityEngine;
namespace IUHCampus.Guardian.Progress {
 public sealed class GuardianProgressService : MonoBehaviour {
  public static GuardianProgressService Instance{get;private set;}
  public GuardianProgressState State{get;private set;}public GuardianWallet Wallet{get;private set;}public GuardianInventory Inventory{get;private set;}
  public GuardianProgressStore Store{get;private set;}public GuardianItemDatabase Database{get;private set;}public GuardianProgressConfig Config{get;private set;}
  bool dirty;float due;
  public static GuardianProgressService EnsureCreated(GuardianItemDatabase database,GuardianProgressConfig config){if(Instance!=null)return Instance;var go=new GameObject("Guardian Progress");var service=go.AddComponent<GuardianProgressService>();service.Initialize(database,config);return service;}
  void Awake(){if(Instance!=null&&Instance!=this){Destroy(gameObject);return;}Instance=this;DontDestroyOnLoad(gameObject);}
  public void Initialize(GuardianItemDatabase database,GuardianProgressConfig config){if(State!=null)throw new InvalidOperationException("Progress already initialized.");Database=database;Config=config;database.Validate();Store=new GuardianProgressStore(Application.persistentDataPath);State=Store.Load(()=>GuardianProgressFactory.Create(config,database));Wallet=new GuardianWallet(State);Inventory=new GuardianInventory(State,database);Wallet.Changed+=MarkDirty;Inventory.Changed+=MarkDirty;Store.Save(State);}
  public void MarkDirty(){dirty=true;due=Time.realtimeSinceStartup+Config.saveDebounceSeconds;}
  void Update(){if(dirty&&Time.realtimeSinceStartup>=due)Flush();}
  public void Flush(){if(!dirty||State==null)return;Store.Save(State);dirty=false;}
  void OnApplicationPause(bool pause){if(pause)Flush();}void OnApplicationQuit(){Flush();}
  void OnDestroy(){if(Instance!=this)return;Flush();Instance=null;}
 }
}
