shader_type spatial;
render_mode unshaded;
render_mode cull_back, depth_draw_opaque, diffuse_burley, specular_schlick_ggx;

uniform bool debug_mode = false;
uniform sampler2DArray mtexture_array : filter_nearest, source_color;

// 🔹 LAS 3 TEXTURAS DE DATOS DESDE C#
uniform sampler2D transform_texture : hint_default_white, filter_nearest;    // R=X, G=Y, B=Z, A=Escala
uniform sampler2D static_data_texture : hint_default_white, filter_nearest;  // R=AtlasX, G=AtlasY, B=Width, A=Height
uniform sampler2D flags_texture : hint_default_white, filter_nearest;        // R=MirrorX, G=MirrorY, B=IsVisible, A=LayerTexture
uniform int texture_width = 256;

// Variables interpoladas hacia el fragment (limpias y precalculadas)
varying float layerTexture;
varying vec3 instance_color;
varying float instance_alpha;
varying vec2 frame_offset;
varying vec2 frame_size;
varying float mirrorX;
varying float mirrorY;
varying float vis;
vec2 get_aspect_ratio(vec2 texture_size) {
    float base_size = 16.0;
    return texture_size / base_size;
}

void vertex() {
   // 1. Color y transparencia directos desde el canal de vértices de Godot
    instance_color = vec3(1,1,1); 
    instance_alpha = 1.0f;

    // Obtener coordenadas del píxel usando INSTANCE_ID
    int id = INSTANCE_ID;
    ivec2 coord = ivec2(id % texture_width, id / texture_width);

    // 2. Leer Transformaciones (Posición 3D y Escala base)
    vec4 transform_data = texelFetch(transform_texture, coord, 0);
    vec3 world_pos =  transform_data.xyz;
    float unit_scale = transform_data.a;

    // 3. Leer Datos Geométricos del Atlas
    vec4 static_data = texelFetch(static_data_texture, coord, 0);
    float x = static_data.r;                  
    float y = static_data.g;                  
    float anchoSubTexture = static_data.b;    
    float altoSubTexture = static_data.a;     

    // 4. Leer Banderas de Control y Capa desde la textura de flags
    vec4 flags_data = texelFetch(flags_texture, coord, 0);
    mirrorX = flags_data.r;             // 1.0 o 0.0
    mirrorY = flags_data.g;             // 1.0 o 0.0
    float is_visible = flags_data.b;    // 1.0 o 0.0
    layerTexture = flags_data.a;        // Capa de la textura en el Texture2DArray

    // Escala final colapsada a 0 si no es visible (culling matemático sin ifs)
    float final_scale =unit_scale * is_visible;
	vis = is_visible;
    // 5. Precalcular geometría del Atlas de una vez en el Vertex Shader
    float atlas_width  = 4096.0;
    float atlas_height = 4096.0;
    frame_offset = vec2(x / atlas_width, y / atlas_height);
    frame_size   = vec2(anchoSubTexture / atlas_width, altoSubTexture / atlas_height);

    // Escalar y posicionar en el mundo 3D
    vec2 scale = get_aspect_ratio(vec2(anchoSubTexture, altoSubTexture));
	VERTEX.xy *= scale * final_scale;
    VERTEX.xyz += world_pos;
}

void fragment() {
	

    vec2 uv = UV;
    
    // 🔹 ESPEJO MATEMÁTICO SIN IFS (Usa interpolación lineal por hardware)
    uv = mix(uv, 1.0 - uv, vec2(mirrorX, mirrorY));

    // Anti-bleeding (evitar filtración de bordes en el atlas)
    vec2 texCoord = uv * (frame_size * 0.98) + (frame_offset + frame_size * 0.01);

    vec3 texCoord3D = vec3(texCoord, layerTexture);
    vec4 texColor = texture(mtexture_array, texCoord3D);

    ALPHA_SCISSOR_THRESHOLD = 0.5;
    ALBEDO = texColor.rgb * instance_color; // 🔹 Tinte de color libre por instancia
    ALPHA = texColor.a * instance_alpha;    // 🔹 Transparencia limpia
}