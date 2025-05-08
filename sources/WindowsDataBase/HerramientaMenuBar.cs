using Godot;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase;
using System;
using System.Collections.Generic;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Weapons;

public partial class HerramientaMenuBar : MenuBar
{
    [Export]
    public SubViewport viewportMainEditor;
    PopupMenu editor;
    Window editorWindow = null;
    bool editorCheck;
    Vector2I editorPosition;


    PopupMenu tileCreator;
    PopupMenu atlasCreator;

    PopupMenu menuComponentes;
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
                    var collection = DataBaseManager.Instance.FindAll<CharacterBaseData>();
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
        }
    }

    private void AtlasCreator_IdPressed(long id)
    {
        switch (id)
        {
            case 0:
                Window win = GD.Load<PackedScene>("res://sources/WindowsDataBase/Materials/windowMaterial.tscn").Instantiate<Window>();               
                AddChild(win);
                win.Show();                
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
                win = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowDataGeneric.tscn").Instantiate<WindowDataGeneric>();
                AddChild(win);
                win.Show();
                 ps =  GD.Load<PackedScene>("res://sources/WindowsDataBase/TileCreator/windowTileSimple.tscn");
                win.SetWindowDetail(ps, GodotEcsArch.sources.utils.WindowState.CRUD,"Tile Simple");
                win.SetLoaddBAction(() => 
                {
                    var collection = DataBaseManager.Instance.FindAll<TileSimpleData>();
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
                ps = GD.Load<PackedScene>("res://sources/WindowsDataBase/TileCreator/windowAutoTile.tscn");
                win.SetWindowDetail(ps, GodotEcsArch.sources.utils.WindowState.CRUD,"Auto Tile");
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
            case 2:
                win = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowDataGeneric.tscn").Instantiate<WindowDataGeneric>();
                AddChild(win);
                win.Show();
                ps = GD.Load<PackedScene>("res://sources/WindowsDataBase/TileCreator/windowTileAnimate.tscn");
                win.SetWindowDetail(ps, GodotEcsArch.sources.utils.WindowState.CRUD,"Tile Animate");
                win.SetLoaddBAction(() =>
                {
                    var collection = DataBaseManager.Instance.FindAll<TileAnimateData>();
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
                win = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowDataGeneric.tscn").Instantiate<WindowDataGeneric>();
                AddChild(win);
                win.Show();
                ps = GD.Load<PackedScene>("res://sources/WindowsDataBase/TileCreator/windowTileDinamic.tscn");
                win.SetWindowDetail(ps, GodotEcsArch.sources.utils.WindowState.CRUD,"Tile Dinamico");
                win.SetLoaddBAction(() =>
                {
                    var collection = DataBaseManager.Instance.FindAll<TileDynamicData>();
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
                editorCheck = !editorCheck;
                editor.SetItemChecked(index, editorCheck);
                
                if (editorWindow==null)
                {
                    editorWindow = GD.Load<PackedScene>("res://sources/WindowsDataBase/Positions/window_positions.tscn").Instantiate<Window>();
                    editorWindow.Position = editorPosition;
                    editorWindow.Show();                    
                    viewportMainEditor.AddChild(editorWindow); 
                }
                if (editorCheck)
                {
                    editorWindow.Position = editorPosition;
                }
                else
                {
                    editorPosition = editorWindow.Position;
                }
                editorWindow.Visible = editorCheck;
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
