using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : Singleton<PlayerManager>
{
    private PlayerInput playerAction;

    private PlayerCombatSystem playerCombatSystemSc;
    private PlayerAnimationBehaviour playerAnimationSc;

    private Rigidbody2D playersRB;


    private void Awake()
    {
        playersRB = GetComponent<Rigidbody2D>();
        playerAction = GetComponent<PlayerInput>();

        playerCombatSystemSc = GetComponent<PlayerCombatSystem>();
        playerAnimationSc = GetComponent<PlayerAnimationBehaviour>();



    }

    private void Start()
    {
        FunctionBindings();

    }

    private void FunctionBindings()
    {
        playerAction.actions["Attack"].performed += OnPlayerAttack;
    }

    public Rigidbody2D GetPlayersRigidBody()
    {
        return playersRB;
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
        playerCombatSystemSc.TakeHit(enemy);
    }

    public bool IsPlayerInvincible()
    {
        return playerCombatSystemSc.isInvincible;
    }

}
