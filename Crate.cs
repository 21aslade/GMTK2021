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
    private bool flipEdge;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        physical = GetNode<CrateVertical>("Physical");
        physical.Enable(UsePhysical);
        physical.Position = new Vector2(0.0f, -PhysicalStart);

        ghost = GetNode<CrateVertical>("Ghost");
        ghost.Enable(UseGhost);
        ghost.Position = new Vector2(0.0f, GhostStart);

        player p = GetNode<player>("/root/Test/Player");
        p.Connect("SwapRealms", this, "SwapRealms");
    }

    private void SwapRealms() {
        if (physical.PlayerColliding() || ghost.PlayerColliding()) {
            flipEdge = true;
        }
    }

    private void ActuallySwap() {

        // Toggle flip
        flipped = !flipped;

        physical.Flip(flipped);
        ghost.Flip(!flipped);
        if (UseGhost && UsePhysical) {
            float topSpeed = physical.verticalSpeed;
            physical.verticalSpeed = ghost.verticalSpeed;
            ghost.verticalSpeed = topSpeed;

            Vector2 topPosition = physical.Position;
            physical.Position = ghost.Position;
            ghost.Position = topPosition;
        }
        else if (UseGhost ^ UsePhysical) {
            var used = UsePhysical ? physical : ghost;

            var teleporter = (vertical)used.GetBodies()[0];

            var phys = teleporter.GetParent<player>().GetNode<vertical>("Physical");
            var ghos = teleporter.GetParent<player>().GetNode<vertical>("Ghost");
            var nontp = (teleporter == ghos) ? phys : ghos;

            Vector2 delta = GlobalPosition - nontp.GlobalPosition;
            GlobalPosition = teleporter.GlobalPosition + delta;
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta) {
        float originalX = physical.Position.x;
        float horiz = MoveHorizontal();

        if (!flipped) {
            if (UsePhysical) {
                MoveCrate(physical, delta, horiz, -1.0f);
            }
            
            if (UseGhost) {
                MoveCrate(ghost, delta, horiz, 1.0f);
            }
        }
        else {
            if (UsePhysical) {
                MoveCrate(physical, delta, horiz, 1.0f);
            }
            
            if (UseGhost) {
                MoveCrate(ghost, delta, horiz, -1.0f);
            }
        }

        // POSITION SET USED AFTER THIS POINT
        // NO MORE PHYSICS CALLS ALLOWED

        if (UsePhysical && UseGhost) {
            SyncPositions(originalX);
        }

        if (flipEdge) {
            flipEdge = false;
            ActuallySwap();
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

    private void SyncPositions(float originalX) {
        float physicalChange = Math.Abs(physical.Position.x - originalX);
        float ghostChange = Math.Abs(ghost.Position.x - originalX);

        if (physicalChange > ghostChange) {
            Vector2 newPosition = new Vector2(ghost.Position.x, physical.Position.y);
            physical.MoveAndCollide(newPosition - physical.Position);
        }
        else {
            Vector2 newPosition = new Vector2(physical.Position.x, ghost.Position.y);
            ghost.MoveAndCollide(newPosition - ghost.Position);
        }
    }
}
