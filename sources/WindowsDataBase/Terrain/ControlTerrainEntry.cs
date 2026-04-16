using Godot;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using System;
using System.Linq;

public partial class ControlTerrainEntry : MarginContainer
{
	public TerrainTileEntry data;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        data = new TerrainTileEntry();
        ButtonRemove.Pressed += ButtonRemove_Pressed;
    }

    private void ButtonRemove_Pressed()
    {
		QueueFree();
    }


    public void SetData(TerrainTileEntry data)
	{
		if (data!=null)
		{
			
            this.data = data;
			SpinBoxHeight.Value = data.height;
			foreach (var item in data.layersRelative)
			{
				switch (item.layerType)
				{
					case TerrinLayerType.Base:

						ControlBase.SetData(item.idAutoTile);
                        break;
					case TerrinLayerType.Superficie:
						ControlSuperficie.SetData(item.idAutoTile);
                        break;
					case TerrinLayerType.DecoracionBase:
                        ControlDecorBase.SetData(item.idAutoTile);
                        break;
					case TerrinLayerType.DecoracionSuperficie:
						ControlDecorSuperficie.SetData(item.idAutoTile);
                        break;
					default:
						break;
				}
			}

        }

    }
	public TerrainTileEntry GetData()
	{		
		data.height = (int)SpinBoxHeight.Value;
		data.layersRelative.Clear();
		if (ControlBase.GetData()!=null)
		{
            data.layersRelative.Add(new TerrainLayer(TerrinLayerType.Base, 0, ControlBase.GetData().id));
        }
        if (ControlDecorBase.GetData() != null)
        {
            data.layersRelative.Add(new TerrainLayer(TerrinLayerType.DecoracionBase, 2, ControlDecorBase.GetData().id));
        }
        if (ControlSuperficie.GetData() != null)
		{
			data.layersRelative.Add(new TerrainLayer(TerrinLayerType.Superficie, 1, ControlSuperficie.GetData().id));
        }
		if (ControlDecorSuperficie.GetData() != null)		
		{
            data.layersRelative.Add(new TerrainLayer(TerrinLayerType.DecoracionSuperficie, 2, ControlDecorSuperficie.GetData().id));
        }            		        
        return data;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
