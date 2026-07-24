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
    public int maxHealth;
    public bool isFriendly;
    public int position;
    public string name;
    public ActorSprite sprite;
    protected int speed; public int speedBonus;
    public int movement;
    public List<Attack> attacks = new List<Attack>();
    public List<int> statuses = new List<int>();
    public List<TileStatus> tileStatuses = new List<TileStatus>();
    public int Speed{get{return speed + speedBonus;}}
    public BattleScene bs;
    protected DisplayMeter hpBar;
    protected PackedScene meterScene, iconScene;
    protected List<DisplayIcon> icons;
    protected Sprite2D highlightSprite;

    public override void _Ready()
    {
        base._Ready();
        meterScene = GD.Load<PackedScene>("res://scenes/ui/display_meter.tscn");
        iconScene = GD.Load<PackedScene>("res://scenes/ui/display_icon.tscn");
        Node2D hpBarNode = (Node2D)meterScene.Instantiate();
        AddChild(hpBarNode);
        hpBar = hpBarNode as DisplayMeter;
        hpBar.GlobalPosition = new Vector2(hpBar.GlobalPosition.X, hpBar.GlobalPosition.Y + 15);
        highlightSprite = GetNode<Sprite2D>("highlight");
        icons = new List<DisplayIcon>();
        for(int i =0; i<=(int)StatusType.Starstruck; i++)
        {
            statuses.Add(0);
        }
    }

    public virtual void TurnStart()
    {
        highlightSprite.Visible = true;
    }

    public virtual bool TurnEnd()
    {
        highlightSprite.Visible = false;
        //count down each status once. may need to adjust for stack based statuses rather than turn based ones
        for(int i = statuses.Count - 1; i>=0; i--)
        {
            if(statuses[i]>0)
            {
                DisplayIcon toRemove = FetchStatusIcon((StatusType)i);
                toRemove.UpdateLabel(-1);
                statuses[i] -= 1;
                //remove ui icon
                if(statuses[i] == 0)
                {
                    icons.Remove(toRemove);
                    toRemove.QueueFree();
                    ArrangeIcons();
                }
            }
        }
        ArrangeIcons();
        return false;
    }

    private DisplayIcon AddStatusIcon(StatusType type, int duration)
    {
        Node2D newIconNode = (Node2D)iconScene.Instantiate();
        AddChild(newIconNode);
        DisplayIcon newIcon = newIconNode as DisplayIcon;
        newIcon.Init(type, duration);
        icons.Add(newIcon);
        return newIcon;
    }

    private DisplayIcon FetchStatusIcon(StatusType status, int duration = 0)
    {
        foreach(DisplayIcon di in icons) if(di.type == status) return di;
        return AddStatusIcon(status, duration);
    }

    private void ArrangeIcons()
    {
        int perRow = 3;
        int rows = (int)MathF.Ceiling((float)icons.Count/perRow);
        //GD.Print(rows);
        for(int i = 0; i < icons.Count; i++)
        {
            DisplayIcon di = icons[i];
            //no clue if this works tbh
            di.Position = new Vector2(-50 + 50*(i%perRow), -50*rows + 50*(i/perRow));
        }
    }

    public bool AddStatus((StatusType, int)[] newStatuses)
    {
        for(int i = 0; i < newStatuses.Length; i++)
        {
            StatusType currentStatus = newStatuses[i].Item1;
            int currentDuration = newStatuses[i].Item2;
            //add/increment status ui icons
            if(statuses[(int)currentStatus] > 0) FetchStatusIcon(currentStatus).UpdateLabel(currentDuration);
            else AddStatusIcon(currentStatus, currentDuration);
            
            statuses[(int)currentStatus] += currentDuration;
        }
        ArrangeIcons();
        //bool return is for if/when immunities/resist chances are implemented
        return true;
    }

    public void ChangeHealth(int damage)
    {
        int oldHealth = health;
        //mark and tough are here instead of in attack use because I want them to also trigger off of non-attack damage (for now)
        if(statuses[(int)StatusType.Marked]>0 && damage>0) damage += 2;
        if(statuses[(int)StatusType.Tough]>0 && damage > 0)
        {
            damage -=2;
            if(damage<0) damage = 0;
        } 
        health -= damage;
        //debug messages aren't set to account for overheal, will change when needed
        if(health > maxHealth) health = maxHealth;
        hpBar.UpdateMeter(health - oldHealth);
        Debug.WriteLine("HP " + (health + damage) + " -> " + health);
        if(health <= 0) Die();
    }

    protected virtual void Die()
    {
        hpBar.QueueFree();
        bs.KillActor(this);
        //remove from battlemanager/scene lists
        //hide and deactivate sprite + collider
    }

    public virtual void OnMove(int from, int to){}
}
