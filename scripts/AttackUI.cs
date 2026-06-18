using Godot;
using System;

public partial class AttackUI : Node2D
{
    public Attack atk;
    public ActionUI ui;

    public override void _Ready()
    {
        ui = GetNode<ActionUI>("ActionUI");
    }

    public void UpdateText(Attack atk)
    {
        this.atk = atk;
        string s = atk.name + "\n" + "Damage: " + atk.damage;
        if(atk.status != 0) s += " Add " + atk.status + "(" + atk.statusDuration + " turns)";
        ui.UpdateText(s);
    }

    public void ResetText(){ui.ResetText();}

}
