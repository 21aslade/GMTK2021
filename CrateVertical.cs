using Godot;
using System;

public class CrateVertical : KinematicBody2D
{
    private Area2D area;
    private Particles2D particles;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        area = GetNode<Area2D>("Swap");
        particles = GetNode<Particles2D>("Particles2D");

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

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta) {
        
    }

    private void SwapEnter(Node body) {
        particles.Emitting = true;
    }

    private void SwapExit(Node body) {
        if (area.GetOverlappingBodies().Count == 0) {
            particles.Emitting = false;
        }
    }
}
