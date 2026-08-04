using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;
public enum Event
{
    None = 0,
    RunStartLeader = 1,
    RunStart2 = 2,
    LostHunter = 10,
    Exile = 11
}
public class EventData
{
    
    public Event eventType;
    private Event nextEvent;
    public List<EventOption> options;
    public string title = "Test Event";
    EventOption option1 = null, option2 = null, option3 = null;
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
            case Event.RunStartLeader:
                title = "Select a Champion";
                option1 = new EventOption("The Tenacious Ranger", this, 0, [new HeroData(HeroType.Ranger, -1, -1, -1, true)]);
                option2 = new EventOption("The Mad Oracle", this, 0, [new HeroData(HeroType.Oracle, -1, -1, -1, true)]);
                option3 = new EventOption("The Cunning Mercenary", this, 0, [new HeroData(HeroType.Mercenary, -1, -1, -1, true)]);
                break;
            case Event.RunStart2:
                title = "Select An Entourage";
                option1 = new EventOption("Bind the cowering serfs to your mission", this, 0, [new HeroData(HeroType.Peasant, -1, -1, 0),new HeroData(HeroType.Peasant, -1, -1, 0),new HeroData(HeroType.Peasant, -1, -1, 0),new HeroData(HeroType.Peasant, -1, -1, 0)]);
                option2 = new EventOption("Bring those who would honour their sacred duty", this, 0, [new HeroData(HeroType.Peasant),new HeroData(HeroType.Peasant)]);
                option3 = new EventOption("Accept their offerings and bring only the strongest among them", this, 100, [new HeroData(HeroType.Peasant)]);
                break;
            case Event.LostHunter:
                //should probably load some amount of this from a json or something
                title = "A Lost Hunter";
                option1 = new EventOption("Recruit The Lost Hunter", this, 0, [new HeroData(HeroType.Hunter)]);
                option2 = new EventOption("Escort The Hunter Home", this, 100);
                //testing action delegates here
                Action freaky = () => {
                    RunManager.Instance.DebugPrint();
                    RunManager.Instance.GetRandomHero().HP -= 2;
                    GD.Print("After: ");
                    RunManager.Instance.DebugPrint();
                };
                option3 = new EventOption("Freak Mode", this, 0, null, freaky);
                break;
            case Event.Exile:
                title = "An Exiled Soothsayer";
                option1 = new EventOption("Confirm his fears: The frozen star is thawing", this, 0, [new HeroData(HeroType.Doomsayer)]);
                option2 = new EventOption("", this, 100);
                break;
        }
        //make this a loop later
        if(option1!=null) options.Add(option1);
        if(option2!=null) options.Add(option2);
        if(option3!=null) options.Add(option3);
    }
    public int SpawnChance()
    {
        return 1;
    }
}

public class EventOption
{
    private EventData parentEvent;
    public HeroData[] newHeroes;
    public int goldChange;
    public string name;
    Action extra = null;

    public EventOption(string name, EventData parentEvent, int goldChange = 0, HeroData[] newHeroes = null)
    {
        this.name = name;
        this.parentEvent =parentEvent;
        this.goldChange = goldChange;
        this.newHeroes = newHeroes;
    }

    public EventOption(string name, EventData parentEvent, int goldChange , HeroData[] newHeroes, Action extra):this(name, parentEvent, goldChange, newHeroes)
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
    
        InventoryManager.Instance.AddItem(LootType.gold, goldChange);
        GD.Print("Added/Subtracted " + goldChange + " gold. (new total " + InventoryManager.Instance.resources[(int)LootType.gold] + ")");
        if(extra != null)
        {
            extra();
            GD.Print("trying func");
        }

        ChoiceResolution();
    }

    private void ChoiceResolution()
    {
        switch (parentEvent.eventType)
        {
            case Event.RunStartLeader:
                RunManager.Instance.NextState();
                break;
            case Event.RunStart2:
                RunManager.Instance.NextState();
                break;
            default:
                RunManager.Instance.CompleteEvent();
                break;
        }
    }
}