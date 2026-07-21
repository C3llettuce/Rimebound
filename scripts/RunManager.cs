using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public partial class RunManager: Node2D
{
    public static RunManager Instance {get; private set;}
    public List<HeroData> heroDatas =  new List<HeroData>();
    Random rand;
    public Event currentEvent;
    public BattleScene currentBattle;
    public int gold = 0;
    PackedScene heroScene = GD.Load<PackedScene>("res://scenes/battles/hero.tscn");


    public override void _Ready()
    {
        Instance = this;
        rand = new Random();
    }

    public void SaveHeroes(BattleScene bs)
    {
        heroDatas.Clear();
        foreach(Hero h in bs.heroes)
        {
            HeroData hd = new HeroData(h.heroType, h.health, h.morale, h.anima, h.isLeader);
            heroDatas.Add(hd);
        }
    }

    public void AddHero(HeroData hd)
    {
        heroDatas.Add(hd);
    }

    public void SetCurrentEvent(Event eventType)
    {
        currentEvent = eventType;
    }

    public void SetDebugHeroes()
    {
        heroDatas.Add(new HeroData(HeroType.Slayer));
        heroDatas.Add(new HeroData(HeroType.Duelist));
        heroDatas.Add(new HeroData(HeroType.Astronomer));
    }

    public void SetDebugThrallHeroes()
    {
        heroDatas.Add(new HeroData(HeroType.Slayer, -1, -1, 5));
        heroDatas.Add(new HeroData(HeroType.Duelist, -1, -1, 10));
        heroDatas.Add(new HeroData(HeroType.Astronomer, 4, 5));
    }

    public HeroData GetRandomHero()
    {
        int randInt = rand.Next(heroDatas.Count);
        return heroDatas[randInt];
    }

    public void DebugPrint()
    {
        foreach (HeroData h in heroDatas)
        {
            GD.Print(h.ToString());
        }
    }
    public List<Hero> LoadHeroes()
    {
        List<Hero> heroes = new List<Hero>();
        foreach(HeroData hd in heroDatas)
        {
            Hero newHero = (Hero)heroScene.Instantiate();
            newHero.hData = hd;
            heroes.Add(newHero);
        }
        return heroes;
    }
}