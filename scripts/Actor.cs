using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public enum StatusType
{
    None = 0, Marked = 1, Snared = 2, Empowered = 3, Defended = 4, Weak = 5, Bleeding = 6, Brave = 7, Tough = 8, Starstruck = 9
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
    public List<int> statuses = new List<int>();
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

    public virtual void TurnStart()
    {
    }

    public virtual bool TurnEnd()
    {
        //count down each status once. may need to adjust for stack based statuses rather than turn based ones
        for(int i = statuses.Count - 1; i>=0; i--)
        {
            if(statuses[i]>0)
            {
                statuses[i] -= 1;
            }
        }
        return false;
    }

    public bool AddStatus((StatusType, int)[] newStatuses)
    {
        for(int i = 0; i < newStatuses.Length; i++)
        {
            GD.Print(newStatuses[i].Item1);
            statuses[(int)newStatuses[i].Item1] += newStatuses[i].Item2;
        }
        //bool return is for if/when immunities/resist chances are implemented
        return true;
    }

    public void ChangeHealth(int damage)
    {
        //mark and tough are here instead of in attack use because I want them to trigger off of non-attack damage (for now)
        if(statuses[(int)StatusType.Marked]>0 && damage>0) damage += 2;
        if(statuses[(int)StatusType.Tough]>0 && damage > 0)
        {
            damage -=2;
            if(damage<0) damage = 0;
        } 
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
