using Godot;
using System;

public partial class skelly : StaticBody3D
{
	[Signal]
	public delegate void StartCombatEventHandler();
	[Export]
	public int Difficulty;
	[Export]
	public bool InstantFight = false;	
	
	private void _on_area_3d_body_entered(Node3D body)
	{
		if (body.Name == "player") 
		{	
			var random = new RandomNumberGenerator();
			random.Randomize();
			int encounterChance = random.RandiRange(0,3); /// ratio for encounter ; chance encounter	
			GD.Print(encounterChance + " encounter ch");
			// here we can pass like with what or whatever
			if (encounterChance == 1)
			{
				EmitSignal("StartCombat", Difficulty, -1);
			}
		}

	}

}
