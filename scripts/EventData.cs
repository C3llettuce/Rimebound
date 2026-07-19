using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;
public enum Event
{
    None = 0,
    LostHunter = 1,

}
public class EventData
{
    
    public Event eventType;
    public List<EventOption> options;
    public string title = "Test Event";
    public EventData(Event eventType)
    {
        this.eventType = eventType;
        options = new List<EventOption>();
        Init();
    }

    private void Init()
    {
        switch (eventType)
        {
            case Event.LostHunter:
                //should probably load some amount of this from a json or something
                title = "A Lost Hunter";
                EventOption option1 = new EventOption("Recruit The Lost Hunter", 0, [new HeroData(HeroType.Hunter)]);
                EventOption option2 = new EventOption("Escort The Hunter Home", 100);
                //testing action delegates here
                Action freaky = () => {
                    RunManager.Instance.DebugPrint();
                    RunManager.Instance.GetRandomHero().HP -= 2;
                    GD.Print("After: ");
                    RunManager.Instance.DebugPrint();
                };
                EventOption option3 = new EventOption("Freak Mode", 0, null, freaky);
                options.Add(option1);
                options.Add(option2);
                options.Add(option3);
                break;
        }
    }
    public int SpawnChance()
    {
        return 1;
    }
}

public class EventOption
{
    public HeroData[] newHeroes;
    public int goldChange;
    public string name;
    Action extra = null;

    public EventOption(string name, int goldChange = 0, HeroData[] newHeroes = null)
    {
        this.name = name;
        this.goldChange = goldChange;
        this.newHeroes = newHeroes;
    }

    public EventOption(string name, int goldChange , HeroData[] newHeroes, Action extra):this(name, goldChange, newHeroes)
    {
        this.extra = extra;
    }

    public void Activate()
    {
        if(newHeroes != null)
        {
            foreach(HeroData hd in newHeroes)
            {
                RunManager.Instance.AddHero(hd);
                GD.Print("Hero Added");
            } 
        }
        
        RunManager.Instance.gold += goldChange;
        GD.Print("Added/Subtracted " + goldChange + " gold. (new total " + RunManager.Instance.gold + ")");
        if(extra != null)
        {
            extra();
            GD.Print("trying func");
        }
    }
}