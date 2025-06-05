shader_type spatial;
render_mode unshaded;

uniform sampler2D main_texture: filter_nearest;
uniform float atlas_width;
uniform float atlas_height;

uniform float transparency : hint_range(0.0, 1.0) = 0.0;
varying float x;
varying float y;
varying float anchoSubTexture;
varying float altoSubTexture;

vec2 get_aspect_ratio(vec2 texture_size) {
	float base_size = 16.0; // Tamanio base de referencia
    return (texture_size / base_size);
}
varying float mirrorX;
varying float mirrorY;
void vertex() {
 	x= INSTANCE_CUSTOM.r;
	y= INSTANCE_CUSTOM.g;
	anchoSubTexture = INSTANCE_CUSTOM.b;
	altoSubTexture = INSTANCE_CUSTOM.a;
	mirrorX = 0.0;
	mirrorY = 0.0;
	if (anchoSubTexture<0.0)
	{
 		mirrorX = 1.0;
		anchoSubTexture = anchoSubTexture *(-1.0);
	}
	if (altoSubTexture<0.0)
	{
		mirrorY = 1.0;
		altoSubTexture = altoSubTexture *(-1.0);
	}

	vec2 scale = get_aspect_ratio(vec2(anchoSubTexture,altoSubTexture));
    VERTEX.xy *= scale;
}

void fragment() {

	if (x == -1.0 )
	{
		discard;
	}
	vec2 uv = UV;
	if (mirrorX<0.0)
	{
 		uv.x = 1.0 - uv.x;
	}
	if (mirrorY<0.0)
	{
 		 uv.y = 1.0 - uv.y;
	}
	vec2 camera_position = SCREEN_UV;
    if (camera_position.x >= 0.0 && camera_position.x <= 1.0 && camera_position.y >= 0.0 && camera_position.y <= 1.0) {
 	vec2 frameOffset = vec2(x / atlas_width, y / atlas_height);
    vec2 frameSize = vec2(anchoSubTexture / atlas_width, altoSubTexture / atlas_height);


	vec2 texCoord = uv * (frameSize * 0.98) + (frameOffset + frameSize * 0.01);
    //vec2 texCoord = UV * frameSize + frameOffset;
    vec4 texColor = texture(main_texture, texCoord);
    // Aplica el color del segmento al resultado final
	texColor.a *= 1.0 - transparency;
	ALPHA_SCISSOR_THRESHOLD = 0.5;

    ALBEDO = texColor.rgb * texColor.rgb * texColor.rgb;
    ALPHA = texColor.a ;
	}
	else
	{
 		// El objeto está fuera de la pantalla, no ejecutes el shader
        discard;
	}
}