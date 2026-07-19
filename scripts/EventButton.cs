using Godot;
using System;

public partial class EventButton : Node2D
{
    [Export] public Area2D clickArea;
    [Export] public RichTextLabel textLabel;
    public EventOption eventOption;
    public Area2D GetArea()
    {
        return GetNode<Area2D>("Area2D");
        
    }
}
