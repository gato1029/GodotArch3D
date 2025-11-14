using Godot;
using GodotEcsArch.sources.KuroTiles;
using GodotFlecs.sources.KuroTiles;
using System;
using static CustomButtonRule;

public partial class ControlKuroRuleItem : MarginContainer
{
    // Called when the node enters the scene tree for the first time.
    public ButtonWorkingMode WorkingMode { get; set; } = ButtonWorkingMode.Group;

	public TileRuleTemplate tileRuleTemplate { get; set; } = new TileRuleTemplate();
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
		ConfigPositions();
		SetWorkingMode(ButtonWorkingMode.AllGroups);
		CBCenter.SetGroup(0,null);
        CBCentralDebajo.SetGroup(0,null);

        CheckButtonRandom.Pressed += CheckButtonRandom_Pressed;
    }

    private void CheckButtonRandom_Pressed()
    {
		tileRuleTemplate.IsRandomTiles = CheckButtonRandom.ButtonPressed;
    }

    void ConfigPositions()
	{
		CBAbajo.neighborPosition = NeighborPosition.Abajo;
		CBArriba.neighborPosition = NeighborPosition.Arriba;
		CBIzquierda.neighborPosition = NeighborPosition.Izquierda;
		CBDerecha.neighborPosition = NeighborPosition.Derecha;
		CBAbajoIzquierda.neighborPosition = NeighborPosition.AbajoIzquierda;
		CBAbajoDerecha.neighborPosition = NeighborPosition.AbajoDerecha;
		CBArribaIzquierda.neighborPosition = NeighborPosition.ArribaIzquierda;
		CBArribaDerecha.neighborPosition = NeighborPosition.ArribaDerecha;
        CBCentralDebajo.neighborPosition = NeighborPosition.DebajoCentro;

		tileRuleTemplate.neighborConditionTemplate[(int)NeighborPosition.Abajo] = CBAbajo.neighborConditionTemplate;
        tileRuleTemplate.neighborConditionTemplate[(int)NeighborPosition.Arriba] = CBArriba.neighborConditionTemplate;
		tileRuleTemplate.neighborConditionTemplate[(int)NeighborPosition.Izquierda] = CBIzquierda.neighborConditionTemplate;
		tileRuleTemplate.neighborConditionTemplate[(int)NeighborPosition.Derecha] = CBDerecha.neighborConditionTemplate;
		tileRuleTemplate.neighborConditionTemplate[(int)NeighborPosition.AbajoIzquierda] = CBAbajoIzquierda.neighborConditionTemplate;
		tileRuleTemplate.neighborConditionTemplate[(int)NeighborPosition.AbajoDerecha] = CBAbajoDerecha.neighborConditionTemplate;
		tileRuleTemplate.neighborConditionTemplate[(int)NeighborPosition.ArribaIzquierda] = CBArribaIzquierda.neighborConditionTemplate;
		tileRuleTemplate.neighborConditionTemplate[(int)NeighborPosition.ArribaDerecha] = CBArribaDerecha.neighborConditionTemplate;
		tileRuleTemplate.TileCentral = new TileTemplate();
        tileRuleTemplate.neighborConditionTemplateCenter = CBCentralDebajo.neighborConditionTemplate;
        CBCenter.OnNotifyChangued += CBCenter_OnNotifyChangued;
    }

    private void CBCenter_OnNotifyChangued(CustomButtonRule objectControl)
    {
		tileRuleTemplate.TileCentral.idGroup = objectControl.groupIdTemp;
    }

    public void SetCentralTile(Texture2D texture2D = null, int idGroup = 0, long idTile = 0)
	{		
		CBCenter.SetGroup(idGroup, texture2D);
        CBCenter.UpdateIcon();
        tileRuleTemplate.TileCentral.idGroup = idGroup;
		tileRuleTemplate.TileCentral.idTileSprite = (int)idTile;

        for (int i = 0; i < tileRuleTemplate.RandomTiles.Count; i++)
		{
            TileTemplate item = tileRuleTemplate.RandomTiles[i];
            item.idGroup = idGroup;
			if (i==0)
			{
                tileRuleTemplate.TileCentral.idTileSprite = item.idTileSprite;
            }            
        }
		
    }
    public void SetCentralTile(TileTemplate tileTemplate)
    {
		if (tileTemplate.idGroup!=0)
		{
            var text = GroupManager.Instance.GetData(tileTemplate.idGroup);
            CBCenter.SetGroup(tileTemplate.idGroup, text.textureVisual);
            CBCenter.State = NeighborType.LlenoMismoGrupo;
            CBCenter.UpdateIcon();            
            tileRuleTemplate.TileCentral.idGroup = tileTemplate.idGroup;
            tileRuleTemplate.TileCentral.idTileSprite = (int)tileTemplate.idTileSprite;

            for (int i = 0; i < tileRuleTemplate.RandomTiles.Count; i++)
            {
                TileTemplate item = tileRuleTemplate.RandomTiles[i];
                item.idGroup = tileTemplate.idGroup;
                if (i == 0)
                {
                    tileRuleTemplate.TileCentral.idTileSprite = item.idTileSprite;
                }
            }
        }
		else
		{
            CBCenter.SetGroup(0, null);
        }
    }
    public void SetWorkingMode(ButtonWorkingMode mode, Texture2D texture2D = null, int idGroup=0, long idTile=0)
	{
		
        WorkingMode = mode;
        switch (WorkingMode)
		{
			case ButtonWorkingMode.Group:
				CBAbajo.SetGroup(idGroup, texture2D);
				CBAbajoDerecha.SetGroup(idGroup, texture2D);
				CBAbajoIzquierda.SetGroup(idGroup, texture2D);	
				CBArriba.SetGroup(idGroup, texture2D);
				CBArribaDerecha.SetGroup(idGroup, texture2D);
				CBArribaIzquierda.SetGroup(idGroup, texture2D);
				CBDerecha.SetGroup(idGroup, texture2D);
				CBIzquierda.SetGroup(idGroup, texture2D);
				CBCenter.SetGroup(idGroup, texture2D);
                CBCentralDebajo.SetGroup(idGroup, texture2D);
                break;
			case ButtonWorkingMode.TileEspecific:
                CBAbajo.SetTile(idTile, texture2D);
				CBAbajoDerecha.SetTile(idTile, texture2D);
				CBAbajoIzquierda.SetTile(idTile, texture2D);
				CBArriba.SetTile(idTile, texture2D);
				CBArribaDerecha.SetTile(idTile, texture2D);
				CBArribaIzquierda.SetTile(idTile, texture2D);
				CBDerecha.SetTile(idTile, texture2D);
				CBIzquierda.SetTile(idTile, texture2D);
                break;
			case ButtonWorkingMode.AllGroups:
				CBAbajo.SetAllGroup();
				CBAbajoDerecha.SetAllGroup();
				CBAbajoIzquierda.SetAllGroup();
				CBArriba.SetAllGroup();
				CBArribaDerecha.SetAllGroup();
				CBArribaIzquierda.SetAllGroup();
				CBDerecha.SetAllGroup();
				CBIzquierda.SetAllGroup();

                break;
			default:
				break;
		}
		
		
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    internal void SetData(TileRuleTemplate tileRuleTemplateData)
    {
        tileRuleTemplate = tileRuleTemplateData;
		if (tileRuleTemplate == null)
        {
			tileRuleTemplate =  new TileRuleTemplate();
            ConfigPositions();
            SetWorkingMode(ButtonWorkingMode.AllGroups);
            CBCenter.SetGroup(0, null);
            CBCentralDebajo.SetGroup(0, null);
        }

        CheckButtonRandom.ButtonPressed = tileRuleTemplate.IsRandomTiles;
        //ConfigPositions();
        CBCentralDebajo.SetData(tileRuleTemplate.neighborConditionTemplateCenter);

        CBAbajo.SetData(tileRuleTemplate.GetNeigbordCondition(NeighborPosition.Abajo));
        CBAbajoDerecha.SetData(tileRuleTemplate.GetNeigbordCondition(NeighborPosition.AbajoDerecha));
        CBAbajoIzquierda.SetData(tileRuleTemplate.GetNeigbordCondition(NeighborPosition.AbajoIzquierda));
        CBArriba.SetData(tileRuleTemplate.GetNeigbordCondition(NeighborPosition.Arriba));
        CBArribaDerecha.SetData(tileRuleTemplate.GetNeigbordCondition(NeighborPosition.ArribaDerecha));
        CBArribaIzquierda.SetData(tileRuleTemplate.GetNeigbordCondition(NeighborPosition.ArribaIzquierda));
        CBDerecha.SetData(tileRuleTemplate.GetNeigbordCondition(NeighborPosition.Derecha));
        CBIzquierda.SetData(tileRuleTemplate.GetNeigbordCondition(NeighborPosition.Izquierda));
    }
}
