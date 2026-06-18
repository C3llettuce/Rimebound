using Godot;
using System;

public partial class EnemySprite : ActorSprite
{
    private Enemy parentEnemy;
    public override void _Ready()
    {
        base._Ready();
        parentEnemy = parent as Enemy;
        parentEnemy.sprite = this;
    }
}
