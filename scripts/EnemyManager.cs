using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class EnemyManager : Node2D
{
    protected List<Enemy> activeEnemies;

    public override void _Ready()
    {
        base._Ready();
        activeEnemies = new List<Enemy>();
    }
}
