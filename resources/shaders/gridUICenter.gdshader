shader_type canvas_item;
render_mode blend_mix;

uniform vec2 size = vec2(1280.0, 720.0);
uniform float line_width : hint_range(0, 1) = 0.1;
uniform vec2 offset = vec2(0.0, 0.0);
uniform float grid_size = 16.0;
uniform vec4 grid_color : source_color = vec4(1.0, 1.0, 1.0, 0.2); // Blanco con transparencia

void fragment() {
    vec2 uv = (offset / size) + UV;
    vec2 r = mod(size * uv, vec2(grid_size));
    vec2 lines = smoothstep(1.0 - line_width, 1.0, r / vec2(grid_size));
    float alpha = dot(lines, vec2(1.0, 1.0));

    vec2 center = vec2(0.5, 0.5); // Centro de la pantalla en coordenadas UV
    float point_size = 0.01; // Tamaño del punto rojo
    float dist = distance(UV, center);
    float point = smoothstep(point_size * 0.8, point_size, dist);

    vec4 point_color = vec4(1.0, 0.0, 0.0, 1.0); // Rojo

    COLOR = mix(vec4(grid_color.rgb, grid_color.a * alpha), point_color, point);
}
