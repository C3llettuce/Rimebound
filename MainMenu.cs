using Godot;
using System;

public partial class MainMenu : Node2D
{
    private BattleScene testingScene;
    private PackedScene packedBattleScene;
    public override void _Ready()
    {
        base._Ready();
        packedBattleScene = GD.Load<PackedScene>("res://scenes/battles/battle_scene.tscn");
    }

    public override void _PhysicsProcess(double delta)
    {
        //Go to current testing battle
        if(Input.IsKeyPressed(Key.Space) && testingScene == null)
        {
            RunManager.Instance.SetDebugHeroes();
            GetTree().ChangeSceneToPacked(packedBattleScene);
            // testingScene = (BattleScene)packedBattleScene.Instantiate();
            // AddChild(testingScene);
        }


        base._PhysicsProcess(delta);
    }


}
