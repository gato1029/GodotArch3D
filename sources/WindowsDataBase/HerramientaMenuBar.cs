using Godot;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.CharacterCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Generic.Facade;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.Resources.DataBase;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Weapons;
using System;
using System.Collections.Generic;

public partial class HerramientaMenuBar : MenuBar
{
    [Export]
    public SubViewport viewportMainEditor;
    PopupMenu editor;
    Window positionWindow = null;
    Window editorWindow = null;
    bool positionCheck;
    bool editorCheck;
    Vector2I editorPosition;


    PopupMenu tileCreator;
    PopupMenu atlasCreator;
    PopupMenu menuComponentes;
    PopupMenu menuArmamento;
    PopupMenu menuMapas;
    public override void _Ready()
	{
        editor = GetNode<PopupMenu>("Editor");
        editorPosition = new Vector2I(250,250);                       
        editor.IdPressed += Editor_IdPressed;

        tileCreator = GetNode<PopupMenu>("Creador Tiles");
        tileCreator.IdPressed += TileCreator_IdPressed;

        atlasCreator = GetNode<PopupMenu>("Atlas");
        atlasCreator.IdPressed += AtlasCreator_IdPressed;

        menuComponentes = GetNode<PopupMenu>("Componentes");
        menuComponentes.IdPressed += MenuComponentes_IdPressed;

        menuArmamento = GetNode<PopupMenu>("Armamento");
        menuArmamento.IdPressed += MenuArmamento_IdPressed;

        menuMapas = GetNode<PopupMenu>("Mapas");
        menuMapas.IdPressed += MenuMapas_IdPressed;
    }

    private void MenuMapas_IdPressed(long id)
    {
        Window win;
        switch (id)
        {
            case 0:
                win = GD.Load<PackedScene>("res://sources/WindowsDataBase/Maps/MapsWindow.tscn").Instantiate<Window>();
                AddChild(win);
                win.Popup();
                break;
                case 1:
                ShowFileExplorer();
                    break;
            default:

                break;
        }
    }

    public void ShowFileExplorer()
    { 
        FileDialog fileDialog;
        // Crear y configurar el FileDialog
        fileDialog = new FileDialog
        {
            Name = "FileDialog",
            Access = FileDialog.AccessEnum.Filesystem,
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Filters = new string[] { "*.json ; JSON Files" }
        };
        // 👉 Establece la ruta por defecto
        fileDialog.CurrentDir = FileHelper.GetPathGameDB( CommonAtributes.pathMaps); // O usa "res://data" si es dentro del proyecto
        // Conectar la señal de selección de archivo
        fileDialog.FileSelected += (string path) =>
        {
            MapManagerEditor.Instance.CurrentMapLevelData = MapLevelData.LoadMap(path);            
        };

        // Agregar el FileDialog como hijo para que funcione
        AddChild(fileDialog);

        // Mostrar el diálogo
        fileDialog.PopupCentered();
    }

    

    private void MenuArmamento_IdPressed(long id)
    {
        switch (id)
        {
            case 0:
                FacadeWindowDataSearch<AccesoryAnimationBodyData> windowQueryModelData = new FacadeWindowDataSearch<AccesoryAnimationBodyData>("res://sources/WindowsDataBase/Accesories/WindowAccesoryAnimation.tscn", this);
                break;
            case 1:
                var winInternal = GD.Load<PackedScene>("res://sources/WindowsDataBase/Accesories/WindowAccessory.tscn").Instantiate<WindowAccessory>();
                AddChild(winInternal);
                winInternal.PopupCentered();
                break;
            default:
                break;
        }
    }

    private void MenuComponentes_IdPressed(long id)
    {
        switch (id)
        {
            case 0:
                FacadeWindowDataSearch<ResourceData> winResources = new FacadeWindowDataSearch<ResourceData>("res://sources/WindowsDataBase/Resources/WindowResources.tscn", this);
                break;
            case 1:
                FacadeWindowDataSearch<TerrainData> winTerrain = new FacadeWindowDataSearch<TerrainData>("res://sources/WindowsDataBase/Terrain/WindowTerrainDetail.tscn", this);            
                break;
            case 2:
                FacadeWindowDataSearch<BuildingData> winBuilding = new FacadeWindowDataSearch<BuildingData>("res://sources/WindowsDataBase/Building/WindowBuilding.tscn", this);
                break;                        
            case 3:
                FacadeWindowDataSearch<AnimationCharacterBaseData> windowQuery = new FacadeWindowDataSearch<AnimationCharacterBaseData>("res://sources/WindowsDataBase/Character/WindowAnimationCharacterRefact.tscn", this);
                break;
            case 4:
                FacadeWindowDataSearch<CharacterModelBaseData> windowQueryModelData = new FacadeWindowDataSearch<CharacterModelBaseData>("res://sources/WindowsDataBase/CharacterCreator/WindowCharacterCreator.tscn", this);
                break;
            case 5:
                FacadeWindowDataSearch<ResourceSourceData> windowResourcesSource = new FacadeWindowDataSearch<ResourceSourceData>("res://sources/WindowsDataBase/ResourceSource/WindowResourcesSource.tscn", this);
                break;
        }
    }

    private void AtlasCreator_IdPressed(long id)
    {
        switch (id)
        {
            case 0:
                FacadeWindowDataSearch<MaterialData> windowQuery = new FacadeWindowDataSearch<MaterialData>("res://sources/WindowsDataBase/Materials/windowNewMaterial.tscn", this);
             
                break;
            default:
                break;
        }
    }

    private void TileCreator_IdPressed(long id)
    {

        switch (id)
        {
            case 0:
                FacadeWindowDataSearch<TileDynamicData> windowDinamic = new FacadeWindowDataSearch<TileDynamicData>("res://sources/WindowsDataBase/TileCreator/WindowTiles.tscn", this);
                break;
            case 1:
                FacadeWindowDataSearch<TileAnimateData> windowQueryModelData = new FacadeWindowDataSearch<TileAnimateData>("res://sources/WindowsDataBase/TileCreator/WindowAnimatedTiles.tscn", this);
                break;
            case 2:
                FacadeWindowDataSearch<AutoTileData> windowAutoTile = new FacadeWindowDataSearch<AutoTileData>("res://sources/WindowsDataBase/TileCreator/windowAutoTile.tscn", this);            
                break;
            default:
                break;
        }
    }

    private void Editor_IdPressed(long id)
    {
        int index = editor.GetItemIndex((int)id);
        switch (index)
        {
            case 0:
                positionCheck = !positionCheck;
                editor.SetItemChecked(index, positionCheck);
                
                if (positionWindow==null)
                {
                    positionWindow = GD.Load<PackedScene>("res://sources/WindowsDataBase/Positions/window_positions.tscn").Instantiate<Window>();
                    positionWindow.Position = editorPosition;
                    positionWindow.Show();                    
                    viewportMainEditor.AddChild(positionWindow); 
                }
                if (positionCheck)
                {
                    positionWindow.Position = editorPosition;
                }
                else
                {
                    editorPosition = positionWindow.Position;
                }
                positionWindow.Visible = positionCheck;
                break;
            case 1:
                editorCheck = !editorCheck;
                editor.SetItemChecked(index, editorCheck);
                if (editorWindow == null)
                {
                    editorWindow = GD.Load<PackedScene>("res://sources/Editor/Scenes/EditorWindow.tscn").Instantiate<Window>();                   
                    editorWindow.Show();
                    AddChild(editorWindow);
                }
                if (editor.IsItemChecked(index))
                {
                    editorWindow.Visible = true;
                }
                else { editorWindow.Visible = false; }
                break;
            default:
                break;
            
        }
        
        
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
