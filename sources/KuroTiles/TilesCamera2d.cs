using Godot;
using System;

public partial class TilesCamera2d : Camera2D
{
    [Export] float zoomStep = 0.1f;
    [Export] Vector2 minZoom = new Vector2(0.5f, 0.5f);
    [Export] Vector2 maxZoom = new Vector2(4f, 4f);
    [Export] SubViewport subViewport;

    public override void _Input(InputEvent @event)
    {
        // Detecta la rueda del ratón
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
                ZoomAtScreenCenter(-zoomStep);
            else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
                ZoomAtScreenCenter(zoomStep);
        }
    }

    private void ZoomAtScreenCenter(float zoomChange)
    {
        Vector2 newZoom = (Zoom + new Vector2(zoomChange, zoomChange)).Clamp(minZoom, maxZoom);
        Zoom = newZoom;
        CenterCameraOnViewport();
    }

    private void CenterCameraOnViewport()
    {
        var vp = GetViewport();
        if (vp == null)
            return;

        // Tomamos el centro del viewport en coordenadas de pantalla del SubViewport
        Vector2 screenCenter = vp.GetVisibleRect().Size / 2;

        // Convertimos ese punto a coordenadas de mundo
        //Vector2 worldCenter = GetScreenTransform().AffineInverse().Xform(screenCenter);

        // Colocamos la cámara en ese punto
        Position = screenCenter;
    }
}
