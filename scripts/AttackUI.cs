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
        if(atk == null){
            ui.UpdateText("");
            return;
        }
        this.atk = atk;
        string s = atk.name + "\n" + "Damage: " + atk.damage;
        if(atk.status[0] != 0)
        {
            for(int i = 0; i< atk.status.Length; i++) s += " Add " + atk.status[i] + "(" + atk.statusDuration[i] + " turns)";
        }
        ui.UpdateText(s);
    }

    public void ResetText(){ui.ResetText();}

}
