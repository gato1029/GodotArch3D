using Godot;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.managers.Multimesh;

namespace GodotEcsArch.sources.managers.Accesories;
internal class AccesoryManager : SingletonBase<AccesoryManager>
{
   
    public Dictionary<int, AccessoryData> accesories;    
    protected override void Initialize()
    {
        accesories = new Dictionary<int, AccessoryData>();
        
    }

    public void RegisterAccesory(int id, AccessoryData materialData)
    {

        if (!accesories.ContainsKey(id))
        {
            accesories.Add(id, materialData);
        }
        else
        {
            if (id == -1)
            {
                accesories[id] = materialData;
            }
        }

    }

    public AccessoryData GetAccesory(int idAccesory)
    {
        if (!accesories.ContainsKey(idAccesory))
        {
            AccessoryData materialData = DataBaseManager.Instance.FindById<AccessoryData>(idAccesory);
            RegisterAccesory(idAccesory, materialData);
            return materialData;
        }
        if (accesories.ContainsKey(idAccesory))
        {
            return accesories[idAccesory];
        }
        return null;
    }

    //public AtlasTexture GetAtlasTexture(int idMaterial, int internalPosition)
    //{

    //    MaterialData materialData;
    //    if (!materials.ContainsKey(idMaterial))
    //    {
    //        materialData = DataBaseManager.Instance.FindById<MaterialData>(idMaterial);
    //        RegisterMaterial(idMaterial, materialData);
    //    }

    //    materialData = materials[idMaterial];
    //    AtlasTexture atlasTexture = new AtlasTexture();
    //    atlasTexture.Atlas = (Texture2D)(materials[idMaterial].textureMaterial);
    //    int columns = (int)(materialData.widhtTexture / materialData.divisionPixelX); //8


    //    int row = internalPosition / columns; // Fila correspondiente al índice
    //    int column = internalPosition % columns; // Columna correspondiente al índice

    //    // Calcular las coordenadas de la subimagen a partir del índice
    //    int x = column * materialData.divisionPixelX;
    //    int y = row * materialData.divisionPixelY;


    //    atlasTexture.Region = new Rect2(x, y, materialData.divisionPixelX, materialData.divisionPixelY);
    //    return atlasTexture;
    //}

    public AtlasTexture GetAtlasTextureView(int idAccesory)
    {
        AccessoryData accesoryData;
        if (!accesories.ContainsKey(idAccesory))
        {
            accesoryData = DataBaseManager.Instance.FindById<AccessoryData>(idAccesory);
            RegisterAccesory(idAccesory, accesoryData);
        }
        accesoryData = accesories[idAccesory];

        return MaterialManager.Instance.GetAtlasTexture(accesoryData.miniatureData.idMaterial, accesoryData.miniatureData.idTile);                
    }
}
