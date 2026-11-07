using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
namespace IUHCampus.Guardian.Progress.UI.Editor
{
 public static class GuardianInventoryBuilder
 {
  const string Data="Assets/IUH_Campus/Data/Guardian/Progress/", Icons="Assets/IUH_Campus/UI/Guardian/Icons/";
  static Font font;
  static RectTransform Box(Transform parent,string name,Vector2 pos,Vector2 size){var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var r=(RectTransform)go.transform;r.anchoredPosition=pos;r.sizeDelta=size;return r;}
  static Text Text(Transform parent,string name,string value,Vector2 pos,Vector2 size,int fontSize=24){var r=Box(parent,name,pos,size);var t=r.gameObject.AddComponent<Text>();t.font=font;t.text=value;t.fontSize=fontSize;t.color=new Color(.9f,.95f,1);t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;return t;}
  static Button Button(Transform parent,string name,string label,Vector2 pos,Vector2 size){var r=Box(parent,name,pos,size);var image=r.gameObject.AddComponent<Image>();image.color=new Color(.12f,.27f,.39f);var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;Text(r,"Label",label,Vector2.zero,size,24);return b;}
  public static string Build(){
   var campus=SceneManager.GetActiveScene();if(campus.isDirty)throw new System.InvalidOperationException("Active scene dirty; refusing to build.");font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
   foreach(string id in new[]{"seed_sunflower","seed_rose","seed_daisy","tool_watering_can","fertilizer"}){string path=Icons+id+".png";var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureShape=TextureImporterShape.Texture2D;importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.SaveAndReimport();string asset=Data+id+".asset";var content=File.ReadAllText(asset);content=System.Text.RegularExpressions.Regex.Replace(content,@"  icon: .*","  icon: {fileID: 21300000, guid: "+AssetDatabase.AssetPathToGUID(path)+", type: 3}");File.WriteAllText(asset,content);AssetDatabase.ImportAsset(asset);}
   var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(scene);
   new GameObject("GuardianProgressService",typeof(GuardianProgressService));
   var camera=new GameObject("Camera",typeof(Camera));camera.tag="MainCamera";camera.GetComponent<Camera>().backgroundColor=new Color(.025f,.045f,.08f);camera.GetComponent<Camera>().clearFlags=CameraClearFlags.SolidColor;
   new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
   var root=new GameObject("GuardianInventoryUI",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(GuardianInventoryInputGate),typeof(GuardianInventoryUI));
   root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;root.GetComponent<Canvas>().sortingOrder=300;
   var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
   var ui=root.GetComponent<GuardianInventoryUI>();ui.font=font;ui.inputGate=root.GetComponent<GuardianInventoryInputGate>();ui.database=AssetDatabase.LoadAssetAtPath<GuardianItemDatabase>(Data+"GuardianItemDatabase.asset");ui.config=AssetDatabase.LoadAssetAtPath<GuardianProgressConfig>(Data+"GuardianProgressConfig.asset");
   ui.safeArea=Box(root.transform,"SafeArea",Vector2.zero,Vector2.zero);ui.safeArea.anchorMin=Vector2.zero;ui.safeArea.anchorMax=Vector2.one;ui.safeArea.offsetMin=ui.safeArea.offsetMax=Vector2.zero;
   ui.bag=Button(ui.safeArea,"Bag","TÚI ĐỒ [I]",new Vector2(-125,-65),new Vector2(220,70));var bagRect=(RectTransform)ui.bag.transform;bagRect.anchorMin=bagRect.anchorMax=Vector2.one;
   ui.panel=Box(ui.safeArea,"InventoryPanel",Vector2.zero,new Vector2(1640,830));ui.panel.gameObject.AddComponent<Image>().color=new Color(.035f,.075f,.12f,.98f);ui.panelGroup=ui.panel.gameObject.AddComponent<CanvasGroup>();ui.panelGroup.alpha=0;ui.panelGroup.blocksRaycasts=false;
   Text(ui.panel,"Heading","KHO ĐỒ GUARDIAN",new Vector2(-440,335),new Vector2(650,70),36);ui.core=Text(ui.panel,"Core","Campus Core: 0",new Vector2(390,335),new Vector2(500,60));ui.close=Button(ui.panel,"Close","Đóng [Esc]",new Vector2(665,335),new Vector2(230,65));
   ui.grid=Box(ui.panel,"Grid",new Vector2(-320,-20),new Vector2(880,530));var grid=ui.grid.gameObject.AddComponent<GridLayoutGroup>();grid.cellSize=new Vector2(260,220);grid.spacing=new Vector2(24,24);grid.constraint=GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=3;
   for(int i=0;i<ui.database.definitions.Count;i++){var b=Button(ui.grid,"Slot"+i,"",Vector2.zero,new Vector2(260,220));Object.DestroyImmediate(b.GetComponentInChildren<Text>().gameObject);var slot=b.gameObject.AddComponent<GuardianInventorySlot>();slot.button=b;slot.border=b.GetComponent<Image>();var fill=Box(b.transform,"Fill",Vector2.zero,new Vector2(248,208)).gameObject.AddComponent<Image>();fill.color=new Color(.06f,.12f,.18f);fill.raycastTarget=false;var icon=Box(b.transform,"Icon",new Vector2(0,28),new Vector2(100,100)).gameObject.AddComponent<Image>();icon.raycastTarget=false;slot.icon=icon;slot.label=Text(b.transform,"Name","",new Vector2(0,-57),new Vector2(245,55),22);slot.count=Text(b.transform,"Quantity","",new Vector2(88,76),new Vector2(70,40),26);}
   ui.empty=Text(ui.panel,"Empty","Kho đồ đang trống",new Vector2(-320,-20),new Vector2(800,100),32);
   ui.title=Text(ui.panel,"ItemName","Chọn một món đồ",new Vector2(520,170),new Vector2(460,100),34);ui.description=Text(ui.panel,"Description","",new Vector2(520,40),new Vector2(460,150),26);ui.quantity=Text(ui.panel,"Count","",new Vector2(520,-85),new Vector2(460,60),26);ui.action=Button(ui.panel,"Action","Trồng",new Vector2(520,-180),new Vector2(330,70));ui.tooltip=Text(ui.panel,"Tooltip","Đứng gần luống đất trống",new Vector2(520,-260),new Vector2(480,85),22);
   Directory.CreateDirectory("Assets/IUH_Campus/Prefabs/Guardian");PrefabUtility.SaveAsPrefabAsset(root,"Assets/IUH_Campus/Prefabs/Guardian/GuardianInventoryUI.prefab");
   ui.isolatedPreview=true;
   var debug=Button(ui.safeArea,"EditorDebug","+ đồ thử",new Vector2(0,-450),new Vector2(250,60));root.AddComponent<GuardianInventoryDebug>().add=debug;
   Directory.CreateDirectory("Assets/IUH_Campus/Scenes/Test");EditorSceneManager.SaveScene(scene,"Assets/IUH_Campus/Scenes/Test/GuardianInventoryTest.unity");EditorSceneManager.CloseScene(scene,true);SceneManager.SetActiveScene(campus);return "Created prefab/test scene; campus dirty="+campus.isDirty;
  }
 }
}
