using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;

public partial class battle : Node3D
{
    public List<EnemyBase> enemyBoard = new List<EnemyBase>();
    private float LowerRangeDamageRatio = 0.5F; // see where this goes, (0.5F to 1F) * def/atk of enemy
    public string IntendedNextTurnAbility = "";
    readonly List<int> SpawnableEnemies = new() {1, 2, 4, 5};
    
    public void createEnemy(int amount, int diff)
    {   
        // earlier escape, check amount before please.
        // only one kind can spawn
        var random = new RandomNumberGenerator();
		random.Randomize();
        // give it a starting action
        int prefSpawnPoint = -1;
        if (amount == 1)
        {
            prefSpawnPoint = 1;
        }
        for (int i = 0; i < amount; i++)
        {
            int enemyToSpawn = SpawnableEnemies.ElementAt(random.RandiRange(0,SpawnableEnemies.Count - 1));
            EnemyBases(enemyToSpawn, diff, prefSpawnPoint);
        }
    }

    public void EnemyBases(int mobID, int diff, int prefSpawnPoint = -1)
    {   

        string Name = "";
        int Hp = 0;
        int Def = 0;
        int Atk = 0;
        int AtkRatio = 0;
        int DefRatio = 0;
        int AbilityRatio = 0; 
        // if not -1 then 0-3

        Godot.Collections.Dictionary<string, int> Abilities = new();
        var random = new RandomNumberGenerator();
		random.Randomize();
        var HpRandomizeRatio = random.RandfRange(0.6F, 1F);

        switch (mobID)
        {
            case 1:
                Name = "Skelly";
                Hp = (int)Math.Round(10 * diff * HpRandomizeRatio);
                Def = 5 * diff;
                Atk = 5 * diff;
                AtkRatio = 2;
                DefRatio = 1;
                break;
            case 2:
                Name = "Spiderslime";
                Hp = (int)Math.Round(20 * diff * HpRandomizeRatio);
                Def = 5 * diff;
                Atk = 5 * diff;
                AtkRatio = 1;
                DefRatio = 2;
                break;
            case 3:
                Name = "GOLIATH";
                Abilities.Add("spawn", 1);
                Abilities.Add("heal all", 5);
                Abilities.Add("damage buff all", 1);
                Hp  = 130 * diff;
                Def = 10  * diff;
                Atk = 15  * diff;
                AtkRatio = 4;
                DefRatio = 4;
                AbilityRatio = 4;
                prefSpawnPoint = 1;
                break;
            case 4:
                Name = "Skruff";
                Hp = 22 * diff;
                Def = 5 * diff;
                Atk = 11 * diff;
                Abilities.Add("damage buff all", 2);
                AtkRatio = 3;
                DefRatio = 2;
                AbilityRatio = 2;
                break;
            case 5: 
                Name = "Vending";
                Hp = 17 * diff;
                Def = 6 * diff;
                Atk =  7 * diff;
                Abilities.Add("heal all", 7);
                AtkRatio = 2;
                DefRatio = 2;
                AbilityRatio = 2;
                break;
        }
        // danger level buffs
        Hp  += GameManager.BonusHpMobDanger;
        Def += GameManager.BonusDefMobDanger;
        Atk += GameManager.BonusAtkMobDanger;

        EnemyBase mob = new EnemyBase(Name, Hp, Def, Atk, AtkRatio, DefRatio, Abilities, AbilityRatio)
        {
            CDef = 0,
            CHp = Hp,
        };
        mob.Action = generateNextAction(mob);
        TryToAddMob(mob, prefSpawnPoint);
    }

    public void TryToAddMob(EnemyBase mob, int spawnPoint)
    {

        // pref spawns should not be twice
        List<int> freeSpots = new() {0,1,2};
        foreach (EnemyBase enemy in enemyBoard)
        {
            freeSpots.Remove(enemy.CurrentSlot);
        }
        if (freeSpots.Count > 0)
        {
            if (spawnPoint != -1 && freeSpots.Contains(spawnPoint)){mob.CurrentSlot = freeSpots[spawnPoint];} 
            else 
            {
                mob.CurrentSlot = freeSpots[0];
            }
            enemyBoard.Add(mob);
        }
        InstanceEnemies3D(); // render enemy
    }

    public void generateEnemies(int Difficulty)
    {   

        // three diff levels 1-3 
        // 1 simple, one to two | low 
        // 2 harder min 2 max 3 | mid
        // 3 three enemies, or one strong af | high
        // -1 boss | extreme

        var random = new RandomNumberGenerator();
		random.Randomize();
        
        switch (Difficulty)
        {
            case 1:
                createEnemy(random.RandiRange(1,2), 1);
                break;
            case 2:
                createEnemy(random.RandiRange(2,3), 1);
                break;
            case 3:
                break;
            case 71:
                EnemyBases(3,1);
                break;
            case -1:
                break;
        }
    }

    public void setGeneratedEnemyVisibility()
    {   

        foreach (EnemyBase enemy in enemyBoard)
        {   
            Label3D head = enemyMarkerList[enemy.CurrentSlot].GetNode<Label3D>("headLabel");
            string enemyHudString = "";
            enemyHudString += enemy.Name + "\n";
            enemyHudString += enemy.CHp.ToString() + "/" + enemy.Hp.ToString() + "❤️ ";
            enemyHudString += enemy.CDef.ToString() + "🛡️" + "\n";
            enemyHudString += EnemyActionValue(enemy);
            head.Text = enemyHudString;
        }

    }

    public void CheckWinState()
    {   
        foreach (EnemyBase enemy in enemyBoard)
        {
            return;
        }
        CombatOverSequence();
    }

    public string EnemyActionValue(EnemyBase enemy){
        // stupid , just for text i guess, not so stupid now maybe
        GD.Print(enemy.Action + " ACTION");
        return enemy.Action switch
        {
            ("attack") => enemy.Action + " for " + Math.Round(enemy.Atk * enemy.ActionMultiplier).ToString() + "🗡️",
            ("defend") => enemy.Action + " for " + Math.Round(enemy.Def * enemy.ActionMultiplier).ToString() +  "🛡️",
            ("heal all") => "Heal all for " + enemy.Abilities["heal all"].ToString(),
            ("spawn") => "Spawn " + enemy.Abilities["spawn"].ToString() + " enemy",
            ("damage buff all") => "+" + enemy.Abilities["damage buff all"].ToString() + " atk to all enemies",
            _ => "-1",
        };
        
    }
    public void DealDamageToEnemy(EnemyBase enemy, int value)
    {   
        // shield check
        int shieldLeft = enemy.CDef - value;
        if (shieldLeft < 0)
        {
            enemy.CHp -= Math.Abs(shieldLeft);
            enemy.CDef = 0;
        } else {
            enemy.CDef = shieldLeft;
        }
    }

    public void EnemyTurn()
    {   
        playerTurn = false;
        List<EnemyBase> enemyBoardCopy = new(enemyBoard); // copy as one can add during process
        foreach (EnemyBase target in enemyBoardCopy)
        {
            ProcessEnemyTurn(target);
        }
        playerTurn = true;
    }

    public void EnemyLifeCheck()
    {   
        List<EnemyBase> arrcopy = new List<EnemyBase>(enemyBoard);


        foreach (EnemyBase enemy in arrcopy)
        {
            if (enemy.CHp <= 0)
            {
                KillRemoveEnemy(enemy.CurrentSlot);
            }
        }
    }

    public void ProcessEnemyTurn(EnemyBase enemy)
    {   
        //enemy.CDef = 0;
        var random = new RandomNumberGenerator();
		random.Randomize();
        // monster lookup table for damage, turn chance
        switch (enemy.Action)
        {
            case "attack":
                PlayerReceiveDamage((int)Math.Round(enemy.Atk * enemy.ActionMultiplier));
                break;
            case "defend":
                enemy.CDef += (int)Math.Round(enemy.Def * enemy.ActionMultiplier);
                break;
            default:
                EnemyUseAbility(enemy);
                break;
        }
        enemy.Action = generateNextAction(enemy);
        setGeneratedEnemyVisibility();
    }

    public void HealAllEnemies(int value)
    {
        foreach (EnemyBase enemy in enemyBoard)
        {
            enemy.CHp += value;
            if (enemy.CHp > enemy.Hp){enemy.CHp = enemy.Hp;}
        }
    }

    public void DamageBuffEnemies(int value)
    {
        foreach (EnemyBase enemy in enemyBoard)
        {
            enemy.Atk += value;
        }
    }

    public string generateNextAction(EnemyBase enemy)
    {
        // here look up the rand

        var random = new RandomNumberGenerator();
		random.Randomize();
        enemy.ActionMultiplier = random.RandfRange(LowerRangeDamageRatio,1F);

        int randSelection = random.RandiRange(1,enemy.AtkRatio + enemy.DefRatio + enemy.AbilityRatio);

        if (randSelection <= enemy.AtkRatio){
            return "attack";
        }
        if (randSelection <= enemy.AtkRatio + enemy.DefRatio){
            return "defend";
        }
        if (randSelection <= enemy.AtkRatio + enemy.DefRatio + enemy.AbilityRatio){
            int randomSelection = random.RandiRange(0,enemy.Abilities.Count - 1);
            return enemy.Abilities.Keys.ElementAt(randomSelection);
        }
        return "";
    }

    public EnemyBase GetEnemyByID(int id)
    {
        foreach (EnemyBase enemy in enemyBoard)
        {
            if (enemy.CurrentSlot == id)
            {
                return enemy;
            }
        }
        return null;
    }
    public void KillRemoveEnemy(int enemyslot)
    {
        // can do rest of the death stuff here
        enemyBoard.Remove(GetEnemyByID(enemyslot));
        RemoveInstanceAtId(enemyslot);
        GameManager.SelectedEnemyId = -1;
        // clear text
        Label3D head = enemyMarkerList[enemyslot].GetNode<Label3D>("headLabel");
        head.Text = "";
        
        Console.WriteLine(enemyBoard);
    }

    public void EnemyUseAbility(EnemyBase enemy)
    { 
        int AbilityValue = enemy.Abilities[enemy.Action];
        switch (enemy.Action)
        {
            case "spawn":
                createEnemy(1,2); // ofc do if can
                break;
            case "heal all":
                HealAllEnemies(AbilityValue);
                break;
            case "damage buff all":
                DamageBuffEnemies(AbilityValue);
                break;
        }
    }

}



public class EnemyBase
{
    public string Name { get; }
    public int CDef {get; set;}
    public int CHp {get; set;}
    public int Hp { get; }
    public int Def { get; set;}
    public int Atk { get; set;}
    public Godot.Collections.Dictionary<string, int> Abilities;
    public string Action {get; set;}

    public bool HasBeenInstanced {get; set;} = false; // so it doesnt get rerendered, atleast the stuff tied to it.
    public int AtkRatio {get; set;}
    public int DefRatio {get; set;}
    public int AbilityRatio {get; set;}
    public int CurrentSlot {get; set;}
    public float ActionMultiplier {get; set;} = 1F;

    public EnemyBase(string name, int hp, int def, int atk, int atkRatio, int defRatio, Godot.Collections.Dictionary<string, int> abilities, int abilityRatio)
    {
        Name = name;
        Hp = hp;
        Def = def;
        Atk = atk;
        Abilities = abilities;
        AbilityRatio = abilityRatio;
        AtkRatio = atkRatio;
        DefRatio = defRatio;


    }
}