using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatSystem : MonoBehaviour
{

    private PlayerInput playerAction;

    private void Awake()
    {
        playerAction.actions["Attack"].performed += Attack;
    }

    private void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Attack(InputAction.CallbackContext value)
    {

    }
}
