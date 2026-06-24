using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public enum StatusType
{
    None = 0, Mark = 1, Snare = 2, Empowered = 3, Defended = 4, Weak = 5, Starstruck = 6
}

public partial class Actor : Node2D
{
    public int health;
    public bool isFriendly;
    public int position;
    public string name;
    public ActorSprite sprite;
    protected int speed; public int speedBonus;
    public int movement;
    public List<Attack> attacks = new List<Attack>();
    public List<int> statuses = new List<int>((int)StatusType.Starstruck);
    public int Speed{get{return speed + speedBonus;}}
    public BattleScene bs;

    public override void _Ready()
    {
        base._Ready();
    }

    public bool tryMove(int targetTile)
    {
        return false;
    }
    public bool addStatus((int, int)[] newStatuses)
    {
        for(int i = 0; i < newStatuses.Length; i++)
        {
            statuses[newStatuses[i].Item1] += newStatuses[i].Item2;
        }
        //bool return is for if/when immunities/resist chances are implemented
        return true;
    }

    public void ChangeHealth(int damage)
    {
        health -= damage;
        Debug.WriteLine("HP " + (health + damage) + " -> " + health);
        if(health <= 0) Die();
    }

    protected virtual void Die()
    {
        bs.KillActor(this);
        //remove from battlemanager/scene lists
        //hide and deactivate sprite + collider
    }

    public virtual void OnMove(int from, int to){}
}
