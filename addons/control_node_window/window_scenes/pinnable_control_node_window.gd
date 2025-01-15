@icon("res://addons/control_node_window/window_scenes/template/ControlNodeWindow.svg")
@tool # use this if you want your content poition updated in the editor
# but be carefull, dont forget to reload the project when changing @tool

extends ControlNodeWindowTemplate
class_name PinnableControlNodeWindow


func _ready() -> void:
	super._ready()
	

func close(force=false):
	if %IsPinnedWindow.button_pressed and force == false:
		return
	super.close()
	
func _on_x_pressed():
	$%IsPinnedWindow.button_pressed = false
	super._on_x_pressed()
