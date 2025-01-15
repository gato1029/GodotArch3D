@icon("res://addons/control_node_window/window_scenes/template/ControlNodeWindow.svg")
@tool # use this if you want your content poition updated in the editor
# but be carefull, dont forget to reload the project when changing @tool

extends Control
class_name ControlNodeWindowTemplate

## When the window is closed its calling queue_free()
@export 
var delete_on_close:bool = false

## The location and size where the window wil popup
@export 
var popup_rect:Rect2 = Rect2(Vector2.ZERO,Vector2.ZERO)

## The name of the Window at the top Bar
@export
var window_name:String  #= "Window" #: set = set_window_name

## put your window content (preferably in a ScrollContainer) as a child of the Window Node. 
## This Variable will be set automaticaly.
var window_content:Control 

func set_window_name(window_name:String):
	#if not Engine.is_editor_hint():
	%WinowName.text = window_name
######
## Closes the window not if its pinne 
## use force to close it anyways 
########

## This is where the content will be placed
@onready
var content_placeholder:ColorRect = $VBoxContainer/HBoxContainer2/CenterPanel/MarginContainer/ContentPlaceholder

# closes and pauses the window 
func close():
	self.hide()
	self.process_mode=Node.PROCESS_MODE_DISABLED

# displays window 
func popup():
	self.process_mode=Node.PROCESS_MODE_INHERIT
	self.show()
	%WinowName.text = window_name
	self.position = popup_rect.position
	self.size = popup_rect.size
	#self.grab_focus()
	
	update_content_placement()


var __is_ready:bool = false
func _ready() -> void:
	if window_name == null :
		window_name = "NULL"
	set_window_name(window_name)
	var viewport:Viewport = self.get_viewport()
	viewport.size_changed.connect(_on_item_rect_changed)
	#need to call deferred because sizes are only calculated in first frame
	#self.close()
	content_placeholder = $VBoxContainer/HBoxContainer2/CenterPanel/MarginContainer/ContentPlaceholder
	__is_ready=true


var update_content_placement_first_call = true
func __r_hellper_for_changing_variable_deferred():
	update_content_placement_first_call=true


func update_content_placement():
	if not update_content_placement_first_call:
		return
	#ensure to stay at same location
	popup_rect = self.get_rect()
	update_content_placement_first_call=false
	_update_content_placement_deferred()
	# to handle "rubberbanding" content that doesent finisch to get to its place
	await get_tree().create_timer(0.01).timeout
	call_deferred("_update_content_placement_deferred")
	#reset flag at end update_content_placement_first_call
	# prevents infinite recursion !
	call_deferred("__r_hellper_for_changing_variable_deferred")
	

func _update_content_placement_deferred():
	if  (not __is_ready):
		if not Engine.is_editor_hint():
			#print("CONTROL WINDOW NOT READY!")
			return
		else: # in engine 
			content_placeholder =  $VBoxContainer/HBoxContainer2/CenterPanel/MarginContainer/ContentPlaceholder
	
	#print("content_placement_update")
	if (self.get_children().size() < 1):
		return
	var window_content_tmp = self.get_children()[-1]
	if window_content_tmp == null:
		window_content_tmp = $Content_dummy
		if window_content_tmp == null:
			printerr("WINDOW HAS NO CONTENT!")
			return
			
	window_content = window_content_tmp
	window_content.global_position = content_placeholder.global_position
	window_content.size = content_placeholder.size


func _on_top_panel_gui_input(event: InputEvent) -> void:
	if event is InputEventMouseMotion :
		if  event.button_mask==1: #left mouse buton is pressed
			self.position += event.relative
			update_content_placement()


func _on_border_left_gui_input(event: InputEvent) -> void:
	if event is InputEventMouseMotion :
		if  event.button_mask==1: #left mouse buton is pressed
			self.size.x += -event.relative.x
			self.position.x += event.relative.x
			update_content_placement()


func _on_border_right_gui_input(event: InputEvent) -> void:
	if event is InputEventMouseMotion :
		if  event.button_mask==1: #left mouse buton is pressed
			self.size.x += event.relative.x
			update_content_placement()


func _on_border_bot_gui_input(event: InputEvent) -> void:
	if event is InputEventMouseMotion :
		if  event.button_mask==1: #left mouse buton is pressed
			self.size.y += event.relative.y
			update_content_placement()


func _on_corner_bot_right_gui_input(event: InputEvent) -> void:
	if event is InputEventMouseMotion :
		if  event.button_mask==1: #left mouse buton is pressed
			self.size.y += event.relative.y
			self.size.x += event.relative.x
			update_content_placement()


func _on_corner_bot_left_gui_input(event: InputEvent) -> void:
	if event is InputEventMouseMotion :
		if  event.button_mask==1: #left mouse buton is pressed
			self.size.x += -event.relative.x
			self.position.x += event.relative.x
			self.size.y += event.relative.y
			update_content_placement()
			#print("border_left")

func _on_item_rect_changed() -> void:
	if not self.get_viewport():
		return 
	if not Engine.is_editor_hint():
		self.position.x = clamp(self.position.x, 0 , self.get_viewport().size.x - self.size.x)
		self.position.y = clamp(self.position.y, 0 , self.get_viewport().size.y - self.size.y)
		self.size.x = clamp(self.size.x, 0 , self.get_viewport().size.x )
		self.size.y = clamp(self.size.y, 0 , self.get_viewport().size.y )
		#print("checkboundry")
	update_content_placement()
	
func _on_child_order_changed() -> void:
	_update_content_placement_deferred()


func _on_x_pressed() -> void:
	self.close()
	if (delete_on_close):
		if not Engine.is_editor_hint():
			self.queue_free()


func _process(delta):
	if Engine.is_editor_hint():
		_update_content_placement_deferred()
