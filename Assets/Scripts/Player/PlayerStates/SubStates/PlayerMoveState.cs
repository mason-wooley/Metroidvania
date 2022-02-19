using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerGroundedState {
    public PlayerMoveState(Player player, PlayerData playerData, PlayerStateMachine stateMachine, string animBoolName) : base(player, playerData, stateMachine, animBoolName) {
    
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

        // Transition to IdleState
        if (player.desiredVelocity.magnitude == 0f && player.velocity.magnitude == 0f) {
            stateMachine.ChangeState(player.IdleState);
        }

        // Player movement
        // TODO: Revisit y and z components for alternative move directions (rotate camera, gravity, worldspace, etc)
        player.desiredVelocity = new Vector3(moveInput.x, 0f, 0f) * playerData.maxVelocity;
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();

        Vector3 xAxis = ProjectDirectionOnPlane(Vector3.right, Vector3.up);
        float currentX = Vector3.Dot(player.velocity, xAxis);
        float maxSpeedChange = playerData.maxAcceleration * Time.deltaTime;
        float newX = Mathf.MoveTowards(currentX, player.desiredVelocity.x, maxSpeedChange);

        player.velocity += xAxis * (newX - currentX);
    }
}
