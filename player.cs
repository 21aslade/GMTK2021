using Godot;
using System;

public class player : Node2D {
    [Export] public float PhysicalStart = 0.0f;
    [Export] public float GhostStart = 0.0f;
    [Export] public float MaxSpeed = 0.0f;

    private KinematicBody2D physical;
    private AnimatedSprite physicalSprite;
    private KinematicBody2D ghost;
    private AnimatedSprite ghostSprite;

    private float physicalYSpeed = 0.0f;
    private float ghostYSpeed = 0.0f;

    private bool swapped = false;
    private bool swapButton = true;

    private void SetFlip(bool flip) {
        physicalSprite.FlipH = flip;
        ghostSprite.FlipH = flip;
    }

    private void SetAnimation(string anim) {
        physicalSprite.Animation = anim;
        ghostSprite.Animation = anim;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        physical = GetNode<KinematicBody2D>("Physical");
        physicalSprite = physical.GetNode<AnimatedSprite>("AnimatedSprite");
        physical.Position = new Vector2(0.0f, -PhysicalStart);
        ghost = GetNode<KinematicBody2D>("Ghost");
        ghostSprite = ghost.GetNode<AnimatedSprite>("AnimatedSprite");
        ghost.Position = new Vector2(0.0f, GhostStart);
    }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta) {
        MoveVertical(delta);
        MoveHorizontal(delta);

        // USES POSITION SET, must be done after all MoveAndCollide
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

    public void MoveVertical(float delta) {
        float ghostDelta = VerticalGhost();
        float physicalDelta = VerticalPhysical();

        if (!swapped) {
            // Flip physical, keep ghost
            ghost.MoveAndCollide(new Vector2(0.0f, ghostDelta));
            physical.MoveAndCollide(new Vector2(0.0f, -physicalDelta));
        }
        else {
            // Flip ghost, keep physical
            ghost.MoveAndCollide(new Vector2(0.0f, -ghostDelta));
            physical.MoveAndCollide(new Vector2(0.0f, physicalDelta));
        }
    }

    private void Swap(KinematicBody2D top, KinematicBody2D bottom) {
        AnimatedSprite topSprite = top.GetNode<AnimatedSprite>("AnimatedSprite");
        AnimatedSprite bottomSprite = bottom.GetNode<AnimatedSprite>("AnimatedSprite");
        topSprite.FlipV = true;
        bottomSprite.FlipV = false;

        // Swap top and bottom sprite offsets
        float topYPosition = topSprite.Position.y;
        topSprite.Position = new Vector2(topSprite.Position.x, bottomSprite.Position.y);
        bottomSprite.Position = new Vector2(bottomSprite.Position.x, topYPosition);

        Vector2 topPosition = top.Position;
        top.Position = bottom.Position;
        bottom.Position = topPosition;
    }

    public float VerticalGhost() {
        //ghostYSpeed -= 1.0f;

        return -1.0f;
    }

    public float VerticalPhysical() {
        return -1.0f;
    }

    public void MoveHorizontal() {
        bool left = Input.IsActionPressed("ui_left");
        bool right = Input.IsActionPressed("ui_right");

        // Either none or both are pressed
        if (left == right) {
            SetAnimation("idle");
            return;
        }

        // Flip sprites based on facing (flipped if left)
        SetFlip(left);
        SetAnimation("walk");

        // Save original position for later...
        float originalX = physical.Position.x;

        // Direction is positive if right, negative otherwise
        // We know left ^ right, so checking for right is enough
        float direction = right ? 1 : -1;
        float distance = direction * MaxSpeed;
        Vector2 move = new Vector2(distance, 0.0f);

        physical.MoveAndSlide(move);
        ghost.MoveAndSlide(move);

        float ghostDelta = ghost.Position.x - originalX;
        float physicalDelta = physical.Position.x - originalX;

        // Match x position with collide
        // Avoids jank teleports?
        if (Math.Abs(ghostDelta) >= Math.Abs(physicalDelta)) {
            ghost.MoveAndCollide(new Vector2(physical.Position.x - ghost.Position.x, 0.0f));
        }
        else {
            physical.MoveAndCollide(new Vector2(ghost.Position.x - physical.Position.x, 0.0f));
        }
    }
}
