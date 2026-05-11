using Godot;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;

public partial class ControlSlotTileDual : PanelContainer
{
    ControlBlackyAtlasTexture commonBlackyAtlasTexture;

    DualTileSlot slotData;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
		ButtonAgregar.Pressed += ButtonAgregar_Pressed;
	}
	private void ButtonAgregar_Pressed()
	{
		var newDualTileData = slotData.CreateDataNextHeight();
		CreateSlotData(newDualTileData, commonBlackyAtlasTexture);
    }
    private enum Corner
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
    private void CreateSlotData(DualTileData dualTileData, ControlBlackyAtlasTexture commonBlackyAtlasTexture)
	{
		var controlArrayTileDual = RuntimeServices.NodeRegistry.Create<ControlArrayTileDual>();
		VBoxContainerItems.AddChild(controlArrayTileDual);
		controlArrayTileDual.SetDualTileData(dualTileData, commonBlackyAtlasTexture);
		LabelSlot.Text = $"Slot {slotData.Slot}";
		controlArrayTileDual.OnControlArrayTileDualRemoved += ControlArrayTileDual_OnControlArrayTileDualRemoved;
        CreateSlotNumbers(TextureTemplate, slotData.Slot);
		 // TextureTemplate aqui, debe ir los numeros

    }
    private void CreateSlotNumbers(TextureRect textureRect, int slot)
    {
        string bits = Convert.ToString(slot, 2).PadLeft(4, '0');

        // Orden:
        // 1 = TopLeft
        // 1 = TopRight
        // 0 = BottomLeft
        // 1 = BottomRight

        CreateCornerLabel(textureRect, bits[0].ToString(), Corner.TopLeft);
        CreateCornerLabel(textureRect, bits[1].ToString(), Corner.TopRight);
        CreateCornerLabel(textureRect, bits[2].ToString(), Corner.BottomLeft);
        CreateCornerLabel(textureRect, bits[3].ToString(), Corner.BottomRight);
    }
    private void CreateCornerLabel(
    Control parent,
    string text,
    Corner corner)
    {
        Label label = new Label();

        label.Text = text;
        label.MouseFilter = Control.MouseFilterEnum.Ignore;

        parent.AddChild(label);

        switch (corner)
        {
            case Corner.TopLeft:
                label.Position = new Vector2(2, 0);
                break;

            case Corner.TopRight:
                label.Position = new Vector2(parent.Size.X - 12, 0);
                break;

            case Corner.BottomLeft:
                label.Position = new Vector2(2, parent.Size.Y - 18);
                break;

            case Corner.BottomRight:
                label.Position = new Vector2(parent.Size.X - 12, parent.Size.Y - 18);
                break;
        }
    }
    private void ControlArrayTileDual_OnControlArrayTileDualRemoved(ControlArrayTileDual dual)
    {
		slotData.RemoveData(dual.GetDualTileData().Height);
        VBoxContainerItems.RemoveChild(dual);
    }

    public void SetData(DualTileSlot data, ControlBlackyAtlasTexture commonBlackyAtlasTexture)
	{
		slotData = data;
		this.commonBlackyAtlasTexture = commonBlackyAtlasTexture;
		foreach (var item in data.GetAllDualTileData())
		{
            CreateSlotData(item,commonBlackyAtlasTexture);
        }		
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
