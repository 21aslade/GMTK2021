using Godot;
using System;

public class vertical : KinematicBody2D {
    [Export] public float JumpLen = 1.0f;
    [Export] public float JumpSpeed = 40.0f;
    [Export] public float TerminalVelocity = -40.0f;
    [Export] public float Gravity = -4.0f;
    [Export] public bool IsGhost = false;

    public Timer timer;
    public AnimatedSprite sprite;

    private enum AirState {
        Jump,
        Climb,
        Fall,
        Grounded
    }
    private AirState airState = AirState.Fall;
    public float verticalSpeed = 0.0f;

    private bool jumpUsed = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        timer = GetNode<Timer>("Timer");
        timer.ProcessMode = Timer.TimerProcessMode.Physics;
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
    }

    public bool IsGrounded() {
        return airState == AirState.Grounded;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public float MoveVertical(float delta) {
        bool nowGrounded = IsOnFloor();
        if ((airState == AirState.Grounded) != nowGrounded) {
            // If collider grounded but not enum, set grounded
            // If enum grounded but not collider, set fall
            airState = nowGrounded ? AirState.Grounded : AirState.Fall;
        }
        else {
            if ((airState == AirState.Grounded || airState == AirState.Jump) && Input.IsActionPressed("jump")) {
                switch (airState) {
                    case AirState.Grounded:
                        if (Input.IsActionJustPressed("jump") && JumpLen > 0.0f) {
                            airState = AirState.Jump;
                            sprite.Animation = "jump_up";
                            timer.Start(JumpLen);
                            verticalSpeed = JumpSpeed;
                        }
                        break;
                    case AirState.Jump:
                        if (timer.TimeLeft <= 0.0f) {
                            airState = AirState.Fall;
                        }
                        
                        verticalSpeed = JumpSpeed;
                        break;
                }
            }
            else {
                switch (airState) {
                    case AirState.Fall:
                        if (verticalSpeed < 0) {
                            sprite.Animation = "fall";
                        }

                        if (verticalSpeed > TerminalVelocity) {
                            verticalSpeed += Gravity * delta;
                            if (verticalSpeed < TerminalVelocity) {
                                verticalSpeed = TerminalVelocity;
                            }
                        }
                        break;
                    case AirState.Jump:
                        airState = AirState.Fall;
                        break;
                }
            }
        }

        if (airState == AirState.Grounded) {
            // Fall slightly on ground so you still collide with it
            verticalSpeed = Gravity * delta;
        }
        return verticalSpeed;
    }
}
