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
    int mode = 0;
    MaterialData objectData;

    public event IFacadeWindow<MaterialData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI                                
        ButtonDividir.Pressed+= TextureSplit_Clik; 
        ButtonSearchFile.Pressed+= FileSearch;
        ButtonSave.Pressed += Guardar_Click;
        ButtonDelete.Pressed += ButtonDelete_Pressed;
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

        LineEditName.Text = objectData.name;
        LineEditId.Text = objectData.id.ToString();
        LineEditPath.Text = objectData.pathTexture;
        SpinBoxX.Value = objectData.divisionPixelX;
        SpinBoxY.Value = objectData.divisionPixelY;
        OptionButtonType.Selected = OptionButtonType.GetItemIndex(objectData.type);
        mode = 1;
        TextureSplit_Clik();
    }

    private void ButtonDelete_Pressed()
    {
        DataBaseManager.Instance.RemoveById<MaterialData>(objectData.id);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }
    private void FileSearch()
    {
        fileDialog.PopupCentered();
    }

    private void Guardar_Click()
    {
        if (LineEditPath.Text.StartsWith("AssetExternals"))
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
             image = TextureHelper.LoadImageLocal(LineEditPath.Text);
        }
        else
        {
            image = TextureHelper.LoadImageLocal(FileHelper.GetPathGameDB(LineEditPath.Text));
        }

        objectData.pathTexture = LineEditPath.Text;


        objectData.name = LineEditName.Text;
        objectData.heightTexture = image.GetHeight();
        objectData.widhtTexture = image.GetWidth();
        objectData.divisionPixelX = (int)SpinBoxX.Value;
        objectData.divisionPixelY = (int)SpinBoxY.Value;
        objectData.type = OptionButtonType.GetItemId(OptionButtonType.GetSelectedId());
        if (LineEditId.Text == string.Empty)
        {
            int idnext = DataBaseManager.Instance.NextID<MaterialData>();
            string path = FileHelper.CopyFileToAssetExternals(LineEditPath.Text,"Material", idnext.ToString());
            objectData.pathTexture = path;
            DataBaseManager.Instance.InsertUpdateLog(objectData);            
        }
        else
        {

            string path = FileHelper.CopyFileToAssetExternals(LineEditPath.Text, "Material", objectData.id.ToString());
            objectData.pathTexture = path;
            DataBaseManager.Instance.InsertUpdateLog(objectData, int.Parse(LineEditId.Text));
        }
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();

    }

    private void OnFileSelected(string path)
    {
        LineEditPath.Text = path;
        TextureSplit_Clik();
    }

    private void TextureSplit_Clik()
    {
        ItemListTiles.Clear();
        List<Texture> list;
        if (mode == 0)
        {
             list = TextureHelper.SplitTexture(LineEditPath.Text, new Vector2I((int)SpinBoxX.Value, (int)SpinBoxY.Value));
        }
        else
        {
             list = TextureHelper.SplitTexture(FileHelper.GetPathGameDB(LineEditPath.Text), new Vector2I((int)SpinBoxX.Value, (int)SpinBoxY.Value));
        }
        
        for (int i = 0; i < list.Count; i++)
        {
            Texture item = list[i];
            if (!TextureHelper.IsTextureEmpty(item))
            {
                ItemListTiles.AddItem("ID:" + i, (Texture2D)item);
            }
        }
    }
}
