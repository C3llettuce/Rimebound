using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;


//mix between specific attacks and general groupings of edge cases
public enum AttackType
{
    None = 0,
    SlayerShot = 1,
    Starfall = 2,
    Zealotry = 3,
    Supplicate = 4,

}

//General targeting types which require special checks but aren't unique to single attacks
//Note some of these are restrictions while others describe attacks hitting multiple units in a specific pattern
public enum TargetingType
{
    Basic = 0,
    Column = 1,
    Row = 2,
    Neighbor = 4,
    SameRow = 8,
    Advancing = 16,
    Retreating = 32,
    Pushing = 64,
    Pulling = 128
}

public class Attack
{
    public string name;
    public int usePosition, targetPosition;
    public StatusType[] status; public int[] statusDuration;
    public BattleScene bs;
    private BattleManager bm;
    public int damage, moraleDamage, animaSpend;
    public bool isBuff, isAoe, isTileTargeted;
    private AttackType attackType = AttackType.None;
    private TargetingType targetingType = TargetingType.Basic;
    public Actor owner;

    public Attack(string name, BattleScene bs, Actor owner, StatusType[] status = null, int[] statusDuration = null, int usePosition = 63, int targetPosition = 63, int damage = 1, int moraleDamage = 0, bool isBuff = false, bool isAoe = false, bool isTileTargeted = false, int animaSpend = 0)
    {
        this.name = name;
        this.bs = bs;
        bm = bs.battleManager;
        this.owner = owner;
        this.usePosition = usePosition;
        this.targetPosition = targetPosition;
        this.status = status;
        this.statusDuration = statusDuration;
        this.damage = damage;
        this.moraleDamage = moraleDamage;
        this.isBuff = isBuff;
        this.isAoe = isAoe;
        this.isTileTargeted = isTileTargeted;
        this.animaSpend = animaSpend;
    }
    //constructor for single/no status attacks
    public Attack(string name, BattleScene bs, Actor owner, StatusType status = 0, int statusDuration = 0, int usePosition = 63, int targetPosition = 63, int damage = 1, int moraleDamage = 0, bool isBuff = false, bool isAoe = false, bool isTileTargeted = false, int animaSpend = 0):
    this(name, bs, owner, [status], [statusDuration], usePosition, targetPosition, damage, moraleDamage, isBuff, isAoe, isTileTargeted, animaSpend){}

    public void DeclareSpecialTypes(AttackType attackType = AttackType.None, TargetingType targetingType = TargetingType.Basic)
    {
        this.attackType = attackType;
        this.targetingType = targetingType;
    }
    //check here instead of in b-mngr to allow for uncluttered special cases (maybe should just give each attack its own class? idk)
    public bool CheckValidAttack(int usePosition, int targetPosition)
    {
        List<Actor> userAllies = bs.GetUnitsAsActors(owner.isFriendly);
        List<Actor> userEnemies = bs.GetUnitsAsActors(!owner.isFriendly);
        //Special cases for unqiue attacks
        switch (attackType)
        {
            //supplicate should only work if targeting another thralled hero, for now no basic usePos/targetPos bit check as it should always be any
            case AttackType.Supplicate:
                foreach(Hero h in bs.heroes) if(h.position == targetPosition && h.position != usePosition && h.anima > 0) return true;
                break;
            //case AttackType.Starfall:
        }
        //Special cases for more generic targeting patterns
        TargetingType tType = targetingType;
        if (tType.HasFlag(TargetingType.Advancing) && usePosition > 3 && owner.statuses[(int)StatusType.Snared] == 0)
        {
            foreach(Actor a in userAllies) if(a.position == usePosition/4 && a.statuses[(int)StatusType.Snared] == 0) tType &= ~TargetingType.Advancing;
        }
        if (tType.HasFlag(TargetingType.Retreating) && usePosition < 32 && owner.statuses[(int)StatusType.Snared] == 0)
        {
            foreach(Actor a in userAllies) if(a.position == usePosition*4 && a.statuses[(int)StatusType.Snared] == 0) tType &= ~TargetingType.Retreating;
        }
        if (tType.HasFlag(TargetingType.Neighbor))
        {
            int dif = Mathf.Abs(usePosition - targetPosition);
            if(dif == 1 || dif == 4 || dif == 16) tType &= ~ TargetingType.Neighbor;
        }
        if (tType.HasFlag(TargetingType.SameRow))
        {
            //I think this math works but need to check. Should be (0/2/4)%2 for row 1 and (1/3/5)%2 for row 2
            if(bm.bitToID[usePosition]%2 == bm.bitToID[targetPosition]%2) tType &= ~TargetingType.SameRow;
        }
        if (tType == TargetingType.Basic)
        {
            if((this.usePosition & usePosition) != 0 && (this.targetPosition & targetPosition) != 0) return true;
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
        //special instructions for unique targeting patterns (also includes movement)
        if (targetingType.HasFlag(TargetingType.Advancing)) bm.MoveActor(owner, owner.position/4, true);
        if (targetingType.HasFlag(TargetingType.Retreating)) bm.MoveActor(owner, owner.position*4, true);
        if (targetingType.HasFlag(TargetingType.Pushing)) if (target.position < 32) if(bm.GetValidMove(target, target.position*4, true)!=0) bm.MoveActor(target, target.position*4, true);
        if (targetingType.HasFlag(TargetingType.Pulling)) if (target.position > 3) if(bm.GetValidMove(target, target.position/4, true)!=0) bm.MoveActor(target, target.position/4, true);


        //special cases for unique attacks
        switch (attackType)
        {
            case AttackType.Zealotry:
                foreach(Hero h in bs.heroes) if(h.anima > 0 && h != target) BasicUse(h, user);
                goto default;
            case AttackType.Starfall:
                TileStarfall ts = new TileStarfall(TileState.Starfall, owner, tc, 3, damage);
                tc.tileStatuses.Add(ts);
                owner.tileStatuses.Add(ts);
                break;
            case AttackType.SlayerShot:
                if (target.statuses[(int)StatusType.Marked] > 0) damage += 3;
                if (target.statuses[(int)StatusType.Snared] > 0) damage += 3;
                goto default;
            case AttackType.Supplicate:
                (user as Hero).ChangeAnima(3);
                (target as Hero).ChangeAnima(-2);
                break;
            //default case for all attacks that deal damage to a target
            default:
                BasicUse(target, user);
            break;
        }
        
    }

    private void BasicUse(Actor target, Actor user)
    {
        Debug.WriteLine(user.name + " using Attack " + name + " on " + target.name);
        if(status[0] != 0)
        {
            (StatusType, int)[] statusTuples = new (StatusType, int)[status.Length];
            for(int i = 0; i< status.Length; i++) statusTuples[i] = (status[i], statusDuration[i]);
            target.AddStatus(statusTuples);
        }
        if(target is Hero && moraleDamage != 0)
        {
            (target as Hero).ChangeMorale(moraleDamage);
        } 
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

        //no valid checks as currently no non-thralls/enemies should have attacks with animaSpend
        if(animaSpend > 0) (user as Hero).ChangeAnima(animaSpend);
    }

    public string GetDescription()
    {
        string s = "";
        if(animaSpend > 0) s += "Cost: " + animaSpend + " anima";
        if(damage > 0) s += "\n" + "Damage: " + damage;
        else if(damage < 0) s += "\n" + "Healing: " + (damage*-1);
        if(status[0] != 0)
        {
            for(int i = 0; i< status.Length; i++) s += "\nAdd " + status[i] + " (" + statusDuration[i] + " turns)";
        }
        switch (attackType){
            case AttackType.SlayerShot:
                s += "\n+2 Damage if target is snared\n+2 Damage if target is marked";
                break;
        }
        return s;
    }
}