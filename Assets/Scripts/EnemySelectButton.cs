using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySelectButton : MonoBehaviour
{
    public GameObject EnemyPrefab;

    public void SelectEnemy()
    {
        GameObject.Find("BattleManager").GetComponent<BattleStateMachine>().EnemySelectButtonPress(EnemyPrefab); //save input
    }

    public void ToggleSelector(bool state)
    {
        EnemyPrefab.transform.Find("EnemySelector").gameObject.SetActive(state);
    }
}
