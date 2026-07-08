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
    protected DisplayMeter hpBar;
    protected PackedScene meterScene;

    public override void _Ready()
    {
        base._Ready();
        meterScene = GD.Load<PackedScene>("res://scenes/ui/display_meter.tscn");
        Node2D hpBarNode = (Node2D)meterScene.Instantiate();
        AddChild(hpBarNode);
        hpBar = hpBarNode as DisplayMeter;
        hpBar.GlobalPosition = new Vector2(hpBar.GlobalPosition.X, hpBar.GlobalPosition.Y + 45);
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
        int oldHealth = health;
        //mark and tough are here instead of in attack use because I want them to trigger off of non-attack damage (for now)
        if(statuses[(int)StatusType.Marked]>0 && damage>0) damage += 2;
        if(statuses[(int)StatusType.Tough]>0 && damage > 0)
        {
            damage -=2;
            if(damage<0) damage = 0;
        } 
        health -= damage;
        hpBar.UpdateMeter(health - oldHealth);
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
