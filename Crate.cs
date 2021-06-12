using Godot;
using System;

public class Crate : Node2D {
    [Export] public bool UsePhysical = true;
    [Export] public float PhysicalStart = 0.0f;
    [Export] public bool UseGhost = true;
    [Export] public float GhostStart = 0.0f;

    private CrateVertical physical;
    private CrateVertical ghost;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        physical = GetNode<CrateVertical>("Physical");
        physical.Enable(UsePhysical);
        physical.Position = new Vector2(0.0f, -PhysicalStart);
        ghost = GetNode<CrateVertical>("Ghost");
        ghost.Enable(UseGhost);
        ghost.Position = new Vector2(0.0f, GhostStart);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta) {

    }
}
