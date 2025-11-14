using Godot;
using GodotEcsArch.sources.KuroTiles;
using GodotFlecs.sources.KuroTiles;
using System;

public partial class CustomButtonRule : Panel
{
    // Called when the node enters the scene tree for the first time.
    //public enum NeighborType
    //{
    //    VacioAlgo = 0, // No interesa el contenido lleno o vacio de cualquier grupo
    //    LlenoMismoGrupo = 1, // Lleno con cualquier tile del mismo grupo            
    //    NoLlenoMismoGrupo = 2, // No hay tile  del mismo grupo
    //    AlgoCualquierGrupo = 3, // Cualquier tile de cualquier grupo
    //    NoLlenoCualquierGrupo = 4, // No hay tile  de cualquier grupo 
    //    LLenoConTileEspecifico = 5, // Lleno con un tile específico
    //}

    public enum ButtonWorkingMode
    {
        Group = 0,
        TileEspecific = 1,
        AllGroups = 2,
    }
    string pathTextureCheck= "res://resources/Textures/iconos/Check Mark.png";
    string pathTextureCross = "res://resources/Textures/iconos/Cross Mark.png";
    string pathTextureEmpty = "res://resources/Textures/iconos/cuadrado-punteado.png";
    string pathTextureAny = "res://resources/Textures/iconos/White Question Mark.png";
    string pathTextureNot = "res://resources/Textures/iconos/Unavailable.png";
    
    private Texture2D texCheck;
    private Texture2D texCross;
    private Texture2D texAny;
    private Texture2D texEmpty;
    private Texture2D texNot;

    private Texture2D currentTexture;
    public NeighborType State { get;  set; } = NeighborType.VacioAlgo;
    
    public NeighborConditionTemplate neighborConditionTemplate { get; set; } = new NeighborConditionTemplate();

    private NeighborPosition _neighborPosition;

    public NeighborPosition neighborPosition
    {
        get => _neighborPosition;
        set
        {
            _neighborPosition = value;
            neighborConditionTemplate.position = _neighborPosition;
        }
    }
    public ButtonWorkingMode WorkingMode { get; set; } = ButtonWorkingMode.Group;

    public int groupIdTemp;
    public long idTileTemp;
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI	
                        // Cargar las texturas
        texCheck = GD.Load<Texture2D>(pathTextureCheck);
        texCross = GD.Load<Texture2D>(pathTextureCross);
        texAny = GD.Load<Texture2D>(pathTextureAny);
        texEmpty = GD.Load<Texture2D>(pathTextureEmpty);
        texNot = GD.Load<Texture2D>(pathTextureNot);
        currentTexture = texEmpty;
    }
    public void SetAllGroup()
    {
        groupIdTemp = 0;
        idTileTemp = 0;
        SetState(ButtonWorkingMode.AllGroups);
    }
    public void SetGroup(int idGroup, Texture2D texture)
    {
        groupIdTemp = idGroup;
        idTileTemp = 0;
        SetState(ButtonWorkingMode.Group, texture);
    }
    public void SetTile(long idTile, Texture2D texture)
    {
        groupIdTemp = 0;
        idTileTemp = idTile;
        SetState(ButtonWorkingMode.TileEspecific, texture);
    }
    public void SetState(ButtonWorkingMode buttonState, Texture2D textureGroupTile = null)
    {
        WorkingMode = buttonState;
        switch (buttonState)
        {
            case ButtonWorkingMode.Group:
                currentTexture = textureGroupTile;
                break;
            case ButtonWorkingMode.TileEspecific:
                currentTexture = textureGroupTile;
                break;
            case ButtonWorkingMode.AllGroups:
                currentTexture = texEmpty;
                break;
            default:
                break;
        }
    }

    private bool _rightClickToggle = false;
    public bool isSelected { get; set; } = true;     
    public override void _GuiInput(InputEvent @event)
    {
        if (!isSelected)
        {
            return;
        }
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            switch (WorkingMode)
            {
                case ButtonWorkingMode.Group:
                    switch (mouseEvent.ButtonIndex)
                    {
                        case MouseButton.Left:
                            State = NeighborType.LlenoMismoGrupo;
                            break;

                        case MouseButton.Right:
                            if (_rightClickToggle)
                                State = NeighborType.VacioAlgo;
                            else
                                State = NeighborType.NoLlenoMismoGrupo;
                            _rightClickToggle = !_rightClickToggle;
                            break;

                        case MouseButton.Middle:
                            State = NeighborType.VacioAlgo;
                            break;
                    }
                    break;

                case ButtonWorkingMode.TileEspecific:
                    switch (mouseEvent.ButtonIndex)
                    {
                        case MouseButton.Left:
                            State = NeighborType.LLenoConTileEspecifico;
                            break;

                        case MouseButton.Right:
                            if (_rightClickToggle)
                                State = NeighborType.VacioAlgo;
                            else
                                State = NeighborType.VacioAlgo; // puedes poner otro si deseas alternar distinto
                            _rightClickToggle = !_rightClickToggle;
                            break;
                    }
                    break;

                case ButtonWorkingMode.AllGroups:
                    switch (mouseEvent.ButtonIndex)
                    {
                        case MouseButton.Left:
                            State = NeighborType.AlgoCualquierGrupo;
                            break;

                        case MouseButton.Right:
                            if (_rightClickToggle)
                                State = NeighborType.VacioAlgo;
                            else
                                State = NeighborType.NoLlenoCualquierGrupo;
                            _rightClickToggle = !_rightClickToggle;
                            break;

                        case MouseButton.Middle:
                            State = NeighborType.VacioAlgo;
                            break;
                    }
                    break;
            }

            UpdateIcon();
            OnNotifyChangued?.Invoke(this);
        }
    }
    public void UpdateIcon()
    {
        
        neighborConditionTemplate.type = State;
        neighborConditionTemplate.groupId = groupIdTemp;
        neighborConditionTemplate.idTile = idTileTemp;
        switch (State)
        {
            case NeighborType.VacioAlgo:                
                TextureIcon.Texture = null;
                TextureImage.Texture = null;
                neighborConditionTemplate.groupId = 0;
                neighborConditionTemplate.idTile = 0;
                break;
            case NeighborType.NoLlenoCualquierGrupo:                
                TextureIcon.Texture = texNot;
                TextureImage.Texture = null;
                neighborConditionTemplate.groupId = 0;
                neighborConditionTemplate.idTile = 0;
                break;
            case NeighborType.LLenoConTileEspecifico:
                TextureIcon.Texture = null;
                TextureImage.Texture = currentTexture;
                break;
            case NeighborType.LlenoMismoGrupo:
                TextureIcon.Texture = null;
                TextureImage.Texture = currentTexture;
                break;
            case NeighborType.NoLlenoMismoGrupo:
                TextureIcon.Texture = texCross;
                TextureImage.Texture = currentTexture;
                break;
            case NeighborType.AlgoCualquierGrupo:                
                TextureIcon.Texture = texAny;
                TextureImage.Texture = null;
                neighborConditionTemplate.groupId = 0;
                neighborConditionTemplate.idTile = 0;
                break;
            default:
                TextureIcon.Texture = null;
                break;
        }
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    internal void SetData(NeighborConditionTemplate neighborConditionTemplate)
    {
        if (neighborConditionTemplate==null)
        {
            return;
        }
        this.neighborConditionTemplate = neighborConditionTemplate;
        State = neighborConditionTemplate.type;
        //groupIdTemp = neighborConditionTemplate.groupId;
        //idTileTemp = neighborConditionTemplate.idTile;

        if (neighborConditionTemplate.groupId == 0 && neighborConditionTemplate.idTile ==0 )
        {
            SetAllGroup();
        }
        if (neighborConditionTemplate.groupId != 0)
        {
            var text = GroupManager.Instance.GetData(neighborConditionTemplate.groupId);
            SetGroup(neighborConditionTemplate.groupId, text.textureVisual);
        }
        if (neighborConditionTemplate.idTile != 0)
        {
            var text = TileSpriteManager.Instance.GetData(neighborConditionTemplate.idTile);
            var dd = MaterialManager.Instance.GetAtlasTextureInternal(text.spriteData);
            SetGroup(neighborConditionTemplate.groupId, dd);
        }
        
        UpdateIcon();
    }


}
