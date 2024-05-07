using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Numerics;
using Godot;

public static class GameManager
{
    private static List<ItemData> items;
    public static int playerMana { get; set;} = 3;
    public static int playerHp { get; set;} = 30;
    public static int playerCHp { get; set;} = 0;
    public static int playerGold { get; set;} = 1000;

    public static bool firstBoot { get; set;} = true;
    public static List<ItemData> deepCopy {get; set;} /// for deck pre combat afte combat clone
    public static Godot.Vector3 playerLastPosition {get; set;} = new();

    //public static int CurrentEnemyId { get; set;} = 0;
    public static int Diff { get; set;} = 0;
    public static int SelectedCardId { get; set;} = -1;
    public static int SelectedEnemyId { get; set;} = -1;
    public static List<int> InstantFightsToRemove {get; set;} = new();

    public static bool CanMove {get; set;} = true;

    public static List<int> AllItems = new() {1,2,3,4,5,7,8,9,10,11,12,13,14,15,16,17}; // these are the ones available to the shop atleast.
    public static List<int> ItemIds = new() {6, 12};
    public static List<int> SelfUseCards = new() {2,3,5,7,10,11,14,16}; // defence, draws so on.

    // danger stuff //

    public static int DangerLevel {get; set;} = 0; 
    public static int UntilHigherDanger {get; set;} = 0;
    public static int BonusAtkMobDanger {get; set;} = 0;
    public static int BonusDefMobDanger {get; set;} = 0;
    public static int BonusHpMobDanger {get; set;} = 0;
    // 50 battle == ultimate endgame
    // 10 - 20 - 30 - 40 - 50
    
    public static List<ItemData> GetItems()
    {
        return items;
    }

    public static void SetItems(List<ItemData> newItems)
    {
        items = newItems;
    }

    public static string getItemDesc(ItemData item) 
    {
        switch (item.Id)
        {
            case 1:
                return "Attack for " + item.Value.ToString();
            case 2:
                return "Defend for " + item.Value.ToString();
            case 3:
                return "Draw " + item.Value.ToString() + " cards";
            case 4:
                return "Deal " + item.Value.ToString() + " to all enemies \n" + "draw " + item.SecondaryValue.ToString();
            case 5:
                return "Draw until hand full \n" + "-" + item.Value.ToString() + " to cost of drawn cards";
            case 7:
                return "+1 to dmg cards" + "\n" + "for each card played this turn";
            case 6:
                return "Card not usable \n adds " + item.SecondaryValue.ToString() + "def each turn";
            case 8:
                return "Attack for " + item.Value.ToString();
            case 9:
                return "Attack for " + item.Value.ToString() + "\n" + "+" + item.SecondaryValue.ToString() + " to mana next turn";
            case 10:
                return "Defend for " + item.Value.ToString();
            case 11:
                return "Defend for " + item.Value.ToString() + "\n+" + item.SecondaryValue.ToString() + " to mana next turn";
            case 12:
                return "Card not usable \n adds " + item.SecondaryValue.ToString() + " to atk";
            case 13:
                return "Remove enemy defence";
            case 14:
                return "Gain " + item.Value.ToString() + " mana";
            case 15:
                return "Attack for " + item.Value.ToString() + " damage"; 
            case 16:
                return "Create random card \n" + "reduce the cost by -" + item.Value;
            case 17:
                return "Attack for " + item.Value.ToString() + "\n+" + item.SecondaryValue.ToString() + " atk to this";
            case 18:
                return "Attack for " + item.Value.ToString() + "\n" + item.SecondaryValue.ToString() + "cost to cards in hand";
            default:
                return "Default";
        }
    }

    public static string GetItemSpriteLocation(int id)
    {
        switch (id)
        {
            case 1:
                return "res://assets/itmepics/Weapons/Weapon_02.png";
            case 2:
                return "res://assets/itmepics/Shields/Shield_16.png";
            case 3:
                return "res://assets/itmepics/Potions/Potion_31.png";
            case 4:
                return "res://assets/itmepics/Weapons/Weapon_57.png";
            case 5:
                return "res://assets/itmepics/Misc/Misc_49.png";
            case 7:
                return "res://assets/itmepics/Potions/Potion_03.png";
            case 6:
                return "res://assets/itmepics/Armor/Armor_134.png";
            case 8:
                return "res://assets/itmepics/Weapons/Weapon_03.png";
            case 9:
                return "res://assets/itmepics/Misc/Misc_50.png";
            case 10:
                return "res://assets/itmepics/Shields/Shield_18.png";
            case 11:
                return "res://assets/itmepics/Shields/Shield_03.png";
            case 12:
                return "res://assets/itmepics/Misc/Misc_08.png";
            case 13:
                return "res://assets/itmepics/Misc/Misc_11.png";
            case 14:
                return "res://assets/itmepics/Potions/Potion_11.png";
            case 15:
                return "res://assets/itmepics/Weapons/Weapon_71.png";
            case 16: 
                return "res://assets/itmepics/Misc/Misc_51.png";
            case 17:
                return "res://assets/itmepics/Weapons/Weapon_52.png";
            case 18:
                return "res://assets/itmepics/Weapons/Weapon_60.png";
            default:
                return "res://assets/itmepics/Misc/Misc_Preview.png";
        }
    }

    public static void PrintAllCards()
    {
        List<ItemData> items = GetItems();
        foreach (ItemData item in items)
        {
            GD.Print(item.Name);
        }
        GD.Print("there are cards; " + items.Count);
    }

    public static void MakeADeepCopy(List<ItemData> items)
    {
        deepCopy = new();
        foreach (ItemData item in items)
        {
            deepCopy.Add(new ItemData(item));
        }
    }

    public static ItemData GenerateCard(int id)
    {
        return id switch // only boost primary value, if boosting ofc ATK FIRST ATK FIRST, cause of bad coding!
        {
            1 => new ItemData(
                                "Slash",
                                "atk",
                                5,
                                1,
                                id
                            ),
            2 => new ItemData(
                                "Block",
                                "def",
                                5,
                                1,
                                id
                            ),
            3 => new ItemData(
                                "Greedy Pot",
                                "draw",
                                2,
                                1,
                                id
                            ),
            4 => new ItemData(
                                "Swipe",
                                "atk all",
                                7,
                                1,
                                id,
                                "draw", //secondary effect
                                1
            ),
            5 => new ItemData(
                                "Pit of \n knowledge",
                                "draw until",
                                1,
                                2,
                                id
            ),
            6 => new ItemData(
                                "Mantle of Security",
                                "item",
                                0,
                                0,
                                id,
                                "defence each turn", // add each turn a bit of defence
                                8
            ),
            7 => new ItemData(
                                "Time Attack",
                                "raiseAtk",
                                1,
                                0,
                                id
            ),
            8 => new ItemData(
                                "Blast",
                                "atk",
                                10,
                                2,
                                id
            ),
            9 => new ItemData(
                                "Book of Knowledge",
                                "atk",
                                5,
                                2,
                                id,
                                "mana next turn",
                                1
            ),
            10 => new ItemData (
                                "Raise Defences",
                                "def",
                                9,
                                1,
                                id
            ),
            11 => new ItemData (
                                "Shield Wall",
                                "def",
                                12,
                                3,
                                id,
                                "mana next turn",
                                1
            ),
            12 => new ItemData (
                                "Bloody tear",
                                "item",
                                0,
                                0,
                                id,
                                "permanent atk",
                                1
            ),
            13 => new ItemData (
                                "Shield Slice",
                                "break shield",
                                0,
                                2,
                                id
            ),
            14 => new ItemData (
                                "Blue Monster",
                                "mana",
                                2,
                                1,
                                id
            ),
            15 => new ItemData (
                                "Stab",
                                "atk",
                                4,
                                0,
                                id
            ),
            16 => new ItemData (
                                "Confusion",
                                "draw random",
                                1,
                                2,
                                id
            ),
            17 => new ItemData (
                                "Reap n Sow",
                                "atk",
                                5,
                                1,
                                id,
                                "raise self atk",
                                2          
            ),
            18 => new ItemData (
                                "Deep Scar",
                                "atk",
                                15,
                                1,
                                id,
                                "debuff hand mana",
                                -1
            ),
            _ => new ItemData("if see this bug", "atk", 1, 1, 0,"",0),
        };
    }

}

public class ItemData
{
    public string Name { get; }
    public string Type { get; }
    public int Value { get; set;}
    public int Cost { get; set;}
    public int Id {get;} // for recognizing shit
    public string SecondaryType;
    public bool hasBeenBuffedThisTurn = false;
    public int SecondaryValue; // dumb af, but wont go over 2 

    // add stuff to all items, item desc, 
    public ItemData(string name, string type, int value, int cost, int id, string secondarytype = "", int secondaryvalue = 0)
    {
        Name = name;
        Type = type;
        Value = value;
        Cost = cost;
        Id = id;
        SecondaryValue = secondaryvalue;
        SecondaryType = secondarytype;
    }

    public ItemData(ItemData original)
    {   // this lets you enter the og into a format like this.
        Name = original.Name;
        Type = original.Type;
        Value = original.Value;
        Cost = original.Cost;
        Id = original.Id;
        SecondaryValue = original.SecondaryValue;
        SecondaryType = original.SecondaryType;
        hasBeenBuffedThisTurn = original.hasBeenBuffedThisTurn;
    }
}
