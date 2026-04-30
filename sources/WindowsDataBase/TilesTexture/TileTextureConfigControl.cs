using Godot;
using GodotEcsArch.sources.CustomWidgets.Internals;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Linq;
[KuroRegisterWindow("res://sources/WindowsDataBase/TilesTexture/TileTextureConfigControl.tscn")]
public partial class TileTextureConfigControl : Window, IFacadeWindow<TileTextureData>
{
    private bool isDragging = false;
    private bool isResizing = false;

    private Vector2 dragStartMouse;
    private Vector2 dragStartOffset;

    private int activeHandle = -1; // 0-3 esquinas
    private const float HANDLE_SIZE = 0.6f;

    private float animTimer = 0f;
    private int animFrame = 0;

    TileTextureData data;
    FastCollider fastCollider;
    MaterialData material;
    public event IFacadeWindow<TileTextureData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;

    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        data = new TileTextureData();
        data.index = -1;
        fastCollider = new FastCollider();
        ComboBoxCollider.ItemSelected += ComboBoxCollider_ItemSelected;
        // Conectamos la señal de dibujo del TextureRect
        TextureRectTile.Draw += OnTextureRectDraw;
        TextureRectTile.GuiInput += OnTextureRectGuiInput;
        KuroTextureButtonSearch.Pressed += KuroTextureButtonSearch_Pressed;
        TextureRectTile.MouseFilter = Control.MouseFilterEnum.Stop;
        KuroCheckButtonCollider.Pressed += KuroCheckButtonCollider_Pressed;
        SpinBoxfps.ValueChanged += SpinBoxfps_ValueChanged;
        //TextureRectTile en este rectecxture debemos dibujar los colliders segun el tipo
        fastCollider.Width = (float)TextureRectTile.Size.X;
        fastCollider.Height = (float)TextureRectTile.Size.Y;
        KuroTextureButtonSave.Pressed += KuroTextureButtonSave_Pressed;
        KuroTextureButtonDelete.Pressed += KuroTextureButtonDelete_Pressed;
        ComboBoxColliderTriangulos.ItemSelected += ComboBoxColliderTriangulos_ItemSelected;
        NivelarComboTriangulo();
        
    }

    private void KuroTextureButtonDelete_Pressed()
    {
        DataBaseManager.Instance.RemoveDirectById<TileTextureData>(data.id);
        QueueFree();
    }

    private void KuroTextureButtonSave_Pressed()
    {
        data.name = data.idMod+":"+data.index;
        data.fastColliderTemplate = fastCollider;
        data.fps = (float)SpinBoxfps.Value;
        data.fastCollider = new FastCollider
        {
            Shape = fastCollider.Shape,
            Height = MeshCreator.PixelsToUnits(fastCollider.Height),
            Width = MeshCreator.PixelsToUnits(fastCollider.Width),
            Offset =MeshCreator.PixelsToUnits(GetColliderCenterCartesian()),
            Slope = fastCollider.Slope
            
        };
        
        DataBaseManager.Instance.InsertUpdate(data);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }

    public void SetData(TileTextureData data)
    {
        this.data = data;
        fastCollider = data.fastColliderTemplate;
        KuroCheckButtonCollider.ButtonPressed = data.hasCollider;
        SpinBoxfps.Value = data.fps;

        var mat = MaterialManager.Instance.GetMaterial(data.idMaterial);
        if (data.isAnimated)
        {
            WindowLocal_OnNotifyMultiSelectionIndex(data.indexAnimation.ToList(), mat);
        }
        else
        {
            WindowLocal_OnNotifyMultiSelectionIndex(new System.Collections.Generic.List<int> { data.index }, mat);
        }

        //KuroCheckButtonCollider_Pressed();
        
        if (data.hasCollider)
        {
            ComboBoxCollider.Visible = true;
            ComboBoxCollider.Selected = (int)fastCollider.Shape;
            if (fastCollider.Shape == ShapeType.Slope)
            {
                ComboBoxColliderTriangulos.Selected = (int)fastCollider.Slope;          
            }
          
        }
        OnTextureRectDraw();



    }
    private void SpinBoxfps_ValueChanged(double value)
    {
        data.fpsTemplate = (float)value;
        data.fps = (float)(1 / value);
    }

    private void KuroTextureButtonSearch_Pressed()
    {
        var windowLocal = GD.Load<PackedScene>(
                "res://sources/KuroTiles/WindowSearchTileMaterial.tscn"
            )
            .Instantiate<WindowSearchTileMaterial>();
        // Escuchar cuando se cierre
        AddChild(windowLocal);
        if (data.index != -1)
        {
            if (data.indexAnimation != null)
            {
                windowLocal.SetSelectionMultiple(data.idMaterial, data.indexAnimation.ToList());
            }
            else
            {
                windowLocal.SetSelection(data.idMaterial, data.index);
            }
        }
        windowLocal.TreeExited += () => windowLocal = null;
        windowLocal.SetMultipleSelectionMode(true);
        windowLocal.OnNotifyMultiSelectionIndex += WindowLocal_OnNotifyMultiSelectionIndex;
        //windowLocal.OnNotifySelectionIndex += WindowLocal_OnNotifySelectionIndex;
        windowLocal.Popup();
    }

    private void WindowLocal_OnNotifyMultiSelectionIndex(
        System.Collections.Generic.List<int> indices,
        MaterialData materialData
    )
    {
        material = materialData;
        if (indices == null || indices.Count == 0)
            return;

        data.idMaterial = materialData.id;
        data.idMod = materialData.idNameMod;

        // 🔹 CASO 1: SOLO UN TILE → comportamiento normal
        if (indices.Count == 1)
        {
            data.isAnimated = false;
            data.indexAnimation = null;

            data.index = indices[0];

            var texture = MaterialManager.Instance.GetAtlasTextureInternal(
                materialData.id,
                data.index
            );

            TextureRectTile.Texture = texture;
        }
        // 🔹 CASO 2: MÚLTIPLES → animación
        else
        {
            data.isAnimated = true;
            data.indexAnimation = indices.ToArray();

            // iniciar desde el primero
            data.index = indices[0];

            StartTileAnimation();
        }

        if (fastCollider.Shape== ShapeType.Slope)
        {
            fastCollider.Width = material.divisionPixelX;
            fastCollider.Height = material.divisionPixelY;

        }

        // 🔹 collider siempre consistente


        TextureRectTile.QueueRedraw();
    }

    private void StartTileAnimation()
    {
        animTimer = 0f;
        animFrame = 0;
    }


    private void OnTextureRectGuiInput(InputEvent @event)
    {
        if (!KuroCheckButtonCollider.ButtonPressed)
            return;

        if (@event is InputEventMouseButton mb)
        {
            if (mb.Pressed && mb.ButtonIndex == MouseButton.Left)
            {
                HandleMouseDown(mb.Position);
            }
            else if (!mb.Pressed)
            {
                isDragging = false;
                isResizing = false;
                activeHandle = -1;
            }
        }

        if (@event is InputEventMouseMotion motion)
        {
            HandleMouseMove(motion.Position);
        }
    }

    private void ResizeRect(Vector2 mousePos)
    {
        var rect = GetColliderRect();

        switch (activeHandle)
        {
            case 0: // top-left
                fastCollider.Width += rect.Position.X - mousePos.X;
                fastCollider.Height += rect.Position.Y - mousePos.Y;
                fastCollider.Offset = mousePos;
                break;

            case 1: // top-right
                fastCollider.Width = mousePos.X - rect.Position.X;
                fastCollider.Height += rect.Position.Y - mousePos.Y;
                fastCollider.Offset = new Vector2(rect.Position.X, mousePos.Y);
                break;

            case 2: // bottom-right
                fastCollider.Width = mousePos.X - rect.Position.X;
                fastCollider.Height = mousePos.Y - rect.Position.Y;
                break;

            case 3: // bottom-left
                fastCollider.Width += rect.Position.X - mousePos.X;
                fastCollider.Height = mousePos.Y - rect.Position.Y;
                fastCollider.Offset = new Vector2(mousePos.X, rect.Position.Y);
                break;
        }
    }

    private Vector2 GetHandlePosition(Rect2 rect, int index)
    {
        return index switch
        {
            0 => rect.Position, // top-left
            1 => rect.Position + new Vector2(rect.Size.X, 0), // top-right
            2 => rect.Position + rect.Size, // bottom-right
            3 => rect.Position + new Vector2(0, rect.Size.Y), // bottom-left
            _ => rect.Position,
        };
    }

    private Rect2 GetColliderRect()
    {
        return new Rect2(fastCollider.Offset, new Vector2(fastCollider.Width, fastCollider.Height));
    }

    private void ClampCollider()
    {
        var maxSize = TextureRectTile.Size;

        // 🔴 RECT
        if (fastCollider.Shape == ShapeType.Rect)
        {
            fastCollider.Width = Mathf.Clamp(fastCollider.Width, 4, maxSize.X);
            fastCollider.Height = Mathf.Clamp(fastCollider.Height, 4, maxSize.Y);

            fastCollider.Offset = new Vector2(
                Mathf.Clamp(fastCollider.Offset.X, 0, maxSize.X - fastCollider.Width),
                Mathf.Clamp(fastCollider.Offset.Y, 0, maxSize.Y - fastCollider.Height)
            );
        }

        if (fastCollider.Shape == ShapeType.Circle)
        {
            float radius = fastCollider.Width / 2f;

            // 🔴 SOLO mover dentro de límites (sin cambiar tamaño)
            fastCollider.Offset = new Vector2(
                Mathf.Clamp(fastCollider.Offset.X, 0, maxSize.X - fastCollider.Width),
                Mathf.Clamp(fastCollider.Offset.Y, 0, maxSize.Y - fastCollider.Height)
            );
        }
    }

    private void HandleMouseMove(Vector2 mousePos)
    {
        // 🟡 MOVER
        if (isDragging)
        {
            var delta = mousePos - dragStartMouse;
            fastCollider.Offset = dragStartOffset + delta;

            ClampCollider();
            TextureRectTile.QueueRedraw();
            return;
        }
        else
        {
            // 🔴 RECT resize
            if (isResizing && fastCollider.Shape == ShapeType.Rect)
            {
                ResizeRect(mousePos);

                ClampCollider();
                TextureRectTile.QueueRedraw();
                return;
            }

            // 🔵 CÍRCULO resize
            if (isResizing && fastCollider.Shape == ShapeType.Circle)
            {
                var rect = GetColliderRect();
                var center = rect.Position + rect.Size / 2f;

                float newRadius = center.DistanceTo(mousePos);
                float size = newRadius * 2f;

                fastCollider.Width = size;
                fastCollider.Height = size;

                // mantener centro fijo
                fastCollider.Offset = center - new Vector2(newRadius, newRadius);

                ClampCollider();
                TextureRectTile.QueueRedraw();
                return;
            }
        }
    }

    private void HandleMouseDown(Vector2 mousePos)
    {
        var rect = GetColliderRect();

        // 🔵 CÍRCULO
        if (fastCollider.Shape == ShapeType.Circle)
        {
            var center = rect.Position + rect.Size / 2f;
            var radius = rect.Size.X / 2f;

            // 🔴 HANDLE (resize)
            var handlePos = center + new Vector2(radius, 0);

            if (mousePos.DistanceTo(handlePos) < 6f)
            {
                isResizing = true;
                isDragging = false;
                activeHandle = 100;
                return;
            }

            // 🟡 SOLO CENTRO (drag) con tolerancia pequeña
            float centerTolerance = 2f;

            if (mousePos.DistanceTo(center) <= centerTolerance)
            {
                isDragging = true;
                isResizing = false;
                activeHandle = -1;

                dragStartMouse = mousePos;
                dragStartOffset = fastCollider.Offset;
                return;
            }

            // fuera del círculo → nada
            return;
        }

        // 🔴 RECT → handles
        if (fastCollider.Shape == ShapeType.Rect)
        {
            float half = HANDLE_SIZE / 2f;

            for (int i = 0; i < 4; i++)
            {
                var handlePos = GetHandlePosition(rect, i);

                switch (i)
                {
                    case 0:
                        handlePos += new Vector2(half, half);
                        break;
                    case 1:
                        handlePos += new Vector2(-half, half);
                        break;
                    case 2:
                        handlePos += new Vector2(-half, -half);
                        break;
                    case 3:
                        handlePos += new Vector2(half, -half);
                        break;
                }

                if (mousePos.DistanceTo(handlePos) < 6f)
                {
                    isResizing = true;
                    isDragging = false;
                    activeHandle = i;
                    return;
                }
            }
            // 🟡 DRAG general (RECT)
            if (rect.HasPoint(mousePos))
            {
                isDragging = true;
                isResizing = false;
                dragStartMouse = mousePos;
                dragStartOffset = fastCollider.Offset;
            }
        }
    }

    private void ResizeCircle(Vector2 mousePos)
    {
        var rect = GetColliderRect();
        var center = rect.Position + rect.Size / 2f;

        // distancia del mouse al centro = nuevo radio
        float newRadius = center.DistanceTo(mousePos);

        // diámetro
        float size = newRadius * 2f;

        fastCollider.Width = size;
        fastCollider.Height = size;

        // mantener el centro fijo
        fastCollider.Offset = center - new Vector2(newRadius, newRadius);
    }

    private void KuroCheckButtonCollider_Pressed()
    {
        if (KuroCheckButtonCollider.ButtonPressed)
        {
            ComboBoxCollider.Visible = true;
            fastCollider = new FastCollider
            {
                Shape = ShapeType.Rect,
                Width = TextureRectTile.Size.X,
                Height = TextureRectTile.Size.Y,
                Offset = Vector2.Zero,
            };
            ComboBoxCollider.Selected = 0;
            data.hasCollider = true;
        }
        else
        {
            data.hasCollider = false;
            fastCollider = default;
            ComboBoxCollider.Visible = false;
            ComboBoxColliderTriangulos.Visible = false;
        }
        TextureRectTile.QueueRedraw();
    }

    private void ComboBoxColliderTriangulos_ItemSelected(long index)
    {
        SlopeType slope = (SlopeType)index;
        fastCollider.Slope = slope;
        fastCollider.Offset = Vector2.Zero;
        TextureRectTile.QueueRedraw();
    }

    private void NivelarComboTriangulo()
    {
        // Enum no es directamente enumerable; obtener sus valores mediante Enum.GetValues
        ComboBoxColliderTriangulos.Clear();
        foreach (SlopeType item in Enum.GetValues(typeof(SlopeType)))
        {
            ComboBoxColliderTriangulos.AddItem(item.ToString());
        }
    }

    private void ComboBoxCollider_ItemSelected(long index)
    {
        switch (index)
        {
            case 0:
                fastCollider.Shape = ShapeType.Rect;
                ComboBoxColliderTriangulos.Visible = false;
                break;
            case 1:
                fastCollider.Shape = ShapeType.Circle;
                ComboBoxColliderTriangulos.Visible = false;
                break;
            case 2:
                fastCollider.Offset = Vector2.Zero;
                if (material!=null)
                {
                    fastCollider.Width = material.divisionPixelX;
                    fastCollider.Height = material.divisionPixelY;
                }                

                fastCollider.Shape = ShapeType.Slope;
                fastCollider.Slope = SlopeType.BottomLeft;
                ComboBoxColliderTriangulos.Visible = true;
                break;
            default:
                break;
        }
        //data.fastColliderTemplate = fastCollider;
        TextureRectTile.QueueRedraw();
    }

    private void OnTextureRectDraw()
    {
        // 🔴 Si el checkbox está apagado, no dibujamos nada
        if (!KuroCheckButtonCollider.ButtonPressed)
            return;

        var color = new Color(0, 1, 0, 0.5f);
        var thickness = 0.2f;

        switch (fastCollider.Shape)
        {
            case ShapeType.Rect:
            {
                var rect = GetColliderRect();
                TextureRectTile.DrawRect(rect, color, false, thickness);
                break;
            }

            case ShapeType.Circle:
            {
                var rect = GetColliderRect();
                var center = rect.Position + rect.Size / 2f;
                var radius = rect.Size.X / 2f; // asumiendo círculo perfecto

                TextureRectTile.DrawArc(center, radius, 0, Mathf.Tau, 32, color, thickness);

                // 📍 Punto en el borde (derecha del círculo)
                var handlePos = center + new Vector2(radius, 0);

                float half = HANDLE_SIZE / 2f;

                TextureRectTile.DrawRect(
                    new Rect2(
                        handlePos - new Vector2(half, half),
                        new Vector2(HANDLE_SIZE, HANDLE_SIZE)
                    ),
                    Colors.Red
                );
                break;
            }

            case ShapeType.Slope:
            {
                // 🔺 NO SE TOCA → usa todo el tile
                var size = TextureRectTile.Size;
                DrawSlopePreview(size, color, thickness);
                break;
            }
        }
        if (fastCollider.Shape == ShapeType.Rect || fastCollider.Shape == ShapeType.Circle)
        {
            var rect = GetColliderRect();
            var center = rect.Position + rect.Size / 2f;
            
            float s = 3f;

            TextureRectTile.DrawLine(
                center + new Vector2(-s, 0),
                center + new Vector2(s, 0),
                Colors.Yellow,
                0.6f
            );
            TextureRectTile.DrawLine(
                center + new Vector2(0, -s),
                center + new Vector2(0, s),
                Colors.Yellow,
                0.6f
            );
        }
        if (fastCollider.Shape == ShapeType.Rect)
        {
            var rect = GetColliderRect();
            float half = HANDLE_SIZE / 2f;

            for (int i = 0; i < 4; i++)
            {
                var p = GetHandlePosition(rect, i);
                
                // 👉 mover el handle hacia adentro (mejor UX)
                switch (i)
                {
                    case 0:
                        p += new Vector2(half, half);
                        break;
                    case 1:
                        p += new Vector2(-half, half);
                        break;
                    case 2:
                        p += new Vector2(-half, -half);
                        break;
                    case 3:
                        p += new Vector2(half, -half);
                        break;
                }

                TextureRectTile.DrawRect(
                    new Rect2(p - new Vector2(half, half), new Vector2(HANDLE_SIZE, HANDLE_SIZE)),
                    Colors.Red
                );
            }
        }

        if (data.isAnimated && data.indexAnimation != null)
        {
            TextureRectTile.DrawString(
                GetThemeDefaultFont(),
                new Vector2(5, 15),
                $"                {data.indexAnimation.Length}",
                HorizontalAlignment.Left,
                -1,
                10,
                Colors.Yellow
            );
        }
    }
    private Vector2 GetColliderCenterCartesian()
    {
        var rect = GetColliderRect();
        var centerUI = rect.Position + rect.Size / 2f;
        var halfSize = TextureRectTile.Size / 2f;

        var result = centerUI - halfSize;

        // opcional: invertir Y para sistema matemático real
        result.Y *= -1;

        return result;
    }
    private void DrawSlopePreview(Vector2 size, Color color, float thickness)
    {
        Vector2 p1,
            p2,
            p3;

        // Dibujamos según el Enum SlopeType que definimos antes
        switch (fastCollider.Slope)
        {
            case SlopeType.BottomLeft: // Triángulo 1
                p1 = new Vector2(0, 0);
                p2 = new Vector2(0, size.Y);
                p3 = new Vector2(size.X, size.Y);
                break;
            case SlopeType.TopRight: // Triángulo 2
                p1 = new Vector2(0, 0);
                p2 = new Vector2(size.X, 0);
                p3 = new Vector2(size.X, size.Y);
                break;
            case SlopeType.TopLeft: // Triángulo 3
                p1 = new Vector2(0, 0);
                p2 = new Vector2(size.X, 0);
                p3 = new Vector2(0, size.Y);
                break;
            case SlopeType.BottomRight: // Triángulo 4
                p1 = new Vector2(size.X, 0);
                p2 = new Vector2(size.X, size.Y);
                p3 = new Vector2(0, size.Y);
                break;
            default:
                return;
        }

        // Dibujamos el triángulo
        Vector2[] points = { p1, p2, p3, p1 };
        TextureRectTile.DrawPolyline(points, color, thickness);
    }

    public override void _Process(double delta)
    {
        if (!data.isAnimated || data.indexAnimation == null || data.indexAnimation.Length == 0)
            return;

        animTimer += (float)delta;

        if (animTimer >= 1f / SpinBoxfps.Value)
        {
            animTimer = 0f;

            animFrame++;
            if (animFrame >= data.indexAnimation.Length)
                animFrame = 0;

            int currentIndex = data.indexAnimation[animFrame];

            var texture = MaterialManager.Instance.GetAtlasTextureInternal(
                data.idMaterial,
                currentIndex
            );

            TextureRectTile.Texture = texture;
        }
    }


}
