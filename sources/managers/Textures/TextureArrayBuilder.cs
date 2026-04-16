using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Textures;

public struct TextureLocation
{
    public int ArrayIndex;
    public int LayerIndex;

    public TextureLocation(int arrayIndex, int layerIndex)
    {
        ArrayIndex = arrayIndex;
        LayerIndex = layerIndex;
    }
    public bool IsValid()
    {
        return ArrayIndex >= 0 && LayerIndex >= 0;
    }
}
public class TextureArrayManager
{
    public int MaxLayersPerArray = 256;

    private List<TextureArrayBuilder> _builders = new();
    private Dictionary<string, int> _nameToIndex = new();
    private Dictionary<int, TextureLocation> _globalMap = new();
    private int _nextIndex = 0;

    public void SetTexture(string idName, int id, string path)
    {
        if (_nameToIndex.TryGetValue(idName, out int existingIndex))
        {
            if (existingIndex != id)
            {
                GD.PrintErr($"❌ ID inconsistente para {idName}: {existingIndex} vs {id}");
                return;
            }
        }
        else
        {
            _nameToIndex[idName] = id;
        }

        SetTexture(id, path);
    }

    public void SetTexture(string id, string path)
    {
        if (!_nameToIndex.TryGetValue(id, out int index))
        {
            index = _nextIndex++;
            _nameToIndex[id] = index;
        }

        SetTexture(index, path);
    }
    public void SetTexture(int globalIndex, string path)
    {
        int builderIndex = globalIndex / MaxLayersPerArray;
        int layerIndex = globalIndex % MaxLayersPerArray;

        // Asegurar que exista el builder
        while (_builders.Count <= builderIndex)
        {
            _builders.Add(new TextureArrayBuilder()
            {
                MaxLayers = MaxLayersPerArray
            });
        }

        _builders[builderIndex].SetTextureAt(layerIndex, path);

        _globalMap[globalIndex] = new TextureLocation(builderIndex, layerIndex);
    }

    public void BuildAll()
    {
        foreach (var builder in _builders)
        {
            builder.BuildAndApply();
        }
    }
    public TextureArrayBuilder GetBuilder(string textureId)
    {
        if (!_nameToIndex.TryGetValue(textureId, out int index))
            return null;

        if (_globalMap.TryGetValue(index, out var location))
        {
            int builderIndex = location.ArrayIndex;

            if (builderIndex >= 0 && builderIndex < _builders.Count)
                return _builders[builderIndex];
        }

        return null;
    }
    public TextureLocation GetLocation(string textureId)
    {
        if (!_nameToIndex.TryGetValue(textureId, out int index))
            return new TextureLocation(-1, -1);

        if (_globalMap.TryGetValue(index, out var location))
            return location;

        return new TextureLocation(-1, -1);
    }
    public TextureLocation GetLocation(int globalIndex)
    {
        if (_globalMap.TryGetValue(globalIndex, out var location))
            return location;

        return new TextureLocation(-1, -1);
    }

    public TextureArrayBuilder GetBuilder(int index)
    {
        if (index < 0 || index >= _builders.Count)
            return null;

        return _builders[index];
    }
    public IReadOnlyList<TextureArrayBuilder> GetBuilders()
    {
        return _builders;
    }
}
public class TextureArrayBuilder
{
    public ShaderMaterial TargetMaterial;
    public int MaxLayers = 256;
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
            mesh = (Mesh)MeshCreator.GetBaseMesh().Duplicate();
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

