using Godot;
using System;

public partial class TileCollider : Node2D
{
    [Export] public int tileID;
    public Sprite2D sprite;
    public Area2D area;
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
