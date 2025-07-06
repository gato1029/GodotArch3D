using Godot;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase;
using System;
using System.Collections.Generic;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Weapons;
using GodotEcsArch.sources.WindowsDataBase.Generic.Facade;
using GodotEcsArch.sources.WindowsDataBase.CharacterCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;

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
            default:
                break;
        }
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
        WindowDataGeneric win;
        PackedScene ps;
        switch (id)
        {
            case 0:
                win = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowDataGeneric.tscn").Instantiate<WindowDataGeneric>();
                AddChild(win);
                win.Show();
                ps = GD.Load<PackedScene>("res://sources/WindowsDataBase/Character/windowCharacterBase.tscn");
                win.SetWindowDetail(ps, GodotEcsArch.sources.utils.WindowState.CRUD,"Character");
                win.SetLoaddBAction(() =>
                {
                    var collection = DataBaseManager.Instance.FindAll<AnimationCharacterBaseData>();
                    List<IdData> ids = new List<IdData>();
                    foreach (var item in collection)
                    {
                        IdData iddata = item;
                        ids.Add(iddata);
                    }
                    return ids;
                }
                );
                break;
            case 1:
                win = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowDataGeneric.tscn").Instantiate<WindowDataGeneric>();
                AddChild(win);
                win.Show();
                ps = GD.Load<PackedScene>("res://sources/WindowsDataBase/Terrain/windowTerrain.tscn");
                win.SetWindowDetail(ps, GodotEcsArch.sources.utils.WindowState.CRUD,"Terrenos",true);
                win.SetLoaddBAction(() =>
                {
                    var collection = DataBaseManager.Instance.FindAll<TerrainData>();
                    List<IdData> ids = new List<IdData>();
                    foreach (var item in collection)
                    {
                        IdData iddata = item;
                        ids.Add(iddata);
                    }
                    return ids;
                }
                );
                break;
            case 2:
                win = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowDataGeneric.tscn").Instantiate<WindowDataGeneric>();
                AddChild(win);
                win.Show();
                ps = GD.Load<PackedScene>("res://sources/WindowsDataBase/Weapons/windowWeapons.tscn");
                win.SetWindowDetail(ps, GodotEcsArch.sources.utils.WindowState.CRUD, "Armas", true);
                win.SetLoaddBAction(() =>
                {
                    var collection = DataBaseManager.Instance.FindAll<AnimationBaseData>();
                    List<IdData> ids = new List<IdData>();
                    foreach (var item in collection)
                    {
                        IdData iddata = item;
                        ids.Add(iddata);
                    }
                    return ids;
                }
                );
                break;
            case 3:
                var winInternal = GD.Load<PackedScene>("res://sources/WindowsDataBase/Accesories/WindowAccessory.tscn").Instantiate<WindowAccessory>();
                AddChild(winInternal);
                winInternal.PopupCentered();               
                break;
            case 4:
                FacadeWindowDataSearch<AnimationCharacterBaseData> windowQuery = new FacadeWindowDataSearch<AnimationCharacterBaseData>("res://sources/WindowsDataBase/Character/WindowAnimationCharacterRefact.tscn", this);
                break;
            case 5:
                FacadeWindowDataSearch<CharacterModelBaseData> windowQueryModelData = new FacadeWindowDataSearch<CharacterModelBaseData>("res://sources/WindowsDataBase/CharacterCreator/WindowCharacterCreator.tscn", this);
                break;
        }
    }

    private void AtlasCreator_IdPressed(long id)
    {
        switch (id)
        {
            case 0:
                FacadeWindowDataSearch<MaterialData> windowQuery = new FacadeWindowDataSearch<MaterialData>("res://sources/WindowsDataBase/Materials/windowNewMaterial.tscn", this);
                //Window win = GD.Load<PackedScene>("res://sources/WindowsDataBase/Materials/windowMaterial.tscn").Instantiate<Window>();               
                //AddChild(win);
                //win.Show();                
                break;
            default:
                break;
        }
    }

    private void TileCreator_IdPressed(long id)
    {
        WindowDataGeneric win;
        PackedScene ps;
        switch (id)
        {
            case 0:
                FacadeWindowDataSearch<TileDynamicData> windowDinamic = new FacadeWindowDataSearch<TileDynamicData>("res://sources/WindowsDataBase/TileCreator/WindowTiles.tscn", this);
                break;
            case 1:
                FacadeWindowDataSearch<TileAnimateData> windowQueryModelData = new FacadeWindowDataSearch<TileAnimateData>("res://sources/WindowsDataBase/TileCreator/WindowAnimatedTiles.tscn", this);
                break;
            case 2:
                win = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowDataGeneric.tscn").Instantiate<WindowDataGeneric>();
                AddChild(win);
                win.Show();
                ps = GD.Load<PackedScene>("res://sources/WindowsDataBase/TileCreator/windowAutoTile.tscn");
                win.SetWindowDetail(ps, GodotEcsArch.sources.utils.WindowState.CRUD, "Auto Tile");
                win.SetLoaddBAction(() =>
                {
                    var collection = DataBaseManager.Instance.FindAll<AutoTileData>();
                    List<IdData> ids = new List<IdData>();
                    foreach (var item in collection)
                    {
                        IdData iddata = item;
                        ids.Add(iddata);
                    }
                    return ids;
                }
                );
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
