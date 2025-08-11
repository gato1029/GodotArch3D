using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.utils;
public static class Message
{
    public static ConfirmationDialog ShowConfirmation(Node parent,string message)
    {
        var dialog = new ConfirmationDialog
        {
            DialogText = message,
            OkButtonText = "Sí",
            CancelButtonText = "Cancelar"
        };

        // Agregarlo como hijo temporal (se eliminará al cerrar)
        parent.AddChild(dialog);

        dialog.VisibilityChanged += () =>
        {
            if (!dialog.Visible) // Solo cuando se oculta
            {
                dialog.QueueFree();
            }
        };


        // Mostrar el diálogo centrado
        dialog.PopupCentered();
        return dialog;
    }
    public static AcceptDialog ShowMessage(Node parent, string message)
    {
        var dialog = new AcceptDialog
        {
            DialogText = message,            
            DialogCloseOnEscape = true,
            DialogHideOnOk = true,
            OkButtonText = "Aceptar"
        };

        parent.AddChild(dialog);

        dialog.VisibilityChanged += () =>
        {
            if (!dialog.Visible)
            {
                dialog.QueueFree();
            }
        };

        dialog.PopupCentered();
        return dialog;
    }

}
