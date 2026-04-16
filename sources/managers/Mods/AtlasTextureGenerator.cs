using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using LiteDB;
using RectangleBinPacking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Mods;

public class AtlasTextureGenerator:SingletonBase<AtlasTextureGenerator> 
{

    private int idAtlasGlobal = 1;
    private MaterialModDbService db = MaterialModDbService.Instance;
    public void GenerateMasterAtlas(List<MaterialData> materials, MaterialType materialType)    
    {        
        int atlasSize = 4096;

        

        MaxRectsBinPack<int> maxRectsBinPack = new MaxRectsBinPack<int>(atlasSize, atlasSize, FreeRectChoiceHeuristic.RectBottomLeftRule, false);
        List<Texture2D> textures = new List<Texture2D>();
        List<InsertResult> placements = new List<InsertResult>();
        List<MaterialData> materialesIds = new List<MaterialData>();

        int id = 0;
        foreach (var item in materials)
        {
            id++;
            InsertResult result = maxRectsBinPack.Insert(id, item.widhtTexture, item.heightTexture);

            // Si no cabe, guarda el atlas actual y empieza uno nuevo
            if (result == null)
            {
                // Guardar el atlas actual
                if (textures.Count > 0)
                {
                    CreateTextureAtlas(idAtlasGlobal, materialType, textures, placements, materialesIds, new Vector2I(atlasSize, atlasSize));
                    idAtlasGlobal++;
                }

                // Resetear para nuevo atlas
                maxRectsBinPack = new MaxRectsBinPack<int>(atlasSize, atlasSize, FreeRectChoiceHeuristic.RectBottomLeftRule, false);
                textures.Clear();
                placements.Clear();
                materialesIds.Clear();
                // Reintentar insertar en el nuevo atlas
                result = maxRectsBinPack.Insert(item.id, item.widhtTexture, item.heightTexture);
                if (result == null)
                {
                    GD.PrintErr($"Error: La textura ID {item.id} ({item.widhtTexture}x{item.heightTexture}) no cabe ni siquiera en un atlas vacío.");
                    continue;
                }
            }
            
            textures.Add((Texture2D)item.textureMaterial);
            placements.Add(result);
            materialesIds.Add(item);
        }

        // Guardar el último atlas si hay datos
        if (textures.Count > 0)
        {
            CreateTextureAtlas(idAtlasGlobal, materialType, textures, placements, materialesIds, new Vector2I(atlasSize, atlasSize));
            idAtlasGlobal++;
        }
    }

    private  void CreateTextureAtlas(int idAtlas, MaterialType materialType, List<Texture2D> textures, List<InsertResult> placements, List<MaterialData> materiales, Vector2I atlasSize)
    {
      

        string pathSave = "ModsGame/Atlas/atlas_" + materialType.ToString() + "_" + idAtlas.ToString() + ".png";
        string path = FileHelper.GetPathGameDB(pathSave);
        int idAtlasHash = StableHash.FromString(pathSave);

        Image atlasImage = Image.CreateEmpty(atlasSize.X, atlasSize.Y, false, Image.Format.Rgba8);
        atlasImage.Fill(new Color(0, 0, 0, 0)); // fondo transparente

        for (int i = 0; i < textures.Count; i++)
        {
            Texture2D texture = textures[i];
            InsertResult placement = placements[i];

            Image image = texture.GetImage();
            image.Convert(Image.Format.Rgba8);

            Vector2I size = image.GetSize();
            atlasImage.BlitRect(image, new Rect2I(Vector2I.Zero, size), new Vector2I(placement.X, placement.Y));
        }

        atlasImage.SavePng(path);


        List<int> idMaterials = new List<int>();
        for (int i = 0; i < materiales.Count; i++)
        {
            MaterialModData materialModData = new MaterialModData();
            materialModData.idNameMod = materiales[i].idNameMod;
            materialModData.idTextureAtlas = idAtlas;
            materialModData.pathTextureAtlas = pathSave;
            materialModData.xInAtlas = placements[i].X;
            materialModData.yInAtlas = placements[i].Y;
            materialModData.timeStamp = materiales[i].timeStamp;
            db.Guardar(materialModData);
        }



        return ;
    }
}
