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
    public List<int> statuses = new List<int>((int)StatusType.Starstruck+1);
    public int Speed{get{return speed + speedBonus;}}
    public BattleScene bs;

    public override void _Ready()
    {
        base._Ready();
        for(int i =0; i<=(int)StatusType.Starstruck; i++)
        {
            statuses.Add(0);
        }
    }

    public bool TryMove(int targetTile)
    {
        return false;
    }

    public void TurnStart()
    {
        
    }

    public void TurnEnd()
    {
        //count down each status once. may need to adjust for stack based statuses rather than turn based ones
        foreach(int i in statuses)
        {
            if(i != 0)
            {
                statuses[i] -= 1;
            }
        }
    }

    public bool AddStatus((StatusType, int)[] newStatuses)
    {
        for(int i = 0; i < newStatuses.Length; i++)
        {
            statuses[(int)newStatuses[i].Item1] += newStatuses[i].Item2;
        }
        //bool return is for if/when immunities/resist chances are implemented
        return true;
    }

    public void ChangeHealth(int damage)
    {
        if(statuses[(int)StatusType.Mark]>0) damage += 2;
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
