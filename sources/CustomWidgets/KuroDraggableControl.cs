using Godot;
using System;

[GlobalClass]
public partial class KuroDraggableControl : PanelContainer
{
    [Export] public string GroupName = "draggable_items";

    public override void _Ready()
    {
        SceneRegistry.Register(this);
        AddToGroup(GroupName);
        MouseFilter = MouseFilterEnum.Stop;

        // 1. Configurar hijos existentes y conectar sus señales
        foreach (var child in GetChildren())
        {
            if (child is Control c)
            {
                SetupChild(c);
                // 🔥 Conectar la señal: cuando el hijo cambie de tamaño, el padre también
                c.MinimumSizeChanged += RefreshMinimumSize;
            }
        }

        // 2. Hacer lo mismo para hijos que se añadan en el futuro
        ChildEnteredTree += (node) => {
            if (node is Control child)
            {
                SetupChild(child);
                child.MinimumSizeChanged += RefreshMinimumSize;
                RefreshMinimumSize();
            }
        };

        // 3. Ajustar tamaño inicial
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

        // Si el tamaño ha cambiado, lo aplicamos
        if (CustomMinimumSize != maxChildSize)
        {
            CustomMinimumSize = maxChildSize;
            // Esto notifica al GridContainer (el abuelo) que debe reacomodar todo
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
        // Forzamos 'Pass' para que el clic llegue al padre (KuroDraggableControl)
        child.MouseFilter = MouseFilterEnum.Pass;

        // Si el hijo tiene sus propios hijos (ej. un botón con un label), 
        // también deben ser 'Pass' o 'Ignore'
        if (child.GetChildCount() > 0)
        {
            SetupChildrenMouseFilter(child);
        }
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
        return data.As<Node>() is KuroDraggableControl;
    }
    public override void _DropData(Vector2 atPosition, Variant data)
    {
        var draggedItem = data.As<Control>();
        if (draggedItem == null || draggedItem == this) return;

        var container = GetParent();
        if (container == null) return;

        // Calculamos si soltamos en la primera mitad o segunda mitad del control
        // atPosition es LOCAL al control sobre el que sueltas.
        int dropIndex = GetIndex();

        // Si soltamos en la mitad superior/izquierda o inferior/derecha
        // Dependiendo de si tu grid es horizontal o vertical:
        bool dropAfter = (atPosition.X > Size.X * 0.5f) || (atPosition.Y > Size.Y * 0.5f);

        if (dropAfter)
            dropIndex++;

        // Corregir el índice si el elemento arrastrado viene de una posición menor
        if (draggedItem.GetParent() == container && draggedItem.GetIndex() < dropIndex)
            dropIndex--;

        container.MoveChild(draggedItem, Mathf.Clamp(dropIndex, 0, container.GetChildCount()));

        // IMPORTANTE: Forzar el reordenamiento visual
        if (container is Container c) c.QueueSort();

        CallDeferred(nameof(UpdatePositions), container);
    }


    protected void UpdatePositions(Node container)
    {
        foreach (var child in container.GetChildren())
        {
            if (child is KuroDraggableControl item)
                item.OnPositionChanged(child.GetIndex());
        }
    }
       
    protected virtual void OnPositionChanged(int index) { }
}
