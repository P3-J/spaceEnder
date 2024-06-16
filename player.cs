using Godot;
using System;
using System.Collections.Generic;

public partial class player : CharacterBody3D
{
	public const float Speed = 3.0f;
	public const float JumpVelocity = 4.5f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	AnimationPlayer animplayer;
	Node3D rotator;
	AudioStreamPlayer WalkingSound;

	private System.Collections.Generic.Dictionary<Vector2, Vector3> directionRotations = new()
    {
        {Vector2.Up, new Vector3(0, 0, 0)},
        {Vector2.Down, new Vector3(0, 180, 0)},
        {Vector2.Left, new Vector3(0, 90, 0)},
        {Vector2.Right, new Vector3(0, -90, 0)},
		{new Vector2(-0.7F, 0.7F), new Vector3(0, 135, 0)},
		{new Vector2(-0.7F, -0.7F), new Vector3(0, 45, 0)},
		{new Vector2(0.7F, -0.7F), new Vector3(0, -45, 0)},
		{new Vector2(0.7F, 0.7F), new Vector3(0, -135, 0)}
    };
	public override void _Ready()
    {
        animplayer = GetNode<AnimationPlayer>("animplayer");
		rotator = GetNode<Node3D>("rotator");
		WalkingSound = GetNode<AudioStreamPlayer>("walk");
    }

	
	public override void _PhysicsProcess(double delta)
	{	


		if (!GameManager.CanMove)
		{
			animplayer.Stop();
			return;
		}
		
		Vector3 velocity = Velocity;
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = JumpVelocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		SetDire(inputDir);

		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
			if (animplayer.IsPlaying() == false)
			{
				WalkingSound.Play();
				animplayer.Play("moving");
			}
		}
		else
		{
			WalkingSound.Stop();
			animplayer.Stop();
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	public void SetDire(Vector2 dire)
	{
		Vector2 direCopy = new Vector2((float)Math.Round(dire.X, 1) , (float)Math.Round(dire.Y, 1));

		if (direCopy == Vector2.Down)
			rotator.RotationDegrees = directionRotations[direCopy];
		else if (direCopy == Vector2.Up)
			rotator.RotationDegrees = directionRotations[direCopy];
		else if (direCopy == Vector2.Left)
			rotator.RotationDegrees = directionRotations[direCopy];
		else if (direCopy == Vector2.Right)
			rotator.RotationDegrees = directionRotations[direCopy];
		else if (direCopy == new Vector2(-0.7F, 0.7F))
			rotator.RotationDegrees = directionRotations[direCopy];
		else if (direCopy == new Vector2(-0.7F, -0.7F))
			rotator.RotationDegrees = directionRotations[direCopy];
		else if (direCopy == new Vector2(0.7F, -0.7F))
			rotator.RotationDegrees = directionRotations[direCopy];
		else if (direCopy == new Vector2(0.7F, 0.7F))
			rotator.RotationDegrees = directionRotations[direCopy];

		if (dire != Vector2.Zero){
			GD.Print(dire, Math.Round(dire.X, 1));
		}
	}
}
   

