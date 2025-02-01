using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase;
using System;
using System.Collections.Generic;


public partial class WindowNewMaterial :  Window , IDetailWindow
{
    FileDialog fileDialog;
    LineEdit linePathFile;
    LineEdit lineName;
    LineEdit lineId;
    SpinBox spinPixel_X;
    SpinBox spinPixel_Y;
    ItemList itemListTiles;
    OptionButton typeMaterial;

    //// Delegado para manejar la solicitud de actualización
    //public delegate void RequestUpdateHandler();
    //public event RequestUpdateHandler OnRequestUpdate;
    public event IDetailWindow.RequestUpdateHandler OnRequestUpdate;


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

        // Conectar la señal "file_selected"
        fileDialog.FileSelected += OnFileSelected;
        // Agregar el nodo a la escena
        AddChild(fileDialog);
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
        Image image;
        if (mode == 0)
        {
             image = TextureHelper.LoadImageLocal(linePathFile.Text);
        }
        else
        {
            image = TextureHelper.LoadImageLocal(FileHelper.GetPathGameDB(linePathFile.Text));
        }
        MaterialData materialData = new MaterialData();
        materialData.pathTexture = linePathFile.Text;
        if (lineName.Text == string.Empty)
        {
            lineName.Text = lineName.GetHashCode().ToString();
        }
        
        materialData.name = lineName.Text;
        materialData.heightTexture = image.GetHeight();
        materialData.widhtTexture = image.GetWidth();
        materialData.divisionPixelX = (int) spinPixel_X.Value;
        materialData.divisionPixelY = (int)spinPixel_Y.Value;
        materialData.type = typeMaterial.GetItemId(typeMaterial.GetSelectedId());
        if (lineId.Text == string.Empty)
        {
            int idnext = DataBaseManager.Instance.NextID<MaterialData>();
            string path = FileHelper.CopyFileToAssetExternals(linePathFile.Text,"Material", idnext.ToString());
            materialData.pathTexture = path;
            DataBaseManager.Instance.InsertUpdate(materialData);
            
        }
        else
        {
            materialData.id = int.Parse(lineId.Text);
            //FileHelper.CopyFileToAssetExternals(linePathFile.Text, "Material", idnext.ToString());
            DataBaseManager.Instance.InsertUpdate(materialData,int.Parse(lineId.Text));
        }
        OnRequestUpdate?.Invoke();
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
