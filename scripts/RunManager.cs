using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public partial class RunManager: RefCounted
{
    public List<HeroData> heroDatas =  new List<HeroData>();
    PackedScene heroScene = GD.Load<PackedScene>("res://scenes/battles/hero.tscn");




    public void SaveHeroes(BattleScene bs)
    {
        heroDatas.Clear();
        foreach(Hero h in bs.heroes)
        {
            HeroData hd = new HeroData(h.heroType, h.health, h.morale, h.anima, h.isLeader);
        }
    }

    public List<Hero> LoadHeroes()
    {
        List<Hero> heroes = new List<Hero>();
        foreach(HeroData hd in heroDatas)
        {
            Hero newHero = (Hero)heroScene.Instantiate();
            heroes.Add(newHero);
        }
        return heroes;
    }
}