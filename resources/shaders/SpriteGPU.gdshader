shader_type spatial;

uniform sampler2D main_texture;

uniform float transparency : hint_range(0.0, 1.0) = 0.0;
varying float x;
varying float y;
varying float ancho;
varying float alto;

void vertex() {
 	x= INSTANCE_CUSTOM.r;
	y= INSTANCE_CUSTOM.g;
	ancho = INSTANCE_CUSTOM.b;
	alto = INSTANCE_CUSTOM.a;
	  // Matriz de escala
}
void fragment() {
	vec2 camera_position = SCREEN_UV;
    if (camera_position.x >= 0.0 && camera_position.x <= 1.0 && camera_position.y >= 0.0 && camera_position.y <= 1.0) {
  	vec2 frameSize = vec2(ancho, alto);
    vec2 frameOffset = vec2(x,y);
    vec2 texCoord = UV * frameSize + frameOffset;
    vec4 texColor = texture(main_texture, texCoord);
    // Aplica el color del segmento al resultado final
	texColor.a *= 1.0 - transparency;
	ALPHA_SCISSOR_THRESHOLD = 0.5;

    ALBEDO = texColor.rgb * texColor.rgb;
    ALPHA = texColor.a ;
	}
	else
	{
 		// El objeto está fuera de la pantalla, no ejecutes el shader
        discard;
	}
}