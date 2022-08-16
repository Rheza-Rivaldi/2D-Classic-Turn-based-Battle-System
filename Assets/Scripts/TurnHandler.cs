using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TurnHandler
{
    public string AttackerName; //name of attacker
    public string Type; //hero or enemy type
    public GameObject AttackerGameObject; //gameobject of attacker
    public GameObject TargetGameObject; //target's gameobject
    public float AttackerSpeed; //attacker's speed

    public BaseAttack choosenAttack;
}
