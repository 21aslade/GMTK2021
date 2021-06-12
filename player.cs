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

    private bool ghostJumpTimer = 4.0f;
    private bool physicalJumpTimer = 4.0f;

    private enum AirState {
        Jump,
        Climb,
        Fall,
        Ground
    }

    private AirState ghostAirState;
    private AirState physicalAirState;

    private void SetFlip(bool flip) {
        physicalSprite.FlipH = flip;
        ghostSprite.FlipH = flip;
    }

    private void SetAnimationIfGrounded(string anim) {
        if (physicalAirState == AirState.Ground) {
            physicalSprite.Animation = anim;
        }

        if (ghostAirState == AirState.Ground) {
            ghostSprite.Animation = anim;
        }
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
        float originalX = physical.Position.x;
        float horiz = MoveHorizontal();
        if (flipped) {
            MovePhysical(horiz, -1.0);
            MoveGhost(horiz, 1.0);
        }
        else {
            MovePhysical(horiz, 1.0);
            MoveGhost(horiz, -1.0);
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
            return;
        }

        // Flip sprites based on facing (flipped if left)
        SetFlip(left);
        SetAnimationIfGrounded("walk");

        // Save original position for later...
        float originalX = physical.Position.x;

        // Direction is positive if right, negative otherwise
        // We know left ^ right, so checking for right is enough
        float direction = right ? 1 : -1;
        return direction * MaxSpeed;
    }

    private void MovePhysical(float horiz, float up) {
        float vert = 0.0f;
        if ((physicalAirState == AirState.Grounded) != physical.isGrounded()) {
            // If collider grounded but not enum, set grounded
            // If enum grounded but not collider, set fall
            physicalAirState = physical.isGrounded() ? AirState.Grounded : AirState.Fall;
        }
        else {
            if (Input.IsActionPressed("jump")) {
                
            }
        }

        Vector2 upDir = new Vector2(0.0f, 1.0f) * up;
        physical.MoveAndSlide(new Vector2(horiz, vert * up), upDir);
    }

    private void MoveGhost(float horiz, float up) {
        

        Vector2 upDir = new Vector2(0.0f, 1.0f) * up;
        ghost.MoveAndSlide(new Vector(horiz, vert * up), upDir);
    }

    private void SyncPositions(float originalX) {
        float physicalChange = Math.Abs(physical.Position.x - originalX);
        float ghostChange = Math.Abs(ghost.Position.x - originalX);

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
}
