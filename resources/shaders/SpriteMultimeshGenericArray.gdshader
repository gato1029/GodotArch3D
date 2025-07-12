shader_type spatial;
render_mode unshaded;
render_mode cull_back, depth_draw_opaque;
uniform sampler2DArray mtexture_array :  filter_nearest, source_color;

uniform float transparency : hint_range(0.0, 1.0) = 0.0;

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
	if (x == -1.0) {
		discard; // Invisible marker
	}

	// Adjust UV if mirrored
	vec2 uv = UV;
	if (mirrorX > 0.0) uv.x = 1.0 - uv.x;
	if (mirrorY > 0.0) uv.y = 1.0 - uv.y;

	// Frustum discard (optional, for optimization)
	vec2 camera_position = SCREEN_UV;
	if (camera_position.x < 0.0 || camera_position.x > 1.0 ||
		camera_position.y < 0.0 || camera_position.y > 1.0) {
		discard;
	}

	// Get atlas texture size from sampler
	ivec3 tex_size = textureSize(mtexture_array, 0);
	float atlas_width  = float(tex_size.x);
	float atlas_height = float(tex_size.y);

	// Compute subtexture region in atlas
	vec2 frameOffset = vec2(x / atlas_width, y / atlas_height);
	vec2 frameSize   = vec2(anchoSubTexture / atlas_width, altoSubTexture / atlas_height);

	// Slight shrink to avoid bleeding
	vec2 texCoord = uv * (frameSize * 0.98) + (frameOffset + frameSize * 0.01);

	// Sample from the correct layer
	float layer = floor(layerTexture + 0.5); // ensure integer index
	vec3 texCoord3D = vec3(texCoord, layer);
	vec4 texColor = texture(mtexture_array, texCoord3D);

	// Output final color
	ALPHA_SCISSOR_THRESHOLD = 0.5;
	ALBEDO = texColor.rgb;
	ALPHA = texColor.a;
}
