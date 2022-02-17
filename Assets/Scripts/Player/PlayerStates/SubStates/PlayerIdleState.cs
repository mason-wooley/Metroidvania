using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerGroundedState {
    public PlayerIdleState(Player player, PlayerData playerData, PlayerStateMachine stateMachine, string animBoolName) : base(player, playerData, stateMachine, animBoolName) {

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
        base.LogicUpdate();

        if (moveInput.x != 0f) {
            stateMachine.ChangeState(player.MoveState);
        }
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();
    }
}
