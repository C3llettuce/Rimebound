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
    public void Init(HeroType heroType, int position, BattleScene bs)
    {
        isFriendly = false;
        this.heroType = heroType;
        this.position = position;
        this.bs = bs;

        switch (heroType){
            case HeroType.Bandit:
            name = "Bandit";
            speed = 5;
            health = 10;
            attacks.Add(new Attack("Stab", StatusType.None, 0, 63, 3, 10));
            break;

            case HeroType.Hunter:
            name = "Hunter";
            speed = 7;
            health = 8;
            attacks.Add(new Attack("Arrow", StatusType.None, 0, 60, 63, 10));
            break;
        }
    }

    protected override void Die()
    {
        base.Die();
        Debug.Write(" hero");
    }

}
