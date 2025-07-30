using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Textures;
public class TextureArrayBuilder
{
    public ShaderMaterial TargetMaterial;
    public Mesh mesh { get; set; }
    public string ShaderUniformName = "mtexture_array";
    public Vector2I TargetSize = new Vector2I(4096, 4096);

    private Dictionary<int, string> _imagePathMap = new();

    public TextureArrayBuilder()
    {
        ShaderMaterial materialGeneric = GD.Load<ShaderMaterial>("res://resources/Material/Sprite3DMultimeshGenericArrayMaterial.tres");
        TargetMaterial = (ShaderMaterial)materialGeneric.Duplicate();
    }

   

    /// <summary>
    /// Asigna una imagen en una capa específica del array.
    /// </summary>
    public void SetTextureAt(int index, string path)
    {
        if (index < 0)
        {
            GD.PrintErr($"❌ Índice inválido: {index}");
            return;
        }

        _imagePathMap[index] = path;
    }

    /// <summary>
    /// Limpia todas las rutas agregadas.
    /// </summary>
    public void Clear()
    {
        _imagePathMap.Clear();
    }

    /// <summary>
    /// Construye el Texture2DArray y lo asigna al material.
    /// </summary>
    public void BuildAndApply()
    {
        if (_imagePathMap.Count == 0)
        {
            GD.PrintErr("No hay imágenes registradas.");
            return;
        }

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
            mesh = MeshCreator.CreateSquareMesh(16, 16, new Vector2(0, 0), new Vector3(0, 0, 0));
            mesh.SurfaceSetMaterial(0, TargetMaterial);

            GD.Print($"✅ Texture2DArray aplicado a '{ShaderUniformName}' con {maxIndex + 1} capas.");
        }
        else
        {
            GD.PrintErr("❌ No se asignó TargetMaterial.");
        }
    }

    private Image LoadAndResizeImage(string path, int width, int height)
    {
        Image source = Image.LoadFromFile(path);
        if (source == null)
        {
            GD.PrintErr($"❌ No se pudo cargar: {path}");
            return CreateTransparentImage(width, height);
        }

        Image result = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        result.Fill(new Color(0, 0, 0, 0));
        result.BlitRect(source, new Rect2I(0, 0, source.GetWidth(), source.GetHeight()), new Vector2I(0, 0));
        return result;
    }

    private Image CreateTransparentImage(int width, int height)
    {
        Image image = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        image.Fill(new Color(0, 0, 0, 0));
        return image;
    }
}

