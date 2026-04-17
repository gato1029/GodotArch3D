using Godot;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;

public partial class TileTextureRuleControl : Control
{

    int idMaterial = -1;
    int idTileTexture = -1;
    NeighborCondition neighborCondition;
    bool isCenter = false;
    WindowSearchTileMaterial windowLocal = null;
    WindowTileTextureRule windowTileTextureRule = null;

    string ignorarImagen = "res://resources/Textures/internal/asterisk.png";
    string VacioImagen = "res://resources/Textures/internal/SquareEmpty.png";
    string LlenoImagen = "res://resources/Textures/internal/question-mark.png";
    string ExactoImagen = "res://resources/Textures/internal/tiles_4647616.png";
    string CentroImagen = "res://resources/Textures/iconos/center.png";
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
                    if (neighborCondition == NeighborCondition.Specific )
                    {
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
                    }
                    else
                    {
                        if (windowTileTextureRule == null)
                        {
                            windowTileTextureRule = GD.Load<PackedScene>("res://sources/WindowsDataBase/TilesTexture/WindowTileTextureRule.tscn").Instantiate<WindowTileTextureRule>();
                            AddChild(windowTileTextureRule);
                            windowTileTextureRule.TreeExited += () => windowTileTextureRule = null;
                            // 🔸 Colocar al costado derecho del Control
                            Vector2I globalPos = (Vector2I)GetScreenPosition();
                            Vector2I size = (Vector2I)GetGlobalRect().Size;
                            windowTileTextureRule.Position = new Vector2I(globalPos.X + size.X + 10, globalPos.Y);
                            windowTileTextureRule.OnNotifySelection += WindowTileTextureRule_OnNotifySelection;
                            windowTileTextureRule.Popup();

                        }
                    }
                 


                    break;
                case MouseButton.Right:
                    neighborCondition = NeighborCondition.Ignore;
                    TextureImage.Texture = GD.Load<Texture2D>(ignorarImagen);
                    OnNotifyChangued?.Invoke(this);
                    break;
                case MouseButton.Middle:

                    break;
            }
        }
    }

    private void WindowTileTextureRule_OnNotifySelection(NeighborCondition neighborCondition, bool isCenter)
    {
        this.neighborCondition = neighborCondition;
        switch (neighborCondition)
        {
            case NeighborCondition.Ignore:
                if (isCenter)
                {
                    TextureImage.Texture = GD.Load<Texture2D>(CentroImagen);
                }
                else
                {
                    TextureImage.Texture = GD.Load<Texture2D>(ignorarImagen);
                }
                
                break;
            case NeighborCondition.Empty:
                TextureImage.Texture = GD.Load<Texture2D>(VacioImagen);
                break;
            case NeighborCondition.Filled:
                TextureImage.Texture = GD.Load<Texture2D>(LlenoImagen);
                break;
            case NeighborCondition.Specific:
                TextureImage.Texture = GD.Load<Texture2D>(ExactoImagen);
                break;
            default:
                break;
        }        
    }

    internal void SetData(NeighborCondition neighborCondition,int idMaterial=-1, int idIndex=0)
    {
        WindowTileTextureRule_OnNotifySelection(neighborCondition, false);

        if (idMaterial !=-1)
        {
            this.idMaterial = idMaterial;
            idTileTexture = idIndex;
            TextureImage.Texture = MaterialManager.Instance.GetAtlasTextureInternal(idMaterial, idTileTexture);
        }        
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
