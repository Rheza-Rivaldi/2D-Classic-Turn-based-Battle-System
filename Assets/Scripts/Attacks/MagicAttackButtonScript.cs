using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicAttackButtonScript : MonoBehaviour
{
    public BaseAttack magicAttackToPerform;

    public void CastMagicAttack()
    {
        GameObject.Find("BattleManager").GetComponent<BattleStateMachine>().MagicAttackButtonPress(magicAttackToPerform);
    }
}
