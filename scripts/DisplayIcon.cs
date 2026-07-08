using Godot;
using System;

public partial class DisplayIcon : Node2D
{
    public StatusType type;
    public int duration;
    private Sprite2D icon;
    private RichTextLabel durationLabel;


    public override void _Ready()
    { 
        durationLabel = GetNode<RichTextLabel>("InfoText");
        icon = GetNode<Sprite2D>("Icon");
        base._Ready();
    }

    public void Init(StatusType type, int duration = 3)
    {
        this.type = type;
        this.duration = duration;
        int i = (int)type -1;
        //subject to change when sprite sheet changes size
        int width = 3;
        int height = 3;
        int spriteSize = 25;
    
        icon.RegionRect = new Rect2(new Vector2(spriteSize*(i%width), spriteSize*(i/height)), new Vector2(spriteSize, spriteSize));
        durationLabel.Text = duration.ToString();
    }

    public void UpdateLabel(int change)
    {
        duration += change;
        durationLabel.Text = duration.ToString();
    }
}
