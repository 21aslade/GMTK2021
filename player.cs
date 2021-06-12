using Godot;
using System;

public class player : Node2D {
    [Export] public float PhysicalStart = 0.0f;
    [Export] public float GhostStart = 0.0f;
    [Export] public float MaxSpeed = 0.0f;

    [Signal] public delegate void SwapRealms();

    private vertical physical;
    private vertical ghost;

    private bool swapped = false;
    private bool swapButton = true;

    private void SetFlip(bool flip) {
        physical.sprite.FlipH = flip;
        ghost.sprite.FlipH = flip;
    }

    private void SetAnimationIfGrounded(string anim) {
        if (physical.IsGrounded()) {
            physical.sprite.Animation = anim;
        }

        if (ghost.IsGrounded()) {
            ghost.sprite.Animation = anim;
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        physical = GetNode<vertical>("Physical");
        physical.Position = new Vector2(0.0f, -PhysicalStart);
        ghost = GetNode<vertical>("Ghost");
        ghost.Position = new Vector2(0.0f, GhostStart);
    }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta) {
        float originalX = physical.Position.x;
        float horiz = MoveHorizontal();

        if (!swapped) {
            MoveVertical(physical, delta, horiz, -1.0f);
            MoveVertical(ghost, delta, horiz, 1.0f);
        }
        else {
            MoveVertical(physical, delta, horiz, 1.0f);
            MoveVertical(ghost, delta, horiz, -1.0f);
        }

        // POSITION SET USED AFTER THIS POINT
        // NO MORE PHYSICS CALLS ALLOWED

        // Make sure positions are together
        SyncPositions(originalX);
        SwapCheck();
    }

    private float MoveHorizontal() {
        bool left = Input.IsActionPressed("ui_left");
        bool right = Input.IsActionPressed("ui_right");

        // Either none or both are pressed
        if (left == right) {
            SetAnimationIfGrounded("idle");
            return 0.0f;
        }

        // Flip sprites based on facing (flipped if left)
        SetFlip(left);
        SetAnimationIfGrounded("walk");

        // Direction is positive if right, negative otherwise
        // We know left ^ right, so checking for right is enough
        float direction = right ? 1 : -1;
        return direction * MaxSpeed;
    }

    private void MoveVertical(vertical v, float delta, float horiz, float up) {
        float vert = v.MoveVertical(delta);

        Vector2 upDir = new Vector2(0.0f, 1.0f) * up;
        v.MoveAndSlide(new Vector2(horiz, vert * up), upDir);
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

    private void SwapCheck() {
        if (Input.IsActionPressed("swap")) {
            if (!swapButton) {
                if (!swapped) {
                    Swap(physical, ghost);
                }
                else {
                    Swap(ghost, physical);
                }

                swapped = !swapped;
                swapButton = true;
            }
            
        }
        else {
            swapButton = false;
        }
    }

    private void Swap(vertical top, vertical bottom) {
        AnimatedSprite topSprite = top.GetNode<AnimatedSprite>("AnimatedSprite");
        AnimatedSprite bottomSprite = bottom.GetNode<AnimatedSprite>("AnimatedSprite");
        topSprite.FlipV = true;
        bottomSprite.FlipV = false;

        // Swap top and bottom sprite offsets
        float topYPosition = topSprite.Position.y;
        topSprite.Position = new Vector2(topSprite.Position.x, bottomSprite.Position.y);
        bottomSprite.Position = new Vector2(bottomSprite.Position.x, topYPosition);

        float topSpeed = top.verticalSpeed;
        top.verticalSpeed = bottom.verticalSpeed;
        bottom.verticalSpeed = topSpeed;

        Vector2 topPosition = top.Position;
        top.Position = bottom.Position;
        bottom.Position = topPosition;

        EmitSignal("SwapRealms");
    }
}
