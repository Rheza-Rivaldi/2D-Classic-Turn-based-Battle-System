using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroStateMachine : MonoBehaviour
{
    BattleStateMachine BSM;
    public BaseClass hero;

    Vector3 startposition;


    public enum TurnState
    {
        ADDTOLIST,
        WAITING,
        ACTION,
        DEAD
    }
    public TurnState currentState;

    //IEnumerator
    bool actionStarted = false;
    public GameObject AttackTarget;
    float animSpeed = 10f;

    bool alive = true;

    //heropanel
    HeroPanelStats stats;
    public GameObject HeroPanel;
    Transform HeroPanelSpacer;


    void Start()
    {
        //instantiate heropanel
        HeroPanelSpacer = GameObject.Find("HeroPanelSpacer").transform;
        CreateHeroPanel();

        currentState = TurnState.WAITING;
        BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        startposition = transform.position;
    }

    void Update()
    {
        switch (currentState)
        {
            case(TurnState.ADDTOLIST):
            BSM.HeroToManage.Add(this.gameObject);
            currentState = TurnState.WAITING;
            break;

            case(TurnState.WAITING):
            break;

            case(TurnState.ACTION):
            StartCoroutine(TimeForAction());
            break;

            case(TurnState.DEAD):
            if(!alive)
            {
                return;
            }
            else
            {
                //change tag
                this.gameObject.tag = "DeadHero";
                //not attackable
                BSM.HeroInBattle.Remove(this.gameObject);
                //not manageable
                BSM.HeroToManage.Remove(this.gameObject);
                //deactivate selector
                this.transform.Find("HeroSelector").gameObject.SetActive(false);
                //reset GUI
                BSM.ActionPanel.SetActive(false);
                BSM.TargetSelectPanel.SetActive(false);
                //remove from performlist
                if(BSM.HeroInBattle.Count > 0)
                {
                    for(int i = 0; i < BSM.PerformList.Count; i++)
                    {
                        if(i != 0)
                        {
                            if(BSM.PerformList[i].AttackerGameObject == this.gameObject)
                            {
                                BSM.deadActor = BSM.PerformList[i].AttackerName;
                                //BSM.PerformList.Remove(BSM.PerformList[i]);
                            }
                            if(BSM.PerformList[i].TargetGameObject == this.gameObject)
                            {
                                BSM.PerformList[i].TargetGameObject = BSM.HeroInBattle[Random.Range(0, BSM.HeroInBattle.Count)];
                            }
                        }
                    }
                }
                //change color
                this.gameObject.GetComponent<SpriteRenderer>().color = new Color32(105,105,105,255);
                //reset heroinput
                BSM.battleState = BattleStateMachine.PerformAction.CHECKALIVE;
                alive = false;
            }
            break;
        }
    }

    IEnumerator TimeForAction()
    {
        if (actionStarted){yield break;}
        actionStarted = true;

        //move to target
        Vector2 targetPosition = new Vector3(AttackTarget.transform.position.x+1f,AttackTarget.transform.position.y,AttackTarget.transform.position.z);
        //ChangeAnimationState(HERO_WALK);
        while(MoveTowardTarget(targetPosition))
        {
            yield return null;
        }
        //ChangeAnimationState(HERO_IDLE);
        //wait a bit
        yield return new WaitForSeconds(0.25f);
        //do damage
        DoDamage();
        yield return new WaitForSeconds(0.25f);
        //go back to original position
        Vector3 firstPosition = startposition;
        //ChangeAnimationState(HERO_WALK);
        while(MoveTowardStart(firstPosition))
        {
            this.gameObject.GetComponent<SpriteRenderer>().flipX = false;
            yield return null;
        }
        this.gameObject.GetComponent<SpriteRenderer>().flipX = true;
        //ChangeAnimationState(HERO_IDLE);
        //remove from perform list
        if(this.gameObject.tag != "DeadHero")
        {
            BSM.PerformList.RemoveAt(0);
        }
        //reset bsm state to waiting
        if(BSM.battleState != BattleStateMachine.PerformAction.WIN && BSM.battleState != BattleStateMachine.PerformAction.LOSE)
        {
            BSM.battleState = BattleStateMachine.PerformAction.CLEANUP;
            //end coroutine
            actionStarted = false;
            //reset this object state
            currentState = TurnState.WAITING;
        }
        else
        {
            currentState = TurnState.WAITING;
        }
    }

    private bool MoveTowardTarget (Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
    }
    private bool MoveTowardStart (Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
    }

    void CreateHeroPanel()
    {
        HeroPanel = Instantiate(HeroPanel) as GameObject;
        stats = HeroPanel.GetComponent<HeroPanelStats>();
        stats.HeroName.text = hero.name;
        stats.HeroHP.text = "HP: " + hero.curHP;
        stats.HeroMP.text = "MP: " + hero.curMP;

        HeroPanel.transform.SetParent(HeroPanelSpacer, false);
    }

    void UpdateHeroPanel()
    {
        stats.HeroHP.text = "HP: " + hero.curHP;
        stats.HeroMP.text = "MP: " + hero.curMP;
    }

    public void TakeDamage (float damageAmount)
    {
        hero.curHP -= ((int)damageAmount);
        if (hero.curHP <= 0)
        {
            hero.curHP = 0;
            currentState = TurnState.DEAD;
        }
        UpdateHeroPanel();
    }

    void DoDamage()
    {
        float calc_damage = hero.curAtk * BSM.PerformList[0].choosenAttack.attackDamage;

        AttackTarget.GetComponent<EnemyStateMachine>().TakeDamage(calc_damage);
    }
}
