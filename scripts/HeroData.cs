using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public partial class HeroData : RefCounted
{
    public HeroData(HeroType heroType = 0, int hp = 0, int morale = 0, int anima = -1, bool leader = false)
    {
        Class = heroType;
        HP = hp;
        Morale = morale;
        Anima = anima;
        Leader = leader;
    }
    public HeroType Class { get; }
    public int HP { get; }
    public int Morale { get; }
    public int Anima { get; }
    public bool Leader { get; }
}