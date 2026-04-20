using Godot;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;

public partial class RuleTextureControl : PanelContainer
{
    [Export] public string GroupName = "draggable_items";
    private string widgetPath = "res://sources/WindowsDataBase/TilesTexture/TileTextureControl.tscn";
    private PackedScene _widgetScene;

    private string widgetRulePath = "res://sources/WindowsDataBase/TilesTexture/TileTextureRuleControl.tscn";
    private PackedScene _widgetRuleScene;

    WindowGroupTileTexture groupTileTexture;

    TileRuleTextureData tileRuleTextureData;

    //
    int position = 0;
    int idMaterial = 0;
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        tileRuleTextureData = new TileRuleTextureData();
        SceneRegistry.Register(this);
        // 1. Cargar la escena del widget
        _widgetScene = GD.Load<PackedScene>(widgetPath);
        _widgetRuleScene = GD.Load<PackedScene>(widgetRulePath);

        // Hacer que el contenedor ocupe todo el espacio de este Control
        FixedGridTiles.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        FixedGridRules.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        // Ejemplo: Crear una rejilla inicial de 3x3
        
        SpinBoxX.ValueChanged += SpinBoxX_ValueChanged;
        SpinBoxY.ValueChanged += SpinBoxY_ValueChanged;

        KuroTextureButtonDelete.Pressed += KuroTextureButtonDelete_Pressed;
        KuroCheckButtonSwitch.Pressed += KuroCheckButtonSwitch_Pressed;
        KuroCheckButtonSwitch_Pressed();

        //
        AddToGroup(GroupName);
        MouseFilter = MouseFilterEnum.Stop;

        // Configurar hijos para que no bloqueen el drag
        SetupChildrenMouseFilter(this);

        // Detectar cambios de tamaño
        foreach (var child in GetChildren())
        {
            if (child is Control c)
                c.MinimumSizeChanged += RefreshMinimumSize;
        }

        ChildEnteredTree += (node) => {
            if (node is Control child)
            {
                SetupChild(child);
                child.MinimumSizeChanged += RefreshMinimumSize;
                RefreshMinimumSize();
            }
        };

        RefreshMinimumSize();
    }
    public void RefreshMinimumSize()
    {
        Vector2 maxChildSize = Vector2.Zero;

        foreach (var child in GetChildren())
        {
            if (child is Control c && c.Visible)
            {
                Vector2 childMin = c.GetCombinedMinimumSize();
                maxChildSize.X = Mathf.Max(maxChildSize.X, childMin.X);
                maxChildSize.Y = Mathf.Max(maxChildSize.Y, childMin.Y);
            }
        }

        if (CustomMinimumSize != maxChildSize)
        {
            CustomMinimumSize = maxChildSize;
            UpdateMinimumSize();
        }

        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        SizeFlagsVertical = SizeFlags.ExpandFill;
    }
    private void SetupChildrenMouseFilter(Node parent)
    {
        foreach (Node child in parent.GetChildren())
        {
            if (child is Control controlChild)
            {
                SetupChild(controlChild);
            }
        }
    }

    private void SetupChild(Control child)
    {
        child.MouseFilter = MouseFilterEnum.Pass;

        if (child.GetChildCount() > 0)
            SetupChildrenMouseFilter(child);
    }
    public override Variant _GetDragData(Vector2 atPosition)
    {
        var preview = (Control)Duplicate();
        preview.Modulate = new Color(1, 1, 1, 0.6f);
        SetDragPreview(preview);
        return this;
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        return data.As<Node>() is RuleTextureControl;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        var draggedItem = data.As<Control>();
        if (draggedItem == null || draggedItem == this) return;

        var container = GetParent();
        if (container == null) return;

        int dropIndex = GetIndex();

        // 👉 mejor para listas verticales
        bool dropAfter = atPosition.Y > Size.Y * 0.5f;

        if (dropAfter)
            dropIndex++;

        if (draggedItem.GetParent() == container && draggedItem.GetIndex() < dropIndex)
            dropIndex--;

        container.MoveChild(draggedItem, Mathf.Clamp(dropIndex, 0, container.GetChildCount()));

        if (container is Container c)
            c.QueueSort();

        CallDeferred(nameof(UpdatePositions), container);
    }
    private void UpdatePositions(Node container)
    {
        foreach (var child in container.GetChildren())
        {
            if (child is RuleTextureControl item)
                item.SetPosition(child.GetIndex());
        }
    }
    public void SetPosition(int position)
    {
        this.position = position;
        LabelPosition.Text = (position + 1).ToString("D2");
    }
    public void SetMaterial(int idMaterial)
    {
        this.idMaterial = idMaterial;

        foreach (var child in FixedGridTiles.GetChildren())
        {
            if (child is TileTextureControl item)
            {
                item.SetMaterial(idMaterial);
            }
        }


        foreach (var child in FixedGridRules.GetChildren())
        {
            if (child is TileTextureRuleControl item)
            {
                item.SetMaterial(idMaterial);
            }
        }
  

    }
    private void KuroTextureButtonDelete_Pressed()
    {
        var container = GetParent();

        QueueFree();

        if (container != null)
            CallDeferred(nameof(UpdatePositions), container);
    }

    public void SwitchTypeRules(bool rules)
    {
        KuroCheckButtonSwitch.ButtonPressed = rules;
        KuroCheckButtonSwitch_Pressed();
    }
    private void KuroCheckButtonSwitch_Pressed()
    {
        bool isOn = KuroCheckButtonSwitch.ButtonPressed;

        FixedGridTiles.Visible = isOn;
        FixedGridRules.Visible = !isOn;
        if (groupTileTexture!=null)
        {
            groupTileTexture.SetDisableSelection();
        }
        
    }

    private void SpinBoxY_ValueChanged(double value)
    {
       SetupGrid((int)SpinBoxX.Value, (int)value);
    }

    private void SpinBoxX_ValueChanged(double value)
    {
        SetupGrid((int)value, (int)SpinBoxY.Value);
    }

    /// <summary>
    /// Configura las dimensiones del grid y genera los widgets necesarios.
    /// </summary>
    public void SetupGrid(int rows, int columns)
    {
        if (FixedGridTiles == null || FixedGridRules == null) return;
        tileRuleTextureData.SetSize(rows, columns);

        // 🔹 Configurar ambos grids
        FixedGridTiles.Rows = rows;
        FixedGridTiles.Columns = columns;

        FixedGridRules.Rows = rows;
        FixedGridRules.Columns = columns;

        // 🔹 Limpiar ambos
        foreach (Node child in FixedGridTiles.GetChildren())
            child.QueueFree();

        foreach (Node child in FixedGridRules.GetChildren())
            child.QueueFree();

        int totalWidgets = rows * columns;

        // 🔹 Crear tiles
        for (int i = 0; i < totalWidgets; i++)
        {
            TileTextureControl instance = _widgetScene.Instantiate<TileTextureControl>();
            FixedGridTiles.AddChild(instance);
            instance.SetGroup(groupTileTexture, tileRuleTextureData, i, idMaterial);
        }

        // 🔹 Crear rules
        for (int i = 0; i < totalWidgets; i++)
        {
            TileTextureRuleControl instance = _widgetRuleScene.Instantiate<TileTextureRuleControl>();            
            FixedGridRules.AddChild(instance);
            instance.SetGroup(groupTileTexture, tileRuleTextureData, i, idMaterial);
            instance.SetData(NeighborCondition.Ignore);
        }

        // 🔹 Forzar refresh
        FixedGridTiles.QueueSort();
        FixedGridRules.QueueSort();

        // 2. Calcular el tamaño necesario para los Grids
        // Usamos Max para que el componente no "salte" al cambiar entre Tiles y Rules
        Vector2 gridMinSize = new Vector2(
            Mathf.Max(FixedGridTiles.GetCombinedMinimumSize().X, FixedGridRules.GetCombinedMinimumSize().X),
            Mathf.Max(FixedGridTiles.GetCombinedMinimumSize().Y, FixedGridRules.GetCombinedMinimumSize().Y)
        );

        Vector2 baseLayoutSize = GetCombinedMinimumSize() ;



        this.CustomMinimumSize = new Vector2(
    Mathf.Max(baseLayoutSize.X, gridMinSize.X),
   Mathf.Max(baseLayoutSize.Y , gridMinSize.Y)
);
        UpdateMinimumSize();

        
    }
    
    internal void SetGroupParent(WindowGroupTileTexture windowGroupTileTexture)
    {
        groupTileTexture = windowGroupTileTexture;
        
        SetupGrid(3, 3);
    }

    internal void SetData(TileRuleTextureData tileRuleTextureData, int idMaterial)
    {
        this.tileRuleTextureData = tileRuleTextureData;
        SetupGrid(tileRuleTextureData.Rows,tileRuleTextureData.Columns);
        SetMaterial(idMaterial);
        
    }

    internal TileRuleTextureData GetData()
    {
       return tileRuleTextureData;
    }
}
