@tool
extends EditorScript

func _run():
	var root_node = get_editor_interface().get_edited_scene_root()
	if root_node == null:
		print("¡Error! No hay ninguna escena cargada.")
		return
	
	var root_script = root_node.get_script()
	if root_script == null:
		print("¡Error! El nodo raíz debe tener un script asociado.")
		return

	var script_path = root_script.resource_path
	var script_dir = script_path.get_base_dir()
	var script_name = script_path.get_file().get_basename()

	# Crear carpeta UIGenerate si no existe
	var ui_gen_dir = script_dir + "/UIGenerate"

	if not DirAccess.dir_exists_absolute(ui_gen_dir):
		DirAccess.make_dir_recursive_absolute(ui_gen_dir)

	var output_path = "%s/%s.generated.cs" % [ui_gen_dir, script_name]

	print("Generando archivo en: ", output_path)
	var script_text = _generate_script(root_node, script_name)

	var file = FileAccess.open(output_path, FileAccess.WRITE)
	if file == null:
		print("¡Error! No se pudo abrir el archivo para escribir.")
		return
	
	file.store_string(script_text)
	file.close()
	print("Código generado correctamente en: ", output_path)
	
	# Leer el script original
	var original_code := FileAccess.get_file_as_string(script_path)

	# Verificamos si ya se llama a InitializeUI()
	var calls_initialize := original_code.find("InitializeUI();") != -1
	if calls_initialize:
		print("La llamada a InitializeUI ya existe. No se modifica el archivo original.")
		return

	var has_ready := original_code.find("public override void _Ready()") != -1
	var new_lines := []
	if has_ready:
			# Insertar dentro del método _Ready()
		var lines := original_code.split("\n")
		
		var inside_ready := false
		var inserted := false

		for i in range(lines.size()):
			var line = lines[i]
			new_lines.append(line)

			# Detectamos el comienzo de _Ready
			if line.strip_edges().begins_with("public override void _Ready()"):
				inside_ready = true
				continue

			# Detectamos la apertura del bloque
			if inside_ready and line.strip_edges() == "{":
				new_lines.append("        InitializeUI(); // Insertado por el generador de UI")
				inserted = true
				inside_ready = false  # Evitamos insertar múltiples veces
	else:
		# Si no hay _Ready, lo agregamos al final
		original_code += "\n\n    public override void _Ready()\n    {\n        InitializeUI(); // Insertado por el generador de UI\n    }\n"
		print("Agregado método _Ready con InitializeUI.")
	# Si insertamos en el cuerpo, reemplazamos el contenido
	if has_ready:
		original_code = "\n".join(new_lines)
	# Escribimos el archivo modificado
	var f = FileAccess.open(script_path, FileAccess.WRITE)
	if f:
		f.store_string(original_code)
		f.close()
		print("Archivo original actualizado con llamada a InitializeUI.")
	else:
		print("¡Error! No se pudo reescribir el archivo original.")
		

func _generate_script(root_node: Node, class_name_base: String) -> String:
	var base_class = root_node.get_class()
	var lines = []
	lines.append("// AUTO-GENERATED FILE. DO NOT EDIT.")
	lines.append("using Godot;")
	lines.append("using System;\n")
	lines.append("public partial class %s : %s" % [class_name_base, base_class])
	lines.append("{")
	lines.append("    public delegate void EventNotifyChangued(%s objectControl);" % class_name_base)
	lines.append("    public event EventNotifyChangued OnNotifyChangued;\n")

	var node_defs = []
	var init_lines = []
	var extra_methods = []

	var nodes = _collect_nodes(root_node)
	
	if root_node is Window:
		init_lines.append("        CloseRequested += CloseRequestedWindow;")

	for n in nodes:
		var node_type = n.get_class()
		if n.get_script() != null:
			var script_path = n.get_script().resource_path
			node_type = script_path.get_file().get_basename()

		if n.name == node_type and n.get_script() == null:
			continue
		if _is_instance_of_scene(n):
			continue

		var rel_path = root_node.get_path_to(n)
		var var_name = n.name.replace(" ", "_")

		var result = {}
		if node_type == "CheckBox":
			result = _process_checkbox(n, var_name, rel_path)
		elif node_type == "OptionButton":
			result = _process_optionbutton(n, var_name, rel_path)
		else:
			result = _process_generic_node(n, var_name, rel_path, node_type)

		node_defs += result["node_defs"]
		init_lines += result["init_lines"]
		extra_methods += result["extra_methods"]

	lines += node_defs
	lines.append("\n    public void InitializeUI()")
	lines.append("    {")
	lines += init_lines
	lines.append("    }")

	if root_node is Window:
		lines.append("\n    private void CloseRequestedWindow()\n    {")
		lines.append("        QueueFree();")
		lines.append("    }\n")

	lines += extra_methods
	lines.append("}")
	return "\n".join(lines)
	
func _process_checkbox(n: Node, var_name: String, rel_path: NodePath) -> Dictionary:
	var original_text = n.text
	return {
		"node_defs": ["    private CheckBox %s;" % var_name],
		"init_lines": [
			"        %s = GetNode<CheckBox>(\"%s\");" % [var_name, rel_path],
			"        %s.Pressed += %s_PressedUI;" % [var_name, var_name],
			"        %s_PressedUI();" % var_name,
		],
		"extra_methods": [
			"",
			"    private void %s_PressedUI()" % var_name,
			"    {",
			"        if (%s.ButtonPressed)" % var_name,
			"            %s.Text = \"%s\";" % [var_name, original_text],
			"        else",
			"            %s.Text = \"No %s\";" % [var_name, original_text],			
			"    }",
		]
	}

func _process_optionbutton(n: Node, var_name: String, rel_path: NodePath) -> Dictionary:
	return {
		"node_defs": ["    private OptionButton %s;" % var_name],
		"init_lines": [
			"        %s = GetNode<OptionButton>(\"%s\");" % [var_name, rel_path],
			"        %s.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;" % var_name,
		],
		"extra_methods": []
	}

func _process_generic_node(n: Node, var_name: String, rel_path: NodePath, node_type: String) -> Dictionary:
	return {
		"node_defs": ["    private %s %s;" % [node_type, var_name]],
		"init_lines": ["        %s = GetNode<%s>(\"%s\");" % [var_name, node_type, rel_path]],
		"extra_methods": []
	}


func _collect_nodes(root: Node) -> Array:
	var list := []
	_traverse(root, list, true)  # <-- indicamos que el root es especial
	list.erase(root)
	return list
func _is_instance_of_scene(node: Node) -> bool:
	# Un nodo es una instancia de PackedScene si tiene un recurso que es un PackedScene
	if node is Node:
		var scene_resource = node.get("scene")
		if scene_resource and scene_resource is PackedScene:
			return true
	return false
func _traverse(node: Node, acc: Array,is_root := false) -> void:
	acc.append(node)
	# Si NO es el nodo raíz y tiene script, no recursar hijos
	if not is_root and node.get_script() != null:
		print("Nodo '%s' tiene script asociado, omitiendo hijos..." % node.name)
		return
	
	if _is_instance_of_scene(node):
		print("Nodo '%s' es una instancia de 'PackedScene', omitiendo recorrido de hijos..." % node.name)
		return
	for child in node.get_children():
		if child is Node:
			_traverse(child, acc)
