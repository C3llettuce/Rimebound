using Godot;
using System;
using System.Collections.Generic;

public partial class TileCollider : Node2D
{
    [Export] public int tileID;
    public Sprite2D sprite;
    public Area2D area;
    public bool isHero = true;
    public List<TileStatus> tileStatuses = new List<TileStatus>();
    public int stateDuration = 0;
    public override void _Ready()
    {
        sprite = GetNode<Sprite2D>("Sprite2D");
        area = GetNode<Area2D>("Area2D");
        sprite.Visible = false;
        base._Ready();
    }

    public Area2D GetArea()
    {
        return GetNode<Area2D>("Area2D");
    }
}
