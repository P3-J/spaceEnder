using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class battle : Node3D
{
    Camera3D battleCam;
    Camera3D playerCam;
    int outsideCurrentEnemyID;
    
    public void SwitchCam(string whichCam)
    {
        switch (whichCam)
        {
            case "player":
                playerCam.Current = true;
                break;
            case "battle":
                battleCam.Current = true;
                break;
        }
    }

    public void CombatOverSequence()
    {
        // mobs dead
        // reward maybe
        GameManager.playerGold += 10;
        GameManager.UntilHigherDanger -= 1;
        InitInvisAndVis(false);
        SceneSwitcher.CallDeferred("Close");
    }

    public void SwitchSceneBackToWorld()
    {
        GetTree().ChangeSceneToFile("res://scenes/hometest.tscn");
    }

}
