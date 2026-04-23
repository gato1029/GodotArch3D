using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;
using System.Reflection.Metadata.Ecma335;

public partial class TileTextureControl : Control
{
    // -1 mantener,-2 eliminar, >=0 nuevo ID
    int idMaterial    = -1;
    int idTileTexture = -1;
    int indexPosition = -1;

    TileRuleTextureData tileRuleTextureData;

    private bool isEditing = false;
    private float blinkTime = 0f;
    WindowGroupTileTexture groupTileTexture;
    WindowSearchTileMaterial windowLocal = null;

    string mantenerImagen = "res://resources/Textures/internal/dot-square.png";
    string eliminarImagen = "res://resources/Textures/internal/clear.png";
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
                    if (idMaterial ==0)
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
                    break;
                case MouseButton.Right:
                    isEditing = false;
                    TextureImage.Modulate = Colors.White;
                    if (idMaterial > 0)
                    {
                        // Tenía material → pasar a mantener
                        idMaterial = -1;
                        TextureImage.Texture = GD.Load<Texture2D>(mantenerImagen);
                        tileRuleTextureData.SetOutputByIndex(indexPosition, -1,-1);
                    }
                    else if (idMaterial == -1)
                    {
                        // Estaba en mantener → pasar a eliminar
                        idMaterial = 0;
                        TextureImage.Texture = GD.Load<Texture2D>(eliminarImagen);
                        tileRuleTextureData.SetOutputByIndex(indexPosition, -2, -1);
                    }
                    else if (idMaterial == 0)
                    {
                        // Estaba en eliminar → volver a mantener
                        idMaterial = -1;
                        TextureImage.Texture = GD.Load<Texture2D>(mantenerImagen);
                        tileRuleTextureData.SetOutputByIndex(indexPosition, -1, -1);
                    }

                    OnNotifyChangued?.Invoke(this);                    
                    break;
                case MouseButton.Middle:

                    break;
            }
        }
    }
    public void SetMaterial(int idMaterial)
    {
        this.idMaterial = idMaterial;
    }
    public void SetData(int idMaterial, int idIndex)
    {
        this.idMaterial = idMaterial;
        idTileTexture = idIndex;                
        TextureImage.Texture = MaterialManager.Instance.GetAtlasTextureInternal(idMaterial, idTileTexture);
        tileRuleTextureData.SetOutputByIndex(indexPosition, idTileTexture,idMaterial);
    }

    public bool HasMaterial() => idMaterial != -1;

    public int GetMaterialId() => idMaterial;

    public int GetTileIndex() => idTileTexture;

    public void SetMaterialData(int materialId, int tileIndex)
    {
        idMaterial = materialId;
        idTileTexture = tileIndex;
        TextureImage.Texture =MaterialManager.Instance.GetAtlasTextureInternal(materialId, tileIndex);        
        TextureImage.Modulate = Colors.White;

        tileRuleTextureData.SetOutputByIndex(indexPosition, idTileTexture,idMaterial);
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
        if (!isEditing) return;

        blinkTime += (float)delta * 6f; // velocidad

        float alpha = 0.5f + 0.5f * Mathf.Sin(blinkTime);
        TextureImage.Modulate = new Color(0.9f, 0.9f, 0.9f, alpha);
    }

    public void StopEditing()
    {
        isEditing = false;
        TextureImage.Modulate = Colors.White;
    }

    
    internal void SetGroup(WindowGroupTileTexture groupTileTexture,TileRuleTextureData tileRuleTextureData, int indexPosition, int materialId)
    {
        this.idMaterial = materialId;
       this.tileRuleTextureData = tileRuleTextureData;
       this.indexPosition = indexPosition;
       this.groupTileTexture = groupTileTexture;
        if (tileRuleTextureData.GetOutputByIndex(indexPosition).TileID!=-1)
        {
            idTileTexture = tileRuleTextureData.GetOutputByIndex(indexPosition).TileID;
            this.idMaterial = tileRuleTextureData.GetOutputByIndex(indexPosition).MaterialID;
            TextureImage.Texture = MaterialManager.Instance.GetAtlasTextureInternal(materialId, idTileTexture);
            TextureImage.Modulate = Colors.White;
        }
    }
}
