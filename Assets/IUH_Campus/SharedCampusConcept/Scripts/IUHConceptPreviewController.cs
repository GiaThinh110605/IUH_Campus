using UnityEngine;
using StarterAssets;
using IUHCampus.Guardian.Match3;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IUHCampus.World
{

    [DefaultExecutionOrder(-100)]
    public class IUHConceptPreviewController : MonoBehaviour
    {
        public Camera overviewCamera;
        public Vector3 entrance=new Vector3(0,.2f,-38);
        Camera walkingCamera;
        ThirdPersonController movement;
        private static int escapeHandledFrame = -1;
        public static bool EscapeHandledThisFrame => escapeHandledFrame == Time.frameCount;
        bool overview,freeCursor;
        public bool IsExplorationPaused => overview || freeCursor;
        void Start(){walkingCamera=Camera.main;movement=GetComponent<ThirdPersonController>();Apply();}
        void Update()
        {
            if (Match3CombatManager.Instance != null && Match3CombatManager.Instance.BlocksExplorationInput) return;
            bool view=false,escape=false;
#if ENABLE_INPUT_SYSTEM
            if(Keyboard.current!=null){view=Keyboard.current.vKey.wasPressedThisFrame;escape=Keyboard.current.escapeKey.wasPressedThisFrame;}
#else
            view=Input.GetKeyDown(KeyCode.V);escape=Input.GetKeyDown(KeyCode.Escape);
#endif
            if(view){overview=!overview;Apply();}
            if(escape){escapeHandledFrame=Time.frameCount;freeCursor=!freeCursor;Apply();}
            if(transform.position.y<-6)
            {
                var cc=GetComponent<CharacterController>();bool enabled=cc!=null&&cc.enabled;if(cc!=null)cc.enabled=false;transform.position=entrance;if(cc!=null)cc.enabled=enabled;movement?.ResetMotion();
            }
        }
        void Apply()
        {
            if(walkingCamera!=null)walkingCamera.enabled=!overview;
            if(overviewCamera!=null)overviewCamera.enabled=overview;
            if(movement!=null){movement.ResetMotion();movement.enabled=!overview&&!freeCursor;}
            Cursor.lockState=overview||freeCursor?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=overview||freeCursor;
        }
    }
}
