namespace GodotEcsArch.sources.managers.GpuDriven;

using Godot;
using System.Collections.Generic;
using System.Linq;



public enum RenderPipelineType
{
    Static = 0,
    AnimatedUnits = 1,
    SimpleAnimation = 2, // Ejemplo a futuro (Frame inicio / fin)
    WaterDistortion = 3,  // Ejemplo a futuro (Shaders de fluidos)
    CustomShader = 100    // Para cualquier experimento
}

public struct TextureLocation
{
    public int ArrayIndex;       // Índice dentro del Texture2DArray de ese pipeline
    public int LayerIndex;       // Capa (layer) específica
    public int PipelineId;       // ID numérico del pipeline ( RenderPipelineType u otro )

    public TextureLocation(int arrayIndex, int layerIndex, int pipelineId)
    {
        ArrayIndex = arrayIndex;
        LayerIndex = layerIndex;
        PipelineId = pipelineId;
    }

    public bool IsValid() => ArrayIndex >= 0 && LayerIndex >= 0;
}
public class GpuTextureArrayBuilder
{
    public ShaderMaterial TargetMaterial { get; private set; }
    public int PipelineId { get; private set; }
    public int MaxLayers { get; set; } = 256;
    public Mesh BaseMesh { get; private set; }
    public string ShaderUniformName { get; set; } = "mtexture_array";
    public Vector2I TargetSize { get; set; } = new Vector2I(4096, 4096);

    private readonly Dictionary<int, string> _imagePathMap = new();

    public GpuTextureArrayBuilder(ShaderMaterial baseMaterial, int pipelineId)
    {
        PipelineId = pipelineId;
        TargetMaterial = (ShaderMaterial)baseMaterial.Duplicate();
    }

    public void SetTextureAt(int index, string path)
    {
        if (index < 0)
        {
            GD.PrintErr($"❌ Índice inválido: {index}");
            return;
        }
        _imagePathMap[index] = path;
    }

    public void Clear() => _imagePathMap.Clear();

    public void BuildAndApply()
    {
        if (_imagePathMap.Count == 0) return;

        int maxIndex = _imagePathMap.Keys.Max();
        var images = new Godot.Collections.Array<Image>();

        for (int i = 0; i <= maxIndex; i++)
        {
            if (_imagePathMap.TryGetValue(i, out var path))
            {
                images.Add(LoadAndResizeImage(path, TargetSize.X, TargetSize.Y));
            }
            else
            {
                images.Add(CreateTransparentImage(TargetSize.X, TargetSize.Y));
            }
        }

        Texture2DArray textureArray = new Texture2DArray();
        textureArray.CreateFromImages(images);

        if (TargetMaterial != null)
        {
            TargetMaterial.SetShaderParameter(ShaderUniformName, textureArray);
            BaseMesh = (Mesh)MeshCreator.GetBaseMesh().Duplicate();
            BaseMesh.SurfaceSetMaterial(0, TargetMaterial);

            GD.Print($"✅ [PipelineId: {PipelineId}] Texture2DArray aplicado a '{ShaderUniformName}' con {maxIndex + 1} capas.");
        }
    }

    private Image LoadAndResizeImage(string path, int width, int height)
    {
        Image source = Image.LoadFromFile(path);
        if (source == null) return CreateTransparentImage(width, height);

        if (source.GetWidth() == width && source.GetHeight() == height)
            return source;

        Image result = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        result.Fill(Colors.Transparent);

        int copyWidth = Mathf.Min(source.GetWidth(), width);
        int copyHeight = Mathf.Min(source.GetHeight(), height);

        result.BlitRect(source, new Rect2I(0, 0, copyWidth, copyHeight), Vector2I.Zero);
        return result;
    }

    private Image CreateTransparentImage(int width, int height)
    {
        Image image = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        image.Fill(new Color(0, 0, 0, 0));
        return image;
    }
}