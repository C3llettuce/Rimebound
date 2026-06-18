using Godot;
using System;

public partial class HeroSprite : ActorSprite
{
    Hero parentHero;
    public override void _Ready()
    {
        base._Ready();
        parentHero = parent as Hero;
        parentHero.sprite = this;
    }
}
