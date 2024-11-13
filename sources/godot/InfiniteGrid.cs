using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Godot.WebSocketPeer;


internal partial class InfiniteGrid : Node2D
{
    [Export] Camera2D camera;
    private float cellSize = 64f;  
    private Color lineColor = new Color(1, 1, 1, 0.5f);
    private Color specialLineColor = new Color(1, 0, 30, 0.5f); // Color para líneas especiales
    private int n = 4; // Cada n cuadrantes se dibujará con un color diferente
    private float zoom = 16; //  en propocion al zoom si es 0.5  tiene que ser 2
    public override void _Draw()
    {

        //// Obtener el tamaño del viewport (pantalla)
        //Vector2 screenSize = GetViewportRect().Size * zoom;

        //// Calcular el centro de la pantalla
        //Vector2 center = ((screenSize ) / 2)  ;

        //// Determinar el punto de inicio desde el centro, ajustado para que empiece en la celda anterior
        //float startX = (float)(Math.Floor(-center.X / cellSize) * cellSize);
        //float startY = (float)(Math.Floor(-center.Y / cellSize) * cellSize);
        //// Dibujar las líneas verticales
        //for (float x = startX; x < screenSize.X ; x += cellSize)
        //{
        //    if (((int)(x / cellSize) % n) == 0) // Cada n cuadrantes
        //    {
        //        DrawLine(new Vector2(x, startY), new Vector2(x, screenSize.Y), specialLineColor, 4);
        //    }
        //    else
        //    {
        //        DrawLine(new Vector2(x, startY), new Vector2(x, screenSize.Y), lineColor, 4);
        //    }

        //    //DrawLine(new Vector2(x, startY), new Vector2(x, screenSize.Y), lineColor, 1);
        //}

        //// Dibujar las líneas horizontales
        //for (float y = startY; y < screenSize.Y ; y += cellSize)
        //{
        //    if (((int)(y / cellSize) % n) == 0) // Cada n cuadrantes
        //    {
        //        DrawLine(new Vector2(startX, y), new Vector2(screenSize.X, y), specialLineColor, 4);
        //    }
        //    else
        //    {
        //        DrawLine(new Vector2(startX, y), new Vector2(screenSize.X, y), lineColor, 4);
        //    }
        //    //DrawLine(new Vector2(startX, y), new Vector2(screenSize.X, y), lineColor, 1);
            
        //}
    }

    public override void _Process(double delta)
    {
        //zoom = (float)Math.Floor(1/camera.Zoom.X);
        //GD.Print(zoom);
        //QueueRedraw();
    }
}

