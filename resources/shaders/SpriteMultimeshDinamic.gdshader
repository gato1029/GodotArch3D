shader_type spatial;
render_mode unshaded;

uniform sampler2D main_texture;
uniform float atlas_width;
uniform float atlas_height;

uniform float transparency : hint_range(0.0, 1.0) = 0.0;
varying float x;
varying float y;
varying float anchoSubTexture;
varying float altoSubTexture;

vec2 get_aspect_ratio(vec2 texture_size) {
    float base_size = 32.0; // Tamanio base de referencia
    return texture_size / base_size;
}

void vertex() {
 	x= INSTANCE_CUSTOM.r;
	y= INSTANCE_CUSTOM.g;
	anchoSubTexture = INSTANCE_CUSTOM.b;
	altoSubTexture = INSTANCE_CUSTOM.a;	
	vec2 scale = get_aspect_ratio(vec2(anchoSubTexture,altoSubTexture));
    VERTEX.xy *= scale;
}

void fragment() {
	if (x == -1.0 )
	{
		discard;
	}
	vec2 camera_position = SCREEN_UV;
    if (camera_position.x >= 0.0 && camera_position.x <= 1.0 && camera_position.y >= 0.0 && camera_position.y <= 1.0) {
 	vec2 frameOffset = vec2(x / atlas_width, y / atlas_height);
    vec2 frameSize = vec2(anchoSubTexture / atlas_width, altoSubTexture / atlas_height);

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