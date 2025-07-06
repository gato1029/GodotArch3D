using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase;
using System;
using System.Collections.Generic;
using GodotEcsArch.sources.WindowsDataBase.CharacterCreator.DataBase;


public partial class WindowNewMaterial :  Window, IFacadeWindow<MaterialData>
{
    FileDialog fileDialog;
    LineEdit linePathFile;
    LineEdit lineName;
    LineEdit lineId;
    SpinBox spinPixel_X;
    SpinBox spinPixel_Y;
    ItemList itemListTiles;
    OptionButton typeMaterial;


    public event IFacadeWindow<MaterialData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;

    MaterialData objectData;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        CloseRequested += WindowTileCreator_CloseRequested;
        GetNode<Button>("Panel/MarginContainer/VBoxContainer/VBoxContainer/GridContainer/HBoxContainer/Button").Pressed += FileSearch;
        GetNode<Button>("Panel/MarginContainer/VBoxContainer/VBoxContainer/GridContainer/HBoxContainer5/ButtonDividir").Pressed += TextureSplit_Clik; ;
        GetNode<Button>("Panel/MarginContainer/VBoxContainer/VBoxContainer/Button").Pressed += Guardar_Click; ;

        linePathFile = GetNode<LineEdit>("Panel/MarginContainer/VBoxContainer/VBoxContainer/GridContainer/HBoxContainer/LineEdit3");
        lineName = GetNode<LineEdit>("Panel/MarginContainer/VBoxContainer/VBoxContainer/GridContainer/LineEdit2");
        lineId = GetNode<LineEdit>("Panel/MarginContainer/VBoxContainer/VBoxContainer/GridContainer/LineEdit");
        itemListTiles = GetNode<ItemList>("Panel/MarginContainer/VBoxContainer/ItemList");

        spinPixel_X = GetNode<SpinBox>("Panel/MarginContainer/VBoxContainer/VBoxContainer/GridContainer/HBoxContainer5/SpinBoxX");
        spinPixel_Y = GetNode<SpinBox>("Panel/MarginContainer/VBoxContainer/VBoxContainer/GridContainer/HBoxContainer5/SpinBoxY");

        typeMaterial = GetNode<OptionButton>("Panel/MarginContainer/VBoxContainer/VBoxContainer/GridContainer/OptionButton");

        fileDialog = new FileDialog();
        fileDialog.Title = "Buscar Imagen";

        // Configurar propiedades del FileDialog
        fileDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
        fileDialog.Access = FileDialog.AccessEnum.Filesystem; // Acceso al sistema de archivos
        fileDialog.Filters = new string[] { "*.png ; PNG Images" }; // Filtros de archivo
        fileDialog.AlwaysOnTop = true;
        // Conectar la señal "file_selected"
        fileDialog.FileSelected += OnFileSelected;
        // Agregar el nodo a la escena
        AddChild(fileDialog);
        objectData = new MaterialData();
    }

    public void SetData(MaterialData data)
    {
        objectData = data;
       
        lineName.Text = objectData.name;
        lineId.Text = objectData.id.ToString();
        linePathFile.Text = objectData.pathTexture;
        spinPixel_X.Value = objectData.divisionPixelX;
        spinPixel_Y.Value = objectData.divisionPixelY;
        typeMaterial.Selected = typeMaterial.GetItemIndex(objectData.type);
        mode = 1;
        TextureSplit_Clik();
    }
    public void LoadData(int id)
    {
        var materialData = DataBaseManager.Instance.FindById<MaterialData>(id);
        lineName.Text = materialData.name;
        lineId.Text = materialData.id.ToString();
        linePathFile.Text = materialData.pathTexture;
        spinPixel_X.Value = materialData.divisionPixelX;
        spinPixel_Y.Value = materialData.divisionPixelY;
        typeMaterial.Selected = typeMaterial.GetItemIndex(materialData.type);
        mode = 1;
        TextureSplit_Clik();
    }

    int mode = 0;
    public void LoadMaterial(int id)
    {
       var materialData = DataBaseManager.Instance.FindById<MaterialData>(id);
        lineName.Text = materialData.name;
        lineId.Text = materialData.id.ToString();
        linePathFile.Text = materialData.pathTexture;
        spinPixel_X.Value = materialData.divisionPixelX;
        spinPixel_Y.Value = materialData.divisionPixelY;
        typeMaterial.Selected = typeMaterial.GetItemIndex(materialData.type);
        mode = 1;
        TextureSplit_Clik();
        
    }
    private void Guardar_Click()
    {
        if (linePathFile.Text.StartsWith("AssetExternals"))
        {
            mode = 1;
        }
        else
        {
            mode = 0;
        }

        Image image;
        if (mode == 0)
        {
             image = TextureHelper.LoadImageLocal(linePathFile.Text);
        }
        else
        {
            image = TextureHelper.LoadImageLocal(FileHelper.GetPathGameDB(linePathFile.Text));
        }

        objectData.pathTexture = linePathFile.Text;


        objectData.name = lineName.Text;
        objectData.heightTexture = image.GetHeight();
        objectData.widhtTexture = image.GetWidth();
        objectData.divisionPixelX = (int) spinPixel_X.Value;
        objectData.divisionPixelY = (int)spinPixel_Y.Value;
        objectData.type = typeMaterial.GetItemId(typeMaterial.GetSelectedId());
        if (lineId.Text == string.Empty)
        {
            int idnext = DataBaseManager.Instance.NextID<MaterialData>();
            string path = FileHelper.CopyFileToAssetExternals(linePathFile.Text,"Material", idnext.ToString());
            objectData.pathTexture = path;
            DataBaseManager.Instance.InsertUpdate(objectData);            
        }
        else
        {

            string path = FileHelper.CopyFileToAssetExternals(linePathFile.Text, "Material", objectData.id.ToString());
            objectData.pathTexture = path;
            DataBaseManager.Instance.InsertUpdate(objectData, int.Parse(lineId.Text));
        }
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();

    }

    private void TextureSplit_Clik()
    {
        itemListTiles.Clear();
        List<Texture> list;
        if (mode == 0)
        {
             list = TextureHelper.SplitTexture(linePathFile.Text, new Vector2I((int)spinPixel_X.Value, (int)spinPixel_Y.Value));
        }
        else
        {
             list = TextureHelper.SplitTexture(FileHelper.GetPathGameDB(linePathFile.Text), new Vector2I((int)spinPixel_X.Value, (int)spinPixel_Y.Value));
        }
        
        for (int i = 0; i < list.Count; i++)
        {
            Texture item = list[i];
            if (!TextureHelper.IsTextureEmpty(item))
            {
                itemListTiles.AddItem("ID:" + i, (Texture2D)item);
            }
        }
    }

    private void FileSearch()
    {
        fileDialog.PopupCentered();
    }

    private void WindowTileCreator_CloseRequested()
    {
        QueueFree();
    }

    private void OnFileSelected(string path)
    {
        linePathFile.Text = path;
        TextureSplit_Clik();
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}


}
