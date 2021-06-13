using Godot;
using System;

public class vertical : KinematicBody2D {
    [Export] public float JumpLen = 1.0f;
    [Export] public float JumpSpeed = 40.0f;
    [Export] public float TerminalVelocity = -40.0f;
    [Export] public float Gravity = -4.0f;
    [Export] public bool IsGhost = false;
    [Export] public float ClimbSpeed = 40.0f;

    public Timer timer;
    public AnimatedSprite sprite;

    public Area2D groundRay;
    public RayCast2D climbRay;
    public RayCast2D cornerRay;
    public bool flippedAndPhys = false;

    private enum AirState {
        Jump,
        Climb,
        ClimbCorner,
        CornerBoost,
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
        groundRay = GetNode<Area2D>("RayCast2D");
        if (!IsGhost) {
            climbRay = GetNode<RayCast2D>("Climb");
            cornerRay = GetNode<RayCast2D>("CornerCheck");
        }
    }

    public bool IsGrounded() {
        return airState == AirState.Grounded;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public float MoveVertical(float delta) {
        bool nowGrounded = groundRay.GetOverlappingBodies().Count > 1;
        if ((airState == AirState.Grounded) != nowGrounded && airState != AirState.Climb) {
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
                if (!IsGhost) {
                    Climb();
                }

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
            verticalSpeed = 0.0f;
        }
        return verticalSpeed;
    }

    public void PhysFlip(bool flip) {
        flippedAndPhys = flip;
        cornerRay.Position = new Vector2(cornerRay.Position.x, flip ? -20.0f : 20.0f);
    }

    private void Climb() {
        if (airState != AirState.Climb && airState != AirState.ClimbCorner) {
            sprite.Play();
        }

        if (climbRay == null) {
            return;
        }

        string up = "ui_up";
        string down = "ui_down";
        if (flippedAndPhys) {
            string temp = up;
            up = down;
            down = temp;
        }

        if (climbRay.IsColliding()) {
            var position = climbRay.GetCollisionPoint();
            var tilemap = climbRay.GetCollider() as TileMap;
            if (tilemap == null) {
                return;
            }

            var tileset = tilemap.TileSet;
        
            var tilePos = tilemap.WorldToMap(position);
            // Negatives meet at a different tile pos
            if (climbRay.Rotation > Math.PI / 2) {
                tilePos.x -= 1;
            }
            var tile = tilemap.GetCell((int)tilePos.x, (int)tilePos.y);
            switch (tile) {
                case 4:
                case 5:
                case 9:
                    if (Input.IsActionPressed(up)) {
                        airState = AirState.Climb;
                        verticalSpeed = ClimbSpeed;
                        sprite.Animation = "climb";
                        sprite.Play();
                    }
                    else if (airState == AirState.Climb && Input.IsActionPressed(down)) {
                        airState = AirState.Climb;
                        verticalSpeed = -ClimbSpeed;
                        sprite.Animation = "climb_down";
                        sprite.Play();

                        if (groundRay.GetOverlappingBodies().Count > 1) {
                            airState = AirState.Grounded;
                        }
                    }
                    else if (airState == AirState.Climb) {
                        sprite.Stop();
                        verticalSpeed = 0;
                    }
                    break;
            }
        }
        else {
            if (airState == AirState.Climb) {
                if (cornerRay.IsColliding()) {
                    // Prep for climbing onto the corner
                    verticalSpeed = 0;
                    airState = AirState.ClimbCorner;
                    sprite.Stop();
                }
                else {
                    // Climbed down off vine
                    airState = AirState.Fall;
                    sprite.Play();
                }
                
            }
            else if (airState == AirState.ClimbCorner) {
                if (Input.IsActionPressed(down)) {
                    airState = AirState.Climb;
                    verticalSpeed = -ClimbSpeed;
                    sprite.Animation = "climb_down";
                    sprite.Play();

                    if (groundRay.GetOverlappingBodies().Count > 1) {
                        airState = AirState.Grounded;
                    }
                }
                else if (Input.IsActionPressed(up)) {
                    airState = AirState.CornerBoost;
                    sprite.Animation = "climb_corner";
                    sprite.Play();
                }
            }
            else if (airState == AirState.CornerBoost) {
                if (cornerRay.IsColliding()) {
                    verticalSpeed = ClimbSpeed * 2.0f;
                }
                else {
                    airState = AirState.Fall;
                }
                
            }
        }
    }
}
