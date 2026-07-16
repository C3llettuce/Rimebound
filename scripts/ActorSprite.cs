using Godot;
using System;

public partial class ActorSprite : Node
{
    protected Actor parent;
    public Sprite2D sprite2D;
    protected Area2D spriteCollider;
    public override void _Ready()
    {
        parent = GetParent() as Actor;
        sprite2D = GetNode<Sprite2D>("Sprite2D");
        spriteCollider = GetNode<Area2D>("Area2D");
        base._Ready();
    }


    public void Hide()
    {
        sprite2D.Visible = false;
        spriteCollider.InputPickable = false;
    }
}
