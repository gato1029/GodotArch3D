using Arch.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class EntityDebug
{
    public EntityDebug(bool isChecked, Entity entity)
    {
        this.isChecked = isChecked;
        this.entity = entity;
    }

    public string name;
    public Entity entity;
    public bool isChecked;
}

