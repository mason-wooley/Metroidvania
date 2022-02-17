using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerGroundedState {
    public PlayerMoveState(Player player, PlayerData playerData, PlayerStateMachine stateMachine, string animBoolName) : base(player, playerData, stateMachine, animBoolName) {
    
    }
}
