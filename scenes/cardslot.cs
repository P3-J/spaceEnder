using Godot;
using System;

public partial class cardslot : ColorRect
{
	[Export]
	int slotID;

	[Signal]
	public delegate void UnSelectOtherCardsEventHandler();
	Sprite2D Highlight;
	Sprite2D Item;
	// Called when the node enters the scene tree for the first time.

	public override void _Ready()
	{
		Highlight = GetNode<Sprite2D>("highlighter");
		Item = GetNode<Sprite2D>("item");
	}
	private void _on_button_pressed()
	{
		GameManager.SelectedCardId = slotID;
		UnSelectOthers();
		Highlight.Visible = true;
	}

	private void UnSelectOthers()
	{
		EmitSignal("UnSelectOtherCards", slotID);
	}

	public void SetSprite(string SpriteLocation)
	{
		Item.Texture = GD.Load<Texture2D>(SpriteLocation);
	}

	public void UnHighlight()
	{
		Highlight.Visible = false;
	}
}
