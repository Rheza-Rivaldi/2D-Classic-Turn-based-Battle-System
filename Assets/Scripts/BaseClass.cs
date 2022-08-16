using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseClass
{
   public string name;
   
   public float baseHP;
   public float curHP;
   public float baseMP;
   public float curMP;

   public float baseAtk;
   public float curAtk;
   public float baseDef;
   public float curDef;
   public float speed;

   public List<BaseAttack> AttackList = new List<BaseAttack>();
   public List<BaseAttack> MagicAttacks = new List<BaseAttack>();

}