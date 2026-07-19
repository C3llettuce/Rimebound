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
        ui.UpdateText(atk.name);
    }

    public void ResetText(){ui.ResetText();}

}
