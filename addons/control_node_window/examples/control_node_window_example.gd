extends Control

# popup the windows
func _on_popup_pressed() -> void:
	$PinnableControlNodeWindow.popup()
	$PinnableControlNodeWindow.popup_rect = Rect2i(100,100,400,500)
	$ControlNodeWindow.popup()
	

# close
func _on_close_all_windows_pressed() -> void:
	$PinnableControlNodeWindow.close()
	$ControlNodeWindow.close()

# swap/change content
func _on_swap_content_pressed() -> void:
	var tmp = $PinnableControlNodeWindow.get_children()[-1] #get content
	$PinnableControlNodeWindow.remove_child(tmp) # remove content
	var tmp2 = $ControlNodeWindow.get_children()[-1]
	$ControlNodeWindow.remove_child(tmp2)
	$PinnableControlNodeWindow.add_child(tmp2)
	$ControlNodeWindow.add_child(tmp)
	
