using Godot;
using System;

public class Crate : Node2D {
    [Export] public bool UsePhysical = true;
    [Export] public float PhysicalStart = 0.0f;
    [Export] public bool UseGhost = true;
    [Export] public float GhostStart = 0.0f;

    private CrateVertical physical;
    private CrateVertical ghost;
    private bool flipped;

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
        float horiz = MoveHorizontal();

        if (!flipped) {
            MoveCrate(physical, delta, horiz, -1.0f);
            MoveCrate(ghost, delta, horiz, 1.0f);
        }
        else {
            MoveCrate(physical, delta, horiz, 1.0f);
            MoveCrate(ghost, delta, horiz, -1.0f);
        }
    }

    private void MoveCrate(CrateVertical crate, float delta, float horiz, float up) {
        float vert = crate.MoveVertical(delta);

        Vector2 upDir = new Vector2(0.0f, 1.0f) * up;
        crate.MoveAndSlide(new Vector2(horiz, vert * up), upDir);
    }

    private float MoveHorizontal() {
        return 0.0f;
    }
}
