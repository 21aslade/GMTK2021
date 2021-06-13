extends Control


# Declare member variables here. Examples:
# var a = 2
# var b = "text"


# Called when the node enters the scene tree for the first time.
func _ready():
	for button in $Menu/CenterRow/CenterContainer/Buttons.get_children():
		button.connect("pressed", self, "on_Button_pressed", [button.scene_to_load])

func _on_Button_pressed(scene_to_load):
	get_tree().change_scene("res://Main.tscn")


func _on_Start_pressed():
	get_tree().change_scene("res://Main.tscn") # Replace with function body.
