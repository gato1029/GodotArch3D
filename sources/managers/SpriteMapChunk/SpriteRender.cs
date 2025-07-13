using Arch.Core;
using Godot;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GodotEcsArch.sources.managers.SpriteMapChunk;


public class SpriteRender
{    
    public EntityReference entityReference { get; set; }
    public bool isAnimate { get; set; }
    public Rid rid { get; set; }
    public int instance { get; set; }
    public Transform3D transform3D { get; set; }
    public Vector2I tilePosition { get; set; }
    public int idMaterial { get; set; }
   
    public SpriteRender(Vector2I tilePosition)
    {
        this.tilePosition = tilePosition;
        transform3D = new Transform3D(Basis.Identity, Vector3.Zero);
    }
    public SpriteRender()
    {

    }
    public void UpdateTile(Rid rid, int instance, int textureBatchPosition,Vector3 worldPosition, SpriteData data) 
    {
       
        transform3D = new Transform3D(Basis.Identity, worldPosition);
        transform3D = transform3D.ScaledLocal(new Vector3(data.scale, data.scale, 1));

        idMaterial = data.idMaterial;
        isAnimate = false;
        this.rid = rid;
        this.instance = instance;
        

        RenderingServer.MultimeshInstanceSetTransform(rid, instance, transform3D);
        RenderingServer.MultimeshInstanceSetCustomData(rid, instance, new Godot.Color(data.x, data.y, data.widhtFormat, data.heightFormat));
        RenderingServer.MultimeshInstanceSetColor(rid, instance,new Color(0,0,0, textureBatchPosition));
          
    }
    public void UpdateTile(Rid rid, int instance, int textureBatchPosition, Vector3 worldPosition, SpriteData data, AnimationStateData dataAnimation)
    {
        isAnimate = true;
        transform3D = new Transform3D(Basis.Identity, worldPosition);
        transform3D = transform3D.ScaledLocal(new Vector3(data.scale, data.scale, 1));

        this.idMaterial = data.idMaterial;
        this.rid = rid;
        this.instance = instance;
        
        RenderingServer.MultimeshInstanceSetTransform(rid, instance, transform3D);
        RenderingServer.MultimeshInstanceSetCustomData(rid, instance, new Godot.Color(data.x, data.y, data.widhtFormat, data.heightFormat));
        RenderingServer.MultimeshInstanceSetColor(rid, instance, new Color(0, 0, 0, textureBatchPosition));
        //if (data is SpriteData spriteData)
        //{
        //    transform3D = new Transform3D(Basis.Identity, worldPosition);
        //    transform3D = transform3D.ScaledLocal(new Vector3(spriteData.scale, spriteData.scale, 1));

        //    this.idMaterial = spriteData.idMaterial;
        //    this.rid = rid;
        //    this.instance = instance;

        //    RenderingServer.MultimeshInstanceSetTransform(rid, instance, transform3D);
        //    RenderingServer.MultimeshInstanceSetCustomData(rid, instance, new Godot.Color(spriteData.x, spriteData.y, spriteData.widhtFormat, spriteData.heightFormat));
        //}
        //else
        //{
        //    throw new ArgumentException("data must implement IDataSprite");
        //}
    }
  
    public void FreeRidRender()
    {
        RenderingServer.MultimeshInstanceSetCustomData(rid, instance, new Godot.Color(-1, -1, -1, -1));
        //idMaterial = 0;
        rid = default;
        instance = -1;
        if (isAnimate)
        {
            EcsManager.Instance.World.Destroy(entityReference.Entity);
        }
    }

   
}
