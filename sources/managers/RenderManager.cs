using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    internal class RenderManager: SingletonBase<RenderManager>
    {

    public Rect2 currentDisplay;
    protected override void Initialize()
    {
        
    }

    protected override void Destroy()
    {
        throw new NotImplementedException();
    }
}

