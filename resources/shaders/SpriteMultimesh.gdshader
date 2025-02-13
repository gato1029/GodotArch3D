shader_type spatial;
render_mode unshaded;

uniform sampler2D mtexture: filter_nearest;
uniform float transparency : hint_range(0.0, 1.0) = 0.0;

uniform float ancho; // Número total de segmentos en el ancho de la textura
uniform float alto;  // Número total de segmentos en el alto de la textura

varying float pos;
varying float invert;
varying float oclusion;
void vertex() {
 	pos= INSTANCE_CUSTOM.r;
	invert = INSTANCE_CUSTOM.g;
	oclusion = INSTANCE_CUSTOM.a;
}
void fragment() {
	if (pos == -1.0 )
	{
		discard;
	}
	if (oclusion == -1.0 )
	{
		discard;
	}
	vec2 camera_position = SCREEN_UV;
    //if (camera_position.x >= 0.0 && camera_position.x <= 1.0 && camera_position.y >= 0.0 && camera_position.y <= 1.0) {
	    // Calcular el tamaño de un segmento de la textura
	    vec2 segmentSize = vec2(1.0 / ancho, 1.0 / alto);

	    // Convertir el entero `pos` a coordenadas de grilla
	    int xIndex = int(mod(float(pos), ancho)); // Índice X en la grilla
	    int yIndex = int(float (pos) / ancho); // Índice Y en la grilla

	    // Calcular el desplazamiento del segmento usando los índices
	    vec2 segmentOffset = vec2(float(xIndex) * segmentSize.x, float(yIndex) * segmentSize.y);

	    // Calcular las coordenadas de textura ajustadas

	    vec2 uv = UV;
	    if (invert==1.0) {
	        uv.x = 1.0 - uv.x;
	    }
	    if (invert==2.0) {
	        uv.y = 1.0 - uv.y;
	    }
	    vec2 texCoord = uv * segmentSize + segmentOffset;

	    // Obtener el color del segmento de la textura
	    vec4 texColor = texture(mtexture, texCoord);

	    // Aplica el color del segmento al resultado final
	    texColor.a *= 1.0 - transparency;
	    ALPHA_SCISSOR_THRESHOLD = 0.5;

	    ALBEDO = texColor.rgb * texColor.rgb;
	    ALPHA = texColor.a;
	/*}
	else
	{
	discard;
	}*/
}
