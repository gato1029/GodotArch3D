using Arch.Core;
using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

public partial class MaterialManager: SingletonBase<MaterialManager>
{
    public Dictionary<int, MaterialData> materials;
    
    protected override void Initialize()
    {
        materials = new Dictionary<int, MaterialData>();
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
        MaterialData materialData;
        if (!materials.ContainsKey(idMaterial))
        {
            materialData = DataBaseManager.Instance.FindById<MaterialData>(idMaterial);
            RegisterMaterial(idMaterial, materialData);
        }
        materialData = materials[idMaterial];
        AtlasTexture atlasTexture = new AtlasTexture();
        atlasTexture.Atlas = (Texture2D)(materialData.textureMaterial);
        atlasTexture.Region = new Rect2(x, y, width, height);           
        return atlasTexture;
    }
    public void RegisterAlterMaterial(string nameOriginalMaterial, string name, Material material)
    {
        //if (materials.ContainsKey(nameOriginalMaterial))
        //{
        //    Material mat = materials[nameOriginalMaterial];
        //    mat.Set("Color", new Color(""));
        //    materials.Add(name, mat);
        //}
    }

    internal void SetMaterialPositionBatch(int idMaterial,int positionBatch)
    {
        materials[idMaterial].idMaterialPositionBatch = positionBatch;
    }
}
