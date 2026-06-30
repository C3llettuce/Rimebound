using System.Collections.Generic;
using System.Diagnostics;


public enum AttackType
{
    None = 0,
    SlayerShot = 1,
    Starfall = 2,
    Zealotry = 3
}

public class Attack
{
    public string name;
    public int usePosition, targetPosition;
    public StatusType[] status; public int[] statusDuration;
    public BattleScene bs;
    public int damage; 
    private int moraleDamage;
    public bool isBuff;
    public bool isAoe;
    public bool isTileTargeted;
    private AttackType attackType;
    public Attack(string name, BattleScene bs, StatusType[] status = null, int[] statusDuration = null, int usePosition = 63, int targetPosition = 63, int damage = 1, int moraleDamage = 0, bool isBuff = false, bool isAoe = false, bool isTileTargeted = false, AttackType attackType = AttackType.None)
    {
        this.name = name;
        this.bs = bs;
        this.usePosition = usePosition;
        this.targetPosition = targetPosition;
        this.status = status;
        this.statusDuration = statusDuration;
        this.damage = damage;
        this.isBuff = isBuff;
        this.isAoe = isAoe;
        this.attackType = attackType;
        this.isTileTargeted = isTileTargeted;
    }
    public Attack(string name, BattleScene bs, StatusType status = 0, int statusDuration = 0, int usePosition = 63, int targetPosition = 63, int damage = 1, int moraleDamage = 0, bool isBuff = false, bool isAoe = false, bool isTileTargeted = false, AttackType attackType = AttackType.None):
    this(name, bs, [status], [statusDuration], usePosition, targetPosition, damage, moraleDamage, isBuff, isAoe, isTileTargeted, attackType){}

    //check here instead of in b-mngr to allow for uncluttered special cases (maybe should just give each attack its own class? idk)
    public bool CheckValidAttack(int usePosition, int targetPosition)
    {

        switch (attackType)
        {
            case AttackType.Starfall:
            //default case for targetting a specific tile from a tile
            default:
                if((this.usePosition & usePosition) != 0 && (this.targetPosition & targetPosition) != 0) return true;
                break;
        }
        return false;
    }

    public void Use(List<Actor> targets, Actor user)
    {
        foreach(Actor a in targets)
        {
            Use(a, user);
        }
    }

    public void Use(Actor target = null, Actor user = null, TileCollider tc = null)
    {
        switch (attackType)
        {
            case AttackType.Zealotry:
                foreach(Hero h in bs.heroes) if(h.anima > 0 && h != target) BasicUse(h, user);
                goto default;
            case AttackType.Starfall:
                tc.tileState = TileState.Starfall;
                tc.stateDuration = 3;
                break;
            case AttackType.SlayerShot:
                if (target.statuses[(int)StatusType.Mark] > 0) damage += 3;
                if (target.statuses[(int)StatusType.Snare] > 0) damage += 3;
                goto default;

            //default case for all attacks that deal damage to a target
            default:
                BasicUse(target, user);
            break;
        }
        
    }
    private void BasicUse(Actor target, Actor user)
    {
        if(status[0] != 0)
        {
            (StatusType, int)[] statusTuples = new (StatusType, int)[status.Length];
            for(int i = 0; i< status.Length; i++) statusTuples[i] = (status[i], statusDuration[i]);
            target.AddStatus(statusTuples);
        }
        if(target is Hero) (target as Hero).ChangeMorale(moraleDamage);
        float attackDamage = damage;
        //statuses that apply to damage. Mark and tough are absent as they apply to non-attack damage as well. The same is true for brave & stress damage
        if (!isBuff)
        {
            //value are subject to change
            if(user.statuses[(int)StatusType.Empowered]>0) attackDamage*=1.5f;
            if(user.statuses[(int)StatusType.Weak]>0) attackDamage*=.67f;
            if(target.statuses[(int)StatusType.Defended]>0) attackDamage*=.67f;
        }
               
        target.ChangeHealth((int)attackDamage);
        Debug.WriteLine(user.name + " using Attack " + name + " on " + target.name);
    }
}