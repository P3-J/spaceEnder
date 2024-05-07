using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

public partial class shop : Node3D
{
	ColorRect card1;
	ColorRect card2;
	ColorRect card3;
	Button BuyerButton;

	public List<ColorRect> rectTable = new();
	public List<ItemData> GeneratedCards = new();
	public List<ItemData> PlayerCurrentDeck;
	List<int> RemovalSelections;
	public int SelectedCardHere;
	Label playerhud;
	public override void _Ready()
	{	
		ConnectCards();
		CardVisibility(false);
		RefreshHud();
	}

	public void ConnectCards()
	{
		card1 = GetNode<ColorRect>("ui/actions/cardslot1");
		card2 = GetNode<ColorRect>("ui/actions/cardslot2");
		card3 = GetNode<ColorRect>("ui/actions/cardslot3");
		rectTable.Add(card1);
		rectTable.Add(card2);
		rectTable.Add(card3);
		Callable UnSelecting = new Callable(this, nameof(UnSelect));
		card1.Connect("UnSelectOtherCards", UnSelecting);
		card2.Connect("UnSelectOtherCards", UnSelecting);
		card3.Connect("UnSelectOtherCards", UnSelecting);

		BuyerButton = GetNode<Button>("ui/buy");
		PlayerCurrentDeck = GameManager.GetItems();
		playerhud = GetNode<Label>("ui/playerhud");
	}

	public void GenerateCardsForSelection()
	{
		GeneratedCards = new();
		for (int i = 0; i < 3; i++)
		{
			ItemData item = SelectRandomCard();
			PopulateCards(rectTable[i], item);
		}
	}

	public void PopulateCards(ColorRect rect, ItemData item)
	{
		rect.GetNode<Label>("cardName").Text = item.Name;
		rect.GetNode<Label>("cost").Text = item.Cost.ToString();
		rect.GetNode<Label>("action").Text = GameManager.getItemDesc(item);
		rect.Call("SetSprite", GameManager.GetItemSpriteLocation(item.Id));
		GeneratedCards.Add(item);
	}

	private void GenerateCardsForRemoval()
	{
		RemovalSelections = new();
		GeneratedCards = new();
		var random = new RandomNumberGenerator();
		random.Randomize();
		

		while (RemovalSelections.Count < 3)
		{
			int randomSelection = random.RandiRange(1, GameManager.AllItems.Count);
			if (!RemovalSelections.Contains(randomSelection))
			{
				RemovalSelections.Add(randomSelection);
			}
		}

		for (int i = 0; i < RemovalSelections.Count; i++)
		{
			ItemData item = PlayerCurrentDeck[RemovalSelections[i]];
			PopulateCards(rectTable[i], item);
		}

	}

	private void _on_buy_pressed()
	{
		BuyCard();
		CardVisibility(false);
		BuyerButton.Visible = false;
	}

	private void _on_remove_pressed()
	{
		if (!CanDoTransaction("remove"))
		{
			return;
		}
		GenerateCardsForRemoval();
		BuyerButton.Text = "Remove";
		BuyerButton.Visible = true;
		CardVisibility(true);
	}

	private void RemoveCard()
	{
		int cardId = RemovalSelections[SelectedCardHere];
		PlayerCurrentDeck.RemoveAt(cardId);
		GameManager.SetItems(PlayerCurrentDeck);
		GameManager.PrintAllCards();
	}
	public void BuyCard()
	{	
		if (BuyerButton.Text == "Remove")
		{
			RemoveCard();
		} else {
			PlayerCurrentDeck.Add(GeneratedCards[SelectedCardHere]);
			GameManager.SetItems(PlayerCurrentDeck);
			GameManager.MakeADeepCopy(PlayerCurrentDeck);
		}
	}

	private void _on_exit_pressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/hometest.tscn");
	}
	public void UnSelect(int selected)
	{	
		SelectedCardHere = selected;
		GD.Print(selected);
		foreach (ColorRect item in rectTable)
		{	
			if ((int)item.Get("slotID") != selected)
			{
				item.Call("UnHighlight");
			}
		}
	}

	private void RefreshHud()
	{
		playerhud.Text = GameManager.playerGold.ToString() + "🪙";
	}
	private bool CanDoTransaction(string action)
	{
        // if can take money
        var cost = action switch
        {
            "gen" => 20,
            "remove" => 20,
            _ => 0,
        };
        if (GameManager.playerGold >= cost)
		{
			GameManager.playerGold -= cost;
			RefreshHud();
			return true;
		} else {
			RefreshHud();
			return false;
		}
	}

	private void _on_gen_pressed()
	{	
		if (!CanDoTransaction("gen"))
		{
			return;
		}
		GenerateCardsForSelection();
		BuyerButton.Text = "Buy";
		BuyerButton.Visible = true;
		CardVisibility(true);
	}

	private void CardVisibility(bool state)
	{
		foreach (ColorRect card in rectTable)
		{
			card.Visible = state;
		}
	}

	public ItemData SelectRandomCard()
	{
		var random = new RandomNumberGenerator();
		random.Randomize();
		int randomSelection = random.RandiRange(1, GameManager.AllItems.Count);
		randomSelection = GameManager.AllItems[randomSelection - 1];
		ItemData item = GameManager.GenerateCard(randomSelection);
		return item;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
