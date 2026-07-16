using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

//Used to track various lists of actors as well as set up UI events
public partial class BattleScene : Node2D
{
    EnemyManager enemyManager;
    public TargetingUI targetingUI;
    public MoveUI moveUI;
    public List<AttackUI> atkUIS;
    public ActionUI passUI;
    public List<Hero> heroes; public List<Enemy> enemies;
    public List<TileCollider> heroGrid, enemyGrid;
    public int resolutionScale = 1;
    public Vector2 baseResolution = new Vector2(0,0);
    Vector2[] heroPositions = {new Vector2(-215, -10), new Vector2(-215, 145), new Vector2(-365, -10), new Vector2(-365, 145), new Vector2(-515, -10), new Vector2(-515, 145)};
    Vector2[] enemyPositions = {new Vector2(215, -10), new Vector2(215, 145), new Vector2(365, -10), new Vector2(365, 145), new Vector2(515, -10), new Vector2(515, 145)};
    int yPosOffset = -50;
    BattleManager battleManager;

    public bool ClickEventCheck(InputEvent e)
    {
        if(e is InputEventMouseButton) if((e as InputEventMouseButton).ButtonIndex == MouseButton.Left && (e as InputEventMouseButton).Pressed) return true;
        return false;
    }
    public override async void _Ready()
    {
        base._Ready();
        //temp adjusting of set position vars for debugging and ui testing
        for(int i =0; i< heroPositions.Length; i++) heroPositions[i].Y += yPosOffset;
        for(int i =0; i< enemyPositions.Length; i++) enemyPositions[i].Y += yPosOffset;
        //initialize variables and fetch child nodes from scene
        heroes = new List<Hero>();
        enemies = new List<Enemy>();
        battleManager = GetNode<BattleManager>("battleManager");
        enemyManager = GetNode<EnemyManager>("EnemyManager");
        targetingUI = GetNode<TargetingUI>("TargetingUI");
        moveUI = GetNode<MoveUI>("MoveUI");
        Debug.WriteLine(moveUI.Name);
        atkUIS = new List<AttackUI>();
        heroGrid = new List<TileCollider>();
        enemyGrid = new List<TileCollider>();
        foreach(Node2D tc in GetNode<TileGrid>("TileGridHero").GetChildren()){heroGrid.Add(tc as TileCollider);}
        foreach(Node2D tc in GetNode<TileGrid>("TileGridEnemy").GetChildren())
        {
        enemyGrid.Add(tc as TileCollider);
        (tc as TileCollider).isHero = false;
        }
        atkUIS.Add(GetNode<AttackUI>("AttackUI"));
        atkUIS.Add(GetNode<AttackUI>("AttackUI2"));
        atkUIS.Add(GetNode<AttackUI>("AttackUI3"));

        //add click events to relevant ui nodes
        //add movement event
        moveUI.ui.GetArea().InputEvent += (a, e, c) => 
        {
            if (ClickEventCheck(e)) battleManager.SelectMovement();
        };

        //add attack events
        foreach(AttackUI aui in atkUIS) aui.ui.GetArea().InputEvent += (a, e, c) => {
            if (ClickEventCheck(e)) battleManager.SelectAttack(aui.atk);
        };

        //add tile selection events
        foreach(TileCollider tc in heroGrid) tc.GetArea().InputEvent += (a, e, c) => {
            if(ClickEventCheck(e)) {battleManager.SelectTile(tc);}//todo
        };
        foreach(TileCollider tc in enemyGrid) tc.GetArea().InputEvent += (a, e, c) => {
            if(ClickEventCheck(e)) {battleManager.SelectTile(tc);}//todo
        };

        //start actual battle
        GenerateEnemies();
        PlaceHeroes();
        await battleManager.Init();
    }

    public void MoveActor(Actor movingActor, int newGridPosition)
    {
        if(movingActor is Hero) movingActor.GlobalPosition = heroPositions[(int)MathF.Log2(newGridPosition)];
        else movingActor.GlobalPosition = enemyPositions[(int)MathF.Log2(newGridPosition)];
    }

    //place heroes onto battle scene
    void PlaceHeroes()
    {
        GD.Print("hi");
    //make this pull from a roster later
        var heroScene = GD.Load<PackedScene>("res://scenes/battles/hero.tscn");
        List<Hero> tempH = RunManager.Instance.LoadHeroes();
        foreach(Hero h in tempH) AddChild(h);

        for(int i = 0; i<tempH.Count && i<6; i++)
        {
            heroes.Add(tempH[i]);
            tempH[i].Init((int)MathF.Pow(2,i), this);
            tempH[i].GlobalPosition = heroPositions[i];
            GD.Print("adding new hero");
        }

        // Node2D heroInstance1 = (Node2D)heroScene.Instantiate();
        // Node2D heroInstance2 = (Node2D)heroScene.Instantiate();
        // Node2D heroInstance3 = (Node2D)heroScene.Instantiate();
        // AddChild(heroInstance1);
        // AddChild(heroInstance2);
        // AddChild(heroInstance3);
        // Hero tempBandit = heroInstance1 as Hero;
        // Hero tempHunter = heroInstance2 as Hero;
        // Hero tempDoomsayer = heroInstance3 as Hero;
        // tempBandit.Init(HeroType.Duelist, 1, this, 1);
        // tempBandit.GlobalPosition = heroPositions[0];
        // tempHunter.Init(HeroType.Slayer, 16, this, 5);
        // tempHunter.GlobalPosition = heroPositions[4];
        // tempDoomsayer.Init(HeroType.Astronomer, 4, this);
        // tempDoomsayer.GlobalPosition = heroPositions[2];
        // heroes.Add(tempBandit);
        // heroes.Add(tempHunter);
        // heroes.Add(tempDoomsayer);

        //add click events to heros that update move UI
        foreach(Hero h in heroes)
        {
            h.GetNode<HeroSprite>("heroSprite").GetNode<Area2D>("Area2D").InputEvent += (a, e, c) => 
            {
                if (ClickEventCheck(e))
                {
                    Debug.WriteLine(h.attacks.Count + ", " + atkUIS.Count);
                    foreach(AttackUI aui in atkUIS) aui.UpdateText(null);
                    for(int i = 0; i < h.attacks.Count; i++) atkUIS[i].UpdateText(h.attacks[i]);
                    moveUI.UpdateText(h);
                    battleManager.SelectCharacter(h);
                }
            };
        }
    }

    //generate enemies for the battle scene
    public void GenerateEnemies()
    {
        GD.Print("spawning new enemies");
        var enemyScene = GD.Load<PackedScene>("res://scenes/battles/enemy.tscn");
        enemies.Clear();
        //randomize this later (or add set pool of seeded encounters)
        Node2D enemyInstance = (Node2D)enemyScene.Instantiate();
        Node2D enemyInstance2 = (Node2D)enemyScene.Instantiate();
        Node2D stressEnemyInstance = (Node2D)enemyScene.Instantiate();
        AddChild(enemyInstance);
        AddChild(enemyInstance2);
        AddChild(stressEnemyInstance);
        Enemy tempEnemy = enemyInstance as Enemy;
		Enemy tempEnemy2 = enemyInstance2 as Enemy;
        Enemy stressEnemy = stressEnemyInstance as Enemy;
        enemies.Add(tempEnemy);
        enemies.Add(tempEnemy2);
        enemies.Add(stressEnemy);
        tempEnemy.Init(EnemyType.Bandit, 1, this);
        tempEnemy2.Init(EnemyType.BanditArcher, 4, this);
        stressEnemy.Init(EnemyType.StarZealot, 16, this);
        tempEnemy.GlobalPosition = enemyPositions[0];
        tempEnemy2.GlobalPosition = enemyPositions[2];
        stressEnemy.GlobalPosition = enemyPositions[4];

        //click events for enemies
        foreach(Enemy enemy in enemies)
        {
            enemy.GetNode<EnemySprite>("EnemySprite").GetNode<Area2D>("Area2D").InputEvent += (a, e, c) => 
            {
                if (ClickEventCheck(e))
                {
                    //enemy selection code here
                    battleManager.SelectCharacter(enemy);
                }
            };
        }
    }

    /// <summary>
    /// Gets the hero node at a given grid position
    /// </summary>
    /// <param name="position">The desired position, should be a base 2 number (valid positions are 1,2,4,8,16,32)</param>
    /// <returns>The hero at the position, or null if there isn't one</returns>
    public Hero GetHeroAt(int position)
    {
        foreach(Hero h in heroes)
        {
            if(h.position == position) return h;
        }
        return null;
    }

    //Kill an actor (hero or enemy)
    //also checks for winning/losing if the last actor of a side is killed
    public void KillActor(Actor actor)
    {
        if(actor is Hero)
        {
            heroes.Remove(actor as Hero);
        }
        else
        {
            enemies.Remove(actor as Enemy);
        }
        battleManager.KillActor(actor);
        if(heroes.Count == 0) CombatLoss();
        else if(enemies.Count == 0) CombatWin();
    }

    public void HeroPanic(Hero hero)
    {
        heroes.Remove(hero);
        battleManager.HeroPanic(hero);
        if(heroes.Count == 0) CombatLoss();
    }

    //temp wincon
    private void CombatWin(){ battleManager.isRunning = false; GD.Print("win");}
    //temp losecon
    private void CombatLoss(){ battleManager.isRunning = false; GD.Print("loss");}
}
