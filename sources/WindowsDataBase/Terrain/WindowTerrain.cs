using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Generic.Facade;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using TileData = GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileData;

public partial class WindowTerrain : Window, IFacadeWindow<TerrainData>
{
    TerrainData terrainData;
                                     
    public event IFacadeWindow<TerrainData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;

    public void SetData(TerrainData data)
    {
        terrainData = data;

        //recursos 

        if (terrainData.idsResources.ContainsKey(TerrainTileType.TierraNivel1))
        {
            RecursosTierraNivel1.SetData(terrainData.idsResources[TerrainTileType.TierraNivel1]);
        }
        if (terrainData.idsResources.ContainsKey(TerrainTileType.CespedNivel1))
        {
            RecursosCespedNivel1.SetData(terrainData.idsResources[TerrainTileType.CespedNivel1]);
        }
        if (terrainData.idsResources.ContainsKey(TerrainTileType.TierraNivel2))
        {
            RecursosTierraNivel2.SetData(terrainData.idsResources[TerrainTileType.TierraNivel2]);
        }
        if (terrainData.idsResources.ContainsKey(TerrainTileType.CespedNivel2))
        {
            RecursosCespedNivel2.SetData(terrainData.idsResources[TerrainTileType.CespedNivel2]);
        }

        foreach (var item in terrainData.idsAutoTileSprite)
        {
            switch (item.Type)
            {
                case TerrainTileType.Agua:
                    ControlAutoAgua.SetData(item.TileId);
                    break;
                case TerrainTileType.TierraNivel1:
                    ControlAutoTierra1.SetData(item.TileId);
                    break;
                case TerrainTileType.CespedNivel1:
                    ControlAutoCesped1.SetData(item.TileId);
                    break;
                case TerrainTileType.TierraNivel2:
                    ControlAutoTierra2.SetData(item.TileId);
                    break;
                case TerrainTileType.CespedNivel2:
                    ControlAutoCesped2.SetData(item.TileId);
                    break;
                case TerrainTileType.AdornosTierra:
                    ControlAutoAdornosTierra.SetData(item.TileId);
                    break;
                case TerrainTileType.AdornosCesped:
                    ControlAutoAdornoCesped.SetData(item.TileId);
                    break;
                case TerrainTileType.AdornosAgua:
                    ControlAutoAdornoAgua.SetData(item.TileId);
                    break;
                case TerrainTileType.AguaCamino:
                    break;
                case TerrainTileType.TierraCamino:
                    break;
                case TerrainTileType.TierraElevacionNivel2:
                    ControlAutoElevacionTierraNivel2.SetData(item.TileId);
                    break;
                default:
                    break;
            }

        }

        LineEditName.Text = terrainData.name;

    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        terrainData = new TerrainData();
        ButtonSave.Pressed += ButtonSave_Pressed;
        ButtonDelete.Pressed += ButtonDelete_Pressed;

    }

    private void ButtonDelete_Pressed()
    {        
        DataBaseManager.Instance.RemoveDirectById<TerrainData>(terrainData.id);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }

    private void ButtonSave_Pressed()
    {
        terrainData.idsAutoTileSprite.Clear();
        // Agua
        if (ControlAutoAgua?.objectData != null)
        {
            terrainData.idsAutoTileSprite.Add(new TerrainTileEntry(TerrainTileType.Agua, ControlAutoAgua.objectData.id));
        }
        // Tierra nivel 1
        if (ControlAutoTierra1?.objectData != null)
        {
            terrainData.idsAutoTileSprite.Add(new TerrainTileEntry(TerrainTileType.TierraNivel1, ControlAutoTierra1.objectData.id));
        }
        // Cesped nivel 1
        if (ControlAutoCesped1?.objectData != null)
        {
            terrainData.idsAutoTileSprite.Add(new TerrainTileEntry(TerrainTileType.CespedNivel1, ControlAutoCesped1.objectData.id));
        }
        // Tierra nivel 2
        if (ControlAutoTierra2?.objectData != null)
        {
            terrainData.idsAutoTileSprite.Add(new TerrainTileEntry(TerrainTileType.TierraNivel2, ControlAutoTierra2.objectData.id));
        }
        // Cesped nivel 2
        if (ControlAutoCesped2?.objectData != null)
        {
            terrainData.idsAutoTileSprite.Add(new TerrainTileEntry(TerrainTileType.CespedNivel2, ControlAutoCesped2.objectData.id));
        }
        // Adornos tierra
        if (ControlAutoAdornosTierra?.objectData != null)
        {
            terrainData.idsAutoTileSprite.Add(new TerrainTileEntry(TerrainTileType.AdornosTierra, ControlAutoAdornosTierra.objectData.id));
        }
        // Adornos cesped
        if (ControlAutoAdornoCesped?.objectData != null)
        {
            terrainData.idsAutoTileSprite.Add(new TerrainTileEntry(TerrainTileType.AdornosCesped, ControlAutoAdornoCesped.objectData.id));
        }
        // Adornos agua
        if (ControlAutoAdornoAgua?.objectData != null)
        {
            terrainData.idsAutoTileSprite.Add(new TerrainTileEntry(TerrainTileType.AdornosAgua, ControlAutoAdornoAgua.objectData.id));
        }
        // Elevacion tierra nivel 2
        if (ControlAutoElevacionTierraNivel2?.objectData != null)
        {
            terrainData.idsAutoTileSprite.Add(new TerrainTileEntry(TerrainTileType.TierraElevacionNivel2, ControlAutoElevacionTierraNivel2.objectData.id));
        }

        terrainData.idsResources.Clear();
        // Aqui inicia el guardado de Recursos
        if (RecursosTierraNivel1.GetData().Count>0)
        {
            terrainData.idsResources.Add(TerrainTileType.TierraNivel1, RecursosTierraNivel1.GetData());
        }
        if (RecursosCespedNivel1.GetData().Count > 0)
        {
            terrainData.idsResources.Add(TerrainTileType.CespedNivel1, RecursosCespedNivel1.GetData());
        }
        if (RecursosTierraNivel2.GetData().Count > 0)
        {
            terrainData.idsResources.Add(TerrainTileType.TierraNivel2, RecursosTierraNivel2.GetData());
        }   
        if (RecursosCespedNivel2.GetData().Count > 0)
        {
            terrainData.idsResources.Add(TerrainTileType.CespedNivel2, RecursosCespedNivel2.GetData());
        }

        terrainData.name = LineEditName.Text;
        
        DataBaseManager.Instance.InsertUpdate<TerrainData>(terrainData);
        MasterDataManager.UpdateRegisterData(terrainData.id, terrainData);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }
}
