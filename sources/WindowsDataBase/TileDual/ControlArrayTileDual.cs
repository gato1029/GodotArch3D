using Godot;
using GodotEcsArch.sources.Helpers;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;

public partial class ControlArrayTileDual : PanelContainer
{
    ControlBlackyAtlasTexture commonBlackyAtlasTexture;
    // Called when the node enters the scene tree for the first time.
    DualTileData dualTileData;
    public event Action<ControlArrayTileDual> OnControlArrayTileDualRemoved;
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        ButtonAgregar.Pressed += ButtonAgregar_Pressed;                
        ButtonRemoverTodo.Pressed += ButtonRemoverTodo_Pressed;
    }

    private void ButtonRemoverTodo_Pressed()
    {
        //if (dualTileData.Height>0)
        {
            OnControlArrayTileDualRemoved?.Invoke(this);
        }
        
    }

    public DualTileData GetDualTileData()
    {
        return dualTileData;        
    }

    public void SetDualTileData(DualTileData dualTileData, ControlBlackyAtlasTexture commonBlackyAtlasTexture)
    {
        this.commonBlackyAtlasTexture = commonBlackyAtlasTexture;
        this.dualTileData = dualTileData;        
        VBoxContainerItems.ClearChildrens();
        foreach (var part in dualTileData.Parts)
        {
            CreateDualTilePartData(part);                        
        }
        if (dualTileData.Height==0)
        {
            LabelAltura.Text = "Generico";
        }
        else
        {
            LabelAltura.Text = $"Altura: {dualTileData.Height}";
        }        
    }
    private void CreateDualTilePartData(DualTilePart dualTilePart)
    {
        var controlTileDual = RuntimeServices.NodeRegistry.Create<ControlTileDual>();
        VBoxContainerItems.AddChild(controlTileDual);
        controlTileDual.SetDualTilePart(dualTilePart);
        controlTileDual.SetCommonBlackyAtlasTexture(commonBlackyAtlasTexture);
        controlTileDual.OnDualTilePartRemoved += ControlTileDual_OnDualTilePartRemoved;
    }

    private void ControlTileDual_OnDualTilePartRemoved(ControlTileDual obj)
    {
        dualTileData.RemovePart(obj.GetDualTilePart());
        VBoxContainerItems.RemoveChild(obj);
    }

    private void ButtonAgregar_Pressed()
    {
       var newPart = dualTileData.CreateDualTilePart();
       CreateDualTilePartData (newPart);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

	}
}
