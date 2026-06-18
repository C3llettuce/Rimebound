using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

//Handles round to round combat as well as attack resolution and enemy AI
public partial class BattleManager : Node2D
{
    BattleScene battleScene; public bool isRunning = true;
    List<Actor> roundOrder;
    Actor activeActor;
    Hero selectedHero; Enemy selectedEnemy; Attack selectedAttack;
    TileCollider selectedTile;
    public event EventHandler HeroUseAttack;
    private TaskCompletionSource<bool> HeroTurn = new TaskCompletionSource<bool>();


    public async Task Init()
    {
        roundOrder = new List<Actor>();
        battleScene = GetParent() as BattleScene;
        await RoundStart();
    }

    public async Task RoundStart()
    {
        //need to double check where breaks will bring me
        //also should add some randomization to initiative
        List<Actor> actors = [.. battleScene.heroes];
        Random random = new Random();
        foreach(Enemy e in battleScene.enemies) {actors.Add(e);}
        foreach(Actor a in actors)
        {
            if(roundOrder.Count == 0) roundOrder.Add(a);
            else
            {
                bool placed = false;
                for(int i = 0; i < roundOrder.Count; i++)
                {
                    int speed = roundOrder[i].Speed;
                    if(a.Speed > speed) 
                    {
                        roundOrder.Insert(i, a);
                        placed = true;
                        break;
                    }
                    else if(a.Speed == speed)
                    {
                        int range = 2;
                        for(int j = i + 1; j<roundOrder.Count; j++)
                        {
                            if(roundOrder[j].Speed == a.Speed) range += 1;
                            else break;
                        }
                        roundOrder.Insert(i+random.Next(range), a);
                        placed = true;
                        break;
                    }
                }
                if(!placed) roundOrder.Add(a);
            }
        }
        // string initiative = "Initiative: ";
        // for(int i = 0; i < roundOrder.Count; i++) {initiative += (roundOrder[i].name + "(" + roundOrder[i].position + "), ");}
        // GD.Print(initiative);
        int rounds = 0;
        while(isRunning)
        {
            rounds++;
            await RunRound();
        }
        GD.Print("Combat over in " + rounds + " rounds");
    }
    private async Task RunRound()
    {
        
        for(int i = 0; i < roundOrder.Count; i++)
        {
            activeActor = roundOrder[i];
            if(roundOrder[i] is Enemy) CalculateEnemyTargets(roundOrder[i] as Enemy);
            else if(roundOrder[i] is Hero)
            {
                GD.Print(activeActor.name + "'s turn");
                await HeroTurn.Task;
                HeroTurn = new TaskCompletionSource<bool>();
            }
            if(!isRunning) break;
        }
    }

    public void KillActor(Actor actor)
    {
        for(int i = 0; i < roundOrder.Count; i++)
        {
            if(roundOrder[i] == actor)
            {
                roundOrder[i] = null;
                break;
            } 
        }
        actor.sprite.Hide();
        //reset relavent selected things if they were tied to the dead actor
        if(selectedHero == actor)
        {
                selectedHero = null;  
                selectedAttack = null;
                battleScene.moveUI.ResetText();
        }
        else if(selectedEnemy == actor) selectedEnemy = null;
    }

    
    public void SelectCharacter(Actor selected)
    {
        //should add reset functions for deselecting actors
        if(selected is Hero)
        {
            if(selected != selectedHero) selectedAttack = null;
            selectedHero = selected as Hero;
            GD.Print(selectedHero.name + " at " + selectedHero.position + " selected");
        }
        else if(selected is Enemy)
        {
            if(selectedEnemy == selected as Enemy && selectedAttack != null && activeActor == selectedHero && CheckValidAttack(selectedAttack, selectedHero, selectedEnemy))
            {
                selectedAttack.Use(selectedEnemy, selectedHero);
                GD.Print(HeroTurn.TrySetResult(true));
                selectedEnemy = null;
            } 
            else 
            {
            selectedEnemy = selected as Enemy;
            GD.Print(selectedEnemy.name + " at " + selectedEnemy.position + " selected");
            }
        }
    }

    //called 
    public void SelectTile(TileCollider selected)
    {
        if(selectedTile != null) selectedTile.sprite.Visible = false;
        selectedTile = selected;
        selectedTile.sprite.Visible = true;
    }

    public void SelectTile(TileCollider tc, bool isHero)
    {
        
    }

    public bool SelectAttack(Attack atk)
    {
        if(atk != null)
        {
            selectedAttack = atk;
            GD.Print(selectedHero.name + " at " + selectedHero.position + "'s attack " + atk.name + " selected");
            return true;
        }
        return false;
    }

    //check if a any unit on this actor's team is already occupying the target square
    private bool CheckValidMove(Actor movingActor, int targetPosition)
    {
        bool valid = true;
        if(movingActor is Hero)
        {
            foreach(Actor a in battleScene.heroes) if(a.position == targetPosition) valid = false;
        }
        else
        {
            foreach(Actor a in battleScene.enemies) if(a.position == targetPosition) valid = false;
        }   
        return valid;
    }

    private bool CheckValidAttack(Attack atk, int usePosition, int targetPosition)
    {
        if((atk.usePosition & usePosition) != 0 && (atk.targetPosition & targetPosition) != 0) return true;
        return false;
    }
    private bool CheckValidAttack(Attack atk, Actor user, Actor target) { return CheckValidAttack(atk, user.position, target.position); }

    private bool AnyValidAttack(Actor actor, bool isHero)
    {
        int targetableTiles = 0;
        List<Actor> potentialTargets;
        if(isHero) potentialTargets = battleScene.enemies.Cast<Actor>() as List<Actor>;
        else potentialTargets = battleScene.heroes.Cast<Actor>() as List<Actor>;
        foreach(Actor a in potentialTargets) targetableTiles += a.position;
        foreach(Attack atk in actor.attacks)
        {
            if(CheckValidAttack(atk, actor.position, targetableTiles)) return true;
        }
        return false;
    }


    private void CalculateEnemyTargets(Enemy enemy)
    {
        int occupiedTiles = 0;
        foreach(Hero h in battleScene.heroes)
        {
            occupiedTiles += h.position;
        }

        Random rand = new Random();
        List<Attack> enemyAttacks = enemy.attacks.ToList();
        Attack atk = null;
        bool validAttack = false;
        //iterate through attacks randomly until we hit one with at least 1 valid target
        while (!validAttack)
        {
            if(enemyAttacks.Count == 0) break;
            atk = enemyAttacks[rand.Next(enemyAttacks.Count)];
            if((atk.usePosition & enemy.position) != 0 && (atk.targetPosition & occupiedTiles) != 0) validAttack = true;
            else enemyAttacks.Remove(atk);
        }
        //assuming there was a valid attack, use the randomly selected one
        if(validAttack)
        {
            List<Actor> possibleTargets = new List<Actor>();
            //filter for specific list of targetable tiles with heroes on them. Will have length of at least 1
            for(int i = 1; i< 33; i *= 2)
            {
                if((atk.targetPosition & occupiedTiles & i) != 0) possibleTargets.Add(battleScene.GetHeroAt(i));
            }
            //need to account for tile-combo specific attacks
            if(atk.isAoe) atk.Use(possibleTargets, enemy);
            else
            {
                int target = rand.Next(possibleTargets.Count);
                atk.Use(possibleTargets[target], enemy);
            } 
        }
        else
        {
            //move code here
        }
    }
}