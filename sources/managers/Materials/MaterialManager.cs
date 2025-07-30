using Arch.Core;
using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

public partial class MaterialManager: SingletonBase<MaterialManager>
{
    public Dictionary<int, MaterialData> materials;
    public Dictionary<int, TextureMasterData> textureMasterData;
    protected override void Initialize()
    {
        materials = new Dictionary<int, MaterialData>();
        textureMasterData = new Dictionary<int, TextureMasterData>();
    }
    public void RegisterTextureMaster(int id, TextureMasterData texture)
    {

        if (!textureMasterData.ContainsKey(id))
        {
            textureMasterData.Add(id, texture);
        }
        else
        {
            if (id == -1)
            {
                textureMasterData[id] = texture;
            }
        }

    }
    public void RegisterMaterial(int id, MaterialData materialData)
    {
        
        if (!materials.ContainsKey(id) )
        {
            materials.Add(id, materialData);
        }
        else
        {
            if (id == -1)
            {
                materials[id] = materialData;
            }
        }
        
    }
    public TextureMasterData GetTextureMaster(int idTexture)
    {
        if (!textureMasterData.ContainsKey(idTexture))
        {
            TextureMasterData textureMasterData = DataBaseManager.Instance.FindById<TextureMasterData>(idTexture);
            RegisterTextureMaster(idTexture, textureMasterData);
            return textureMasterData;
        }
        if (textureMasterData.ContainsKey(idTexture))
        {
            return textureMasterData[idTexture];
        }
        return null;
    }
    public MaterialData GetMaterial(int idMaterial)
    {
        if (!materials.ContainsKey(idMaterial))
        {
            MaterialData materialData = DataBaseManager.Instance.FindById<MaterialData>(idMaterial);
            RegisterMaterial(idMaterial, materialData);
            return materialData;
        }
        if (materials.ContainsKey(idMaterial))
        {
            return materials[idMaterial];
        }
        return null;
    }

    public AtlasTexture GetAtlasTexture(int idMaterial, int internalPosition)
    {

        MaterialData materialData;
        if (!materials.ContainsKey(idMaterial))
        {
            materialData = DataBaseManager.Instance.FindById<MaterialData>(idMaterial);
            RegisterMaterial(idMaterial, materialData);
        }

        materialData = materials[idMaterial];
        AtlasTexture atlasTexture = new AtlasTexture();
        atlasTexture.Atlas = (Texture2D)(materials[idMaterial].textureMaterial); 
        int columns = (int)(materialData.widhtTexture / materialData.divisionPixelX); //8
        

        int row = internalPosition / columns; // Fila correspondiente al índice
        int column = internalPosition % columns; // Columna correspondiente al índice

        // Calcular las coordenadas de la subimagen a partir del índice
        int x = column * materialData.divisionPixelX;
        int y = row * materialData.divisionPixelY;


        atlasTexture.Region = new Rect2(x, y, materialData.divisionPixelX, materialData.divisionPixelY);
        return atlasTexture;
    }

    public AtlasTexture GetAtlasTexture(int idMaterial, float x, float y, float width, float height)
    {
        MaterialData materialData=null;
        TextureMasterData textureMasterData;
        Texture2D textureBase = null;
        if (!materials.ContainsKey(idMaterial))
        {
            materialData = DataBaseManager.Instance.FindById<MaterialData>(idMaterial);            
            RegisterMaterial(idMaterial, materialData);
        }
        else
        {
            materialData = materials[idMaterial];
        }
        if (materialData.type <= 4)
        {
            textureMasterData = GetTextureMaster(materialData.idTextureMaster);
            textureBase = (Texture2D)textureMasterData.texture;
        }
        else
        {            
            textureBase = (Texture2D)(materialData.textureMaterial);
        }

        AtlasTexture atlasTexture = new AtlasTexture();
        atlasTexture.Atlas = textureBase;
        atlasTexture.Region = new Rect2(x, y, width, height);           
        return atlasTexture;
    }

    public AtlasTexture GetAtlasTextureInternal(int idMaterial, float x, float y, float width, float height)
    {
        MaterialData materialData = null;
        TextureMasterData textureMasterData;
        Texture2D textureBase = null;
        if (!materials.ContainsKey(idMaterial))
        {
            materialData = DataBaseManager.Instance.FindById<MaterialData>(idMaterial);
            RegisterMaterial(idMaterial, materialData);
        }
        else
        {
            materialData = materials[idMaterial];
        }
        
        textureBase = (Texture2D)(materialData.textureMaterial);    
        AtlasTexture atlasTexture = new AtlasTexture();
        atlasTexture.Atlas = textureBase;
        atlasTexture.Region = new Rect2(x, y, width, height);
        return atlasTexture;
    }
    public AtlasTexture GetAtlasTextureInternal(SpriteData spriteData)
    {
        MaterialData materialData = null;
        TextureMasterData textureMasterData;
        Texture2D textureBase = null;
        if (!materials.ContainsKey(spriteData.idMaterial))
        {
            materialData = DataBaseManager.Instance.FindById<MaterialData>(spriteData.idMaterial);
            RegisterMaterial(spriteData.idMaterial, materialData);
        }
        else
        {
            materialData = materials[spriteData.idMaterial];
        }

        textureBase = (Texture2D)(materialData.textureMaterial);
        AtlasTexture atlasTexture = new AtlasTexture();
        atlasTexture.Atlas = textureBase;
        atlasTexture.Region = new Rect2(spriteData.x, spriteData.y, spriteData.widht, spriteData.height);
        return atlasTexture;
    }
    internal void SetMaterialPositionBatch(int idMaterial,int positionBatch)
    {
        if (GetMaterial(idMaterial) !=null)
        {
            materials[idMaterial].idMaterialPositionBatch = positionBatch;
        }
        else
        {
            GD.PrintErr("Material no encontrado");
        }                
    }
}
