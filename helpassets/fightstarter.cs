using Godot;
using System;

public partial class fightstarter : Area3D
{
	[Signal]
	public delegate void StartCombatEventHandler();
	[Export]
	public int Difficulty;
	[Export]
	public bool InstantFight = false;	
	[Export]
	public int ID;
	// so how do we get rid of this this 
	// id -> list at start  can send instance of self?
	// rework enemy spawn stufff
	// fck that
	
	private void _on_body_entered(Node3D body)
	{
		if (body.Name == "player") 
		{	
			// change
			if (InstantFight)
			{
				EmitSignal("StartCombat", Difficulty, ID);
			}
		}
		
	}
}
