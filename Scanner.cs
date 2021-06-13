using Godot;
using System;

public class Scanner : Light2D
{
    vertical physical;
    Timer reset;
    AudioStreamPlayer2D sound;

    public override void _Ready() {
        physical =  GetNode<vertical>("/root/Test/Player/Physical");
        reset = GetNode<Timer>("Timer");
        sound = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta) {
        var targetPos = physical.GlobalPosition;
        var rect = physical.GetNode<CollisionShape2D>("CollisionShape2D").Shape as RectangleShape2D;
        var xDist = targetPos.x - GlobalPosition.x;
        if (xDist > rect.Extents.x) {
            targetPos.x -= rect.Extents.x;
        }
        else if (xDist < -rect.Extents.x) {
            targetPos.x += rect.Extents.x;
        }

        if (IsInSight(targetPos)) {
            Color = new Color(1.0f, 0.0f, 0.0f);
            

            // Start the end timer if not already running
            if (reset.TimeLeft <= 0.0f) {
                sound.Play();
                reset.Start();
            }
        }
    }

    public bool IsInSight(Vector2 pos) {
        Vector2 delta = pos - GlobalPosition;
        // Base of triangle: 48 pixels
        // Checks if x coordinates within 48 pixels
        // Triangle is 96 pixels, checks if in radius (using squared for speed)
        if (Math.Abs(delta.x) < 48 && delta.LengthSquared() < 9216.0f) {
            // Full angle of triangle: 0.451 RAD
            // Checks if delta angle is within half that distance (+/-) light angle + 2PI (light starts down) 
            if (Math.Abs(delta.Angle() - (Rotation + Math.PI / 2)) < (0.451f / 2)) {
                // Within angle, so raycast
                return CheckRaycast(pos);
            }
        }
        
        return false;
    }

    private bool CheckRaycast(Vector2 pos) {
        var exclusions = new Godot.Collections.Array { };

        var spaceState = GetWorld2d().DirectSpaceState;

        var startPos = GlobalPosition;
        var raycastDirection = (pos - startPos).Normalized();

        // We return from this loop whenever it exits
        // Bound the loop to avoid an infinite one
        var iterationsLeft = 5;
        while (true) {
            var result = spaceState.IntersectRay(startPos, pos, exclusions);
            if (result.Count > 0) {
                var collider = (Godot.Object)result["collider"];
                if (collider is TileMap) {
                    // Get tile collided with
                    var position = (Vector2)result["position"];
                    var tilemap = collider as TileMap;
                    if (TileOccluded(tilemap, position)) {
                        return false;
                    }
                    else {
                        // Offset start position
                        startPos = position + raycastDirection * tilemap.CellSize.y;
                    }
                }
                else if (collider == physical) {
                    // Just collided with the player, neat!
                    return true;
                }
                else {
                    // Collided with something else, we're done here
                    return false;
                }
            }

            if (--iterationsLeft <= 0) {
                // Iteration timeout
                return false;
            }
        }
    }

    private bool TileOccluded(TileMap tilemap, Vector2 position) {
        var tileset = tilemap.TileSet;
        
        var tilePos = tilemap.WorldToMap(position);
        var tile = tilemap.GetCell((int)tilePos.x, (int)tilePos.y);
        // If tile exists has occluder, do another raycast
        if (tile > 0) {
            var occluder = tileset.TileGetLightOccluder(tile);
            if (occluder != null) {
                // Collided with occlusive tile; we're done here
                return true;
            }
            else {
                return false;
            }
        }
        else {
            // If there isn't a tile, assume it's not occlusive?
            return false;
        }
        
    }

    private void Reset() {
        GetTree().ReloadCurrentScene();
    }
}
