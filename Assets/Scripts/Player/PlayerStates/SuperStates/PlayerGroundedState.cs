using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerState {
    protected Vector2 moveInput;

    public PlayerGroundedState(Player player, PlayerData playerData, PlayerStateMachine stateMachine, string animBoolName) : base(player, playerData, stateMachine, animBoolName) {

    }

    public override void DoChecks() {
        base.DoChecks();
    }

    public override void Enter() {
        base.Enter();
    }

    public override void Exit() {
        base.Exit();
    }

    public override void LogicUpdate() {
        moveInput = player.Input.Movement;
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();
    }
}
