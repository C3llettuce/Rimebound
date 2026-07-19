using Godot;
using System;

public partial class EventScene : Node2D
{
    [Export] EventButton[] options;
    [Export] RichTextLabel eventDescription;
    public EventData eventData;

    public bool ClickEventCheck(InputEvent e)
    {
        if(e is InputEventMouseButton) if((e as InputEventMouseButton).ButtonIndex == MouseButton.Left && (e as InputEventMouseButton).Pressed) return true;
        return false;
    }
    public override async void _Ready()
    {
        base._Ready();
        eventData = new EventData(RunManager.Instance.currentEvent);
        for(int i = 0; i< options.Length && i<eventData.options.Count; i++)
        {
            EventButton eb = options[i];
            eb.textLabel.Text = eventData.options[i].name;
            eb.eventOption = eventData.options[i];
            eb.GetArea().InputEvent += (a, e, c) =>
            {
                if (ClickEventCheck(e))
                {
                    eb.eventOption.Activate();
                } 
            };
        }
    }
}
