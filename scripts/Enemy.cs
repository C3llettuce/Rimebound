using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;


public enum EnemyType
{
    Bandit = 0, BanditArcher = 1, Wolf = 2, StarZealot = 3, Starspawn = 10
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
            maxHealth = 10;
            attacks.Add(new Attack("Stab", bs, StatusType.None, 0, 63, 3, 3));
            attacks.Add(new Attack("Shank", bs, StatusType.None, 0, 63, 48, 2));
            break;
            case EnemyType.BanditArcher:
            name = "Bandit Archer (E)";
            speed = 4;
            maxHealth = 8;
            attacks.Add(new Attack("Arrow", bs, StatusType.None, 0, 48, 63, 1));
            break;
            //temp morale damaging enemy for testing
            case EnemyType.StarZealot:
            name = "Starstruck Zealot";
            speed = 6;
            maxHealth = 6;
            attacks.Add(new Attack("Maddening Prophecy", bs, StatusType.None, 0, 63, 15, 0, 2));
            break;
        }
        health = maxHealth;
        hpBar.Init(MeterType.Health, maxHealth, health);
    }
    protected override void Die()
    {
        base.Die();
        Debug.Write(" enemy");
    }
}
