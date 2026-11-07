using UnityEngine;
using UnityEngine.UI;
namespace IUHCampus.Guardian.Progress.UI
{
 public sealed class GuardianInventoryDebug : MonoBehaviour
 {
  public Button add;
  void Start(){
#if UNITY_EDITOR
   add.onClick.AddListener(()=>{var s=GetComponent<GuardianInventoryUI>();foreach(var item in s.database.definitions)s.Inventory.Add(item.id,1);s.Wallet.AddCore(10,"debug:"+System.Guid.NewGuid());});
#else
   add.gameObject.SetActive(false);
#endif
  }
 }
}
