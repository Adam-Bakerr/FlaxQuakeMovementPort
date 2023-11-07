using System.Collections.Generic;
using FlaxEngine;
using Game;

namespace Q3Movement
{
    /// <summary>
    /// This script handles Quake III CPM(A) mod style player movement logic.
    /// </summary>
    public class Q3PlayerController : Script
    {
        [System.Serializable]
        public class MovementSettings
        {
            public float MaxWalkSpeed;
            public float MaxRunSpeed;
            public float Acceleration;
            public float Deceleration;

            public MovementSettings(float maxWalkSpeed, float maxRunSpeed, float accel, float decel)
            {
                MaxWalkSpeed = maxWalkSpeed;
                MaxRunSpeed = maxRunSpeed;
                Acceleration = accel;
                Deceleration = decel;
            }
            public MovementSettings(float maxWalkSpeed, float accel, float decel)
            {
                MaxWalkSpeed = maxWalkSpeed;
                Acceleration = accel;
                Deceleration = decel;
            }
        }

    

        [Header("Aiming")]
         public Camera m_Camera;
         public MouseLook m_MouseLook;

        [Header("Movement")]
         public float m_Friction = 120;
         public float m_Gravity = 400;
         public float m_JumpForce = 160;
         public LayersMask WhatIsGround;
        [Tooltip("Automatically jump when holding jump button")]
         public bool m_AutoBunnyHop = false;
        [Tooltip("How precise air control is")]
         public float m_AirControl = 0.3f;
         public MovementSettings m_GroundSettings = new MovementSettings(140,210, 280, 200);
         public MovementSettings m_AirSettings = new MovementSettings(140, 40, 40);
         public MovementSettings m_StrafeSettings = new MovementSettings(20, 1000, 1000);




        public CharacterController m_Character;
        private Vector3 m_PlayerVelocity = Vector3.Zero;

        // Used to queue the next jump just before hitting the ground.
        private bool m_JumpQueued = false;

        private Vector3 m_MoveInput;
        public Actor m_CamPiv;



        private void OnUpdate()
        {
            m_MoveInput = new Vector3(InputManager.MoveInput.X, 0, InputManager.MoveInput.Y);
            
            QueueJump();

            // Set movement state.
            if (m_Character.IsGrounded)
            {
                GroundMove();
            }
            else
            {
                AirMove();
            }

            // Rotate the character and camera.
            m_MouseLook.LookRotation(Actor, m_Camera, m_CamPiv);

            // Move the character.
            m_Character.Move(m_PlayerVelocity * Time.DeltaTime);
        }

        // Queues the next jump.
        private void QueueJump()
        {
            if (m_AutoBunnyHop)
            {
                m_JumpQueued = InputManager.isJumpingHeld;
                return;
            }

            if (InputManager.isJumpingPressed && !m_JumpQueued)
            {
                m_JumpQueued = true;
            }

            if (InputManager.isJumpingReleased)
            {
                m_JumpQueued = false;
            }
        }

        // Handle air movement.
        private void AirMove()
        {
            float accel;

            var wishdir = new Vector3(m_MoveInput.X, 0, m_MoveInput.Z);
            wishdir = m_CamPiv.Transform.TransformDirection(wishdir);

            float wishspeed = wishdir.Length;
            wishspeed *= m_AirSettings.MaxWalkSpeed;

            wishdir.Normalize();

            // CPM Air control.
            float wishspeed2 = wishspeed;
            if (Vector3.Dot(m_PlayerVelocity, wishdir) < 0)
            {
                accel = m_AirSettings.Deceleration;
            }
            else
            {
                accel = m_AirSettings.Acceleration;
            }

            // If the player is ONLY strafing left or right
            if (m_MoveInput.Z == 0 && m_MoveInput.X != 0)
            {
                if (wishspeed > m_StrafeSettings.MaxWalkSpeed)
                {
                    wishspeed = m_StrafeSettings.MaxWalkSpeed;
                }

                accel = m_StrafeSettings.Acceleration;
            }

            Accelerate(wishdir, wishspeed, accel);
            if (m_AirControl > 0)
            {
                AirControl(wishdir, wishspeed2);
            }

            // Apply gravity
            m_PlayerVelocity.Y -= m_Gravity * Time.DeltaTime;
        }

        // Air control occurs when the player is in the air, it allows players to move side 
        // to side much faster rather than being 'sluggish' when it comes to cornering.
        private void AirControl(Vector3 targetDir, float targetSpeed)
        {
            // Only control air movement when moving forward or backward.
            if (Mathf.Abs(m_MoveInput.Z) < 0.001 || Mathf.Abs(targetSpeed) < 0.001)
            {
                return;
            }

            float zSpeed = m_PlayerVelocity.Y;
            m_PlayerVelocity.Y = 0;
            /* Next two lines are equivalent to idTech's VectorNormalize() */
            float speed = m_PlayerVelocity.Length;
            m_PlayerVelocity.Normalize();

            float dot = Vector3.Dot(m_PlayerVelocity, targetDir);
            float k = 32;
            k *= m_AirControl * dot * dot * Time.DeltaTime;

            // Change direction while slowing down.
            if (dot > 0)
            {
                m_PlayerVelocity.X *= speed + targetDir.X * k;
                m_PlayerVelocity.Y *= speed + targetDir.Y * k;
                m_PlayerVelocity.Z *= speed + targetDir.Z * k;

                m_PlayerVelocity.Normalize();
            }

            m_PlayerVelocity.X *= speed;
            m_PlayerVelocity.Y = zSpeed; // Note this line
            m_PlayerVelocity.Z *= speed;
        }

        // Handle ground movement.
        private void GroundMove()
        {
            // Do not apply friction if the player is queueing up the next jump
            if (!m_JumpQueued)
            {
                ApplyFriction(1.0f);
            }
            else
            {
                ApplyFriction(0);
            }

            var wishdir = new Vector3(m_MoveInput.X, 0, m_MoveInput.Z);
            wishdir = m_CamPiv.Transform.TransformDirection(wishdir);
            wishdir.Normalize();

            var wishspeed = wishdir.Length;
            wishspeed *= InputManager.isSprinting ? m_GroundSettings.MaxRunSpeed : m_GroundSettings.MaxWalkSpeed ;

            Accelerate(wishdir, wishspeed, m_GroundSettings.Acceleration);

            // Reset the gravity velocity
            m_PlayerVelocity.Y = -m_Gravity * Time.DeltaTime;

            if (m_JumpQueued)
            {
                m_PlayerVelocity.Y = m_JumpForce;
                m_JumpQueued = false;
            }
        }

        private void ApplyFriction(float t)
        {
            // Equivalent to VectorCopy();
            Vector3 vec = m_PlayerVelocity; 
            vec.Y = 0;
            float speed = vec.Length;
            float drop = 0;

            // Only apply friction when grounded.
            if (m_Character.IsGrounded)
            {
                float control = speed < m_GroundSettings.Deceleration ? m_GroundSettings.Deceleration : speed;
                drop = control * m_Friction * Time.DeltaTime * t;
            }

            float newSpeed = speed - drop;

            if (newSpeed < 0)
            {
                newSpeed = 0;
            }

            if (speed > 0)
            {
                newSpeed /= speed;
            }

            m_PlayerVelocity.X *= newSpeed;
            // playerVelocity.Y *= newSpeed;
            m_PlayerVelocity.Z *= newSpeed;
        }

        // Calculates acceleration based on desired speed and direction.
        private void Accelerate(Vector3 targetDir, float targetSpeed, float accel)
        {
            float currentspeed = Vector3.Dot(m_PlayerVelocity, targetDir);
            float addspeed = targetSpeed - currentspeed;
            if (addspeed <= 0)
            {
                return;
            }

            float accelspeed = accel * Time.DeltaTime * targetSpeed;
            if (accelspeed > addspeed)
            {
                accelspeed = addspeed;
            }

            m_PlayerVelocity.X += accelspeed * targetDir.X;
            m_PlayerVelocity.Z += accelspeed * targetDir.Z;
        }
    }
}