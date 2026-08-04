using System;
using System.Collections.Generic;
using Godot;

public class RosterManager
{
    public RosterManager()
    {
        rand = new Random();
    }
    public List<HeroData> heroDatas =  new List<HeroData>();
    public Random rand;
    PackedScene heroScene = GD.Load<PackedScene>("res://scenes/battles/hero.tscn");
    public void AddHero(HeroData hd)
    {
        heroDatas.Add(hd);
    }

    public void SetDebugHeroes()
    {
        heroDatas.Add(new HeroData(HeroType.Slayer));
        heroDatas.Add(new HeroData(HeroType.Duelist, -1, -1, 1, true));
        heroDatas.Add(new HeroData(HeroType.Astronomer));
    }

    public void SetDebugHeroes2()
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

    public void SaveHeroes(BattleScene bs)
    {
        heroDatas.Clear();
        foreach(Hero h in bs.heroes)
        {
            HeroData hd = new HeroData(h.heroType, h.health, h.morale, h.anima, h.isLeader);
            heroDatas.Add(hd);
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