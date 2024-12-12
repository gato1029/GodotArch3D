shader_type spatial;
render_mode unshaded;


uniform sampler2D mtexture:  filter_nearest;
uniform float transparency : hint_range(0.0, 1.0) = 0.0;
uniform int invert_horizontal : hint_range(0, 1)=0;
uniform int invert_vertical : hint_range(0, 1)=0;

//uniform sampler2D mtextures[10]:  filter_nearest;
//uniform int index;

void fragment() {
	vec2 camera_position = SCREEN_UV;
    if (camera_position.x >= 0.0 && camera_position.x <= 1.0 && camera_position.y >= 0.0 && camera_position.y <= 1.0) {

		    vec2 uv = UV;
    if (invert_horizontal==1) {
        uv.x = 1.0 - uv.x;
    }
    if (invert_vertical==1) {
        uv.y = 1.0 - uv.y;
    }

	    vec2 texCoord = uv;
	    vec4 texColor = textureLod(mtexture, texCoord,1.0);
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
