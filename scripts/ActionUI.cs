using Godot;
using System;

public enum ActionButtonType
{
    Attack, Move, Pass, Enthrall
}
public partial class ActionUI : Node2D
{
    public RichTextLabel label;
    public ActionButtonType actionType;
    public override void _Ready()
    {
        label = GetNode<RichTextLabel>("MoveDescription");
        base._Ready();
    }

    public virtual void UpdateText(string newText)
    {
        label.Text = newText;
    }

    public Area2D GetArea()
    {
        return GetNode<Area2D>("Area2D");
    }

    public void ResetText()
    {
        label.Text = "";
    }
}
