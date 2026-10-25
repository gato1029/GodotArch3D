shader_type spatial;
render_mode unshaded, cull_disabled, depth_test_disabled;

// Colores estilo Retro/Pixel Art (más contrastados y vibrantes)
uniform vec4 bg_color : source_color = vec4(0.15, 0.15, 0.22, 1.0);     // Fondo oscuro retro
uniform vec4 normal_color : source_color = vec4(0.22, 0.82, 0.38, 1.0); // Verde clásico de videojuego
uniform vec4 low_color : source_color = vec4(0.95, 0.26, 0.21, 1.0);    // Rojo intenso estilo arcade
uniform float low_threshold : hint_range(0.0, 1.0) = 0.3;               // Umbral del 30%

// Resolución interna de los "píxeles" de la barra (ej: 16x4 o 32x8 píxeles)
uniform vec2 pixel_grid_size = vec2(64.0, 16.0);

// Varyings para pasar los datos por instancia
varying float health_percent;
varying float is_visible;

void vertex() {
    health_percent = INSTANCE_CUSTOM.r;
    is_visible = INSTANCE_CUSTOM.g;
}

void fragment() {
    // 1. Verificamos el flag de visibilidad
    if (is_visible < 0.5) {
        discard;
    }

    // 2. Cuantizamos las UVs a una cuadrícula de píxeles para dar el efecto retro
    vec2 pixel_uv = floor(UV * pixel_grid_size) / pixel_grid_size;
    
    // Coordenadas centradas para calcular esquinas estilo pixel art
    vec2 centered = pixel_uv - vec2(0.5);
    
    // Opcional: Recorte en las esquinas para hacerlas "pixeladas" (como un chaflán/corte de 1 píxel)
    // Si quieres esquinas completamente cuadradas/rectas, puedes comentar o borrar este bloque 'if'.
    if ((abs(centered.x) > 0.45 && abs(centered.y) > 0.35) || 
        (abs(centered.y) > 0.40 && abs(centered.x) > 0.40)) {
        discard; // Recorta las esquinas de forma escalonada (pixel art)
    }

    // 3. Pintamos la barra usando la coordenada X pixelada
    vec3 final_color;
    // Usamos pixel_uv.x en lugar de UV.x para que el llenado también avance "por bloques" de píxeles
    if (pixel_uv.x < health_percent) {
        if (health_percent <= low_threshold) {
            final_color = low_color.rgb;   // Rojo arcade
        } else {
            final_color = normal_color.rgb; // Verde clásico
        }
    } else {
        final_color = bg_color.rgb;     // Fondo de la barra vacía
    }

    ALBEDO = final_color;
}