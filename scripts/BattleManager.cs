using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public enum SelectMode
{
    Any = 7,
    Hero = 1,
    Enemy = 2,
    Empty = 4
}

//Handles round to round combat as well as attack resolution and enemy AI
public partial class BattleManager : Node2D
{
    BattleScene battleScene; public bool isRunning = true;
    List<Actor> roundOrder;
    Actor activeActor;
    Hero selectedHero; Enemy selectedEnemy; Attack selectedAttack; bool isMoving = false;
    TileCollider selectedTile;
    public SelectMode selectMode = SelectMode.Any;
    public event EventHandler HeroUseAttack;
    private TaskCompletionSource<bool> HeroTurn = new TaskCompletionSource<bool>();
    private int[] adjacencies = {6, 9, 25, 38,36, 24};


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
            //call start turn for next actor in initiative
            if(activeActor != null){activeActor.TurnStart();}
            //run enemy ai if enemy
            if(roundOrder[i] is Enemy) CalculateEnemyTargets(roundOrder[i] as Enemy);
            //otherwise wait for player input w/ async task if hero
            else if(roundOrder[i] is Hero)
            {
                GD.Print(activeActor.name + "'s turn");
                await HeroTurn.Task;
                HeroTurn = new TaskCompletionSource<bool>();
            }
            if(!isRunning) break;
        }
    }

    private void EndActorTurn()
    {
        bool dead = activeActor.TurnEnd();
        if(activeActor is Hero && dead == true)
        {
            KillActor(activeActor);
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
        //apply morale change if hero died/panicked
        if(actor is Hero)
        {
            int stress = 2;
            //less stress for fleeing than death
            if(actor.health>0) stress = 1;
            foreach(Hero h in battleScene.heroes) h.ChangeMorale(stress);
        }
        
        //reset relavent selected things if they were tied to the dead actor
        if(selectedHero == actor)
        {
                selectedHero = null;  
                selectedAttack = null;
                battleScene.moveUI.ResetText();
        }
        else if(selectedEnemy == actor) selectedEnemy = null;
    }

    public void HeroPanic(Hero hero)
    {
        //for now just kills panicker
        KillActor(hero);
    }

    public void ChangeSelectMode(SelectMode sm)
    {
        if (selectMode == sm) return;
        selectMode = sm;
        if(((int)sm&(int)SelectMode.Hero)!=0){}
    }

    
    public void SelectCharacter(Actor selected)
    {
        //should add reset functions for deselecting actors
        if(selected is Hero)
        {
            //code for using buff attacks
            if(selectedAttack != null)
            {
                if (selectedAttack.isTileTargeted)
                {
                    int tileID = (int)MathF.Log2(selected.position);
                    GD.Print(MathF.Pow(2,tileID) + " =? " + battleScene.heroGrid[tileID].tileID);
                    SelectTile(battleScene.heroGrid[tileID]);
                }
                else if(selectedAttack.isBuff && activeActor == selectedHero
                && CheckValidAttack(selectedAttack, selectedHero, selected))
                {
                    selectedAttack.Use(selected, selectedHero);
                    EndActorTurn();
                    HeroTurn.TrySetResult(true);
                }
                //bad redundant code here, will fix later
                else
                {
                    if(selected != selectedHero) selectedAttack = null;
                    selectedHero = selected as Hero;
                    GD.Print(selectedHero.name + " at " + selectedHero.position + " selected");
                }
            }
            else
            {
                if(selected != selectedHero) selectedAttack = null;
                selectedHero = selected as Hero;
                GD.Print(selectedHero.name + " at " + selectedHero.position + " selected");
            }
            //reset attack if choosing a new character
            
        }
        else if(selected is Enemy)
        {
            //check if attack is null first to avoid error
            if (selectedAttack.isTileTargeted)
            {
                int tileID = (int)MathF.Log2(selected.position);
                GD.Print(MathF.Pow(2,tileID) + " =? " + battleScene.enemyGrid[tileID].tileID);
                SelectTile(battleScene.enemyGrid[tileID]);
            }
            else if(selectedEnemy == selected as Enemy && selectedAttack != null && activeActor == selectedHero && !selectedAttack.isBuff
            && CheckValidAttack(selectedAttack, selectedHero, selectedEnemy))
            {
                selectedAttack.Use(selectedEnemy, selectedHero);
                EndActorTurn();
                HeroTurn.TrySetResult(true);
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

        if (isMoving)
        {
            GD.Print("attempting move");
            if(CheckValidMove(activeActor, selected.tileID)) MoveActor(activeActor, selected.tileID);
        }
        else if(selectedAttack != null)
        {
            if(selectedAttack.isTileTargeted && activeActor == selectedHero && ((selectedAttack.isBuff && selected.isHero)||(!selectedAttack.isBuff && !selected.isHero))
            && CheckValidAttack(selectedAttack, activeActor.position, selected.tileID)){
                selectedAttack.Use(null, null, selected);
                GD.Print("tile target succesful");
                EndActorTurn();
                HeroTurn.TrySetResult(true);
            }
        }
    }

    public bool SelectAttack(Attack atk)
    {
        if(atk != null)
        {
            isMoving = false;
            selectedAttack = atk;
            GD.Print(selectedHero.name + " at " + selectedHero.position + "'s attack " + atk.name + " (buff: " + atk.isBuff + ") selected");
            return true;
        }
        return false;
    }

    public void SelectMovement()
    {
        isMoving = true;
    }

    private void MoveActor(Actor movingActor, int targetPosition, bool isFree = false)
    {
        GD.Print(movingActor.name + " moved to tile " + targetPosition + " from tile " + movingActor.position);
        if(movingActor is Hero) foreach(Hero h in battleScene.heroes) if(h.position == targetPosition) SwapActor(h, movingActor.position);
        if(movingActor is Enemy) foreach(Enemy e in battleScene.enemies) if(e.position == targetPosition) SwapActor(e, movingActor.position);
        movingActor.OnMove(movingActor.position, targetPosition);
        movingActor.position = targetPosition;
        battleScene.MoveActor(movingActor, targetPosition);
        
        isMoving = false;
        if (!isFree)
        {
            EndActorTurn();
            if(activeActor is Hero) HeroTurn.TrySetResult(true);
        }
    }

    private void SwapActor(Actor swappingActor, int targetPosition)
    {
        swappingActor.OnMove(swappingActor.position, targetPosition);
        swappingActor.position = targetPosition;
        battleScene.MoveActor(swappingActor, targetPosition);
    }

    private int GetValidMove(Actor movingActor, int targetPosition,bool teleport = false)
    {
        //can't move if snared
        if(movingActor.statuses[(int)StatusType.Snared]>0) return 0;
        //check if selected tile is adjacent. log is inneficient and should be stored in a dict or something but idc rn
        if (!teleport)
        {
            if((adjacencies[(int)MathF.Log2(movingActor.position)]&targetPosition)==0) return 0;
        }
        if(movingActor is Hero)
        {
            foreach(Actor a in battleScene.heroes)
            {
                if((a.position & targetPosition) != 0 && a.statuses[(int)StatusType.Snared] != 0) targetPosition -= a.position;
                if(targetPosition == 0) return 0;
            } 
        }
        else
        {
            foreach(Actor a in battleScene.enemies)
            {
                if((a.position & targetPosition) != 0 && a.statuses[(int)StatusType.Snared] != 0) targetPosition -= a.position;
                if(targetPosition == 0) return 0;
            }  
        }   
        return targetPosition;
    }

    private bool CheckValidMove(Actor movingActor, int targetPosition,bool teleport = false)
    {
        if (GetValidMove(movingActor, targetPosition, teleport) != 0) return true;
        return false;
    }


    private bool CheckValidAttack(Attack atk, int usePosition, int targetPosition)
    {
        return atk.CheckValidAttack(usePosition, targetPosition);
    }
    private bool CheckValidAttack(Attack atk, Actor user, Actor target) { return CheckValidAttack(atk, user.position, target.position); }

    //check if an actor has any usable attacks based on current position
    private bool AnyValidAttack(Actor actor, bool isHero)
    {
        int targetableTiles = 0;
        List<Actor> potentialTargets;
        if(isHero) potentialTargets = battleScene.enemies.Cast<Actor>().ToList();
        else potentialTargets = battleScene.heroes.Cast<Actor>().ToList();
        
        foreach(Actor a in potentialTargets) targetableTiles += a.position;
        foreach(Attack atk in actor.attacks)
        {
            if(CheckValidAttack(atk, actor.position, targetableTiles)) return true;
        }
        return false;
    }

    //check if an attack has any valid targets, regardless of user position
    private bool AnyValidTarget(Attack attack, bool isHero)
    {
        List<Actor> targets;
        if (isHero) targets = battleScene.enemies.Cast<Actor>().ToList();
        else  targets = battleScene.heroes.Cast<Actor>().ToList();
        foreach(Actor a in targets)
        {
            if((a.position & attack.targetPosition)!=0) return true;
        }
        return false;
    }


    //run a given enemy's entire turn. may need to split up into multiple functions later
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
            //should add options for priority attacks
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
            CalculateEnemyMovement(enemy);
            //move code here
        }
        //call relevant turn end stuff
        EndActorTurn();
    }

    private void CalculateEnemyMovement(Enemy enemy)
    {
        Random rand = new Random();
        int validMoves = adjacencies[(int)MathF.Log2(enemy.position)];
        int allSmartMoves = 0;
        List<Attack> enemyAttacks = enemy.attacks.ToList();
        foreach(Attack a in enemyAttacks)
        {
            int potentialMove = validMoves & a.usePosition;
            if (potentialMove != 0)
            {
                potentialMove = GetValidMove(enemy, potentialMove, true);
                if (AnyValidTarget(a,false)) allSmartMoves = allSmartMoves|potentialMove;
            }
        }
        //if there is a move that would allow an attack next turn, go there
        if(allSmartMoves != 0)
        {
            List<int> smartMoves = GetSplitPosition(allSmartMoves);
            MoveActor(enemy, smartMoves[rand.Next(smartMoves.Count)]);
        }
        //otherwise go to a random adjacent space (should try to look 1 step into the future)
        else
        {
            validMoves = GetValidMove(enemy, validMoves, true);
            if(validMoves == 0) return;
            List<int> randMoves = GetSplitPosition(validMoves);
            MoveActor(enemy, randMoves[rand.Next(randMoves.Count)]);
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