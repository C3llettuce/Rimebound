using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public partial class HeroData : RefCounted
{
    //-1 indicates a value will be set to its max (or not exist for anima) on hero initialization
    public HeroData(HeroType heroType = 0, int hp = -1, int morale = -1, int anima = -1, bool leader = false)
    {
        Class = heroType;
        HP = hp;
        Morale = morale;
        Anima = anima;
        Leader = leader;
    }
    public HeroType Class { get; }
    public int HP { get; set;}
    public int Morale { get; set;}
    public int Anima { get; set;}
    public bool Leader { get; }

    public override string ToString()
    {
        string s = "";
        if(Leader) s += "Leader ";
        s += Class + ", HP: " + HP;
        if(Anima<0) s += ", Morale: " + Morale;
        else s += ", Anima: " + Anima;
        return s;
    }
}