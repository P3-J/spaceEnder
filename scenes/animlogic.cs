using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class battle : Node3D
{

    List<Marker3D> enemyMarkerList = new();
    List<Node3D> EnemyModelInstances = new();

    private string enemyPath = "res://realmodels/skelly_model.tscn";

    public void InstanceEnemies3D()
    {
        // truly my 3D renderer with a bad name
        var sceneResource = (PackedScene)ResourceLoader.Load(enemyPath);
        Node3D battleground = GetNode<Node3D>("battleground");

        foreach (EnemyBase enemy in enemyBoard)
        {   
            if (enemy.HasBeenInstanced){continue;}
            enemy.HasBeenInstanced = true;
            Node3D instance = sceneResource.Instantiate() as Node3D;
            instance.Position = enemyMarkerList[enemy.CurrentSlot].Position;
            instance.Rotation = enemyMarkerList[enemy.CurrentSlot].Rotation;
            instance.Set("SpotID", enemyMarkerList[enemy.CurrentSlot].Get("slot"));
            instance.Set("Unit", enemy.Name);
            instance.Scale = new Vector3(1,1,1);
            battleground.AddChild(instance);
            EnemyModelInstances.Add(instance);
        }
    }

    public void RemoveInstanceAtId(int id)
    {
        foreach (Node3D instance in EnemyModelInstances)
        {
            int spotID = (int)instance.Get("SpotID");
            if (spotID == id){
                instance.QueueFree();
                EnemyModelInstances.Remove(instance);
                return;
            }
        }
    }

}