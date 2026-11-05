using System.Collections.Generic;
using Godot;

namespace GodotEcsArch.sources.managers.GpuDriven;
/// <summary>
/// Representa la configuración estática de un clip de animación dentro del atlas de imágenes.
/// </summary>
public struct AnimClipDefinition
{
    public float StartX;      // Posición X inicial en el Atlas (en píxeles)
    public float StartY;      // Posición Y inicial en el Atlas (en píxeles)
    public float FrameWidth;  // Ancho de cada cuadro individual (en píxeles)
    public float FrameHeight; // Alto de cada cuadro individual (en píxeles)
    public float TotalFrames; // Cantidad total de cuadros que componen la animación
    public float Speed;       // Velocidad de reproducción (cuadros por segundo)
    public bool IsLoop;       // true = Repetición infinita, false = Se detiene en el último frame
}
/// <summary>
/// Gestiona la tabla de consulta global (LUT) en VRAM utilizada por el shader animado.
/// Mapea la configuración de cada animación a píxeles dentro de una mini-textura Rgbaf.
/// </summary>
public class AnimationLUTManager
{
    private readonly Dictionary<int, AnimClipDefinition> _definitions = new();
    private ImageTexture _lutTexture;
    private int _nextGlobalAnimId = 0;
    /// <summary>
    /// Registra un clip y devuelve su índice global.
    /// </summary>
    public int RegisterClip(AnimClipDefinition clip)
    {
        int globalId = _nextGlobalAnimId++;

        _definitions[globalId] = clip;

        return globalId;
    }

    /// <summary>
    /// Construye la mini-textura flotante LUT y la asigna al parámetro 'anim_config_texture' del ShaderMaterial.
    /// </summary>
    public void BuildAndApply(ShaderMaterial animatedMaterial)
    {
        if (_definitions.Count == 0) return;

        int maxIndex = 0;
        foreach (var key in _definitions.Keys)
        {
            if (key > maxIndex) maxIndex = key;
        }

        // Cada animación ocupa 2 píxeles horizontales contiguos en la textura LUT:
        // Píxel [index * 2]: R = StartX, G = StartY, B = 0, A = 0
        // Píxel [index * 2 + 1]: R = FrameWidth, G = FrameHeight, B = TotalFrames, A = Speed
        int textureWidth = (maxIndex + 1) * 2;
        Image lutImage = Image.CreateEmpty(textureWidth, 1, false, Image.Format.Rgbaf);

        foreach (var (index, clip) in _definitions)
        {
            int pixelPos = index * 2;
            float loopFlag = clip.IsLoop ? 1.0f : 0.0f;
            
            // Píxel 0: StartX, StartY, IsLoop, 0
            lutImage.SetPixel(pixelPos, 0, new Color(clip.StartX, clip.StartY, loopFlag, 0.0f));

            // Píxel 1: FrameWidth, FrameHeight, TotalFrames, Speed
            lutImage.SetPixel(pixelPos + 1, 0, new Color(clip.FrameWidth, clip.FrameHeight, clip.TotalFrames, clip.Speed));
        }

        _lutTexture = ImageTexture.CreateFromImage(lutImage);
        animatedMaterial.SetShaderParameter("anim_config_texture", _lutTexture);
    }
}