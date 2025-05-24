using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public partial class ContainerAnimationCharacter : PanelContainer
{
    // Called when the node enters the scene tree for the first time.

    ColliderScene control;
    double currentfps = 0;
    int indexFrame = 0;
    AnimationTilesData objectData;

    List<int> tiles = new List<int>();
    List<int> tilesAnimation = new List<int>();
    List<int> tilesAnimationExtras = new List<int>();
    public AnimationTilesData ObjectData { get => objectData; set => objectData = value; }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Ready()
    {
        InitializeUI();

        ButtonBuscar.Pressed += ButtonBuscar_Pressed;
        objectData = new AnimationTilesData();

        ViewItems.MultiSelected += ViewItems_MultiSelected;
        CheckBoxModeSelection.Pressed += CheckBoxModeSelection_Pressed;
        SpinBoxDuration.ValueChanged += SpinBoxDuration_ValueChanged;
        CheckBoxLoop.Pressed += CheckBoxLoop_Pressed;
        CheckBoxMirror.Pressed += CheckBoxMirror_Pressed;
        CheckBoxHasCollision.Pressed += CheckBoxHasCollision_Pressed;
        CheckBoxFrameDuplicate.Pressed += CheckBoxFrameDuplicate_Pressed;
        ButtonForcedFrames.Pressed += ButtonForcedFrames_Pressed;
        OptionButtonAnimation.ItemSelected += OptionButtonAnimation_ItemSelected;
    }

    private void OptionButtonAnimation_ItemSelected(long index)
    {
        tiles.Clear();
        ViewItems.DeselectAll();
        TextEditFrames.Clear();
        if (index == 0) //animacion basica
        {
            TextEditFrames.Text = string.Join(",", tilesAnimation);
           // tilesAnimation.Clear();
        }
        else
        {
            TextEditFrames.Text = string.Join(",", tilesAnimationExtras);
            //tilesAnimationExtras.Clear();
        }
    }

    public void SetData(AnimationTilesData pObjectData)
    {
        objectData = pObjectData;
        tiles = pObjectData.idFrames.ToList();

        TextEditFrames.Text = string.Join(",", tiles);
        CheckBoxLoop.ButtonPressed = objectData.loop;
        CheckBoxMirror.ButtonPressed = objectData.mirrorHorizontal;
        CheckBoxHasCollision.ButtonPressed = objectData.hasCollider;
        SpinBoxDuration.Value = objectData.frameDuration;

        if (CheckBoxHasCollision.ButtonPressed)
        {

            control = GD.Load<PackedScene>("res://sources/WindowsDataBase/Character/Colliders/ColliderScene.tscn").Instantiate<ColliderScene>();
            VBoxContainerCollision.AddChild(control);
            control.SetOcluccionButton();
            control.OnNotifyPreview += Control_OnNotifyPreview;
            CollisionShapeView.Visible = true;
            control.SetData(objectData.collider);
        }
    }
    public override void _Process(double delta)
    {

        if (objectData.idFrames != null && objectData.idFrames.Length > 0)
        {

            currentfps += delta;
            if (currentfps >= objectData.frameDuration)
            {
                var iFrame = objectData.idFrames[indexFrame];
                indexFrame++;
                currentfps = 0;
                var dataTexture = MaterialManager.Instance.GetAtlasTexture(objectData.idMaterialTiles, iFrame);
                Sprite2DView.Texture = dataTexture;
            }
            if (indexFrame >= objectData.idFrames.Length)
            {
                indexFrame = 0;
            }

        }
    }

  

    private void ButtonBuscar_Pressed()
    {
        WindowViewDb windowViewDb = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowViewDB.tscn").Instantiate<WindowViewDb>();
        AddChild(windowViewDb);
        windowViewDb.Show();
        windowViewDb.EnableFilter(false);
        windowViewDb.LoadItems<MaterialData>();
        windowViewDb.OnRequestSelectedItem += WindowViewDb_OnRequestSelectedItem;


    }

    private void ButtonForcedFrames_Pressed()
    {
        string input = TextEditFrames.Text;
        int[] numbers = input
            .Split(',')
            .Select(s => s.Trim())
            .Where(s => int.TryParse(s, out _))
            .Select(int.Parse)
            .ToArray();
        objectData.idFrames = numbers;
    }

    private void CheckBoxFrameDuplicate_Pressed()
    {
        TextEditFrames.Text = "";
        ViewItems.DeselectAll();
        tiles.Clear();
        objectData.idFrames = null;
    }
    private void CheckBoxHasCollision_Pressed()
    {
        objectData.hasCollider = CheckBoxHasCollision.ButtonPressed;
        if (CheckBoxHasCollision.ButtonPressed)
        {

            control = GD.Load<PackedScene>("res://sources/WindowsDataBase/Character/Colliders/ColliderScene.tscn").Instantiate<ColliderScene>();
            VBoxContainerCollision.AddChild(control);
            control.SetOcluccionButton();
            control.OnNotifyPreview += Control_OnNotifyPreview;
            CollisionShapeView.Visible = true;
        }
        else
        {
            VBoxContainerCollision.RemoveChild(control);
            objectData.collider = null;
            CollisionShapeView.Visible = false;
        }
    }

    private void CheckBoxLoop_Pressed()
    {
        objectData.loop = CheckBoxLoop.ButtonPressed;
    }

    private void CheckBoxMirror_Pressed()
    {
        objectData.mirrorHorizontal = CheckBoxMirror.ButtonPressed;
    }

    private void CheckBoxModeSelection_Pressed()
    {
        indexFrame = 0;
        if (CheckBoxModeSelection.ButtonPressed)
        {
            var data = ViewItems.GetSelectedItems();

            StringBuilder allIds = new StringBuilder();
            tiles.Clear();
            foreach (var item in data)
            {
                int id = (int)ViewItems.GetItemMetadata(item);
                tiles.Add(id);
                if (allIds.Length > 0)
                {
                    allIds.Append(",");
                }
                allIds.Append(id);
            }

            TextEditFrames.Text = allIds.ToString();
            objectData.idFrames = tiles.ToArray();

            CheckBoxFrameDuplicate.Visible = false;
            LabelFrameDuplicate.Visible = false;
        }
        else
        {
            CheckBoxFrameDuplicate.Visible = true;
            LabelFrameDuplicate.Visible = true;
        }
    }

    private void Control_OnNotifyPreview(GodotEcsArch.sources.managers.Collision.GeometricShape2D itemData)
    {
        objectData.collider = itemData;

        CollisionShapeView.Position = new Vector2((float)itemData.originPixelX, (float)itemData.originPixelY * (-1));
        switch (itemData)
        {
            case Rectangle:
                var shape = new RectangleShape2D();
                CollisionShapeView.Shape = shape;
                shape.Size = new Vector2((float)itemData.widthPixel, (float)itemData.heightPixel);
                break;
            case Circle:
                var shapeC = new CircleShape2D();
                CollisionShapeView.Shape = shapeC;
                shapeC.Radius = itemData.widthPixel;
                break;
            default:
                break;
        }
    }
    private void SpinBoxDuration_ValueChanged(double value)
    {
        objectData.frameDuration = (float)value;
    }
    private void ViewItems_MultiSelected(long index, bool selected)
    {
        indexFrame = 0;
        if (CheckBoxModeSelection.ButtonPressed)
        {
            var data = ViewItems.GetSelectedItems();

            StringBuilder allIds = new StringBuilder();
            tiles.Clear();
            foreach (var item in data)
            {
                int id = (int)ViewItems.GetItemMetadata(item);
                tiles.Add(id);
                if (allIds.Length > 0)
                {
                    allIds.Append(",");
                }
                allIds.Append(id);
            }

            TextEditFrames.Text = allIds.ToString();
            objectData.idFrames = tiles.ToArray();
        }
        else
        {
            int id = (int)ViewItems.GetItemMetadata((int)index);
            if (tiles.Contains(id) && !CheckBoxFrameDuplicate.ButtonPressed)
            {
                tiles.Remove(id);
            }
            else
            {
                tiles.Add(id);
            }
            TextEditFrames.Text = string.Join(",", tiles);
            objectData.idFrames = tiles.ToArray();
        }

        if (OptionButtonAnimation.GetSelectedId() == 0)
        {
            tilesAnimation = tiles;
        }
        else 
        {
            tilesAnimationExtras = tiles;
        }
    }



    private void WindowViewDb_OnRequestSelectedItem(int id)
    {
        objectData.idMaterialTiles = id;
        tiles.Clear();
        TextEditFrames.Text = "";
        ViewItems.Clear();
        objectData.idFrames = null;

        MaterialData materialData = DataBaseManager.Instance.FindById<MaterialData>(id);
        List<Texture> list = TextureHelper.SplitTexture(FileHelper.GetPathGameDB(materialData.pathTexture), new Vector2I(materialData.divisionPixelX, materialData.divisionPixelY));
        for (int i = 0; i < list.Count; i++)
        {
            Texture item = list[i];
            if (!TextureHelper.IsTextureEmpty(item))
            {
                int idx = ViewItems.AddItem("ID:" + i, (Texture2D)item);
                ViewItems.SetItemMetadata(idx, i);
            }
        }
    }
}
