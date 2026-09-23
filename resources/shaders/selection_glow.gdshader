shader_type spatial;
render_mode unshaded, blend_add, cull_disabled;

uniform vec4 line_color : source_color = vec4(0.0, 1.0, 0.5, 1.0);
uniform float speed = 2.0;
uniform float frequency = 15.0;
uniform float sketchy_intensity = 0.05;
uniform float line_thickness : hint_range(0.1, 2.0) = 0.6; // 🔹 Control de ancho de la línea (0.1 = muy delgado, 1.0 = ocupa todo el grosor del quad)

void fragment() {
    // Efecto de temblor / dibujado a mano usando ruido temporal
    float noise = sin(TIME * 12.0 + UV.x * 30.0) * sketchy_intensity;
    
    // Patrón de puntos a lo largo de la barra
    float pattern = sin((UV.x + noise - TIME * speed) * frequency);
    pattern = step(0.2, pattern); 

    // Máscara basada en el ancho (UV.y va de 0 a 1 de un extremo al otro del grosor del quad)
    float edge = abs(UV.y - 0.5) * 2.0;
    float thickness_mask = 1.0 - smoothstep(line_thickness * 0.5, line_thickness * 0.5 + 0.05, edge);

    ALBEDO = line_color.rgb;
    ALPHA = line_color.a * pattern * thickness_mask;
}