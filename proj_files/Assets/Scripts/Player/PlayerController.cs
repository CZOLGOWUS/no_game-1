using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
        public PlayerMovement movementSC;
        
        
        public void OnMovement(InputAction.CallbackContext value)
        {
                movementSC.MovementValue = value.ReadValue<float>();
        }

        public void OnJump(InputAction.CallbackContext value)
        {
                movementSC.JumpBtnValue = value.ReadValueAsButton();
        }

        public void OnDownInput(InputAction.CallbackContext value)
        {
                movementSC.DownInputValue = value.ReadValueAsButton();
        }
}
