using System.Collections.Generic;
using System.Diagnostics;

public class Attack
{
    public string name;
    public int usePosition, targetPosition;
    public StatusType status; public int statusDuration;
    public int damage; 
    public bool isBuff;
    public bool isAoe;
    public Attack(string name, StatusType status = 0, int statusDuration = 0, int usePosition = 63, int targetPosition = 63, int damage = 1, bool isBuff = false, bool isAoe = false)
    {
        this.name = name;
        this.usePosition = usePosition;
        this.targetPosition = targetPosition;
        this.status = status;
        this.statusDuration = statusDuration;
        this.damage = damage;
        this.isBuff = false;
        this.isAoe = isAoe;
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
        if(status != 0) target.AddStatus([(status, statusDuration)]);
        target.ChangeHealth(damage);
    }
}