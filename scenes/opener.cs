using Godot;
using System;

public partial class opener : Control
{
	AnimationPlayer openup;

	[Signal]
	public delegate void closeEventHandler();
	public override void _Ready()
	{
		openup = GetNode<AnimationPlayer>("AnimationPlayer");

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public void Open()
	{
		openup.Play("open");
	}

	public void Close()
	{
		Visible = true;
		openup.Play("close");
	}

	public void Closed()
	{
		EmitSignal("close");
	}
}
