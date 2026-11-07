using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace IUHCampus.Guardian.Progress.UI
{
 public sealed class GuardianInventorySlot : MonoBehaviour
 {
  public Button button; public Image icon,border;public Text label,count;
  int previous;string previousId;Coroutine pulse;
  public void Bind(GuardianInventoryRow row,Action select){button.onClick.RemoveAllListeners();button.onClick.AddListener(()=>select());icon.sprite=row.Definition.icon;icon.color=icon.sprite!=null?Color.white:Color.clear;label.text=row.Definition.displayName;count.text=row.Quantity.ToString();var tint=row.Definition.category==GuardianItemCategory.Seed?new Color(.3f,.85f,.55f):row.Definition.category==GuardianItemCategory.Tool?new Color(.35f,.65f,1f):new Color(1f,.75f,.25f);border.color=tint;if(previousId==row.Definition.id&&row.Quantity>previous&&isActiveAndEnabled){if(pulse!=null)StopCoroutine(pulse);pulse=StartCoroutine(Flash(tint));}previous=row.Quantity;previousId=row.Definition.id;}
  IEnumerator Flash(Color tint){for(float t=0;t<.35f;t+=Time.unscaledDeltaTime){border.color=Color.Lerp(Color.white,tint,t/.35f);yield return null;}border.color=tint;pulse=null;}
 }
}
