@tool
extends EditorPlugin

func _enter_tree() -> void:
	# Initialization of the plugin goes here.
	add_custom_type("AnimatedBoxContainer", "Container", preload("AnimatedBoxContainer.cs"), preload("icon.svg"))

func _exit_tree() -> void:
	# Clean-up of the plugin goes here.
	remove_custom_type("AnimatedBoxContainer")
