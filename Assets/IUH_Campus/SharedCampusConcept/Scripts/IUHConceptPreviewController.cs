using UnityEngine;
using StarterAssets;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IUHCampus.World
{
    // Local controls for the standalone campus art scene.
    public class IUHConceptPreviewController : MonoBehaviour
    {
        public Camera overviewCamera;
        public Vector3 entrance=new Vector3(0,.2f,-38);
        Camera walkingCamera;
        ThirdPersonController movement;
        bool overview,freeCursor;
        void Start(){walkingCamera=Camera.main;movement=GetComponent<ThirdPersonController>();Apply();}
        void Update()
        {
            bool view=false,escape=false;
#if ENABLE_INPUT_SYSTEM
            if(Keyboard.current!=null){view=Keyboard.current.vKey.wasPressedThisFrame;escape=Keyboard.current.escapeKey.wasPressedThisFrame;}
#else
            view=Input.GetKeyDown(KeyCode.V);escape=Input.GetKeyDown(KeyCode.Escape);
#endif
            if(view){overview=!overview;Apply();}
            if(escape){freeCursor=!freeCursor;Apply();}
            if(transform.position.y<-6)
            {
                var cc=GetComponent<CharacterController>();bool enabled=cc!=null&&cc.enabled;if(cc!=null)cc.enabled=false;transform.position=entrance;if(cc!=null)cc.enabled=enabled;movement?.ResetMotion();
            }
        }
        void Apply()
        {
            if(walkingCamera!=null)walkingCamera.enabled=!overview;
            if(overviewCamera!=null)overviewCamera.enabled=overview;
            if(movement!=null)movement.enabled=!overview&&!freeCursor;
            Cursor.lockState=overview||freeCursor?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=overview||freeCursor;
        }
        void OnGUI()
        {
            var scale=Mathf.Clamp(Screen.height/900f,.65f,1.6f);var old=GUI.matrix;GUI.matrix=Matrix4x4.Scale(Vector3.one*scale);
            var title=new GUIStyle(GUI.skin.label){fontSize=25,fontStyle=FontStyle.Bold};title.normal.textColor=Color.white;
            var text=new GUIStyle(GUI.skin.label){fontSize=15};text.normal.textColor=new Color(.85f,.91f,.92f);
            GUI.color=new Color(.025f,.08f,.12f,.86f);GUI.DrawTexture(new Rect(24,22,440,104),Texture2D.whiteTexture);GUI.color=Color.white;
            GUI.Label(new Rect(42,32,400,38),"CAMPUS ADVENTURE",title);
            GUI.Label(new Rect(42,70,400,26),overview?"Khuôn viên chung • Góc nhìn toàn cảnh":"Khuôn viên chung • Tự do khám phá",text);
            GUI.Label(new Rect(42,96,420,25),"WASD: đi • Space: nhảy • V: toàn cảnh • Esc: chuột",text);GUI.matrix=old;
        }
    }
}
