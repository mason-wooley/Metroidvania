using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState {
    protected Player player;
    protected PlayerData playerData;
    protected PlayerStateMachine stateMachine;

    // Track time since beginning of state transition
    protected float startTime;

    private string animBoolName;

    public PlayerState (Player player, PlayerData playerData, PlayerStateMachine stateMachine, string animBoolName) {
        this.player = player;
        this.playerData = playerData;
        this.stateMachine = stateMachine;
        this.animBoolName = animBoolName;
    }

    public virtual void Enter () {
        DoChecks();
        player.Anim.SetBool(animBoolName, true);
        startTime = Time.time;
        Debug.Log(animBoolName);
    }

    public virtual void Exit () {
        player.Anim.SetBool(animBoolName, true);
    }

    public virtual void LogicUpdate () {

    }

    public virtual void PhysicsUpdate () {
        DoChecks();
    }

    public virtual void DoChecks () {

    }
}
