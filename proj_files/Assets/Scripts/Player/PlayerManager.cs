using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : Singleton<PlayerManager>
{
    private PlayerInput playerAction;

    private PlayerManager playerManagerSc;
    private PlayerCombatSystem playerCombatSystemSc;
    private PlayerAnimationBehaviour playerAnimationSc;


    private void Awake()
    {
        playerAction = GetComponent<PlayerInput>();

        playerCombatSystemSc = GetComponent<PlayerCombatSystem>();
        playerAnimationSc = GetComponent<PlayerAnimationBehaviour>();
        playerManagerSc = GetComponent<PlayerManager>();


    }

    private void Start()
    {
        FunctionBindings();

    }

    private void FunctionBindings()
    {
        playerAction.actions["Attack"].performed += OnPlayerAttack;
    }

    private void OnPlayerAttack( InputAction.CallbackContext value )
    {
        if( !playerCombatSystemSc.isAbleToAttack )
            return;

        StartCoroutine( playerAnimationSc.AttackPlay());
        playerCombatSystemSc.Attack();
            
    }

    public void TakeHit(GameObject enemy)
    {

        if(playerCombatSystemSc.timeSinceLastHit > playerCombatSystemSc.timeBeetwenHits)
        {

        }

    }
}
