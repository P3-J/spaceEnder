using Godot;
using System;

public partial class highlighter : Node3D
{
	[Export]
	int SpotID;
	public string Unit;
	MeshInstance3D spiderSlime;
	MeshInstance3D skelly;
	MeshInstance3D player;
	MeshInstance3D goliath;
	MeshInstance3D skruff;
	MeshInstance3D vending;

	MeshInstance3D SelectedMesh;

	Material MaterialOverride = new Material();
	Material BaseMaterial = new Material();
	public override void _Ready()
	{	
		// get meshes
		spiderSlime = GetNode<MeshInstance3D>("spiderslime");
		skelly = GetNode<MeshInstance3D>("skelly");
		player = GetNode<MeshInstance3D>("player");
		goliath = GetNode<MeshInstance3D>("goliath");
		vending = GetNode<MeshInstance3D>("makina");
		skruff = GetNode<MeshInstance3D>("skruff");

		if (SpotID == -2)
		{
			player.Visible = true;
			SelectedMesh = player;
		}
		// select correct mesh
		ChooseEnemy();

		// mesh visiblity
		MaterialOverride.Set("albedo_color", new Color(255, 255, 0)); // hover
	}

	public override void _Process(double delta)
	{
	}

	public void ChooseEnemy()
	{
		switch (Unit) {
			case "Skelly":
				skelly.Visible = true;
				SelectedMesh = skelly;
				break;
			case "Spiderslime":
				spiderSlime.Visible = true;
				SelectedMesh = spiderSlime;
				break;
			case "GOLIATH":
				goliath.Visible = true;
				SelectedMesh = goliath;
				break;
			case "Skruff":
				skruff.Visible = true;
				SelectedMesh = skruff;
				break;
			case "Vending":
				vending.Visible = true;
				SelectedMesh = vending;
				break;
		}
	}

	private void _on_area_3d_mouse_entered()
	{
		if (SelectedMesh != null)
		{
			SelectedMesh.MaterialOverride = MaterialOverride;
			GameManager.SelectedEnemyId = SpotID;
        }
	}

	private void _on_area_3d_mouse_exited()
	{
		if (SelectedMesh != null)
		{
			SelectedMesh.MaterialOverride = null;
		}
		GameManager.SelectedEnemyId = -1;
	}

}
