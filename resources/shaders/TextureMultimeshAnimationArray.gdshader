shader_type spatial;
render_mode unshaded;
render_mode cull_back, depth_draw_opaque, diffuse_burley, specular_schlick_ggx;

uniform bool debug_mode = false;
uniform sampler2DArray mtexture_array : filter_nearest, source_color;

// 🔹 TEXTURAS DE DATOS Y CONFIGURACIÓN GLOBAL (LUT)
uniform sampler2D transform_texture : hint_default_white, filter_nearest;   // Slot 0: R=X, G=Y, B=Z, A=Escala
uniform sampler2D anim_state_texture : hint_default_white, filter_nearest; // Slot 3: R=IndiceLUT, G=StartTime, B=Libre, A=Libre
uniform sampler2D flags_texture : hint_default_white, filter_nearest;       // Slot 1: R=MirrorX, G=MirrorY, B=IsVisible, A=LayerTexture
uniform sampler2D anim_config_texture : hint_default_white, filter_nearest;// Tabla LUT global de animaciones
uniform int texture_width = 256;
global uniform float game_time;

// Variables interpoladas hacia el fragment shader
varying float layerTexture;
varying vec3 instance_color;
varying float instance_alpha;
varying vec2 frame_offset;
varying vec2 frame_size;
varying float mirrorX;
varying float mirrorY;

vec2 get_aspect_ratio(vec2 texture_size) {
    float base_size = 16.0;
    return texture_size / base_size;
}

void vertex() {
    instance_color = vec3(1,1,1); 
    instance_alpha = 1.0f;

    // Obtener coordenadas del píxel usando INSTANCE_ID
    int id = INSTANCE_ID;
    ivec2 coord = ivec2(id % texture_width, id / texture_width);

    // 1. Leer Transformaciones (Posición 3D y Escala base)
    vec4 transform_data = texelFetch(transform_texture, coord, 0);
    vec3 world_pos = transform_data.xyz;
    float unit_scale = transform_data.a;

    // 2. Leer Estado de Animación de la unidad
    vec4 anim_state = texelFetch(anim_state_texture, coord, 0);
    int indice_global_lut = int(anim_state.r); // ID directo del clip registrado en la LUT
    float start_time      = anim_state.g;      // TIME exacto en que inició la animación

    // 3. Leer Banderas de Control y Capa desde C#
    vec4 flags_data = texelFetch(flags_texture, coord, 0);
    mirrorX = flags_data.r;             // 1.0 o 0.0
    mirrorY = flags_data.g;             // 1.0 o 0.0
    float is_visible = flags_data.b;    // 1.0 o 0.0
    layerTexture = flags_data.a;        // Capa de la textura en el Texture2DArray

    // 🔹 CULLING MATEMÁTICO: Si is_visible es 0.0, la escala se anula a 0 (cero ifs)
    float final_scale = unit_scale * is_visible;

    // 4. CONSULTAR LA TABLA DE CONFIGURACIÓN GLOBAL (LUT)
    vec4 config_pos  = texelFetch(anim_config_texture, ivec2(indice_global_lut * 2, 0), 0);
    vec4 config_size = texelFetch(anim_config_texture, ivec2(indice_global_lut * 2 + 1, 0), 0);

    float atlas_start_x = config_pos.r;
    float atlas_start_y = config_pos.g;
    float is_loop       = config_pos.b; // 1.0 para loop, 0.0 para clamp

    // Detectar espejos mediante el signo (si ya lo manejas así)
    mirrorX = max(-sign(config_size.r), 0.0);
    mirrorY = max(-sign(config_size.g), 0.0);

    float frame_w = abs(config_size.r);
    float frame_h = abs(config_size.g);
    
    float total_frames   = config_size.b;
    float frame_duration = config_size.a; // <--- Ahora A representa la duración de cada frame en segundos (ej: 0.1f)

    // 5. CÁLCULO DISCRETO DEL FRAME ACTUAL (Estilo tu lógica C# con Cero "If"s)
    float tiempo_transcurrido = game_time - start_time;
    
    // Evitamos división por cero por seguridad
    float safe_duration = max(frame_duration, 0.0001);
    
    // Obtenemos el índice del frame entero usando floor
    float raw_frame = floor(tiempo_transcurrido / safe_duration);

    // Opción A: Reproducción continua (Loop)
    float frame_loop = mod(raw_frame, total_frames);
    
    // Opción B: Reproducción retenida en el último cuadro (Clamp)
    float frame_clamp = clamp(raw_frame, 0.0, total_frames - 1.0);
    
    // Selección matemática por hardware usando mix() según is_loop
    float current_frame = mix(frame_clamp, frame_loop, is_loop);
    current_frame = min(current_frame, total_frames - 1.0);

    // (Opcional para Clamp): Si quieres que al terminar un loop=0 la animación se quede en el frame 0 
    // en lugar del último, puedes manejarlo en C# reiniciando el 'start_time' en anim_state_texture
    // cuando la animación expire.
	
    // 6. COORDENADAS FINALES EN EL ATLAS MASIVO (4096x4096)
    float x = atlas_start_x + (current_frame * frame_w);
    float y = atlas_start_y;

    float atlas_width  = 4096.0;
    float atlas_height = 4096.0;
    frame_offset = vec2(x / atlas_width, y / atlas_height);
    frame_size   = vec2(frame_w / atlas_width, frame_h / atlas_height);

    // Escalar y posicionar en el mundo 3D
    vec2 scale = get_aspect_ratio(vec2(frame_w, frame_h));
    VERTEX.xy *= scale * final_scale;
    VERTEX.xyz += world_pos;
}

void fragment() {

    vec2 uv = UV;
    
    // 🔹 ESPEJO MATEMÁTICO POR HARDWARE (Cero condicionales "if")
    uv = mix(uv, 1.0 - uv, vec2(mirrorX, mirrorY));

    // Anti-bleeding
    vec2 texCoord = uv * (frame_size * 0.98) + (frame_offset + frame_size * 0.01);

    vec3 texCoord3D = vec3(texCoord, layerTexture);
    vec4 texColor = texture(mtexture_array, texCoord3D);

    ALPHA_SCISSOR_THRESHOLD = 0.5;
    ALBEDO = texColor.rgb * instance_color; // Tinte RGB por instancia
    ALPHA = texColor.a * instance_alpha;    // Transparencia limpia
}