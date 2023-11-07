using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game
{
    /// <summary>
    /// InputManager Script.
    /// </summary>
    public class InputManager : Script
    {
        #region Mouse

        public static float HorizontalInput;
        public static float VerticalInput;
        public static bool isFiring;
        public static bool isScoping;

        bool mouseLock = true;

        #endregion

        #region Keyboard

        public static Vector2 MoveInput;
        public static bool isJumpingPressed;
        public static bool isJumpingHeld;
        public static bool isJumpingReleased;
        public static bool isSprinting;
        public static bool isCrouching;

        #endregion


        void OnStart()
        {
            LockMouse();
        }

        void LockMouse()
        {
            Screen.CursorLock = CursorLockMode.Locked;
            Screen.CursorVisible = false;
        }

        void UnlockMouse()
        {
            Screen.CursorLock = CursorLockMode.None;
            Screen.CursorVisible = true;
        }

        public override void OnFixedUpdate()
        {
            if (!mouseLock) return;
            HorizontalInput = Input.GetAxis("Mouse X");
            VerticalInput = Input.GetAxis("Mouse Y");
        }
        

        public override void OnUpdate()
        {
            if(Input.GetKeyDown(KeyboardKeys.Escape)) mouseLock = !mouseLock;

            if(mouseLock)LockMouse();
            else
            {
                UnlockMouse();
                return;
            };


            MoveInput.X = Input.GetAxisRaw("Horizontal");
            MoveInput.Y = Input.GetAxisRaw("Vertical");


            isFiring = Input.GetAction("Fire");
            isScoping = Input.GetAction("Scope");

            isJumpingPressed = Input.GetAction("Jump");
            isJumpingHeld = Input.GetActionState("Jump") == InputActionState.Pressing;
            isJumpingReleased = Input.GetActionState("Jump") == InputActionState.Release;
            isSprinting = Input.GetAction("Sprint");
            isCrouching = Input.GetAction("Crouch");
        }
    }
}
