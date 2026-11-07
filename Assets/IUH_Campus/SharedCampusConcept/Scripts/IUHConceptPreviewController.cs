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
        public static bool IsAnyUIOpen()
        {
            if (IUHCampus.DailyActivities.IUHCampusActivitiesHUD.Instance != null)
            {
                if (IUHCampus.DailyActivities.IUHCampusActivitiesHUD.Instance.IsMascotDialogOpen ||
                    IUHCampus.DailyActivities.IUHCampusActivitiesHUD.Instance.IsAlbumOpen)
                    return true;
            }
            if (IUHCampus.Combat.IUHCombatHUD.Instance != null && IUHCampus.Combat.IUHCombatHUD.Instance.IsGuideOpen)
                return true;
            if (IUHCampus.Guardian.Match3.Match3CombatManager.Instance != null && IUHCampus.Guardian.Match3.Match3CombatManager.Instance.IsCombatActive)
                return true;
            return false;
        }

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
            if (escape)
            {
                escapeHandledFrame = Time.frameCount;
                if (!IsAnyUIOpen()) freeCursor = !freeCursor;
                Apply();
            }
            if (IsAnyUIOpen())
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (movement != null) movement.enabled = false;
            }
            if(transform.position.y<-6)
            {
                var cc=GetComponent<CharacterController>();bool enabled=cc!=null&&cc.enabled;if(cc!=null)cc.enabled=false;transform.position=entrance;if(cc!=null)cc.enabled=enabled;movement?.ResetMotion();
            }
        }
        public void Apply()
        {
            bool uiOpen = IsAnyUIOpen();
            bool unlock = overview || freeCursor || uiOpen;
            if(walkingCamera!=null)walkingCamera.enabled=!overview;
            if(overviewCamera!=null)overviewCamera.enabled=overview;
            if(movement!=null){movement.ResetMotion();movement.enabled=!overview&&!freeCursor&&!uiOpen;}
            Cursor.lockState=unlock?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=unlock;
        }
    }
}
