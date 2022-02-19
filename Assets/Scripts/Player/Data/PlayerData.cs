using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newPlayerData", menuName = "Data/Player Data/Base Data")]
public class PlayerData : ScriptableObject {
    [Header("Ground State")]
    [Range(0f, 100f)]
    public float maxVelocity = 10f;

    [Range(0f, 100f)]
    public float maxAcceleration = 10f;

    [Range(0, 90f)]
    public float maxGroundAngle = 25f;

    [Header("Air State")]
    [Range(0f, 100f)]
    public float maxAirAcceleration = 10f;
}
