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
    public int maxMorale = 10;
    public int anima = -1;
    public bool isLeader = false;
    private DisplayMeter mentalBar;
    private Texture2D defaultTexture;
    private Texture2D thrallTexture;
    public HeroData hData;

    public override void _Ready()
    {
        base._Ready();
        Node2D mentalBarNode = (Node2D)meterScene.Instantiate();
        AddChild(mentalBarNode);
        mentalBar = mentalBarNode as DisplayMeter;
        mentalBar.GlobalPosition = new Vector2(hpBar.GlobalPosition.X, mentalBar.GlobalPosition.Y + 40);   
    }



    public void ChangeMorale(int stress)
    {
        //thralled characters have no morale
        if(anima > 0) return;
        int oldMorale = morale;
        //stress defence here
        if(stress>0 && statuses[(int)StatusType.Brave]>0) stress -= 1;
        morale -= stress;
        //clamp morale to possible range
        if (morale>10) morale = 10;
        else if (morale < 0) morale = 0;

        GD.Print(name + " at " + position + ": Morale " + oldMorale + " -> " + morale);
        mentalBar.UpdateMeter(morale - oldMorale);
        //if morale reached 0, do something
        if(morale == 0) Panic();
    }

    public void ChangeAnima(int animaLoss)
    {
        if(anima < 0) return;
        anima -= animaLoss;
        mentalBar.UpdateMeter(-animaLoss);
        if(anima <= 0) Die();
    }

    public void Init(int position, BattleScene bs)
    {
        Init(hData.Class, position, bs, hData.Anima, hData.Morale, hData.HP, hData.Leader);
    }

    public void Init(HeroType heroType, int position, BattleScene bs, int startingAnima = -1, int startingMorale = -1, int startingHealth = -1, bool isLeader = false)
    {
        anima = startingAnima;
        isFriendly = true;
        this.isLeader = isLeader;
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
            maxHealth = 5;
            maxHealth = 5;
            attacks.Add(new Attack("Throw Rock", bs, this, StatusType.None));
        }
        //bandit, merc, duelist
        else if(heroInit < 10)
        {
            speed = 5;
            maxHealth = 10;
            attacks.Add(new Attack("Stab", bs, this, StatusType.None, 0, 63, 3, 10));
            if(level >= 2)
            {
                maxHealth = 13;
                attacks.Add(new Attack("Guard", bs, this, StatusType.Defended, 2, 63, 63, 0, 0, true));
            }
            if(level >= 3)
            {
                speed = 7;
                maxHealth = 17;
                attacks.Add(new Attack("Counter", bs, this, StatusType.None));
            }
        }
        //Hunter, Ranger, Slayer
        else if(heroInit < 20)
        {
            speed = 7;
            maxHealth = 8;
            defaultTexture = GD.Load<Texture2D>("res://assets/sprites/tempArcher.png");
            thrallTexture = GD.Load<Texture2D>("res://assets/sprites/tempThrallArcher.png");
            attacks.Add(new Attack("Arrow", bs, this, StatusType.None, 0, 60, 63, 3));
            if(level >= 2)
            {
                maxHealth = 11;
                attacks.Add(new Attack("Snare", bs, this, StatusType.Snared, 3, 63, 63, 0));
            }
            if(level >= 3)
            {
                maxHealth = 14;
                Attack a = new Attack("Slaying Shot", bs, this, StatusType.None, 0, 60, 63, 2, 0, false, false, false);
                a.DeclareSpecialTypes(AttackType.SlayerShot);
                attacks.Add(a);
            }
        }
        else if(heroInit < 30)
        {
            speed = 6;
            maxHealth = 7;
            attacks.Add(new Attack("Portend", bs, this, StatusType.Marked, 3, 63, 63, 1));
            if(level >= 2)
            {
                maxHealth = 9;
                attacks.Add(new Attack("Predict", bs, this, StatusType.Weak, 2, 63, 63, 1));
            }
            if(level >= 3)
            {
                maxHealth = 12;
                Attack a = new Attack("Starfall", bs, this, StatusType.None, 0, 63, 63, 10, 0, false, false, true);
                a.DeclareSpecialTypes(AttackType.Starfall);
                attacks.Add(a);
            }
        }
        //if more classes get added need an else if here instead, for now its default case though
        else
        {
            speed = 4;
            maxHealth = 9;
            attacks.Add(new Attack("Mend", bs, this, StatusType.None, 0, 60, 63, -3, -1, true));
            if(level >= 2)
            {
                maxHealth = 12;
                attacks.Add(new Attack("Inspire", bs, this, [StatusType.Empowered, StatusType.Brave], [3,3], 63, 63, 0, 0, true));
            }
            if(level >= 3)
            {
                maxHealth = 16;
                Attack a = new Attack("Zealotry", bs, this, StatusType.Empowered, 3, 63, 63, 0, 0, true, true, false);
                a.DeclareSpecialTypes(AttackType.Zealotry);
                attacks.Add(a);
            }
        }

        if(defaultTexture != null && thrallTexture != null)
        {
            if(anima == -1) sprite.sprite2D.Texture = defaultTexture;
            else sprite.sprite2D.Texture = thrallTexture;
            sprite.sprite2D.Scale *= .3f;
            sprite.sprite2D.Position = new Vector2(sprite.sprite2D.Position.X+15, sprite.sprite2D.Position.Y - 30);
        }
        
        //set health
        if(startingHealth == -1) health = maxHealth;
        else health = startingHealth;
        //set morale
        if(startingMorale == -1) morale = maxMorale;
        else morale = startingMorale;

        hpBar.Init(MeterType.Health, maxHealth, health);
        if(startingAnima > 0) ThrallInit(startingAnima);
        else mentalBar.Init(MeterType.Morale, maxMorale, morale);
    }

    public void ThrallInit(int startingAnima)
    {
        mentalBar.Init(MeterType.Anima, startingAnima, true);
        int heroInit = (int)heroType;
        int level = heroInit%10;
        if(heroInit == 0)
        {
            maxHealth += 4;
            Attack a = new Attack("Supplicate", bs, this, StatusType.None, 0, 63, 63, 0, -1, true, false, false);
            a.DeclareSpecialTypes(AttackType.Supplicate);
            attacks.Add(a);
        }
        //Bandit tree
        else if(heroInit < 10)
        {
            
        }
        //Hunter tree
        else if(heroInit < 20)
        {
            attacks.Add(new Attack("Evil Shot", bs, this, StatusType.None, 0, 60, 63, 5, 0, false, false, false, 1));
        }
        //Oracle Tree
        else if(heroInit < 30)
        {
            
        }
        //Pilgrim tree
        else
        {
            
        }
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
    //method for instantiating a thrall units special properties on spawn
    

    //method for enthralling a unit mid combat
    public void Enthrall()
    {
        if(thrallTexture != null) sprite.sprite2D.Texture = thrallTexture;
        int heroInit = (int)heroType;
        if(heroInit == 0)
        {
            
        }
        else if(heroInit < 10)
        {
            
        }
        else if(heroInit < 20)
        {
            
        }
        else if(heroInit < 30)
        {
            
        }
        else
        {
            
        }
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