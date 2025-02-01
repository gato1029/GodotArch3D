using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;
using System.Collections.Generic;

public partial class WindowTileCreator : Window
{
    // Called when the node enters the scene tree for the first time.

    SpinBox pixel_X;
    SpinBox pixel_Y;
    ItemList itemListTiles;
    FileDialog fileDialog;
    LineEdit pathFile;
    OptionButton optionButton;
    TextureRect simpleTextureRect;
    Button guardarBDButton;

    VBoxContainer vBoxContainerTiles;
    VBoxContainer vBoxContainerBD;
    HBoxContainer leftContainer;
    public override void _Ready()
	{

        CloseRequested += WindowTileCreator_CloseRequested;
        GetNode<Button>("Panel/MarginContainer/SplitContainer/VBoxContainer/VBoxContainer/HBoxContainer/ButtonDisco").Pressed += Disco_Pressed;
        GetNode<Button>("Panel/MarginContainer/SplitContainer/VBoxContainer/VBoxContainer/HBoxContainer/ButtonBD").Pressed += BD_Pressed;
        GetNode<Button>("Panel/MarginContainer/SplitContainer/VBoxContainer/VBoxContainer/HBoxContainer2/ButtonDividir").Pressed += Dividir_Pressed;

        pathFile = GetNode<LineEdit>("Panel/MarginContainer/SplitContainer/VBoxContainer/VBoxContainer/HBoxContainer/LineEditPath");
        pixel_X = GetNode<SpinBox>("Panel/MarginContainer/SplitContainer/VBoxContainer/VBoxContainer/HBoxContainer2/SpinBoxX");
        pixel_Y= GetNode<SpinBox>("Panel/MarginContainer/SplitContainer/VBoxContainer/VBoxContainer/HBoxContainer2/SpinBoxY");

        itemListTiles = GetNode<ItemList>("Panel/MarginContainer/SplitContainer/VBoxContainer/ItemList");

        // -- Left Panel
        leftContainer = GetNode<HBoxContainer>("Panel/MarginContainer/SplitContainer/LeftContainer");

        vBoxContainerTiles = GetNode<VBoxContainer>("Panel/MarginContainer/SplitContainer/LeftContainer/VBoxContainer");
        vBoxContainerBD = GetNode<VBoxContainer>("Panel/MarginContainer/SplitContainer/LeftContainer/VBoxBD");

        optionButton = GetNode<OptionButton>("Panel/MarginContainer/SplitContainer/LeftContainer/VBoxContainer/HBoxContainer/OptionButton");
        optionButton.ItemSelected += OptionButton_ItemSelected;

        guardarBDButton = GetNode<Button>("Panel/MarginContainer/SplitContainer/LeftContainer/VBoxContainer/PanelSimpleTile/VBoxContainer/Button");
        guardarBDButton.Pressed += GuardarBD_Pressed;

        //----fileDialog
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

        //------ simpleTile
        simpleTextureRect = GetNode<TextureRect>("Panel/MarginContainer/SplitContainer/LeftContainer/VBoxContainer/PanelSimpleTile/VBoxContainer/TextureRect");
        SimpleTile();





        


        
        
    }

    private void OptionButton_ItemSelected(long index)
    {
        string selected = optionButton.GetItemText((int)index);
        switch (selected)
        {
            case "Simple Tile":
                SimpleTile();
                break;
            case "Animation Tile":
                break;
            case "Auto Tile":
                break;
            default:
                break;
        }
    }

    private void SimpleTile()
    {      
        if (!itemListTiles.IsConnected(ItemList.SignalName.ItemSelected, Callable.From<long>(ItemListTiles_ItemSelected)))
        {
            itemListTiles.SelectMode = ItemList.SelectModeEnum.Single;
            itemListTiles.Connect(ItemList.SignalName.ItemSelected, Callable.From<long>(ItemListTiles_ItemSelected));
        }
    }


    private void ItemListTiles_ItemSelected(long index)
    {
        var texture = itemListTiles.GetItemIcon((int)index);
        simpleTextureRect.Texture = texture;
      
    }

    private void WindowTileCreator_CloseRequested()
    {
        this.QueueFree();
    }

    private void OnFileSelected(string path)
    {
        pathFile.Text = path;
        GD.Print($"Archivo seleccionado: {path}");
    }
    private void GuardarBD_Pressed()
    {
       

        MaterialData materialData = new MaterialData();
        materialData.pathTexture = "demo5.png";
        DataBaseManager.Instance.Insert(materialData);

        var matdata = DataBaseManager.Instance.FindById<MaterialData>(1);

        matdata.divisionPixelX = 1;
    }

    private Texture LoadTexture(string filePath)
    {
        // Crear una instancia de Image para cargar el archivo
        Image image = new Image();
        Error result = image.Load(filePath);

        if (result == Error.Ok)
        {
            // Convertir la imagen cargada en una textura
            ImageTexture texture =  ImageTexture.CreateFromImage(image);
            return texture;
        }

        GD.PrintErr($"Error al cargar la imagen desde: {filePath}. Error: {result}");
        return null;
    }

    private List<Texture> SplitTexture(string texturePath, Vector2I cellSize)
    {
        List<Texture> textures = new List<Texture>();

        // Cargar la textura como Image
        Image image = new Image();
        Error result = image.Load(texturePath);

        if (result != Error.Ok)
        {
            GD.PrintErr($"Error al cargar la textura: {texturePath}. Error: {result}");
            return textures;
        }

        // Obtener el tamaño total de la imagen
        Vector2 imageSize = new Vector2(image.GetWidth(), image.GetHeight());

        // Iterar sobre las celdas de la imagen
        for (int y = 0; y < imageSize.Y; y += cellSize.Y)
        {
            for (int x = 0; x < imageSize.X; x += cellSize.X)
            {
                // Extraer la región de la imagen
                Rect2I rect = new Rect2I(new Vector2I(x, y), cellSize);
                Image subImage = image.GetRegion(rect);
                
                // Convertir la subimagen en una textura
                ImageTexture texture = ImageTexture.CreateFromImage(subImage);
               

                // Añadir a la lista de texturas
                textures.Add(texture);
            }
        }

        return textures;
    }

    public bool IsTextureEmpty(Texture texture)
    {
        ImageTexture imageTexture = texture as ImageTexture;
        // Convertir la textura a una instancia de Image
        Image image = imageTexture.GetImage();
        
        // Recorrer los píxeles y verificar si alguno no es completamente transparente
        for (int y = 0; y < image.GetHeight(); y++)
        {
            for (int x = 0; x < image.GetWidth(); x++)
            {
                // Obtener el color del píxel en (x, y)
                Color color = image.GetPixel(x, y);

                // Si el píxel no es completamente transparente
                if (color.A > 0.0f) // Cambia la condición según tu criterio
                {                    
                    return false; // La textura no está vacía
                }
            }
        }

      
        return true; // La textura está vacía
    }


    private void Dividir_Pressed()
    {

        itemListTiles.Clear();
        
        List<Texture> list = SplitTexture(pathFile.Text, new Vector2I((int)pixel_X.Value, (int)pixel_Y.Value));
        for (int i = 0; i < list.Count; i++)
        {
            Texture item = list[i];
            if (!IsTextureEmpty(item))
            {
                itemListTiles.AddItem("ID:" + i, (Texture2D)item);
            }            
        }        
    }

    private void BD_Pressed()
    {
        leftContainer.Visible = true;
        vBoxContainerBD.Visible = true;
        vBoxContainerTiles.Visible = false;
    }

    private void Disco_Pressed()
    {
        fileDialog.PopupCentered();
        
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
