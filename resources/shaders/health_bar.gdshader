shader_type spatial;
render_mode unshaded, cull_disabled, depth_test_disabled;

uniform vec4 bg_color : source_color = vec4(0.1, 0.1, 0.1, 1.0);       // Color de fondo (gris oscuro)
uniform vec4 normal_color : source_color = vec4(0.0, 1.0, 0.0, 1.0);   // Color normal (Verde)
uniform vec4 low_color : source_color = vec4(1.0, 0.0, 0.0, 1.0);       // Color crítico < 30% (Rojo)
uniform float low_threshold : hint_range(0.0, 1.0) = 0.3;              // Umbral del 30%

// Declaramos los varyings para pasar los datos por instancia desde vertex hacia fragment
varying float health_percent;
varying float is_visible;

void vertex()
{
    // Extraemos los datos personalizados desde INSTANCE_CUSTOM por cada instancia del MultiMesh
    health_percent = INSTANCE_CUSTOM.r;
    is_visible = INSTANCE_CUSTOM.g;
}

void fragment() {
    // 1. Verificamos el flag de visibilidad
    if (is_visible < 0.5) {
        discard; // Oculta la barra descartando el píxel
    }

    // 2. Pintamos la barra usando el health_percent que vino desde vertex()
    if (UV.x < health_percent) {
        // Evaluamos si la vida es baja para cambiar el color
        if (health_percent <= low_threshold) {
            ALBEDO = low_color.rgb;   // Color crítico (Rojo)
        } else {
            ALBEDO = normal_color.rgb; // Color normal (Verde)
        }
    } else {
        // Parte vacía de la barra
        ALBEDO = bg_color.rgb;
    }
}