@tool
extends EditorPlugin
# Esta variable mantendrá una referencia al botón
var generate_button : Button

func _enter_tree():
	# Creamos el botón en la barra de herramientas
	generate_button = Button.new()
	generate_button.text = "Generar UI C#"
	generate_button.connect("pressed",  _on_generate_button_pressed)
	
	# Añadimos el botón a la barra de herramientas
	add_control_to_container(CONTAINER_CANVAS_EDITOR_MENU, generate_button)
	add_tool_menu_item("Generar UI C#",_on_generate_button_pressed)	
func _exit_tree():
	# Limpiamos al salir del plugin
	remove_control_from_container(CONTAINER_CANVAS_EDITOR_MENU, generate_button)
	remove_tool_menu_item("Generar UI C#")
	generate_button.queue_free()

# Función que se llama cuando el botón es presionado
func _on_generate_button_pressed():
	# Ejecutamos el generador de código
	print("Botón presionado. Ejecutando el generador...")
	var script = preload("res://addons/UIGeneratorCSharp/ui_generator.gd").new()
	script._run()
