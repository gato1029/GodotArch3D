using Godot;
using System;

[Tool]
public partial class ContenedorVentana : Container
{
	[Export]
    private Control _contenedorPath;
	// Called when the node enters the scene tree for the first time.
	private Control contenedor = null;
	public override void _Ready()
	{
     
        QueueSort();
    }
    public override void _Notification(int what)
    {
        if (what == NotificationSortChildren)
        {
            //// Must re-sort the children
            //foreach (Control c in GetChildren())
            //{
            //    // Fit to own size
                
            //}
        }
    }
  
}
