using Godot;
using System;

public class player : Node2D {
    [Export] public float PhysicalStart = 0.0f;
    [Export] public float GhostStart = 0.0f;
    [Export] public float MaxSpeed = 0.0f;

    private KinematicBody2D physical;
    private KinematicBody2D ghost;

    private void SetFlip(bool flip) {
        physical.GetNode<AnimatedSprite>("AnimatedSprite").FlipH = flip;
        ghost.GetNode<AnimatedSprite>("AnimatedSprite").FlipH = flip;
    }

    private void SetAnimation(string anim) {
        physical.GetNode<AnimatedSprite>("AnimatedSprite").Animation = anim;
        ghost.GetNode<AnimatedSprite>("AnimatedSprite").Animation = anim;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        physical = GetNode<KinematicBody2D>("Physical");
        physical.Position = new Vector2(0.0f, -PhysicalStart);
        ghost = GetNode<KinematicBody2D>("Ghost");
        ghost.Position = new Vector2(0.0f, GhostStart);
    }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta) {
        MoveHorizontal(delta);
    }


    public void MoveHorizontal(float delta) {
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


        // Direction is positive if right, negative otherwise
        // We know left ^ right, so checking for right is enough
        float direction = right ? 1 : -1;
        float distance = direction * MaxSpeed * delta;
        Vector2 move = new Vector2(distance, 0.0f);
        
        // Test collision for walls
        KinematicCollision2D testCollision = physical.MoveAndCollide(move, true, true, true);
        // If it doesn't move the entire distance, shorten it
        if (testCollision != null) {
            distance = testCollision.Travel.x;
            move.x = distance;
        }

        // Actually move ghost
        KinematicCollision2D ghostCollision = ghost.MoveAndCollide(move, true, true, false);
        // If it doesn't move the entire distance, shorten it for physical
        if (ghostCollision != null) {
            distance = testCollision.Travel.x;
            move.x = distance;
        }

        // Actually move person
        KinematicCollision2D physicalCollision = physical.MoveAndCollide(move, true, true, false);
        // Person had BETTER not be colliding with anything
        if (physicalCollision != null) {
            GD.Print("Physical body collision changed between test and movement");
        }
    }
}
