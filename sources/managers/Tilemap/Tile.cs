using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Arch.AOT.SourceGenerator;
using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;

using TileData = GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileData;
namespace GodotEcsArch.sources.managers.Tilemap
{

    public class Tile
    {
        //public int idUnique {  get; set; }

        public int idTile { get; set; }
        public EntityReference entityReference { get; set; }
        public bool isAnimate { get; set; }
        public int idMaterial { get; set; }
        public Rid rid { get; set; }
        public int instance { get; set; }

        //public Vector2 position { get; set; }
        //public GeometricShape2D collisionBody { get; set; }
        public Vector2I tilePosition { get; set; }


        public Tile(Vector2I tilePosition)
        {
            this.tilePosition = tilePosition;
        }
        public Tile()
        {            

        }
        public void UpdateTile(int idMaterial,Rid rid, int instance, Transform3D xform, int idTile)
        {
            this.idMaterial = idMaterial;
            this.rid = rid;
            this.instance = instance;
            this.idTile = idTile;

            TileData tileData = TilesManager.Instance.GetTileData(idTile);
            if (tileData.type == "TileSimpleData")
            {
                TileSimpleData simpleData = (TileSimpleData)tileData;
                RenderingServer.MultimeshInstanceSetTransform(rid, instance, xform);
                RenderingServer.MultimeshInstanceSetCustomData(rid, instance, new Godot.Color(simpleData.idInternalPosition, 0, 0, 0));                
            }
            if (tileData.type == "TileDynamicData")
            {
                TileDynamicData tileDynamicData = (TileDynamicData)tileData;
                RenderingServer.MultimeshInstanceSetTransform(rid, instance, xform);
                RenderingServer.MultimeshInstanceSetCustomData(rid, instance, new Godot.Color(tileDynamicData.x, tileDynamicData.y, tileDynamicData.widht, tileDynamicData.height));
            }
            if (tileData.type == "TileAnimateData")
            {
                TileAnimateData tileAnimateData = (TileAnimateData)tileData;
                RenderingServer.MultimeshInstanceSetTransform(rid, instance, xform);
                RenderingServer.MultimeshInstanceSetCustomData(rid, instance, new Godot.Color(tileAnimateData.idFrames[0], 0, 0, 0));
                            
                Entity entity = EcsManager.Instance.World.Create();
                TileAnimation tileAnimation = new TileAnimation();
                tileAnimation.TileAnimateData = tileAnimateData;
                tileAnimation.currentFrameIndex = 0;
                tileAnimation.complete = false;
                tileAnimation.loop = true;
                tileAnimation.rid = rid;
                tileAnimation.instance = instance;
                entity.Add(tileAnimation);

                entityReference = entity.Reference();
            }
        }

        public void FreeTile()
        {
            RenderingServer.MultimeshInstanceSetCustomData(rid, instance, new Godot.Color(-1, 0, 0, 0));
            idMaterial = 0;
            rid = default;
            instance = -1;
            if (isAnimate)
            {
                EcsManager.Instance.World.Destroy(entityReference.Entity);
            }
        }
    }

    public class TileBase
    {
        public Rid idRid;
        public int idSpriteOrAnimation;
        public int idInstance;
        public byte spriteAnimation; // 0 - Sprite 1- Animation
        public Byte layer;
    }

    public class TileSimple : TileBase
    {
        
    }

    public class TileAnimation 
    {
        public Rid rid;
        public int instance;
        public TileAnimateData TileAnimateData;

        public int currentFrame;
        public int currentFrameIndex;
        public float TimeSinceLastFrame;        
        public bool loop;
        public bool complete;
        public int horizontalOrientation;
    }
}
