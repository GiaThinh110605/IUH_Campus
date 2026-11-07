using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
namespace IUHCampus.Guardian.Progress.UI
{
 [DefaultExecutionOrder(-300)]
 public sealed class GuardianInventoryUI : MonoBehaviour
 {
  public GuardianItemDatabase database; public GuardianProgressConfig config;
  public Button bag,close,action; public RectTransform safeArea,panel,grid;
  public CanvasGroup panelGroup; public Text core,title,description,quantity,tooltip,empty;
  public Font font; public GuardianInventoryInputGate inputGate;
  public float animationSeconds=.18f;
  public bool isolatedPreview;
  public GuardianInventory Inventory{get;private set;} public GuardianWallet Wallet{get;private set;}
  public bool IsOpen{get;private set;}
  public event Action<string> ActionRequested;
  public IGuardianPlantingContext PlantingContext{get;private set;}
  GuardianProgressService service; readonly List<GuardianInventorySlot> slots=new List<GuardianInventorySlot>();
  string selected;Coroutine transition;Rect lastSafe;Vector2 lastSize;
  void Start(){if(isolatedPreview){var state=GuardianProgressFactory.Create(config,database);Inventory=new GuardianInventory(state,database);Wallet=new GuardianWallet(state);}else{service=GuardianProgressService.Instance;if(service==null)service=GuardianProgressService.EnsureCreated(database,config);else if(service.State==null)service.Initialize(database,config);Inventory=service.Inventory;Wallet=service.Wallet;}Inventory.Changed+=Refresh;Wallet.Changed+=Refresh;bag.onClick.AddListener(Toggle);close.onClick.AddListener(Close);action.onClick.AddListener(Act);foreach(Transform child in grid){var slot=child.GetComponent<GuardianInventorySlot>();if(slot!=null)slots.Add(slot);}panelGroup.alpha=0;panelGroup.blocksRaycasts=false;panelGroup.interactable=false;Refresh();ApplySafeArea();}
  void Update(){var keyboard=Keyboard.current;if(keyboard!=null){if(IsOpen&&keyboard.escapeKey.wasPressedThisFrame)Close();else if(keyboard.iKey.wasPressedThisFrame)Toggle();}if(Screen.safeArea!=lastSafe||lastSize!=new Vector2(Screen.width,Screen.height))ApplySafeArea();}
  void ApplySafeArea(){lastSafe=Screen.safeArea;lastSize=new Vector2(Screen.width,Screen.height);if(lastSize.x<=0||lastSize.y<=0)return;safeArea.anchorMin=lastSafe.position/lastSize;safeArea.anchorMax=(lastSafe.position+lastSafe.size)/lastSize;safeArea.offsetMin=safeArea.offsetMax=Vector2.zero;}
  public void Toggle(){if(IsOpen)Close();else Open();}
  public bool Open(){if(Inventory==null||inputGate.IsBlocked())return false;IsOpen=true;inputGate.Acquire();Animate(true);return true;}
  public void Close(){if(!IsOpen)return;IsOpen=false;inputGate.Release();Animate(false);}
  void Animate(bool opening){if(transition!=null)StopCoroutine(transition);panelGroup.blocksRaycasts=opening;panelGroup.interactable=opening;transition=StartCoroutine(Fade(opening));}
  IEnumerator Fade(bool opening){float start=panelGroup.alpha;Vector2 from=panel.anchoredPosition;Vector2 target=opening?Vector2.zero:new Vector2(0,-40);for(float t=0;t<animationSeconds;t+=Time.unscaledDeltaTime){float k=Mathf.SmoothStep(0,1,t/animationSeconds);panelGroup.alpha=Mathf.Lerp(start,opening?1:0,k);panel.anchoredPosition=Vector2.Lerp(from,target,k);yield return null;}panelGroup.alpha=opening?1:0;panel.anchoredPosition=target;transition=null;}
  public void Refresh(){if(Inventory==null)return;var rows=GuardianInventoryPresenter.Rows(Inventory,database);core.text="Campus Core: "+Wallet.Core;empty.gameObject.SetActive(rows.Count==0);for(int i=0;i<slots.Count;i++){slots[i].gameObject.SetActive(i<rows.Count);if(i<rows.Count){var row=rows[i];slots[i].Bind(row,()=>Select(row.Definition.id));}}if(selected==null||Inventory.Count(selected)==0)selected=rows.Count>0?rows[0].Definition.id:null;ShowDetail();}
  public void Select(string id){selected=id;ShowDetail();}
  public void SetPlantingContext(IGuardianPlantingContext context){PlantingContext=context;ShowDetail();}
  void ShowDetail(){tooltip.text="";var item=database.Find(selected);title.text=item!=null?item.displayName:"Chọn một món đồ";description.text=item!=null?item.description:"Kho đồ đang trống";quantity.text=item!=null?"Số lượng: "+Inventory.Count(item.id):"";action.gameObject.SetActive(item!=null&&item.category!=GuardianItemCategory.Tool);if(item==null)return;action.GetComponentInChildren<Text>().text=GuardianInventoryPresenter.ActionLabel(item);action.interactable=GuardianInventoryPresenter.CanAct(item,PlantingContext);tooltip.text=item.category==GuardianItemCategory.Seed&&!action.interactable?"Đứng gần luống đất trống":item.category==GuardianItemCategory.Consumable?"Chưa thể dùng — chờ P2":"";}
  void Act(){if(selected==null)return;var item=database.Find(selected);if(!GuardianInventoryPresenter.CanAct(item,PlantingContext))return;PlantingContext.Plant(selected);ActionRequested?.Invoke(selected);Refresh();}
  void OnDisable(){if(inputGate!=null)inputGate.Release();IsOpen=false;}
  void OnDestroy(){if(Inventory!=null)Inventory.Changed-=Refresh;if(Wallet!=null)Wallet.Changed-=Refresh;}
 }
}
