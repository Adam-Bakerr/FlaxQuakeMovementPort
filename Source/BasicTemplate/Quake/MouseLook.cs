using System;
using FlaxEngine;


namespace Q3Movement
{
    /// <summary>
    /// Custom script based on the version from the Standard Assets.
    /// </summary>
    [Serializable]
    public class MouseLook : Script
    {
        private float m_XSensitivity = .2f;
        private float m_YSensitivity = .2f;
        private float m_MinimumX = -90F;
        private float m_MaximumX = 90F;
        private bool m_LockCursor = true;

        Vector2 CurrentLook;
        private bool m_cursorIsLocked = true;


        public void LookRotation(Actor character, Camera camera , Actor CamPiv)
        {
            Vector2 mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            mouseInput.X *= m_XSensitivity;
            mouseInput.Y *= m_YSensitivity;

            CurrentLook.X += mouseInput.X;
            CurrentLook.Y = Mathf.Clamp(CurrentLook.Y += mouseInput.Y, -90, 90);

            CamPiv.LocalOrientation = Quaternion.Euler(CurrentLook.Y, CurrentLook.X, 0);

        }

        public void SetCursorLock(bool value)
        {
            m_LockCursor = value;
            if (!m_LockCursor)
            {//we force unlock the cursor if the user disable the cursor locking helper
                Screen.CursorLock = CursorLockMode.None;
                Screen.CursorVisible = true;
            }
        }



        private Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.X /= q.W;
            q.Y /= q.W;
            q.Z /= q.W;
            q.W = 1.0f;

            float angleX = 2.0f * Mathf.RadiansToDegrees * Mathf.Atan(q.X);

            angleX = Mathf.Clamp(angleX, m_MinimumX, m_MaximumX);

            q.X = Mathf.Tan(0.5f * Mathf.DegreesToRadians * angleX);

            return q;
        }
    }
}
