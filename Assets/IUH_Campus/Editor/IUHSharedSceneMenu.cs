#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using IUHCampus.UI;
using IUHCampus.Minigames.PhotoHunter.Editor;
using IUHCampus.Minigames.GreenCampus.Editor;

namespace IUHCampus.Editor
{
    public static partial class IUHSharedSceneMenu
    {
        [MenuItem("IUH Campus/UI/Sync Shared Games and Rebuild Menu")]
        public static void Build()
        {
            var scene=EditorSceneManager.GetActiveScene();
            if(Application.isPlaying||scene.path!=IUHPhotoHunterSetup.TargetScenePath)throw new InvalidOperationException("Open main scene in Edit Mode.");
            Directory.CreateDirectory("Docs/Backups");
            string backup="Docs/Backups/IUH_Campus_Main_before_shared_menu_20261024.unity";
            if(!File.Exists(backup)&&!EditorSceneManager.SaveScene(scene,backup,true))throw new IOException("Scene backup failed.");
            if(!IUHPhotoHunterSetup.SetupPhotoHunter(false))throw new InvalidOperationException("Photo setup failed.");
            if(!IUHGreenCampusSetup.Setup())throw new InvalidOperationException("Green setup failed.");
            var portal=UnityEngine.Object.FindFirstObjectByType<IUHMiniGamePortalUI>(FindObjectsInactive.Include);
            if(portal==null)throw new InvalidOperationException("Portal missing.");
            var old=portal.transform.Find("PortalMenuPanel");if(old!=null)Undo.DestroyObjectImmediate(old.gameObject);
            var panel=BuildSixGameMenu(portal.transform);
            var serialized=new SerializedObject(portal);serialized.FindProperty("m_MenuPanel").objectReferenceValue=panel.gameObject;serialized.FindProperty("m_OpenOnStart").boolValue=true;serialized.ApplyModifiedProperties();
            var floating=portal.transform.Find("FloatingPortalBtn")?.GetComponent<RectTransform>();
            if(floating!=null)
            {
                Undo.RecordObject(floating,"Position menu button");floating.anchorMin=floating.anchorMax=new Vector2(1,0);floating.pivot=new Vector2(1,0);floating.anchoredPosition=new Vector2(-28,24);floating.sizeDelta=new Vector2(270,58);
                var text=floating.GetComponentInChildren<Text>();if(text!=null){Undo.RecordObject(text,"Readable menu label");text.text="MENU TRÒ CHƠI  [M]";text.fontSize=21;}
            }
            var custom=portal.transform.Find("CustomizationPanel");
            if(custom!=null)
            {
                custom.gameObject.SetActive(true);
                var ui=custom.GetComponent<IUHCustomizationUI>();
                if(ui!=null){var settings=new SerializedObject(ui);settings.FindProperty("m_OpenOnStart").boolValue=false;settings.ApplyModifiedProperties();}
            }
            var photoCard=portal.transform.Find("PhotoHunter_UI_Root/BriefingPanel/Card")?.GetComponent<RectTransform>();
            if(photoCard!=null)
            {
                Undo.RecordObject(photoCard,"Readable photo briefing");photoCard.sizeDelta=new Vector2(1020,650);
                foreach(var text in photoCard.GetComponentsInChildren<Text>(true))
                {
                    Undo.RecordObject(text,"Readable photo instructions");
                    text.fontSize=text.name=="TxtTitle"?26:text.name=="TxtStatus"?18:20;
                }
                var desc=photoCard.Find("TxtDesc")?.GetComponent<RectTransform>();
                if(desc!=null){desc.offsetMin=new Vector2(32,118);desc.offsetMax=new Vector2(-32,-108);}
            }
            panel.gameObject.SetActive(false);portal.ConfigureReferenceDestinations();EditorUtility.SetDirty(portal);
            AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject=panel.gameObject;Debug.Log("Shared campus menu rebuilt; Photo Hunter and Green Campus synchronized.");
        }
        static RectTransform Box(string name,Transform parent,Vector2 min,Vector2 max,Vector2 insetMin,Vector2 insetMax,Color color)
        {
            var g=new GameObject(name,typeof(RectTransform),typeof(Image));g.transform.SetParent(parent,false);
            var r=g.GetComponent<RectTransform>();r.anchorMin=min;r.anchorMax=max;r.offsetMin=insetMin;r.offsetMax=insetMax;g.GetComponent<Image>().color=color;return r;
        }
        static Text Label(string name,Transform parent,string value,int size,Color color,Vector2 position,Vector2 dimensions)
        {
            var g=new GameObject(name,typeof(RectTransform),typeof(Text));g.transform.SetParent(parent,false);var r=g.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=position;r.sizeDelta=dimensions;
            var t=g.GetComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=value;t.fontSize=size;t.color=color;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;
        }
        static RectTransform Button(string name,Transform parent,string title,Color color)
        {
            var r=Box(name,parent,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,color);var b=r.gameObject.AddComponent<Button>();b.targetGraphic=r.GetComponent<Image>();
            var colors=b.colors;colors.highlightedColor=new Color(1.15f,1.15f,1.15f);colors.pressedColor=new Color(.75f,.85f,.95f);b.colors=colors;
            var text=Label("Txt",r,title,22,Color.white,Vector2.zero,Vector2.zero);text.alignment=TextAnchor.MiddleCenter;text.fontStyle=FontStyle.Bold;var t=text.rectTransform;t.anchorMin=Vector2.zero;t.anchorMax=Vector2.one;t.offsetMin=new Vector2(8,0);t.offsetMax=new Vector2(-8,0);return r;
        }
        static void TopRight(RectTransform r,Vector2 p,Vector2 size){r.anchorMin=r.anchorMax=new Vector2(1,1);r.pivot=new Vector2(1,1);r.anchoredPosition=p;r.sizeDelta=size;}
    }
}
#endif
