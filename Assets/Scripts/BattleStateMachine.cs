using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BattleStateMachine : MonoBehaviour
{
    public enum PerformAction
    {
        ACTIVATE,
        WAIT,
        TAKEACTION,
        PERFORMACTION,
        CLEANUP,
        CHECKALIVE,
        WIN,
        LOSE
    }
    public PerformAction battleState;

    public List<TurnHandler> PerformList = new List<TurnHandler>();
    public List<GameObject> HeroInBattle = new List<GameObject>();
    public List<GameObject> EnemyInBattle = new List<GameObject>();


    public enum HeroGUI
    {
        ACTIVATE,
        WAITING,
        DONE
    }
    public HeroGUI HeroInput;

    public List<GameObject> HeroToManage = new List<GameObject>();
    private TurnHandler HeroChoice;

    public string deadActor;

    public GameObject enemyButton;
    public GameObject actionButton;
    public GameObject magicButton;
    public Transform TargetSpacer;
    public Transform ActionSpacer;
    public Transform MagicSpacer;

    public GameObject ActionPanel;
    public GameObject TargetSelectPanel;
    public GameObject MagicPanel;

    List<GameObject> atkBtns = new List<GameObject>();
    public List<GameObject> enemyBtns = new List<GameObject>();

    public List<Transform> enemySpawnPoints = new List<Transform>();



    void Start()
    {
        battleState = PerformAction.ACTIVATE;
        HeroInBattle.AddRange(GameObject.FindGameObjectsWithTag("Hero"));
        EnemyInBattle.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        HeroInput = HeroGUI.ACTIVATE;

        ActionPanel.SetActive(false);
        TargetSelectPanel.SetActive(false);
        MagicPanel.SetActive(false);

        EnemyButtons();
    }

    void Update()
    {
        switch (battleState)
        {
            case(PerformAction.ACTIVATE):
            if (PerformList.Count == 0)
            {
                for(int i = 0; i < HeroInBattle.Count; i++)
                {
                    HeroInBattle[i].GetComponent<HeroStateMachine>().currentState = HeroStateMachine.TurnState.ADDTOLIST;
                }
                for(int i = 0; i < EnemyInBattle.Count; i++)
                {
                    EnemyInBattle[i].GetComponent<EnemyStateMachine>().currentState = EnemyStateMachine.TurnState.CHOOSEACTION;
                }
                battleState = PerformAction.WAIT;
            }
            break;
            case(PerformAction.WAIT):
            //check if everyone is done choosing action
            if(PerformList.Count == HeroInBattle.Count + EnemyInBattle.Count)
            {
                //sort by speed before changing to action state
                PerformList.Sort((act1, act2) => act2.AttackerSpeed.CompareTo(act1.AttackerSpeed) );
                battleState = PerformAction.TAKEACTION;
            }
            break;

            case(PerformAction.TAKEACTION):
            GameObject performer = GameObject.Find(PerformList[0].AttackerName);

            if(PerformList[0].Type == "Enemy")
            {
                EnemyStateMachine ESM = performer.GetComponent<EnemyStateMachine>();
                for(int i = 0; i<HeroInBattle.Count;i++){
                    if(PerformList[0].TargetGameObject == HeroInBattle[i])
                    {
                        ESM.AttackTarget = PerformList[0].TargetGameObject;
                        ESM.currentState = EnemyStateMachine.TurnState.ACTION;
                        break;
                    }
                    else
                    {
                        PerformList[0].TargetGameObject = HeroInBattle[Random.Range(0, HeroInBattle.Count)];
                        ESM.AttackTarget = PerformList[0].TargetGameObject;
                        ESM.currentState = EnemyStateMachine.TurnState.ACTION;
                    }
                }
                
            }

            if(PerformList[0].Type == "Hero")
            {
                HeroStateMachine HSM = performer.GetComponent<HeroStateMachine>();
                HSM.AttackTarget = PerformList[0].TargetGameObject;
                HSM.currentState = HeroStateMachine.TurnState.ACTION;
            }

            battleState = PerformAction.PERFORMACTION;
            break;

            case(PerformAction.PERFORMACTION):
            //idling till enumerator is done
            break;

            case(PerformAction.CLEANUP):
            ClearDeadActors();
            if(EnemyInBattle.Count < 1 || HeroInBattle.Count < 1)
            {
                battleState = PerformAction.CHECKALIVE;
            }
            else
            {
                if(PerformList.Count != 0){
                    battleState = PerformAction.TAKEACTION;
                }
                else{
                    battleState = PerformAction.ACTIVATE;
                }
            }
            break;

            case(PerformAction.CHECKALIVE):
            if(HeroInBattle.Count<1)
            {
                battleState = PerformAction.LOSE;
            }
            else if (EnemyInBattle.Count<1)
            {
                battleState = PerformAction.WIN;
            }
            else
            {
                ClearAttackPanel();
                HeroInput = HeroGUI.ACTIVATE;
            }
            break;

            case(PerformAction.WIN):
            for(int i = 0; i < HeroInBattle.Count; i++)
            {
                HeroInBattle[i].GetComponent<HeroStateMachine>().currentState = HeroStateMachine.TurnState.WAITING;
            }
            break;

            case(PerformAction.LOSE):
            for(int i = 0; i < EnemyInBattle.Count; i++)
            {
                EnemyInBattle[i].GetComponent<EnemyStateMachine>().currentState = EnemyStateMachine.TurnState.WAITING;
            }
            break;
        }

        switch(HeroInput)
        {
            case(HeroGUI.ACTIVATE):
            if(HeroToManage.Count > 0)
            {
                HeroToManage[0].transform.Find("HeroSelector").gameObject.SetActive(true);
                HeroChoice = new TurnHandler();
                ActionPanel.SetActive(true);
                CreateAttackButton();

                HeroInput = HeroGUI.WAITING;
            }
            break;

            case(HeroGUI.WAITING):
            //idle
            break;

            case(HeroGUI.DONE):
            HeroInputDone();
            break;
        }
    }

    public void CollectAction(TurnHandler input)
    {
        PerformList.Add(input);
    }

    public void EnemyButtons()
    {
        foreach(GameObject enemyBtn in enemyBtns)
        {
            Destroy(enemyBtn);
        }
        enemyBtns.Clear();
        foreach (GameObject enemy in EnemyInBattle)
        {
            GameObject newButton = Instantiate(enemyButton) as GameObject;
            EnemySelectButton button = newButton.GetComponent<EnemySelectButton>();
            EnemyStateMachine cur_enemy = enemy.GetComponent<EnemyStateMachine>();

            Text buttonText = newButton.transform.Find("Text").gameObject.GetComponent<Text>();
            buttonText.text = cur_enemy.enemy.name;

            button.EnemyPrefab = enemy;

            newButton.transform.SetParent(TargetSpacer,false);
            enemyBtns.Add(newButton);
        }
    }

    public void AttackButtonPress()
    {
        ActionPanel.SetActive(false);
        TargetSelectPanel.SetActive(true);
        HeroChoice.choosenAttack = HeroToManage[0].GetComponent<HeroStateMachine>().hero.AttackList[0];
    }

    public void MagicButtonPress()
    {
        if(HeroToManage[0].GetComponent<HeroStateMachine>().hero.MagicAttacks.Count>0)
        {
            ActionPanel.SetActive(false);
            MagicPanel.SetActive(true);
        }
        return;
    }

    public void MagicAttackButtonPress(BaseAttack choosenMagic)
    {
        MagicPanel.SetActive(false);
        TargetSelectPanel.SetActive(true);
        HeroChoice.choosenAttack = choosenMagic;
    }

    public void EnemySelectButtonPress(GameObject choosenEnemy)
    {
        HeroChoice.AttackerName = HeroToManage[0].name;
        HeroChoice.AttackerGameObject = HeroToManage[0];
        HeroChoice.Type = "Hero";
        HeroChoice.TargetGameObject = choosenEnemy;
        HeroInput = HeroGUI.DONE;
    }

    public void HeroInputDone()
    {
        HeroChoice.AttackerSpeed = HeroToManage[0].GetComponent<HeroStateMachine>().hero.speed;
        PerformList.Add (HeroChoice);
        ClearAttackPanel();

        HeroToManage[0].transform.Find("HeroSelector").gameObject.SetActive(false);
        HeroToManage.RemoveAt(0);
        HeroInput = HeroGUI.ACTIVATE;
    }

    void ClearAttackPanel()
    {
        TargetSelectPanel.SetActive(false);
        ActionPanel.SetActive(false);
        MagicPanel.SetActive(false);

        foreach(GameObject atkBtn in atkBtns)
        {
            Destroy(atkBtn);
        }
        atkBtns.Clear();
    }

    //delete dead actor from perform list
    void ClearDeadActors()
    {
        for(int i = 0; i<PerformList.Count;i++)
        {
            if(deadActor == PerformList[i].AttackerName)
            {
                if(PerformList[i].AttackerGameObject.tag == "DeadHero" || PerformList[i].AttackerGameObject.tag == "DeadEnemy")
                {
                    PerformList.RemoveAt(i);
                }
            }
        }
    }

    void CreateAttackButton()
    {
        GameObject AttackButton = Instantiate(actionButton) as GameObject;
        Text AttackButtontext = AttackButton.transform.Find("Text").gameObject.GetComponent<Text>();
        AttackButtontext.text = "Attack";
        AttackButton.GetComponent<Button>().onClick.AddListener(()=>AttackButtonPress());
        AttackButton.transform.SetParent(ActionSpacer, false);
        atkBtns.Add(AttackButton);

        GameObject MagicButton = Instantiate(actionButton) as GameObject;
        Text MagicButtontext = MagicButton.transform.Find("Text").gameObject.GetComponent<Text>();
        MagicButtontext.text = "Magic";
        MagicButton.GetComponent<Button>().onClick.AddListener(()=>MagicButtonPress());
        MagicButton.transform.SetParent(ActionSpacer, false);
        atkBtns.Add(MagicButton);

        if(HeroToManage[0].GetComponent<HeroStateMachine>().hero.MagicAttacks.Count>0)
        {
            foreach(BaseAttack magicatk in HeroToManage[0].GetComponent<HeroStateMachine>().hero.MagicAttacks)
            {
                GameObject MagicAttackButton = Instantiate(magicButton) as GameObject;
                Text MagicAttackButtontext = MagicAttackButton.transform.Find("Text").gameObject.GetComponent<Text>();
                MagicAttackButtontext.text = magicatk.attackName;
                MagicAttackButton.GetComponent<Button>().onClick.AddListener(()=>MagicAttackButtonPress(magicatk));
                //MagicAttackButtonScript MTB = MagicAttackButton.GetComponent<MagicAttackButtonScript>();
                //MTB.magicAttackToPerform = magicatk;
                MagicAttackButton.transform.SetParent(MagicSpacer, false);
                atkBtns.Add(MagicAttackButton);
            }
        }
        else
        {
            MagicButton.GetComponent<Button>().interactable = false;
        }
    }
}
