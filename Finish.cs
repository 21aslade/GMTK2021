using Godot;
using System;

public class Finish : Area2D {
    bool started = false;
    public void _on_Area2D_body_entered(Node body) {

        if (body is vertical) {
            if (!started) {
                GetNode<Timer>("Timer").Start();
                GetNode<AudioStreamPlayer>("Audio").Play();
                GetNode<Sprite>("Sprite").Visible = true;
                started = true;
            }
        }
    }

    public void TimeOver() {
        GetTree().ChangeScene("res://title_screen/titlescreen.tscn");
    }
}
