using Arch.AOT.SourceGenerator;
using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TileData = GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileData;
namespace GodotEcsArch.sources.managers.Tilemap
{

    public class Tile
    {        
        public int idTile { get; set; }
        public EntityReference entityReference { get; set; }
        public bool isAnimate { get; set; }       
        public Rid rid { get; set; }
        public int instance { get; set; }
        public Transform3D transform3D { get; set; }

        public Vector2I tilePosition { get; set; }       

        public Tile(Vector2I tilePosition)
        {
            this.tilePosition = tilePosition;
            transform3D = new Transform3D(Basis.Identity, Vector3.Zero);
        }
        public Tile()
        {            

        }
        public void UpdateTile(Rid rid, int instance, int positionBatchTexture, Vector3 worldPosition, float scale, int idTile)
        {
            transform3D = new Transform3D(Basis.Identity, worldPosition);
            transform3D = transform3D.ScaledLocal(new Vector3(scale, scale, 1));

            //this.idMaterial = idMaterial;
            this.rid = rid;
            this.instance = instance;
            this.idTile = idTile;
            
            TileData tileData = TilesManager.Instance.GetTileData(idTile);
            if (tileData.type == "TileSimpleData")
            {
                TileSimpleData simpleData = (TileSimpleData)tileData;
                
                RenderingServer.MultimeshInstanceSetTransform(rid, instance, transform3D);
                RenderingServer.MultimeshInstanceSetCustomData(rid, instance, new Godot.Color(simpleData.idInternalPosition, 0, 0, 0));
                RenderingServer.MultimeshInstanceSetColor(rid, instance, new Color(0, 0, 0, positionBatchTexture));
                //  RenderingServer.MultimeshInstanceSetColor(rid, instance, tileData.color);
            }
            if (tileData.type == "TileDynamicData")
            {
                TileDynamicData tileDynamicData = (TileDynamicData)tileData;
                
                RenderingServer.MultimeshInstanceSetTransform(rid, instance, transform3D);
                RenderingServer.MultimeshInstanceSetCustomData(rid, instance, new Godot.Color(tileDynamicData.x, tileDynamicData.y, tileDynamicData.widht, tileDynamicData.height));
                RenderingServer.MultimeshInstanceSetColor(rid, instance, new Color(0, 0, 0, positionBatchTexture));
            }
            if (tileData.type == "TileAnimateData")
            {
                TileAnimateData tileAnimateData = (TileAnimateData)tileData;
                
                RenderingServer.MultimeshInstanceSetTransform(rid, instance, transform3D);
                RenderingServer.MultimeshInstanceSetColor(rid, instance, new Color(0, 0, 0, positionBatchTexture));
                Entity entity = EcsManager.Instance.World.Create();
                TileAnimation tileAnimation = new TileAnimation();
                tileAnimation.TileAnimateData = tileAnimateData;
                tileAnimation.frameRender = new Color
                {
                    R = tileAnimateData.framesArray[0].x,
                    G = tileAnimateData.framesArray[0].y,
                    B = tileAnimateData.framesArray[0].widht,
                    A = tileAnimateData.framesArray[0].height
                };


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
            //idMaterial = 0;
            rid = default;
            instance = -1;
            if (isAnimate)
            {
                EcsManager.Instance.World.Destroy(entityReference.Entity);
            }
        }
    }



    public class TileAnimation 
    {
        public Rid rid;
        public int instance;
        public TileAnimateData TileAnimateData;
        public Color frameRender;
        public int currentFrame;
        public int currentFrameIndex;
        public float TimeSinceLastFrame;        
        public bool loop;
        public bool complete;
        public int horizontalOrientation;
    }
}
