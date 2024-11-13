using Arch.Core;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class RenderWindowGui: SingletonBase<RenderWindowGui>
{
    private MainWindowGodot window;
    private bool isActive;

    public bool IsActive { get => isActive; set => isActive = value; }

    protected override void Initialize()
    {
       
    }

    public void SetNode(MainWindowGodot window)
    {
        this.window = window;
    }
}

