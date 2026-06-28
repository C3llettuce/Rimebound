using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public enum HeroType
{
    Peasant = 0, Bandit = 1, Mercenary = 2, Duelist = 3, Hunter = 11, Ranger = 12, Slayer = 13, Doomsayer = 21, Oracle = 22, Astronomer = 23, Pilgrim = 31, Monk = 32, Priest = 33
}
public partial class Hero : Actor
{
    public HeroType heroType;
    public int morale = 10;
    public int anima = -1;
    public bool isLeader = false;


    public void ChangeMorale(int stress)
    {
        //thralled characters have no morale
        if(anima > 0) return;
        morale -= stress;
        //clamp morale to possible range
        if (morale>10) morale = 10;
        else if (morale < 0) morale = 0;
        //if morale reached 0, do something
        if(morale == 0){}
    }

    public void Init(HeroType heroType, int position, BattleScene bs)
    {
        isFriendly = false;
        this.heroType = heroType;
        this.position = position;
        this.bs = bs;
        int heroInit = (int)heroType;
        int level = heroInit%10;
        name = heroType.ToString();

        //peasant
        if (heroInit == 0)
        {
            speed = 3;
            health = 5;
            attacks.Add(new Attack("Throw Rock", StatusType.None));
        }
        //bandit, merc, duelist
        else if(heroInit < 10)
        {
            speed = 5;
            health = 10;
            attacks.Add(new Attack("Stab", StatusType.None, 0, 63, 3, 10));
            if(level >= 2)
            {
                health = 13;
                attacks.Add(new Attack("Guard", StatusType.Defended, 2, 63, 63, 0, 0, true));
            }
            if(level >= 3)
            {
                speed = 7;
                health = 17;
                attacks.Add(new Attack("Counter", StatusType.None));
            }
        }
        //Hunter, Ranger, Slayer
        else if(heroInit < 20)
        {
            speed = 7;
            health = 8;
            attacks.Add(new Attack("Arrow", StatusType.None, 0, 60, 63, 3));
            if(level >= 2)
            {
                health = 11;
                attacks.Add(new Attack("Snare", StatusType.Snare, 3, 63, 0));
            }
            if(level >= 3)
            {
                health = 14;
                attacks.Add(new Attack("Slaying Shot", StatusType.None, 0, 60, 63, 2, 0, false, false, DamageType.SlayerShot));
            }
        }
        else if(heroInit < 30)
        {
            speed = 6;
            health = 7;
            attacks.Add(new Attack("Portend", StatusType.Mark, 3, 63, 63, 1));
            if(level >= 2)
            {
                health = 9;
                attacks.Add(new Attack("Predict", StatusType.Weak, 2, 63, 63, 1));
            }
            if(level >= 3)
            {
                health = 12;
                attacks.Add(new Attack("Starfall", StatusType.None, 0, 63, 63, 0, 0, false, false, DamageType.Starfall));
            }
        }
        //if more classes get added need an else if here instead, for now its default case though
        else
        {
            speed = 4;
            health = 9;
            attacks.Add(new Attack("Mend", StatusType.None, 0, 60, 63, -3, -1, true));
            if(level >= 2)
            {
                health = 12;
                attacks.Add(new Attack("Inspire", [StatusType.Empowered, StatusType.Brave], [3,3], 63, 63, 1));
            }
            if(level >= 3)
            {
                health = 16;
                attacks.Add(new Attack("Inspi", StatusType.None, 0, 63, 63, 0, 0, false, false, DamageType.Starfall));
            }
        }
    }

    public override bool TurnEnd()
    {
        base.TurnEnd();
        if(anima > 0) anima -=1;
        if(anima == 0)
        {
            return true;
        }
        return false;
    }


    protected override void Die()
    {
        base.Die();
        //curse transition goes here
        if(isLeader){};
        Debug.Write(" hero");
    }

    protected void Panic()
    {
        bs.HeroPanic(this);
    }

}
