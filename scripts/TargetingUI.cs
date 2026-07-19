using Godot;
using System;
using System.Collections.Generic;

public partial class TargetingUI : Node2D
{
    [Export] public Sprite2D[] heroTiles;
    [Export] public Sprite2D[] enemyTiles;

    public override void _Ready()
    {
    }


    public void PreviewAttack(Attack atk)
    {
        foreach(Sprite2D s in heroTiles) s.Visible = false;
        foreach(Sprite2D s in enemyTiles) s.Visible = false;
        if(atk == null) return;
        List<int> splitUse = GetSplitPosition(atk.usePosition);
        List<int> splitTarget = GetSplitPosition(atk.targetPosition);
        for(int i =0; i < splitUse.Count; i++) heroTiles[(int)MathF.Log2(splitUse[i])].Visible = true;
        for(int i =0; i < splitTarget.Count; i++) enemyTiles[(int)MathF.Log2(splitTarget[i])].Visible = true;
    }


    private List<int> GetSplitPosition(int positions)
    {
        List<int> splitPositions = new List<int>();
        while(positions > 0)
        {
            int minBit = positions & -positions;
            splitPositions.Add(minBit);
            positions -= minBit;
        }
        return splitPositions;
    }

}
