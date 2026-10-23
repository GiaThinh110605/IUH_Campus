using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IUHCampus.Core
{
    /// <summary>
    /// Safe, crash-free input utility for IUH Campus.
    /// Unifies Unity New Input System, Gamepad, and Virtual Mobile Controls.
    /// Never throws InvalidOperationException when Input System Package is active.
    /// </summary>
    public static class IUHInputHelper
    {
        // Virtual Mobile Inputs (set by IUHMobileControlsUI)
        public static Vector2 VirtualMoveAxis = Vector2.zero;
        public static bool VirtualVehiclePressed = false;
        public static bool VirtualInteractPressed = false;
        public static bool VirtualTurboHeld = false;
        public static bool VirtualBrakeHeld = false;

        public static Vector2 GetMovementAxis()
        {
            Vector2 axis = Vector2.zero;

            // 1. Virtual Mobile Joystick
            if (VirtualMoveAxis.sqrMagnitude > 0.01f)
            {
                axis += VirtualMoveAxis;
            }

            // 2. New Input System Keyboard
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) axis.y += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) axis.y -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) axis.x += 1f;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) axis.x -= 1f;
            }

            if (Gamepad.current != null)
            {
                Vector2 stick = Gamepad.current.leftStick.ReadValue();
                if (stick.sqrMagnitude > 0.04f) axis += stick;
            }
#endif

            // 3. Legacy Input Fallback
            if (axis.sqrMagnitude < 0.01f)
            {
                try
                {
                    float h = Input.GetAxisRaw("Horizontal");
                    float v = Input.GetAxisRaw("Vertical");
                    axis = new Vector2(h, v);
                }
                catch { }
            }

            return Vector2.ClampMagnitude(axis, 1f);
        }

        public static bool IsInteractKeyDown()
        {
            if (VirtualInteractPressed)
            {
                VirtualInteractPressed = false;
                return true;
            }

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
                return true;
#endif
            try { return Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space); } catch { return false; }
        }

        public static bool IsVehicleKeyDown()
        {
            if (VirtualVehiclePressed)
            {
                VirtualVehiclePressed = false;
                return true;
            }

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                return true;
#endif
            try { return Input.GetKeyDown(KeyCode.E); } catch { return false; }
        }

        public static bool IsTurboHeld()
        {
            if (VirtualTurboHeld) return true;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed))
                return true;
#endif
            try { return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); } catch { return false; }
        }

        public static bool IsBrakeHeld()
        {
            if (VirtualBrakeHeld) return true;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
                return true;
#endif
            try { return Input.GetKey(KeyCode.Space); } catch { return false; }
        }

        public static bool IsMenuKeyDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
                return true;
#endif
            try { return Input.GetKeyDown(KeyCode.M); } catch { return false; }
        }

        public static bool IsEscapeKeyDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                return true;
#endif
            try { return Input.GetKeyDown(KeyCode.Escape); } catch { return false; }
        }

        public static bool IsCustomizationKeyDown()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.uKey.wasPressedThisFrame)
                return true;
#endif
            try { return Input.GetKeyDown(KeyCode.U); } catch { return false; }
        }

        public static bool IsNumberKeyDown(int num)
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                switch (num)
                {
                    case 1: if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame) return true; break;
                    case 2: if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame) return true; break;
                    case 3: if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame) return true; break;
                    case 4: if (Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame) return true; break;
                    case 5: if (Keyboard.current.digit5Key.wasPressedThisFrame || Keyboard.current.numpad5Key.wasPressedThisFrame) return true; break;
                    case 6: if (Keyboard.current.digit6Key.wasPressedThisFrame || Keyboard.current.numpad6Key.wasPressedThisFrame) return true; break;
                }
            }
#endif
            try
            {
                switch (num)
                {
                    case 1: return Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1);
                    case 2: return Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2);
                    case 3: return Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3);
                    case 4: return Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4);
                    case 5: return Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5);
                    case 6: return Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6);
                }
            }
            catch { }
            return false;
        }

        // ================= PHOTO HUNTER MINIGAME INPUTS =================
        public static bool VirtualPhotoCapturePressed = false;
        public static bool VirtualPhotoTogglePressed = false;
        public static bool VirtualPhotoExitPressed = false;

        public static bool IsPhotoToggleKeyDown()
        {
            if (VirtualPhotoTogglePressed)
            {
                VirtualPhotoTogglePressed = false;
                return true;
            }
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (Keyboard.current.cKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame))
                return true;
#endif
            try { return Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.P); } catch { return false; }
        }

        public static bool IsPhotoCaptureKeyDown(bool pointerOverUI = false)
        {
            if (VirtualPhotoCapturePressed)
            {
                VirtualPhotoCapturePressed = false;
                return true;
            }
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
                return true;
            if (!pointerOverUI && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                return true;
#endif
            try { return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || (!pointerOverUI && Input.GetMouseButtonDown(0)); } catch { return false; }
        }

        public static bool IsPhotoExitKeyDown()
        {
            if (VirtualPhotoExitPressed)
            {
                VirtualPhotoExitPressed = false;
                return true;
            }
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (Keyboard.current.qKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame))
                return true;
#endif
            try { return Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape); } catch { return false; }
        }
    }
}
