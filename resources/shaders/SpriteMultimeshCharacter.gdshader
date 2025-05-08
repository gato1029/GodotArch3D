shader_type spatial;
render_mode unshaded;

uniform sampler2D body_texture;      // Textura del cuerpo principal
uniform sampler2D weapon_texture;    // Textura del arma
uniform sampler2DArray mtexture_array; // Array de texturas de accesorios

uniform float transparency : hint_range(0.0, 1.0) = 0.0;
uniform float ancho; // Número de columnas en el atlas
uniform float alto;  // Número de filas en el atlas
uniform int layers; // Cantidad de capas en el array de texturas

varying float pos;
varying float invert;
varying float render_weapon; // Ahora usamos esto en lugar de `oclusion`

void vertex() {
    pos = INSTANCE_CUSTOM.r;
    invert = INSTANCE_CUSTOM.g;
    render_weapon = INSTANCE_CUSTOM.a; // 1.0 = renderizar arma, 0.0 = no renderizar
}

void fragment() {
    if (pos == -1.0) {
        discard;
    }

    // Tamaño de cada segmento en la textura
    vec2 segmentSize = vec2(1.0 / ancho, 1.0 / alto);

    // Obtener la posición del segmento en la grilla
    int xIndex = int(mod(float(pos), ancho));
    int yIndex = int(float(pos) / ancho);
    vec2 segmentOffset = vec2(float(xIndex) * segmentSize.x, float(yIndex) * segmentSize.y);

    // Ajustar las coordenadas UV
    vec2 uv = UV;
    if (invert == 1.0) {
        uv.x = 1.0 - uv.x;
    }
    if (invert == 2.0) {
        uv.y = 1.0 - uv.y;
    }
    vec2 texCoord = uv * segmentSize + segmentOffset;

    // Obtener el color del cuerpo principal
    vec4 bodyColor = texture(body_texture, UV);

    // Mezclar todas las capas del array de texturas de accesorios
    vec4 finalColor = bodyColor; // Iniciamos con el color del cuerpo

    for (int i = 0; i < layers; i++) {
        vec4 texColor = texture(mtexture_array, vec3(texCoord, float(i)));

        // Mezclar accesorios respetando transparencia
        finalColor.rgb += texColor.rgb * texColor.a;
        finalColor.a = max(finalColor.a, texColor.a);
    }

    // Si `render_weapon` es 1.0, renderiza el arma
    if (render_weapon == 1.0) {
        vec4 weaponColor = texture(weapon_texture, UV);
        finalColor.rgb = mix(finalColor.rgb, weaponColor.rgb, weaponColor.a);
        finalColor.a = max(finalColor.a, weaponColor.a);
    }

    // Aplicar transparencia general
    finalColor.a *= 1.0 - transparency;
    ALPHA_SCISSOR_THRESHOLD = 0.5;

    ALBEDO = finalColor.rgb;
    ALPHA = finalColor.a;
}
