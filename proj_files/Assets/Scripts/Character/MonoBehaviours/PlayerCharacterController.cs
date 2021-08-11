using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


namespace noGame.Character.MonoBehaviours
{
    [RequireComponent( typeof( CharacterController2D ) )]
    [RequireComponent( typeof( PlayerInput ) )]
    public class PlayerCharacterController : MonoBehaviour
    {

        private CharacterController2D characterController;


        private void Start()
        {
            characterController = GetComponent<CharacterController2D>();
        }

    }
}
