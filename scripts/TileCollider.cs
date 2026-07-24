using Godot;
using System;
using System.Collections.Generic;

public partial class TileCollider : Node2D
{
    [Export] public int tileID;
    public Sprite2D sprite;
    public Area2D area;
    public bool isHero = true;
    public readonly List<TileStatus> tileStatuses = new List<TileStatus>();
    public List<DisplayIcon> icons = new List<DisplayIcon>();
    private PackedScene iconScene;
    public int stateDuration = 0;
    public override void _Ready()
    {
        iconScene = GD.Load<PackedScene>("res://scenes/ui/display_icon.tscn");
        sprite = GetNode<Sprite2D>("Sprite2D");
        area = GetNode<Area2D>("Area2D");
        sprite.Visible = false;
        base._Ready();
    }

    public Area2D GetArea()
    {
        return GetNode<Area2D>("Area2D");
    }

    public void AddTileStatus(TileStatus ts)
    {
        tileStatuses.Add(ts);
        int ti = 100;
        if(ts is TilePassive) ti = (ts as TilePassive).duration;
        DisplayIcon di = AddTileStateIcon(ts.status, ti);
        ts.SetIcon(di);
        ArrangeIcons();
    }


    public void ArrangeIcons()
    {
        int perRow = 3;
        int rows = (int)MathF.Ceiling((float)icons.Count/perRow);
        //GD.Print(rows);
        for(int i = 0; i < icons.Count; i++)
        {
            DisplayIcon di = icons[i];
            //no clue if this works tbh
            di.Position = new Vector2(-50 + 50*(i%perRow), 50*rows + 50*(i/perRow));
        }
    }

    private DisplayIcon AddTileStateIcon(TileState type, int duration)
    {
        Node2D newIconNode = (Node2D)iconScene.Instantiate();
        AddChild(newIconNode);
        DisplayIcon newIcon = newIconNode as DisplayIcon;
        newIcon.Init(type, duration);
        icons.Add(newIcon);
        return newIcon;
    }
}
