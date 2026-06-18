using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;


public enum EnemyType
{
    Bandit = 0, Wolf = 1, Starspawn = 2
}
public partial class Enemy : Actor
{
    public EnemyType enemyType;

    public override void _Ready()
    {
        base._Ready();
    }

    public void Init(EnemyType enemyType, int position, BattleScene bs)
    {
        isFriendly = false;
        this.enemyType = enemyType;
        this.position = position;
        this.bs = bs;

        switch (enemyType){
            case EnemyType.Bandit:
            name = "Bandit (E)";
            speed = 5;
            health = 10;
            attacks.Add(new Attack("Stab", StatusType.None, 0, 63, 3, 3));
            attacks.Add(new Attack("Shank", StatusType.None, 0, 63, 48, 2));
            break;
        }
    }
    protected override void Die()
    {
        base.Die();
        Debug.Write(" enemy");
    }
}
