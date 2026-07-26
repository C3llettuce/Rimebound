using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;

public partial class TargetingUI : Node2D
{
    [Export] public Sprite2D[] heroTiles;
    [Export] public Sprite2D[] buffTiles;
    [Export] public Sprite2D[] enemyTiles;

    public override void _Ready()
    {
    }


    public void PreviewAttack(Attack atk)
    {
        foreach(Sprite2D s in heroTiles) s.Visible = false;
        foreach(Sprite2D s in enemyTiles) s.Visible = false;
        foreach(Sprite2D s in buffTiles) s.Visible = false;
        if(atk == null) return;
        List<int> splitUse = GetSplitPosition(atk.usePosition);
        List<int> splitTarget = GetSplitPosition(atk.targetPosition);
        for(int i = 0; i < splitUse.Count; i++) heroTiles[(int)MathF.Log2(splitUse[i])].Visible = true;
        if(atk.targetingType == TargetingType.Basic)
        {
            if(atk.isBuff) for(int i = 0; i < splitTarget.Count; i++) buffTiles[(int)MathF.Log2(splitTarget[i])].Visible = true;
            else for(int i = 0; i < splitTarget.Count; i++) enemyTiles[(int)MathF.Log2(splitTarget[i])].Visible = true;
        }
        else
        {
            bool[] validTiles = {false, false, false, false, false, false};
            for(int i = 0; i < splitTarget.Count; i++)
            {
                if(atk.CheckValidAttack(atk.owner.position, splitTarget[i])) validTiles[atk.bs.battleManager.bitToID[splitTarget[i]]] = true;
            }
            if(atk.isBuff) 
            {
            for(int i = 0; i < validTiles.Length; i++) if(validTiles[i]) buffTiles[i].Visible = true;
            }
            else for(int i = 0; i < validTiles.Length; i++) if(validTiles[i]) enemyTiles[i].Visible = true;
        } 
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
