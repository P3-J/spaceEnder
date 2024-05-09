using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class battle : Node3D
{
	public List<ItemData> playerCards;
	public List<ItemData> playerHand = new List<ItemData>();
	public int playerDefence = 0;
	public int round = 0;

	public int playerCurrentMana;
	public bool playerTurn = false;

	List<ColorRect> cSlots = new List<ColorRect>();
	List<Control> eSlots = new List<Control>();
	Label3D playerHead; // player hud
	Control CardArea;
	AnimationPlayer CardAnim;
	Control hudParent;
	Node3D cameraBase;
	Control opener;
	Control SceneSwitcher;
	public override void _Ready()
	{	
		// on load connect combatSignal
		
		
		GetNodesConnects();
		InitInvisAndVis(false);
		//SwitchCam("player");
		StartBattle();
		//initBattle();

	}

	public void GetNodesConnects()
	{	
		hudParent = GetNode<Control>("hudParent");
		cSlots.Add(GetNode<ColorRect>("hudParent/cards/cardslot1"));
		cSlots.Add(GetNode<ColorRect>("hudParent/cards/cardslot2"));
		cSlots.Add(GetNode<ColorRect>("hudParent/cards/cardslot3"));
		cSlots.Add(GetNode<ColorRect>("hudParent/cards/cardslot4"));
		cSlots.Add(GetNode<ColorRect>("hudParent/cards/cardslot5"));

		Callable unHighlight = new(this, nameof(UnHighLightOtherCards));
		foreach (ColorRect cardSlot in cSlots)
		{
			cardSlot.Connect("UnSelectOtherCards", unHighlight);
		}

		playerHead = GetNode<Label3D>("areanstuff/skellyModel/playerHead");

		battleCam = GetNode<Camera3D>("cameraBase/worldCam");
		cameraBase = GetNode<Node3D>("cameraBase");

		enemyMarkerList.Add(GetNode<Marker3D>("battleground/position1"));
		enemyMarkerList.Add(GetNode<Marker3D>("battleground/position2"));
		enemyMarkerList.Add(GetNode<Marker3D>("battleground/position3"));

		CardAnim = GetNode<AnimationPlayer>("hudParent/cardanim");

		Callable closer = new(this, nameof(SwitchSceneBackToWorld));
        SceneSwitcher = GetNode<Control>("opener");
        SceneSwitcher.Connect("close", closer);
		SceneSwitcher.Call("Open");
	}

	
	public override void _Process(double delta)
	{
		if (Input.IsActionPressed("ui_right"))
		{
			cameraBase.RotateY((float)0.01);
		}
		if (Input.IsActionPressed("ui_left"))
		{
			cameraBase.RotateY((float)-0.01);
		}

		if (Input.IsActionJustPressed("l_click"))
		{
			ClickOnEnemy(GameManager.SelectedEnemyId);
		}
	}



	public void initBattle(int Difficulty)
	{
		//spawn player / enemy
		initPlayerValues();
		playerCards = GameManager.GetItems().ToList();
		generateEnemies(Difficulty);
		setGeneratedEnemyVisibility();

		playerTurn = true;
		InstanceEnemies3D();
		playerTurnProcess();
		//generate cards of player
		//start turn 
	}

	public void initPlayerValues()
	{
		playerCurrentMana = GameManager.playerMana;
	}

	public void playerTurnProcess() 
	{	

		playerCurrentMana = GameManager.playerMana; // up here cause turnbonus applys later

		DisableAllThisTurnBuffs();
		discardHand();
		CheckTurnBonuses();

		if (round == 0) 
		{
			draw(5);
		} else {
			draw(3);
		}

		playerDefence = 0 + DefenceBuffEachTurnPlayer; // same as current defence

		displayCurrentHand();
		RefreshHud();		
	}


	public void displayCurrentHand()
	{	
		int currentSlot = 0;
		foreach (ItemData item in playerHand)
		{
			ColorRect cSlot = cSlots[currentSlot];
			Label name = cSlot.GetNode<Label>("cardName");
			Label cost = cSlot.GetNode<Label>("cost");
			Label action = cSlot.GetNode<Label>("action");

			cSlot.Call("SetSprite", GameManager.GetItemSpriteLocation(item.Id));
			
			name.Text = item.Name;
			cost.Text = item.Cost.ToString();
			action.Text = GameManager.getItemDesc(item);

			cSlot.Visible = true;
			currentSlot += 1;
		}

		for (int i = 0; i < 5 - currentSlot; i++)
		{
			cSlots[currentSlot + i].Visible = false;
		}

		RefreshHud();
	}

	public void RefreshHud()
	{
		playerHead.Text = 
			playerDefence.ToString() + " 🛡️\n" + 
			GameManager.playerCHp.ToString() + "/" + GameManager.playerHp.ToString() + " ❤️\n" +
			playerCurrentMana + "/" + GameManager.playerMana.ToString() + " ⚡";
	}


	public void CheckTurnBonuses()
	{	
		// set the turn bonuses to zero and calculate them each turn.
		DefenceBuffEachTurnPlayer = 0; 
		AtkBuffPlayerPerma = 0;

		// item cards.
		foreach (ItemData item in playerCards)
		{
			if (GameManager.ItemIds.Contains(item.Id) && item.Type == "item")
			{
				// switch based on effect
				switch (item.SecondaryType)
				{
					case ("defence each turn"):
						DefenceBuffEachTurnPlayer += item.SecondaryValue;
						break;
					case ("permanent atk"):
						AtkBuffPlayerPerma += item.SecondaryValue;
						break;
				}
			}
		}

		// other
		playerCurrentMana += ManaBuffPlayer;
		ManaBuffPlayer = 0;
	}

	public void PlayerReceiveDamage(int amount)
	{
		// shield check
		int shieldLeft = playerDefence - amount;
		if (shieldLeft < 0)
		{
			GameManager.playerCHp -= Math.Abs(shieldLeft);
		} else {
			playerDefence = shieldLeft;
		}
	}

	public void printHand()
	{
		Console.WriteLine("////////hand////////");
		foreach (ItemData item in playerHand)
		{
			Console.WriteLine(item.Name);
		}
		Console.WriteLine("////////deck////////");
		foreach (ItemData item in playerCards)
		{
			Console.WriteLine(item.Name);
		}
	}

	public void draw(int amount, string buff = "", int buffAmm = 0) 
	{	
		int drawCount   = amount;
		int freeSlots = 5 - playerHand.Count;

		if (drawCount >= freeSlots)
		{
			drawCount = freeSlots;
		}

		var random = new RandomNumberGenerator();
		random.Randomize();
		for (int i = 0; i < drawCount; i++)
		{	
			if (playerCards.Count == 0){
				break;
			}
			int nr = random.RandiRange(0,playerCards.Count - 1);
			ItemData drawnCard = playerCards[nr];
			playerCards.RemoveAt(nr);

			if (buff != ""){
				drawnCard = buffCard(drawnCard, buff, buffAmm);
			}
			// BAAAD
			if (AtkBuffPlayerPerma > 0)
			{	
				drawnCard = buffCard(drawnCard, "permanent atk", AtkBuffPlayerPerma);
			}

			playerHand.Add(drawnCard);	
		}
	}

	public void InitInvisAndVis(bool state)
	{	
		hudParent.Visible = state;
	}

	private void StartBattle()
	{	
		initBattle(GameManager.Diff);
		InitInvisAndVis(true);
	}

	public void TryToPlay(int enemySlot, int cardSlot)
	{
		ItemData usedCard = playerHand[cardSlot];

		// check if card should be self used.
		bool IsCardUsedOnSelf = GameManager.SelfUseCards.Contains(usedCard.Id);
		if (enemySlot == -2) // -2 is player slot
		{
			if (!IsCardUsedOnSelf){return;}
		} else 
		{
			if (IsCardUsedOnSelf){return;}
		}
		// play it on the enemy
		int manaLeft = playerCurrentMana - playerHand[cardSlot].Cost;
		if (manaLeft < 0 || playerHand[cardSlot].Type == "item")
		{
			return;
		}
		playerCurrentMana = manaLeft;
		// if succeed remove card
		playerHand.RemoveAt(cardSlot);
	
		PlayCard(usedCard, enemySlot); // doing the actual action the card requires
		cSlots[cardSlot].Visible = false;
		AddRemovedCardToDeck(usedCard);
		displayCurrentHand();
		// deselect card
		GameManager.SelectedCardId = -1;
		UnHighLightOtherCards(-1);

		// check for card buffs
		CheckForAfterCardPlayBuffs();
		
	}

	public void AddRemovedCardToDeck(ItemData card)
	{
		if (card.hasBeenBuffedThisTurn == true)
		{
			ItemData freshItem = GameManager.GenerateCard(card.Id);
			playerCards.Add(freshItem);
			return;
		}
		playerCards.Add(card);
	}

	public void turnSequence()
	{	
		UnHighLightOtherCards(-1);
		GameManager.SelectedCardId = -1;

		round += 1;
		EnemyTurn();
		playerTurnProcess();
		
	}

	
	private void discardHand()
	{
		foreach (var card in playerHand)
		{
			AddRemovedCardToDeck(card);
		}

		playerHand = new List<ItemData>();

	}

	private void _on_end_turn_pressed()
	{
		turnSequence();
		
	}

	// hide show card logic
	bool CardsUp = false;

	public void PlayCardHide()
	{
		if (CardsUp == false)
		{
			CardAnim.Play("hidecard");
			return;
		}
		CardAnim.Play("showcard");
	}
	private void _on_cards_mouse_entered()
	{
		if (CardsUp != true)
		{
			CardsUp = true;
			CallDeferred("PlayCardHide");
		}
	}
	private void _on_cards_mouse_exited()
	{
		CardsUp = false;
		CallDeferred("PlayCardHide");
	}


	private void ClickOnEnemy(int enemySlot)
	{	
		if (playerTurn == true && GameManager.SelectedCardId != -1 && GameManager.SelectedEnemyId != -1)
		{
			TryToPlay(enemySlot, GameManager.SelectedCardId);
		}
	}
}
