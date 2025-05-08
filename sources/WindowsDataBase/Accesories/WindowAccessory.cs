using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using LiteDB;
using System;
using System.Collections.Generic;

public partial class WindowAccessory : Window
{

	
    PackedScene packedScene = GD.Load<PackedScene>("res://sources/WindowsDataBase/Accesories/AccesoryControl.tscn");
    public override void _Ready()
	{
        InitializeUI();
        ButtonNew.Pressed += ButtonNew_Pressed;        
        foreach (AccesoryClassType type in Enum.GetValues(typeof(AccesoryClassType)))
        {
            OptionButtonClass.AddItem(type.ToString());
        }
        foreach (AccesoryType type in Enum.GetValues(typeof(AccesoryType)))
        {
            OptionButtonType.AddItem(type.ToString());
        }

        foreach (AccesoryBodyPartType type in Enum.GetValues(typeof(AccesoryBodyPartType)))
        {
            OptionButtonBody.AddItem(type.ToString());
        }
        OptionButtonBody.ItemSelected += OptionButtonSelectection;
        OptionButtonType.ItemSelected += OptionButtonSelectection;
        OptionButtonClass.ItemSelected += OptionButtonSelectection;

        OptionButtonSelectection(0);
        ItemListView.GuiInput += ItemListView_GuiInput;
    }

    private void ItemListView_GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            // Verifica doble clic con el botón izquierdo
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.DoubleClick)
            {
                // Obtener el índice del ítem bajo el mouse
                int itemIndex = ItemListView.GetItemAtPosition(mouseEvent.Position, true);

                if (itemIndex >= 0)
                {
                    int idInternal = (int)ItemListView.GetItemMetadata(itemIndex);
                    var objectData = DataBaseManager.Instance.FindById<AccessoryData>(idInternal);
                    accessoryControl = packedScene.Instantiate<AccessoryControl>();
                    accessoryControl.PopupExclusiveCentered(this);
                    accessoryControl.SetData(objectData);
                    accessoryControl.OnNotifyChangued += AccessoryControl_OnNotifyChangued;
                }
            }
        }
    }

    private void AccessoryControl_OnNotifyChangued(AccessoryControl objectControl)
    {
        OptionButtonSelectection(0);
    }

    private void OptionButtonSelectection(long index)
    {
        ItemListView.Clear();
        var p1 =(AccesoryClassType)OptionButtonClass.GetSelectedId();
        var p2 = (AccesoryType)OptionButtonType.GetSelectedId();
        var p3 = (AccesoryBodyPartType)OptionButtonBody.GetSelectedId();
        var conditions = new List<string>();
        var parameters = new BsonDocument();

        // Solo agrega si no es NONE o 0
        if (p1 != AccesoryClassType.NONE && p1 != 0)
        {
            conditions.Add("accesoryClassType = @accesoryClassType");
            parameters["accesoryClassType"] = p1.ToString();
        }
        if (p2 != AccesoryType.NONE && p2 != 0)
        {
            conditions.Add("accesoryType = @accesoryType");
            parameters["accesoryType"] = p2.ToString();
        }
        if (p3 != AccesoryBodyPartType.NONE && p3 != 0)
        {
            conditions.Add("accesoryBodyPartType = @accesoryBodyPartType");
            parameters["accesoryBodyPartType"] = p3.ToString();
        }

        // Construir expresión solo si hay condiciones
        BsonExpression bsonExpression = null;
        List<AccessoryData> result = new List<AccessoryData>();
        if (conditions.Count > 0)
        {
            string expressionText = string.Join(" AND ", conditions);
            bsonExpression = BsonExpression.Create(expressionText, parameters);
            result = DataBaseManager.Instance.FindAllFilter<AccessoryData>(bsonExpression);
        }
        else
        {
            result = DataBaseManager.Instance.FindAll<AccessoryData>();
        }

       
        foreach (AccessoryData accessoryData in result)
        {
            int id = ItemListView.AddItem(accessoryData.name,accessoryData.textureVisual);
            ItemListView.SetItemMetadata(id, accessoryData.id);
        }
    }

    AccessoryControl accessoryControl;
    private void ButtonNew_Pressed()
    {
        accessoryControl= packedScene.Instantiate<AccessoryControl>();
        accessoryControl.PopupExclusiveCentered(this);      
    }



    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
