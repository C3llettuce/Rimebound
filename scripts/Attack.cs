using System.Collections.Generic;
using System.Diagnostics;


public enum DamageType
{
    None = 0,
    SlayerShot = 1,
    Starfall = 2
}

public enum AttackType
{
    Basic = 0,
    Status = 1,
    TileStatus = 2,
}
public class Attack
{
    public string name;
    public int usePosition, targetPosition;
    public StatusType[] status; public int[] statusDuration;
    public int damage; 
    private int moraleDamage;
    public bool isBuff;
    public bool isAoe;
    private DamageType damageType;
    public Attack(string name, StatusType[] status = null, int[] statusDuration = null, int usePosition = 63, int targetPosition = 63, int damage = 1, int moraleDamage = 0, bool isBuff = false, bool isAoe = false, DamageType dt = DamageType.None)
    {
        this.name = name;
        this.usePosition = usePosition;
        this.targetPosition = targetPosition;
        this.status = status;
        this.statusDuration = statusDuration;
        this.damage = damage;
        this.isBuff = isBuff;
        this.isAoe = isAoe;
        this.damageType = dt;
    }
    public Attack(string name, StatusType status = 0, int statusDuration = 0, int usePosition = 63, int targetPosition = 63, int damage = 1, int moraleDamage = 0, bool isBuff = false, bool isAoe = false, DamageType damageType = DamageType.None)
    {
        this.name = name;
        this.usePosition = usePosition;
        this.targetPosition = targetPosition;
        this.status = [status];
        this.statusDuration = [statusDuration];
        this.damage = damage;
        this.isBuff = isBuff;
        this.isAoe = isAoe;
        this.moraleDamage = moraleDamage;
        this.damageType = damageType;
    }

    //check here instead of in b-mngr to allow for uncluttered special cases (maybe should just give each attack its own class? idk)
    public bool CheckValidAttack(int usePosition, int targetPosition)
    {
        if(damageType == DamageType.Starfall)
        {
            
        }
        //default behavior
        else if((this.usePosition & usePosition) != 0 && (this.targetPosition & targetPosition) != 0) return true;
        return false;
    }

    public void Use(List<Actor> targets, Actor user)
    {
        foreach(Actor a in targets)
        {
            Use(a, user);
        }
    }

    public void Use(Actor target, Actor user)
    {
        Debug.WriteLine(user.name + " using Attack " + name + " on " + target.name);
        if(status[0] != 0)
        {
            (StatusType, int)[] statusTuples = new (StatusType, int)[status.Length];
            for(int i = 0; i< status.Length; i++) statusTuples[i] = (status[i], statusDuration[i]);
            target.AddStatus(statusTuples);
        }
        
        if(damageType == DamageType.SlayerShot)
        {
            if (target.statuses[(int)StatusType.Mark] > 0) damage += 3;
            if (target.statuses[(int)StatusType.Snare] > 0) damage += 3;
        }
        if(target is Hero) (target as Hero).ChangeMorale(moraleDamage);
        target.ChangeHealth(damage);
        
        
    }
}