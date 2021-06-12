using Godot;
using System;

public class CrateVertical : KinematicBody2D
{
    [Export] public float TerminalVelocity = -80.0f;
    [Export] public float Gravity = -320.0f;

    private float verticalSpeed;
    private Area2D area;
    private Particles2D particles;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        area = GetNode<Area2D>("Swap");

        area.Connect("body_entered", this, nameof(SwapEnter));
        area.Connect("body_exited", this, nameof(SwapExit));

        particles = GetNode<Particles2D>("Particles2D");
        particles.Emitting = false;
    }

    public void Flip(bool flipped) {
        Vector2 scale = this.Scale;
        scale.y = flipped ? -1 : 1;
        this.Scale = scale;
    }

    public bool PlayerColliding() {
        return particles.Emitting;
    }

    public void Enable(bool enabled) {
        Visible = enabled;
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled = !enabled;
        area.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = !enabled;
    }

    public float MoveVertical(float delta) {
        if (IsOnFloor()) {
            verticalSpeed = 0.0f;
        }
        else {
            if (verticalSpeed > TerminalVelocity) {
                verticalSpeed += Gravity * delta;
                if (verticalSpeed < TerminalVelocity) {
                    verticalSpeed = TerminalVelocity;
                }
            }
        }

        return verticalSpeed;
    }

    private void SwapEnter(Node body) {
        particles.Emitting = true;
    }

    private void SwapExit(Node body) {
        if (area.GetOverlappingBodies().Count <= 1) {
            particles.Emitting = false;
        }
    }
}
