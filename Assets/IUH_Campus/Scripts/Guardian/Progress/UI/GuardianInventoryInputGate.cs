using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace IUHCampus.Guardian.Progress.UI
{
 public sealed class GuardianInventoryInputGate : MonoBehaviour
 {
  readonly List<MonoBehaviour> cached=new List<MonoBehaviour>();
  readonly Dictionary<Behaviour,bool> enabledStates=new Dictionary<Behaviour,bool>();
  readonly List<System.Action> restoreFields=new List<System.Action>();
  CursorLockMode cursorLock; bool cursorVisible, held;
  void Awake(){foreach(var component in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None)){if(component==null)continue;string n=component.GetType().Name;if(n=="ThirdPersonController"||n=="StarterAssetsInputs"||n=="IUHConceptPreviewController"||n=="IUHGameFlowManager"||n=="Match3CombatManager"||n=="IUHGrabMotorbike")cached.Add(component);}}
  public bool IsBlocked(){foreach(var c in cached){if(c==null)continue;var t=c.GetType();if(ReadBool(c,"BlocksExplorationInput")||ReadBool(c,"IsExplorationPaused"))return true;if(t.Name=="IUHGameFlowManager"){var value=t.GetProperty("CurrentState")?.GetValue(c);if(value!=null&&value.ToString()!="Playing")return true;}}return Time.timeScale==0;}
  static bool ReadBool(object c,string name)=>c.GetType().GetProperty(name)?.GetValue(c) is bool value && value;
  public void Acquire(){if(held)return;held=true;cursorLock=Cursor.lockState;cursorVisible=Cursor.visible;foreach(var c in cached){if(c==null||c.GetType().Name=="Match3CombatManager")continue;enabledStates[c]=c.enabled;var t=c.GetType();t.GetMethod("ResetMotion",BindingFlags.Public|BindingFlags.Instance)?.Invoke(c,null);foreach(string name in new[]{"move","look"})t.GetField(name)?.SetValue(c,Vector2.zero);foreach(string name in new[]{"cursorLocked","cursorInputForLook"}){var field=t.GetField(name);if(field!=null){var value=field.GetValue(c);restoreFields.Add(()=>{if(c!=null)field.SetValue(c,value);});field.SetValue(c,false);}}c.enabled=false;}Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
  public void Release(){if(!held)return;foreach(var c in enabledStates)if(c.Key!=null)c.Key.enabled=c.Value;foreach(var restore in restoreFields)restore();restoreFields.Clear();enabledStates.Clear();Cursor.lockState=cursorLock;Cursor.visible=cursorVisible;held=false;}
  void OnDisable()=>Release();
 }
}
