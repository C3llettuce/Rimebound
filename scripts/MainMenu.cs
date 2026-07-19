using Godot;
using System;

public partial class MainMenu : Node2D
{
    private BattleScene testingScene;
    private PackedScene packedBattleScene;
    private PackedScene packedEventScene;
    public override void _Ready()
    {
        base._Ready();
        packedBattleScene = GD.Load<PackedScene>("res://scenes/battles/battle_scene.tscn");
        packedEventScene = GD.Load<PackedScene>("res://scenes/events/event_scene.tscn");
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
        else if(Input.IsKeyPressed(Key.Shift) && testingScene == null)
        {
            RunManager.Instance.SetDebugThrallHeroes();
            GetTree().ChangeSceneToPacked(packedBattleScene);
        }
        else if(Input.IsKeyPressed(Key.E) && testingScene == null)
        {
            RunManager.Instance.SetCurrentEvent(Event.LostHunter);
            GetTree().ChangeSceneToPacked(packedEventScene);
        }


        base._PhysicsProcess(delta);
    }


}
