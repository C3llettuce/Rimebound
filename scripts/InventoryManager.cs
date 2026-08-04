
using System;
using System.Collections.Generic;
using Godot;

public enum RelicTypes
{
    MaxHP = 0,
    StressResist = 10,
    Discount = 20
}
public enum LootType
{
    gold = 0,
    trophies = 1,
    renown = 2,
    secrets = 3,
    rations = 4
}
public partial class InventoryManager : Node2D
{
    public static InventoryManager Instance {get; private set;}
    public int[] resources = new int[5];
    public List<int> relics = new List<int>();
    public override void _Ready()
    {
        Instance = this;
    }


    public bool CanBuy(LootType lootType, int price)
    {
        if(resources[(int)lootType] < price) return false;
        return true;
    }

    public bool CanBuy(LootType[] lootTypes, int[] prices)
    {
        for(int i = 0; i < lootTypes.Length; i++) if(!CanBuy(lootTypes[i], prices[i])) return false;
        return true;
    }

    public void Buy(LootType lootType, int price)
    {
        resources[(int)lootType] -= price;
        if(resources[(int)lootType]<0) resources[(int)lootType] = 0;
    }

    public void Buy(LootType[] lootTypes, int[] prices) { for(int i = 0; i < lootTypes.Length; i++) Buy(lootTypes[i], prices[i]); }

    public void AddItem(LootType lootType, int amount) { resources[(int)lootType] += amount; }

    public void AddItems(LootType[] lootTypes, int[] amounts) { for(int i = 0; i < lootTypes.Length; i++) AddItem(lootTypes[i], amounts[i]); }
}