shader_type spatial;

//render_mode unshaded;
//render_mode cull_back, depth_draw_opaque;

render_mode cull_back, depth_draw_opaque, diffuse_burley, specular_schlick_ggx;
// 🔹 Modo de render simple sin reflejos, pero con sombras
//render_mode diffuse_lambert, cull_back, depth_draw_opaque;

// ACTIVADOR DEL DEBUG
uniform bool debug_mode = false;
uniform sampler2DArray mtexture_array :  filter_nearest, source_color;

uniform float transparency : hint_range(0.0, 1.0) = 0.0;
// ---- NUEVO: flag visible desde INSTANCE_CUSTOM.a ----
varying float visible_flag;
varying float visible_flag2;
// Per-instance values
varying float x;                 // Sub-texture X origin in pixels
varying float y;                 // Sub-texture Y origin in pixels
varying float anchoSubTexture;  // Sub-texture width  in pixels
varying float altoSubTexture;   // Sub-texture height in pixels
varying float layerTexture;     // Texture layer index (as float)
varying float mirrorX;
varying float mirrorY;

// Ratio to scale the quad (based on base tile size)
vec2 get_aspect_ratio(vec2 texture_size) {
	float base_size = 16.0; // Adjust to your tile size
	return texture_size / base_size;
}

void vertex() {
	// Read per-instance data
	layerTexture = COLOR.a;

	x = INSTANCE_CUSTOM.r;
	y = INSTANCE_CUSTOM.g;
	anchoSubTexture = INSTANCE_CUSTOM.b;
	altoSubTexture = INSTANCE_CUSTOM.a;

	// visible flag (1 = visible, 0 = invisible)
	visible_flag = INSTANCE_CUSTOM.b; // PUEDES CAMBIARLO A .w SI USAS 4 CANALES
	visible_flag2 = INSTANCE_CUSTOM.a;
	
	mirrorX = 0.0;
	mirrorY = 0.0;

	// Handle mirroring (negative dimensions)
	if (anchoSubTexture < 0.0) {
		mirrorX = 1.0;
		anchoSubTexture *= -1.0;
	}
	if (altoSubTexture < 0.0) {
		mirrorY = 1.0;
		altoSubTexture *= -1.0;
	}

	// Scale quad to match subtexture aspect ratio
	vec2 scale = get_aspect_ratio(vec2(anchoSubTexture, altoSubTexture));
	VERTEX.xy *= scale;
}

void fragment() {
	
	// --------------------------------------
	// 🔴 DEBUG 1: INSTANCIA INVISIBLE (visible_flag == 0)
	// --------------------------------------
	if (debug_mode && visible_flag <= 0.01f || visible_flag2<=0.01f) {
		// Color rojo intenso para detectar corrupción
		ALBEDO = vec3(1.0, 0.1, 0.1);
		ALPHA = 0.8;
		
	}
	
	if (anchoSubTexture == 0.0) {
		if (debug_mode) {
			ALBEDO = vec3(1.0, 0.0, 1.0); // magenta: ancho=0 → inconsistencia
			ALPHA = 1.0;		
		}else
		{
		discard; // Invisible marker	
		}		
	}

	// Adjust UV if mirrored
	vec2 uv = UV;
	if (mirrorX > 0.0) uv.x = 1.0 - uv.x;
	if (mirrorY > 0.0) uv.y = 1.0 - uv.y;

	// Frustum discard (optional, for optimization)
	//vec2 camera_position = SCREEN_UV;
	//if (camera_position.x < 0.0 || camera_position.x > 1.0 ||
		//camera_position.y < 0.0 || camera_position.y > 1.0) {
		//discard;
	//}

	// Get atlas texture size from sampler
	ivec3 tex_size = textureSize(mtexture_array, 0);
	float atlas_width  = 4096.0; //float(tex_size.x);
	float atlas_height = 4096.0;//float(tex_size.y);

	// Compute subtexture region in atlas
	vec2 frameOffset = vec2(x / atlas_width, y / atlas_height);
	vec2 frameSize   = vec2(anchoSubTexture / atlas_width, altoSubTexture / atlas_height);

	// Slight shrink to avoid bleeding
	vec2 texCoord = uv * (frameSize * 0.98) + (frameOffset + frameSize * 0.01);

	// Sample from the correct layer
	float layer = floor(layerTexture); // +0.5 ensure integer index
	vec3 texCoord3D = vec3(texCoord, layer);
	vec4 texColor = texture(mtexture_array, texCoord3D);

	// Output final color
	ALPHA_SCISSOR_THRESHOLD = 0.5;
	ALBEDO = texColor.rgb;
	ALPHA = texColor.a;
	// =====================================================
    //  DEBUG 2: Borde amarillo para visibles
    // =====================================================
    if (debug_mode) {
        float edge = max(abs(UV.x - 0.5), abs(UV.y - 0.5));
        if (edge > 0.45) {
            ALBEDO = vec3(1.0, 1.0, 0.0); // yellow border
        }
    }
}
