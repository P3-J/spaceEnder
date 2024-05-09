using Godot;
using System;
using System.Collections.Generic;
public partial class worldstuff : Node3D
{

	Node3D enemyContainer;
    Node3D instaFightContainer;
    Control SceneSwitcher;
    CharacterBody3D player;
    Label playerHudLabel;
    Label dangerLabel;
    ProgressBar dangerprogress;
    List<Area3D> Fightstarters = new();
    bool immunity = true;
	public override void _Ready()
	{

        ConnectStuff();
        GameManager.CanMove = true;

        if (GameManager.firstBoot)
        {
            GameManager.playerCHp = GameManager.playerHp;
            SetPlayerDeck();
            GameManager.firstBoot = false;
            GameManager.playerLastPosition = player.Position;
        }

        RemoveInstantFights();
        player.Position = GameManager.playerLastPosition;
        DangerLevelBuffCalculator();
        SetDeckCopy();
        RefreshHud();
        SceneSwitcher.Call("Open"); // SceneSwitcher as in the anims, logic in opener.cs
	}

    public void ConnectStuff()
    {   
        player = GetNode<CharacterBody3D>("playerstuff/player");
        playerHudLabel = GetNode<Label>("ui/hpandgold");
        dangerLabel = GetNode<Label>("ui/dangerlabel");
        dangerprogress = GetNode<ProgressBar>("ui/dangerprogress");

        Callable battleStart = new Callable(this, nameof(StartBattle));
		enemyContainer = GetNode<Node3D>("enemies");
		foreach (Node enemy in enemyContainer.GetChildren())
		{
			enemy.Connect("StartCombat", battleStart);
		}

        instaFightContainer = GetNode<Node3D>("instafights");
        foreach (Node enemy in instaFightContainer.GetChildren())
		{    
            if (enemy is Area3D fightstarter)
            {
                enemy.Connect("StartCombat", battleStart);
                Fightstarters.Add((Area3D)enemy);     
            }
		}

        Callable closer = new Callable(this, nameof(GoToBattle));
        SceneSwitcher = GetNode<Control>("opener");
        SceneSwitcher.Connect("close", closer);
    }

	public List<ItemData> GenerateDeck(List<int> cardIDs)
    {
    	List<ItemData> Deck = new();
    	foreach (var id in cardIDs)
    	{
        	Deck.Add(GameManager.GenerateCard(id));
    	}
     	return Deck;
    }

    public void RefreshHud()
    {
        playerHudLabel.Text = GameManager.playerGold.ToString() + "🪙 \n" + GameManager.playerCHp + "/" + GameManager.playerHp + "❤️";
        dangerLabel.Text = "DANGER LV" + GameManager.DangerLevel;
        dangerprogress.MaxValue = GameManager.DangerLevel * 3;
        dangerprogress.Value = GameManager.UntilHigherDanger;
    }
    public void RemoveInstantFights()
    {
        List<Area3D> FightStarterCopy = new(Fightstarters);
        foreach (Area3D enemy in FightStarterCopy)
        {
            if (GameManager.InstantFightsToRemove.Contains((int)enemy.Get("ID")))
            {
                Fightstarters.Remove(enemy);
                enemy.QueueFree();
            }
        }
    }
    // make some kind of copy of the cards before combat, after combat feed it back here in hometest, as no perma changes should be done in combat
    public void PreCombatDeckCopy()
    {      
        List<ItemData> CurrentItems = new List<ItemData>(GameManager.GetItems());
        GameManager.MakeADeepCopy(CurrentItems);
    }

    private void _on_timer_timeout()
    {
        immunity = false;
    }
    public void SetDeckCopy()
    {
        // set to the copy if there is one
        if (GameManager.deepCopy != null)
        {
            GameManager.SetItems(GameManager.deepCopy);
        }
    }
	
	private void StartBattle(int Diff, int ID = -1)
	{	
        if (immunity){return;}
		GameManager.Diff = Diff;
        GameManager.CanMove = false;
        GameManager.playerLastPosition = player.Position;
        if (ID != -1)
        {
            GameManager.InstantFightsToRemove.Add(ID);
        }
        PreCombatDeckCopy();
		CloseCurtains();
	}

    private void CloseCurtains()
    {   
        playerHudLabel.Visible = false;
        SceneSwitcher.Call("Close");
    }

	private void GoToBattle()
	{   
		GetTree().ChangeSceneToFile("res://scenes/arena.tscn");
	}

	public void SetPlayerDeck()
    {   
        List<int> deckList = new() {1,2,3,4,17,17,17,2,2,18,18,18};
        GameManager.SetItems(GenerateDeck(deckList));
    }

    public void GoToShop()
    {
        GetTree().ChangeSceneToFile("res://scenes/shop.tscn");
    }
	
    public void _on_shop_enter_body_exited(Node3D body)
    {
        if (body.Name == "player")
        {
            GameManager.playerLastPosition = player.Position;
            CallDeferred("GoToShop");
        }
    }

    public void DangerLevelBuffCalculator()
    {
        // uses current danger level to give boosts to mobs 
        if (GameManager.UntilHigherDanger <= 0)
        {
            GameManager.DangerLevel += 1;
            GameManager.UntilHigherDanger = GameManager.DangerLevel * 3;
            GameManager.BonusAtkMobDanger += GameManager.DangerLevel; 
            GameManager.BonusDefMobDanger += GameManager.DangerLevel;
            GameManager.BonusHpMobDanger  += GameManager.DangerLevel;
        }
    }

}
