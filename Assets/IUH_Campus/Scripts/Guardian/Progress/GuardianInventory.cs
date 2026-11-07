using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
namespace IUHCampus.Guardian.Progress {
 public sealed class GuardianInventory {
  readonly GuardianProgressState state;readonly GuardianItemDatabase database;readonly HashSet<string> warned=new HashSet<string>();
  public event Action Changed;
  public GuardianInventory(GuardianProgressState state,GuardianItemDatabase database){this.state=state;this.database=database;database.Validate();}
  public IReadOnlyDictionary<string,int> Items {get{var copy=new Dictionary<string,int>();foreach(var s in state.inventory)copy.Add(s.itemId,s.qty);return new ReadOnlyDictionary<string,int>(copy);}}
  public int Count(string itemId){return state.inventory.Find(s=>s.itemId==itemId)?.qty??0;}
  public bool Has(string itemId,int qty=1)=>qty>0&&Count(itemId)>=qty;
  public bool Add(string itemId,int qty,string rewardId=null){var d=database.Find(itemId);if(d==null){Warn(itemId);return false;}if(qty<=0||(rewardId!=null&&string.IsNullOrWhiteSpace(rewardId))||(!string.IsNullOrEmpty(rewardId)&&state.appliedRewardIds.Contains(rewardId)))return false;
   int current=Count(itemId),capacity=d.stackable?d.maxStack:1;if(qty>capacity-current)return false;
   var stack=state.inventory.Find(s=>s.itemId==itemId);if(stack==null)state.inventory.Add(new GuardianItemStack{itemId=itemId,qty=qty});else stack.qty+=qty;
   if(!string.IsNullOrEmpty(rewardId))state.appliedRewardIds.Add(rewardId);Changed?.Invoke();return true;}
  public bool Remove(string itemId,int qty){if(database.Find(itemId)==null){Warn(itemId);return false;}if(!Has(itemId,qty))return false;var stack=state.inventory.Find(s=>s.itemId==itemId);stack.qty-=qty;if(stack.qty==0)state.inventory.Remove(stack);Changed?.Invoke();return true;}
  void Warn(string id){if(warned.Add(id??""))Debug.LogWarning("[IUH Guardian] Unknown item id: "+id);}
 }
}
