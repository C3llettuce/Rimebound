using Godot;
using System;
using System.Data;


public enum MeterType
{
    Health = 1,
    Morale = 2,
    Anima = 3
}
public partial class DisplayMeter : Node2D
{
    public RichTextLabel label;
    public MeterType meterType;
    public int meterMax;
    public int meterValue;
    private Sprite2D meterFill;
    private float meterFillWidthMax;
    private float meterFillWidth;
    private float fillWidth;

    public override void _Ready()
    {
        label = GetNode<RichTextLabel>("MeterText");
        meterFill = GetNode<Sprite2D>("MeterFill");
        meterFillWidthMax = meterFill.Transform.Scale.X;
        fillWidth = meterFill.Texture.GetWidth()*meterFillWidthMax;
        base._Ready();
    }

    public void Init(MeterType meterType, int meterMax, int meterValue)
    {
        this.meterType = meterType;
        this.meterMax = meterMax;
        this.meterValue = meterValue;
        UpdateMeter();
    }

    public void Init(MeterType meterType, int meterMax){Init(meterType, meterMax, meterMax);}

    public void ResetMeter(){}

    public void OverwriteMeter(int newMax, int newValue)
    {
        meterMax = newMax;
        meterValue = newValue;
        UpdateMeter(0);
    }
    public void UpdateMeter(int change = 0)
    {
        meterValue += change;
        //clamp value
        if(meterValue > meterMax) meterValue = meterMax;
        else if(meterValue < 0) meterValue = 0;
        //update text
        label.Text = meterType + ": " + meterValue + "/" + meterMax;
        //update bar size and position
        meterFillWidth = ((float)meterValue/meterMax)*meterFillWidthMax;
        meterFill.Scale = new Vector2(meterFillWidth, meterFill.Scale.Y);
        float newX = -fillWidth/2 + (fillWidth/2)*((float)meterValue/meterMax);
        meterFill.Position = new Vector2(newX, 0);
    }

}
