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
    private DisplayMeter mentalBar;
    private Texture2D defaultTexture;
    private Texture2D thrallTexture;

    public override void _Ready()
    {
        base._Ready();
        Node2D mentalBarNode = (Node2D)meterScene.Instantiate();
        AddChild(mentalBarNode);
        mentalBar = mentalBarNode as DisplayMeter;
        mentalBar.GlobalPosition = new Vector2(hpBar.GlobalPosition.X, mentalBar.GlobalPosition.Y + 70);   
    }



    public void ChangeMorale(int stress)
    {
        //thralled characters have no morale
        if(anima > 0) return;
        GD.Print(stress);
        int oldMorale = morale;
        //stress defence here
        if(stress>0 && statuses[(int)StatusType.Brave]>0) stress -= 1;
        GD.Print(stress);
        morale -= stress;
        //clamp morale to possible range
        if (morale>10) morale = 10;
        else if (morale < 0) morale = 0;

        GD.Print(name + " at " + position + ": Morale " + oldMorale + " -> " + morale);
        mentalBar.UpdateMeter(morale - oldMorale);
        //if morale reached 0, do something
        if(morale == 0) Panic();
    }

    public void Init(HeroType heroType, int position, BattleScene bs, int startingAnima = -1)
    {
        anima = startingAnima;
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
            attacks.Add(new Attack("Throw Rock", bs, StatusType.None));
        }
        //bandit, merc, duelist
        else if(heroInit < 10)
        {
            speed = 5;
            health = 10;
            attacks.Add(new Attack("Stab", bs, StatusType.None, 0, 63, 3, 10));
            if(level >= 2)
            {
                health = 13;
                attacks.Add(new Attack("Guard", bs, StatusType.Defended, 2, 63, 63, 0, 0, true));
            }
            if(level >= 3)
            {
                speed = 7;
                health = 17;
                attacks.Add(new Attack("Counter", bs, StatusType.None));
            }
        }
        //Hunter, Ranger, Slayer
        else if(heroInit < 20)
        {
            speed = 7;
            health = 8;
            defaultTexture = GD.Load<Texture2D>("res://assets/sprites/tempArcher.png");
            thrallTexture = GD.Load<Texture2D>("res://assets/sprites/tempThrallArcher.png");
            attacks.Add(new Attack("Arrow", bs, StatusType.None, 0, 60, 63, 3));
            if(level >= 2)
            {
                health = 11;
                attacks.Add(new Attack("Snare", bs, StatusType.Snared, 3, 63, 63, 0));
            }
            if(level >= 3)
            {
                health = 14;
                attacks.Add(new Attack("Slaying Shot", bs, StatusType.None, 0, 60, 63, 2, 0, false, false, false, AttackType.SlayerShot));
            }
        }
        else if(heroInit < 30)
        {
            speed = 6;
            health = 7;
            attacks.Add(new Attack("Portend", bs, StatusType.Marked, 3, 63, 63, 1));
            if(level >= 2)
            {
                health = 9;
                attacks.Add(new Attack("Predict", bs, StatusType.Weak, 2, 63, 63, 1));
            }
            if(level >= 3)
            {
                health = 12;
                attacks.Add(new Attack("Starfall", bs, StatusType.None, 0, 63, 63, 0, 0, false, false, true, AttackType.Starfall));
            }
        }
        //if more classes get added need an else if here instead, for now its default case though
        else
        {
            speed = 4;
            health = 9;
            attacks.Add(new Attack("Mend", bs, StatusType.None, 0, 60, 63, -3, -1, true));
            if(level >= 2)
            {
                health = 12;
                attacks.Add(new Attack("Inspire", bs, [StatusType.Empowered, StatusType.Brave], [3,3], 63, 63, 0, 0, true));
            }
            if(level >= 3)
            {
                health = 16;
                attacks.Add(new Attack("Zealotry", bs, StatusType.Empowered, 3, 63, 63, 0, 0, true, true, false, AttackType.Zealotry));
            }
        }

        if(defaultTexture != null && thrallTexture != null)
        {
            if(anima == -1) sprite.sprite2D.Texture = defaultTexture;
            else sprite.sprite2D.Texture = thrallTexture;
            sprite.sprite2D.Scale *= .3f;
            sprite.sprite2D.Position = new Vector2(sprite.sprite2D.Position.X+15, sprite.sprite2D.Position.Y - 30);
        }
        

        hpBar.Init(MeterType.Health, health);
        if(startingAnima > 0) mentalBar.Init(MeterType.Anima, startingAnima, true);
        else mentalBar.Init(MeterType.Morale, 10);
    }

    public override bool TurnEnd()
    {
        base.TurnEnd();
        if(anima > 0)
        {
            anima -=1;
            GD.Print("anima: " + anima);
            mentalBar.UpdateMeter(-1);
        } 
        if(anima == 0)
        {
            Die();
            return true;
        }
        return false;
    }

    public void Enthrall()
    {
        sprite.sprite2D.Texture = thrallTexture;
    }
    protected override void Die()
    {
        mentalBar.QueueFree();
        base.Die();
        //curse transition goes here
        if(isLeader){};
    }

    protected void Panic()
    {
        bs.HeroPanic(this);
    }
}