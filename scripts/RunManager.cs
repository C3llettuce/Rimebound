using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public enum RunState
{
    StartingEvent = -10,
    StartingEvent2 = -9,
    StartRun = -8,
    Travelling = 1,
    Fighting = 2,
    Town = 3
}
public partial class RunManager: Node2D
{
    public static RunManager Instance {get; private set;}
    public List<HeroData> heroDatas =  new List<HeroData>();
    Random rand;
    public EventData currentEvent;
    public BattleScene currentBattle;
    public RosterManager rosterManager;
    RunState runState = RunState.StartingEvent;
    PackedScene heroScene = GD.Load<PackedScene>("res://scenes/battles/hero.tscn");
    PackedScene eventScene = GD.Load<PackedScene>("res://scenes/events/event_scene.tscn");
    PackedScene packedBattleScene = GD.Load<PackedScene>("res://scenes/battles/battle_scene.tscn");


    public override void _Ready()
    {
        Instance = this;
        rand = new Random();
        rosterManager = new RosterManager();
    }

    public void SaveHeroes(BattleScene bs)
    {
        rosterManager.SaveHeroes(bs);
    }

    public void StartRun()
    {
        runState = RunState.StartingEvent;
        SetCurrentEvent(Event.RunStartLeader);
    }

    public void NextState()
    {
        RunState nextRunState = (RunState)((int)runState + 1);
        switch (nextRunState)
        {
            case RunState.StartingEvent2:
                SetCurrentEvent(Event.RunStart2);
                break;
            case RunState.StartRun:
                GetTree().ChangeSceneToPacked(packedBattleScene);
                break;
            default:
               
                break;
        }
    }


    public void CompleteEvent()
    {
        runState = RunState.Fighting;
        GetTree().ChangeSceneToPacked(packedBattleScene);
    }

    public void CompleteBattle()
    {
        runState = RunState.Travelling;
        Random rand = new Random();
        int nextEvent = rand.Next(11,12);
        SetCurrentEvent((Event)nextEvent);
    }

    public void AddHero(HeroData hd)
    {
        rosterManager.AddHero(hd);
    }

    public void SetCurrentEvent(Event eventType)
    {
        currentEvent = new EventData(eventType);
        GetTree().ChangeSceneToPacked(eventScene);
    }

    public void SetDebugHeroes()
    {
        rosterManager.SetDebugHeroes();
    }

    public void SetDebugThrallHeroes()
    {
        rosterManager.SetDebugHeroes2();
    }

    public HeroData GetRandomHero()
    {
        return rosterManager.GetRandomHero();
    }

    public void DebugPrint()
    {
        foreach (HeroData h in rosterManager.heroDatas)
        {
            GD.Print(h.ToString());
        }
    }
    public List<Hero> LoadHeroes()
    {
        return rosterManager.LoadHeroes();
    }
}