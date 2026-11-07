using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace IUHCampus.Guardian.Progress.UI.Editor
{
 public static class GuardianInventoryCapture
 {
  public static string Resolution(int width,int height){var asm=typeof(UnityEditor.Editor).Assembly;var flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.FlattenHierarchy;var sizes=asm.GetType("UnityEditor.GameViewSizes");var instance=sizes.GetProperty("instance",flags).GetValue(null);var group=sizes.GetMethod("GetGroup").Invoke(instance,new[]{Enum.Parse(asm.GetType("UnityEditor.GameViewSizeGroupType"),"Standalone")});var size=Activator.CreateInstance(asm.GetType("UnityEditor.GameViewSize"),new[]{Enum.Parse(asm.GetType("UnityEditor.GameViewSizeType"),"FixedResolution"),(object)width,height,"P15 "+width+"x"+height});group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});int index=(int)group.GetType().GetMethod("GetBuiltinCount").Invoke(group,null)+(int)group.GetType().GetMethod("GetCustomCount").Invoke(group,null)-1;var view=EditorWindow.GetWindow(asm.GetType("UnityEditor.GameView"));view.GetType().GetProperty("selectedSizeIndex",flags).SetValue(view,index);view.Focus();return "Selected "+width+"x"+height;}
 }
}
