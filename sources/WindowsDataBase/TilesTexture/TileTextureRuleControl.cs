using Godot;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;
using System.Reflection.Metadata.Ecma335;

public partial class TileTextureRuleControl : Control
{
    private bool isEditing = false;
    private float blinkTime = 0f;

    int idMaterial = -1;
    int idTileTexture = -1;
    int indexPosition = -1;

    NeighborCondition neighborCondition;
    bool isCenter = false;
    
    TileRuleTextureData tileRuleTextureData;

    WindowSearchTileMaterial windowLocal = null;
    WindowTileTextureRule windowTileTextureRule = null;
    WindowGroupTileTexture groupTileTexture;

    string ignorarImagen = "res://resources/Textures/internal/asterisk.png";
    string VacioImagen = "res://resources/Textures/internal/SquareEmpty.png";
    string LlenoImagen = "res://resources/Textures/internal/question-mark.png";
    string ExactoImagen = "res://resources/Textures/internal/tiles_4647616.png";
    string CentroImagen = "res://resources/Textures/iconos/center.png";

    string SeleccionActualImagen = "res://resources/Textures/iconos/Rectangular.png";
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
                        if (idMaterial == 0)
                        {
                            return;
                        }
                        // 🔥 NUEVO: avisar al grupo
                        groupTileTexture?.SetCurrent(this);
                        isEditing = true;
                        TextureImage.Modulate = new Color(0.6f, 0.8f, 1f, 1f);
                        blinkTime = 0f;

                        Vector2I globalPos = (Vector2I)GetScreenPosition();
                        Vector2I size = (Vector2I)GetGlobalRect().Size;
                        groupTileTexture.OpenMaterialWindow(
                            this,
                            new Vector2I(globalPos.X + size.X + 10, globalPos.Y)
                        );
                        
                    }
                    else
                    {
                        isEditing = false;
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
                    isEditing = false;
                    neighborCondition = NeighborCondition.Ignore;
                    TextureImage.Texture = GD.Load<Texture2D>(ignorarImagen);
                    OnNotifyChangued?.Invoke(this);
                    break;
                case MouseButton.Middle:
                    isEditing = false;
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
                    tileRuleTextureData.SetConditionByIndex(indexPosition, NeighborCondition.Ignore);
                    tileRuleTextureData.SetAnchorByIndex(indexPosition);
                }
                else
                {
                    TextureImage.Texture = GD.Load<Texture2D>(ignorarImagen);
                    tileRuleTextureData.SetConditionByIndex(indexPosition, NeighborCondition.Ignore);
                }
                
                break;
            case NeighborCondition.Empty:
                TextureImage.Texture = GD.Load<Texture2D>(VacioImagen);
                tileRuleTextureData.SetConditionByIndex(indexPosition, NeighborCondition.Empty);
                break;
            case NeighborCondition.Filled:
                TextureImage.Texture = GD.Load<Texture2D>(LlenoImagen);
                tileRuleTextureData.SetConditionByIndex(indexPosition, NeighborCondition.Filled);
                break;
            case NeighborCondition.Specific:
                TextureImage.Texture = GD.Load<Texture2D>(ExactoImagen);
                tileRuleTextureData.SetConditionByIndex(indexPosition, NeighborCondition.Specific);
                // 🔥 NUEVO: avisar al grupo
                groupTileTexture?.SetCurrent(this);
                isEditing = true;
                TextureImage.Modulate = new Color(0.6f, 0.8f, 1f, 1f);
                blinkTime = 0f;
                break;
            default:
                break;
        }        
    }
    public void SetMaterial(int idMaterial)
    { 
        this.idMaterial = idMaterial;
    }
    internal void SetData(NeighborCondition neighborCondition,int idMaterial=-1, int idIndex=0)
    {
        WindowTileTextureRule_OnNotifySelection(neighborCondition, false);

        if (idMaterial !=-1)
        {
            this.idMaterial = idMaterial;
            idTileTexture = idIndex;
            TextureImage.Texture = MaterialManager.Instance.GetAtlasTextureInternal(idMaterial, idTileTexture);
            tileRuleTextureData.SetConditionByIndex(indexPosition, NeighborCondition.Specific, idTileTexture);
        }        
    }


    public bool HasMaterial() => idMaterial != -1;

    public int GetMaterialId() => idMaterial;

    public int GetTileIndex() => idTileTexture;

    public void SetMaterialData(int materialId, int tileIndex)
    {

        idMaterial = materialId;
        idTileTexture = tileIndex;
        TextureImage.Texture = MaterialManager.Instance.GetAtlasTextureInternal(materialId, tileIndex);
        tileRuleTextureData.SetConditionByIndex(indexPosition, NeighborCondition.Specific, idTileTexture);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        if (!isEditing) return;

        blinkTime += (float)delta * 6f; // velocidad

        float alpha = 0.5f + 0.5f * Mathf.Sin(blinkTime);
        TextureImage.Modulate = new Color(0.9f, 0.9f, 0.9f, alpha);
    }

    internal void SetGroup(WindowGroupTileTexture groupTileTexture, TileRuleTextureData tileRuleTextureData, int indexPosition, int idMaterial)
    {
        this.idMaterial = idMaterial;
        this.tileRuleTextureData = tileRuleTextureData;
        this.indexPosition = indexPosition;
        this.groupTileTexture = groupTileTexture;

        WindowTileTextureRule_OnNotifySelection(tileRuleTextureData.GetConditionByIndex(indexPosition).Condition,tileRuleTextureData.IsAnchor(indexPosition));
        if (tileRuleTextureData.GetConditionByIndex(indexPosition).Condition == NeighborCondition.Specific)
        {
            idTileTexture = tileRuleTextureData.GetConditionByIndex(indexPosition).TargetID;
            TextureImage.Texture = MaterialManager.Instance.GetAtlasTextureInternal(idMaterial, idTileTexture);
            tileRuleTextureData.SetConditionByIndex(indexPosition, NeighborCondition.Specific, idTileTexture);
        }


    }

    public void StopEditing()
    {
        isEditing = false;
        TextureImage.Modulate = Colors.White;
    }
}
