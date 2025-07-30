shader_type canvas_item;
render_mode blend_mix;

uniform vec2 cell_size = vec2(16.0, 16.0);        // Tamaño de cada celda en píxeles
uniform vec2 node_size = vec2(256.0, 256.0);       // Tamaño del nodo (pasado por script)
uniform float line_width : hint_range(0.0, 1.0) = 0.05;
uniform float center_line_width_factor : hint_range(1.0, 10.0) = 2.0; // Factor multiplicador

uniform vec2 offset = vec2(0.0, 0.0);             // Desplazamiento del grid en píxeles

uniform vec4 center_line_color : source_color = vec4(1.0, 0.0, 0.0, 0.8); // Color de la línea central
uniform vec4 grid_color : source_color = vec4(1.0, 1.0, 1.0, 0.2);        // Color del resto del grid

float smoothstep_custom(float edge0, float edge1, float x) {
    float t = clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0);
    return t * t * (3.0 - 2.0 * t);
}

void fragment() {
    vec2 pixel_pos = UV * node_size + offset;

    vec2 grid_pos = pixel_pos / cell_size;
    vec2 cell_pos = fract(grid_pos);
    vec2 dist = min(cell_pos, 1.0 - cell_pos);

    float base_width = line_width * 0.5;

    // Detectar si estamos cerca del centro (en píxeles)
    vec2 half_grid = node_size * 0.5 + offset;
    float center_x_dist = abs(pixel_pos.x - half_grid.x);
    float center_y_dist = abs(pixel_pos.y - half_grid.y);

    float center_tolerance = max(1.0, line_width * cell_size.x * center_line_width_factor); // en píxeles

    bool is_center_v = center_x_dist < center_tolerance * 0.5;
    bool is_center_h = center_y_dist < center_tolerance * 0.5;

    float fx = smoothstep_custom(0.0, base_width, dist.x);
    float fy = smoothstep_custom(0.0, base_width, dist.y);
    float alpha = 1.0 - min(fx, fy);

    vec4 base_color = (is_center_v || is_center_h) ? center_line_color : COLOR;

    // Multiplica la alpha si está en línea central (más gruesa visualmente)
    if (is_center_v || is_center_h) {
        float cx = smoothstep_custom(0.0, base_width * center_line_width_factor, dist.x);
        float cy = smoothstep_custom(0.0, base_width * center_line_width_factor, dist.y);
        alpha = 1.0 - min(cx, cy);
    }

    COLOR = vec4(base_color.rgb, base_color.a * alpha);
}
