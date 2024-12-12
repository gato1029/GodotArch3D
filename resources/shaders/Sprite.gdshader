shader_type spatial;
render_mode unshaded;

uniform sampler2D mtexture: filter_nearest;
uniform float transparency : hint_range(0.0, 1.0) = 0.0;
uniform int pos; // Posición en la grilla como un solo número entero
uniform float ancho; // Número total de segmentos en el ancho de la textura
uniform float alto;  // Número total de segmentos en el alto de la textura

void fragment() {
	vec2 camera_position = SCREEN_UV;
    if (camera_position.x >= 0.0 && camera_position.x <= 1.0 && camera_position.y >= 0.0 && camera_position.y <= 1.0) {
	    // Calcular el tamaño de un segmento de la textura
	    vec2 segmentSize = vec2(1.0 / ancho, 1.0 / alto);

	    // Convertir el entero `pos` a coordenadas de grilla
	    int xIndex = int(mod(float(pos), ancho)); // Índice X en la grilla
	    int yIndex = int(float (pos) / ancho); // Índice Y en la grilla

	    // Calcular el desplazamiento del segmento usando los índices
	    vec2 segmentOffset = vec2(float(xIndex) * segmentSize.x, float(yIndex) * segmentSize.y);

	    // Calcular las coordenadas de textura ajustadas
	    vec2 texCoord = UV * segmentSize + segmentOffset;

	    // Obtener el color del segmento de la textura
	    vec4 texColor = texture(mtexture, texCoord);

	    // Aplica el color del segmento al resultado final
	    texColor.a *= 1.0 - transparency;
	    ALPHA_SCISSOR_THRESHOLD = 0.5;

	    ALBEDO = texColor.rgb * texColor.rgb;
	    ALPHA = texColor.a;
	}
	else
	{
	discard;
	}
}
