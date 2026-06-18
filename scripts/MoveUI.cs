using Godot;
using System;

public partial class MoveUI : Node2D
{
   
    public ActionUI ui;
    public override void _Ready()
    {
        ui = GetNode<ActionUI>("ActionUI");
        base._Ready();
    }

    public void UpdateText(Actor a)
    {
        string s = a.name + "\n" + "Move: " + a.movement;
        ui.UpdateText(s);
    }

    public void ResetText()
    {
    }
}
