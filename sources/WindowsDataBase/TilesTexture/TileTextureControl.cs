using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;

public partial class TileTextureControl : Control
{
    // -1 mantener, 0 eliminar, >0 nuevo ID
    int idMaterial    = -1;
    int idTileTexture = -1;


    WindowSearchTileMaterial windowLocal = null;

    string mantenerImagen = "res://resources/Textures/internal/dot-square.png";
    string eliminarImagen = "res://resources/Textures/internal/clear.png";
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
		TextureImage.GuiInput += _GuiInputTexture;
    }

    private void _GuiInputTexture(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            switch (mouseEvent.ButtonIndex)
            {
                case MouseButton.Left:
                    if (windowLocal == null)
                    {
                        windowLocal = GD.Load<PackedScene>("res://sources/KuroTiles/WindowSearchTileMaterial.tscn").Instantiate<WindowSearchTileMaterial>();
                        // Escuchar cuando se cierre

                        AddChild(windowLocal);

                        windowLocal.TreeExited += () => windowLocal = null;
                        //windowLocal.PopupHide += () => windowLocal = null;
                        // 🔸 Colocar al costado derecho del Control
                        Vector2I globalPos = (Vector2I)GetScreenPosition();
                        Vector2I size = (Vector2I)GetGlobalRect().Size;

                        // Si la ventana tiene tamaño fijo:
                        windowLocal.Position = new Vector2I(globalPos.X + size.X + 10, globalPos.Y);
                        windowLocal.OnNotifySelectionIndex += WindowLocal_OnNotifySelectionIndex;
                        windowLocal.Popup();
                        if (idMaterial != -1)
                        {

                            windowLocal.SetSelection(idMaterial, idTileTexture);
                        }
                    }
                    break;
                case MouseButton.Right:
                    if (idMaterial > 0)
                    {
                        // Tenía material → pasar a mantener
                        idMaterial = -1;
                        TextureImage.Texture = GD.Load<Texture2D>(mantenerImagen);
                    }
                    else if (idMaterial == -1)
                    {
                        // Estaba en mantener → pasar a eliminar
                        idMaterial = 0;
                        TextureImage.Texture = GD.Load<Texture2D>(eliminarImagen);
                    }
                    else if (idMaterial == 0)
                    {
                        // Estaba en eliminar → volver a mantener
                        idMaterial = -1;
                        TextureImage.Texture = GD.Load<Texture2D>(mantenerImagen);
                    }

                    OnNotifyChangued?.Invoke(this);                    
                    break;
                case MouseButton.Middle:

                    break;
            }
        }
    }
    internal void SetData(int idMaterial, int idIndex)
    {
        this.idMaterial = idMaterial;
        idTileTexture = idIndex;                
        TextureImage.Texture = MaterialManager.Instance.GetAtlasTextureInternal(idMaterial, idTileTexture);
    }
    private void WindowLocal_OnNotifySelectionIndex(int index, MaterialData materialData)
    {
        idTileTexture = index;
        idMaterial = materialData.id;
        TextureImage.Texture = MaterialManager.Instance.GetAtlasTextureInternal(materialData.id, index);
    }
    
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
