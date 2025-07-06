shader_type canvas_item;
render_mode blend_mix;

uniform vec2 size = vec2(1280.0, 720.0);
uniform float line_width : hint_range(0, 2) = 2;
uniform vec2 offset = vec2(0.0, 0.0);
uniform float grid_size = 16.0;
uniform vec4 grid_color : source_color = vec4(1.0, 1.0, 1.0, 0.2); // Blanco con transparencia

void fragment() {
vec2 uv = (offset / size) + UV;
	vec2 r = mod(size * uv, vec2(grid_size));
	
	// Calcular distancia a la línea más cercana (horizontal y vertical)
	vec2 dist = min(r, vec2(grid_size) - r);
	
	// Suavizar el borde de la línea con smoothstep
	vec2 lines = smoothstep(line_width , 0.0, dist);
	
	// Unir las líneas horizontal y vertical
	float alpha = max(lines.x, lines.y);
	
	COLOR = vec4(grid_color.rgb, grid_color.a * alpha);
}