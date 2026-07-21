using Godot;

public enum TileState
{
    None = 0,
    Starfall = 1,
    Trapped = 2

}

//Using abstract classes for now, unsure if best method, might change later to something else
public abstract class TileStatus
{
    public TileState status;
    public Actor owner;
    public TileCollider attachedTile;

    public TileStatus(TileState status, Actor owner, TileCollider attachedTile)
    {
        this.status = status;
        this.owner = owner;
        this.attachedTile = attachedTile;
    }
    public void RemoveStatus()
    {
        attachedTile.tileStatuses.Remove(this);
    }
}

public abstract class TileTrigger : TileStatus
{
    public bool onEnter, onTurnStart, onTurnEnd;
    public int triggers;
    public TileTrigger(TileState status, Actor owner, TileCollider attachedTile, int triggers, bool onEnter = false, bool onTurnStart = false, bool onTurnEnd = false):base(status, owner, attachedTile)
    {
        this.triggers = triggers;
        this.onEnter = onEnter;
        this.onTurnStart = onTurnStart;
        this.onTurnEnd = onTurnEnd;
    }
}
public abstract class TilePassive : TileStatus
{
    public int duration;
    public TilePassive(TileState status, Actor owner, TileCollider attachedTile, int duration):base(status, owner, attachedTile)
    {
        this.duration = duration;
    }
    public abstract void Tick();
}


//Actual tile effects here
public class TileStarfall: TilePassive
{
    public int damageOnCast;
    public TileStarfall(TileState status, Actor owner, TileCollider attachedTile, int duration, int damageOnCast):base(status, owner,attachedTile, duration)
    {
        this.damageOnCast = damageOnCast;
    }
    public override void Tick()
    {
        duration -= 1;
        GD.Print("Ticking Starfall at tile " + attachedTile.tileID + ". Duration: " + duration);
        if(duration == 0)
        {
            BattleScene bs = RunManager.Instance.currentBattle;
            //for now only checking enemies unless I add an evil astronomer
            foreach(Enemy e in bs.enemies)
            {
                if(e.position == attachedTile.tileID)
                {
                    e.ChangeHealth(damageOnCast);
                    break;
                }
            }
            attachedTile.tileStatuses.Remove(this);
        }

    }
}
