using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Arch.AOT.SourceGenerator;
using static SpriteSimple;
using static SpriteMultimesh;
using System.Xml.Linq;


[Component]
public struct Sprite3D
{
    public Rid idRid;   
    public int idSpriteOrAnimation;
    public int idInstance;

    public byte spriteAnimation; // 0 - Sprite 1- Animation
    public Byte layer;
}

[Component]
public struct Animation
{
    public AnimationAction currentAction;
    public AnimationAction updateAction;

    public FrameAnimation frameAnimation;
    public int CurrentFrame;
    public float TimeSinceLastFrame;
    public float TimePerFrame;
    public bool loop;
    public bool complete;
    public int horizontalOrientation;
}


public struct FrameAnimation
{
    public int startFrame;
    public int totalFrames;
    public float timePerFrame;

    public FrameAnimation(int startFrame, int totalFrames, float timePerFrame) : this()
    {
        this.totalFrames = totalFrames - 1;
        this.startFrame = startFrame;
        this.timePerFrame = timePerFrame;
    }
}
internal class TypeAnimation
{

    public FrameAnimation[] Frames { get; set; }  // Array de fotogramas
    public float FrameDuration { get; set; } // Duración de cada fotograma
    public bool Looping { get; set; }  // Si la animación se repite
    public string NameTypeAnimation { get; set; }

    public TypeAnimation(int frameCount, string nameTypeAnimation, bool loop = true)
    {
        NameTypeAnimation = nameTypeAnimation; 
        Frames = new FrameAnimation[frameCount];
        Looping = loop;
    }

    public void AddFrame(int index, FrameAnimation frame)
    {
        Frames[index] = frame;
    }
    public FrameAnimation GetFrame(int index)
    {
        return Frames[index];
    }
}
internal class AnimationIndividual
{
    public int idAtlas;
    public int idAnimation;
    public string nameAnimation;

    public TypeAnimation[] typeAnimations { get; set; }
    public AnimationIndividual(string name, int countTypeAnimation)
    {
        nameAnimation = name;
        typeAnimations = new TypeAnimation[countTypeAnimation];
    }

    public void AddTypeAnimation(int idTypeAnimation, TypeAnimation typeAnimation)
    {
        typeAnimations[idTypeAnimation] = typeAnimation;
    }

    public TypeAnimation GetTypeAnimation(int idTypeAnimation)
    {
        return typeAnimations[idTypeAnimation];
    }
    public TypeAnimation GetTypeAnimation(AnimationAction idAnimationAction)
    {
        return typeAnimations[(int)idAnimationAction];
    }
    public FrameAnimation GetFrame(int idTypeAnimation, int idMovement)
    {
        return typeAnimations[idTypeAnimation].GetFrame(idMovement);
    }
    public FrameAnimation GetFrame(AnimationAction idAnimationAction, int idMovement)
    {
        return typeAnimations[(int)idAnimationAction].GetFrame(idMovement);
    }
}
internal class AtlasMultimesh
{
    public Rid idMultimeshInstance;
    public Texture2D texture2D;
    public ShaderMaterial shaderMaterial;
    public MultiMesh multiMesh;
    public Mesh mesh;
    public ushort maxInstance;
    public int currentInstance;
    public Stack<int> freeInstance;
    public Vector3 tileSize;

    public AtlasMultimesh()
    {
        currentInstance = 0;
        freeInstance = new Stack<int>();
    }

    public int GetInstance()
    {
        int dev = currentInstance;
        currentInstance++;
        return dev;
    }
}
internal class SpriteAnimation
{   
    Dictionary<Vector3, Mesh> StoreMesh = new Dictionary<Vector3, Mesh>();

    Dictionary<int, AtlasMultimesh> StoreAtlas = new Dictionary<int, AtlasMultimesh>();
    Dictionary<int, AnimationIndividual> StoreAnimations = new Dictionary<int, AnimationIndividual>();


    ShaderMaterial materialGeneric;
    public SpriteAnimation()
    {
        materialGeneric = GD.Load<ShaderMaterial>("res://resources/Material/Sprite3DMultimesh.tres");
    }

    public void AddFreeInstance(int idAnimation, int idInstance)
    {      
        AnimationIndividual textureIndividual = StoreAnimations[idAnimation];

        AtlasMultimesh atlasMultimesh = StoreAtlas[textureIndividual.idAtlas];
        atlasMultimesh.freeInstance.Push(idInstance);
    }

    public AnimationIndividual GetSprite(int idAnimation)
    {
        if (!StoreAnimations.ContainsKey(idAnimation))
        {
            return default;
        }
        return StoreAnimations[idAnimation];
    }
    public AnimationIndividual GetAnimation(int idSprite)
    {
        if (!StoreAnimations.ContainsKey(idSprite))
        {
            return default;
        }

        AnimationIndividual textureIndividual = StoreAnimations[idSprite];     
        return StoreAnimations[idSprite];
    }
    public AnimationIndividual GetAnimation(int idSprite, out AtlasMultimesh atlasMultimesh)
    {
        atlasMultimesh = default;
        if (!StoreAnimations.ContainsKey(idSprite))
        {
            return default;
        }

        AnimationIndividual textureIndividual = StoreAnimations[idSprite];

        atlasMultimesh = StoreAtlas[textureIndividual.idAtlas];
        return StoreAnimations[idSprite];
    }
    public int CreateAnimation(int idAtlas,string nameAnimation , int countTypesAnimation)
    {
        if (!StoreAtlas.ContainsKey(idAtlas))
        {
            return default;
        }
        int idAnimation = StoreAnimations.Count + 1;
        AnimationIndividual animacionIndividual =  new AnimationIndividual(nameAnimation, countTypesAnimation);
        animacionIndividual.idAtlas = idAtlas;      
        animacionIndividual.nameAnimation = nameAnimation;
        StoreAnimations.Add(idAnimation, animacionIndividual);          
        return idAnimation;
    }
    public int CreateAnimation(int idAtlas, int idAnimation, string nameAnimation, int countTypesAnimation)
    {
        if (!StoreAtlas.ContainsKey(idAtlas))
        {
            return default;
        }
        if (!StoreAnimations.ContainsKey(idAnimation))
        {
            AnimationIndividual animacionIndividual = new AnimationIndividual(nameAnimation, countTypesAnimation);
            animacionIndividual.idAtlas = idAtlas;
            animacionIndividual.nameAnimation = nameAnimation;
            StoreAnimations.Add(idAnimation, animacionIndividual);
            return idAnimation;
        }
        return -1;
    }
    public int LoadTexture(string pathTexture, Vector3 tileSize, Vector3 offset, Vector2 unitPixel)
    {
        Texture2D texture2D = GD.Load<Texture2D>(pathTexture);

        int idAtlas = StoreAtlas.Count + 1;
        ushort instanceMax = 65500;

        float widht = texture2D.GetWidth() / tileSize.X;
        float height = texture2D.GetHeight() / tileSize.Y;

        ShaderMaterial shaderMaterial = (ShaderMaterial)materialGeneric.Duplicate();
        shaderMaterial.SetShaderParameter("mtexture", texture2D);
        shaderMaterial.SetShaderParameter("ancho", widht);
        shaderMaterial.SetShaderParameter("alto", height);

        Mesh meshBase = (Mesh)GetMesh(tileSize,offset, unitPixel).Duplicate();
        meshBase.SurfaceSetMaterial(0, shaderMaterial);

        MultiMesh multiMesh = new MultiMesh();
        multiMesh.Mesh = meshBase;
        multiMesh.UseCustomData = true;
        multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
        multiMesh.InstanceCount = instanceMax;
        multiMesh.VisibleInstanceCount = instanceMax;

        AtlasMultimesh atlasMultimesh = new AtlasMultimesh();
        atlasMultimesh.multiMesh = multiMesh;
        atlasMultimesh.shaderMaterial = shaderMaterial;
        atlasMultimesh.texture2D = texture2D;

        atlasMultimesh.mesh = meshBase;
        atlasMultimesh.maxInstance = instanceMax;
        atlasMultimesh.currentInstance = 0;
        atlasMultimesh.tileSize = tileSize;
        atlasMultimesh.freeInstance = new Stack<int>();

        Rid instanceRid = RenderingServer.InstanceCreate();
        RenderingServer.InstanceSetBase(instanceRid, multiMesh.GetRid());
        RenderingServer.InstanceSetScenario(instanceRid, EcsManager.Instance.RidWorld3D);

        atlasMultimesh.idMultimeshInstance = instanceRid;

        StoreAtlas.Add(idAtlas, atlasMultimesh);

        return idAtlas;
    }

    Mesh GetMesh(Vector3 meshSize,Vector3 offset,Vector2 unitPixel)
    {
        if (StoreMesh.ContainsKey(meshSize))
        {
            return StoreMesh[meshSize];
        }

        int idMesh = StoreMesh.Count + 1;
        Mesh mesh = MeshCreator.CreateSquareMesh(meshSize.X, meshSize.Y,unitPixel,offset);
        StoreMesh.Add(meshSize, mesh);
        return mesh;
    }

}
internal class SpriteMultimesh
{
    public struct TextureIndividualMulti
    {
        public int idAtlas;
        public int idSprite;
        public int idInternalSprite;        
    }

 
    
    Dictionary<Vector3, Mesh> StoreMesh = new Dictionary<Vector3, Mesh>();
    
    Dictionary<int, AtlasMultimesh> StoreAtlas = new Dictionary<int, AtlasMultimesh>();
    Dictionary<int, TextureIndividualMulti> StoreAtlasTextures = new Dictionary<int, TextureIndividualMulti>();
    
    ShaderMaterial materialGeneric;
    public SpriteMultimesh()
    {
        materialGeneric = GD.Load<ShaderMaterial>("res://resources/Material/Sprite3DMultimesh.tres");
    }

    public TextureIndividualMulti GetSprite(int idSprite)
    {
        if (!StoreAtlasTextures.ContainsKey(idSprite))
        {
            return default;
        }
        return StoreAtlasTextures[idSprite];
    }    
    public TextureIndividualMulti GetSprite(int idSprite,out AtlasMultimesh atlasMultimesh)
    {
     
    
        atlasMultimesh = default;
        if (!StoreAtlasTextures.ContainsKey(idSprite))
        {
            return default;
        }

        TextureIndividualMulti textureIndividual = StoreAtlasTextures[idSprite];     
       
        atlasMultimesh = StoreAtlas[textureIndividual.idAtlas];
        return StoreAtlasTextures[idSprite];
    }
    public void LoadTexture(string pathTexture, Vector3 tileSize, Vector3 offset, Vector2 unitPixel, bool atlas = false)
    {
        Texture2D texture2D = GD.Load<Texture2D>(pathTexture);

        int idAtlas = StoreAtlas.Count + 1;
        ushort instanceMax = 65500;

        float widht = texture2D.GetWidth()/tileSize.X;
        float height = texture2D.GetHeight() / tileSize.Y;

        ShaderMaterial shaderMaterial = (ShaderMaterial)materialGeneric.Duplicate();
        shaderMaterial.SetShaderParameter("mtexture", texture2D);
        shaderMaterial.SetShaderParameter("ancho", widht);
        shaderMaterial.SetShaderParameter("alto", height);

        Mesh meshBase = (Mesh)GetMesh(tileSize, offset, unitPixel).Duplicate();
        meshBase.SurfaceSetMaterial(0, shaderMaterial);
             
        MultiMesh multiMesh = new MultiMesh();
        multiMesh.Mesh = meshBase;
        multiMesh.UseCustomData = true;
        multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
        multiMesh.InstanceCount = instanceMax;
        multiMesh.VisibleInstanceCount = instanceMax;
                
        AtlasMultimesh atlasMultimesh = new AtlasMultimesh();
        atlasMultimesh.multiMesh = multiMesh;
        atlasMultimesh.shaderMaterial = shaderMaterial;
        atlasMultimesh.texture2D = texture2D;

        atlasMultimesh.mesh = meshBase;
        atlasMultimesh.maxInstance = instanceMax;
        atlasMultimesh.currentInstance = 0;
        atlasMultimesh.tileSize = tileSize;
        atlasMultimesh.freeInstance = new Stack<int>();

        Rid instanceRid = RenderingServer.InstanceCreate();
        RenderingServer.InstanceSetBase(instanceRid, multiMesh.GetRid());
        RenderingServer.InstanceSetScenario(instanceRid, EcsManager.Instance.RidWorld3D);

        atlasMultimesh.idMultimeshInstance = instanceRid;

        StoreAtlas.Add(idAtlas, atlasMultimesh);



        if (atlas)
        {

            int xRow = texture2D.GetWidth() / (int)tileSize.X;
            int yColumn = texture2D.GetHeight() / (int)tileSize.Y;

            int idIndividual = 0;
            for (int x = 0; x < xRow; x++)
            {
                for (int y = 0; y < yColumn; y++)
                {
                    int idSprite = StoreAtlasTextures.Count + 1;
                    TextureIndividualMulti textureIndividual = new TextureIndividualMulti();
                    textureIndividual.idAtlas = idAtlas;                    
                    textureIndividual.idInternalSprite = idIndividual;
                    textureIndividual.idSprite = idSprite;                    
                    StoreAtlasTextures.Add(idSprite, textureIndividual);
                    idIndividual++;
                }
            }
        }
        else
        {
            int idSprite = StoreAtlasTextures.Count + 1;
            TextureIndividualMulti textureIndividual = new TextureIndividualMulti();
            textureIndividual.idAtlas = idAtlas;
            textureIndividual.idInternalSprite = default;
            textureIndividual.idSprite = idSprite;          
            StoreAtlasTextures.Add(idSprite, textureIndividual);

        }

    }

    Mesh GetMesh(Vector3 meshSize, Vector3 offset, Vector2 unitPixel)
    {
        if (StoreMesh.ContainsKey(meshSize))
        {        
            return StoreMesh[meshSize];
        }

        int idMesh = StoreMesh.Count + 1;
        Mesh mesh = MeshCreator.CreateSquareMesh(meshSize.X, meshSize.Y, unitPixel,offset);
        StoreMesh.Add(meshSize, mesh);      
        return mesh;
    }

    public void AddFreeInstance(int idSprite,int idInstance)
    {
        TextureIndividualMulti textureIndividual = StoreAtlasTextures[idSprite];
        AtlasMultimesh atlasMultimesh = StoreAtlas[textureIndividual.idAtlas];
        atlasMultimesh.freeInstance.Push(idInstance);
    }
}

internal class SpriteSimple
{
    public struct TextureIndividual
    {
        public int idMaterial;
        public int idTexture;
        public int idMesh;
        public int idInternalSprite;
        public int idAtlasTexture;
        public Mesh mesh;
    }

    Dictionary<int, Vector3> StoreMeshVector = new Dictionary<int, Vector3>();
    Dictionary<int, Mesh> StoreMesh = new Dictionary<int, Mesh>();
    Dictionary<Vector3, int> StoreMeshSize = new Dictionary<Vector3, int>();

    Dictionary<int, Texture2D> StoreTexture = new Dictionary<int, Texture2D>();
    Dictionary<int, ShaderMaterial> StoreMaterial = new Dictionary<int, ShaderMaterial>();

    Dictionary<int, TextureIndividual> StoreAtlasTextures = new Dictionary<int, TextureIndividual>();


    ShaderMaterial materialGeneric;
    public SpriteSimple()
    {
        materialGeneric = GD.Load<ShaderMaterial>("res://resources/Material/Sprite3DGeneric.tres");
    }

    public TextureIndividual GetSprite(int idSprite)
    {
        if (!StoreAtlasTextures.ContainsKey(idSprite))
        {
            return default;
        }
        return StoreAtlasTextures[idSprite];
    }
    public TextureIndividual GetSprite(int idSprite, out ShaderMaterial shaderMaterial,out Mesh mesh,out Texture2D texture2D, out Vector3 tileSize)
    {
        shaderMaterial = null;
        mesh = null;
        texture2D = null;
        tileSize = default;
        if (!StoreAtlasTextures.ContainsKey(idSprite))
        {
            return default;
        }

        TextureIndividual textureIndividual = StoreAtlasTextures[idSprite];
        shaderMaterial = StoreMaterial[textureIndividual.idMaterial];
        mesh = textureIndividual.mesh;
        texture2D = StoreTexture[textureIndividual.idTexture];
        tileSize = StoreMeshVector[textureIndividual.idMesh];
        return StoreAtlasTextures[idSprite];
    }
    public void LoadTexture(string pathTexture, Vector3 tileSize, Vector3 offset, Vector2 unitPixel,bool atlas=false)
    {
        Texture2D texture2D = GD.Load<Texture2D>(pathTexture);

        int idTexture = StoreTexture.Count + 1;        
        int idMaterial = StoreMaterial.Count + 1;
        int idMesh = GetMesh(tileSize,offset, unitPixel);

        ShaderMaterial shaderMaterial = (ShaderMaterial)materialGeneric.Duplicate();
        shaderMaterial.SetShaderParameter("mtexture", texture2D);

        StoreTexture.Add(idTexture, texture2D);
        StoreMaterial.Add(idMaterial, shaderMaterial);

        if (atlas)
        {

            int xRow = texture2D.GetWidth() / (int)tileSize.X;
            int yColumn = texture2D.GetHeight() / (int)tileSize.Y;

            int idIndividual = 0;
            for (int x = 0; x < xRow; x++)
            {
                for (int y = 0; y < yColumn; y++)
                {
                    int idSprite = StoreAtlasTextures.Count + 1;
                    TextureIndividual textureIndividual = new TextureIndividual();
                    textureIndividual.idTexture = idTexture;
                    textureIndividual.idMaterial = idMaterial;
                    textureIndividual.idMesh = idMesh;
                    textureIndividual.idInternalSprite = idIndividual;
                    textureIndividual.idAtlasTexture = idSprite;
                    textureIndividual.mesh = (Mesh)StoreMesh[idMesh].Duplicate();
                    StoreAtlasTextures.Add(idSprite, textureIndividual);
                    idIndividual++;
                }
            }
        }
        else
        {
            int idSprite = StoreAtlasTextures.Count + 1;
            TextureIndividual textureIndividual = new TextureIndividual();
            textureIndividual.idTexture = idTexture;
            textureIndividual.idMaterial = idMaterial;
            textureIndividual.idMesh = idMesh;
            textureIndividual.idInternalSprite = -1;
            textureIndividual.idAtlasTexture = idSprite;
            textureIndividual.mesh = (Mesh)StoreMesh[idMesh].Duplicate();
            StoreAtlasTextures.Add(idSprite, textureIndividual);

        }
        
    }

    int GetMesh(Vector3 meshSize, Vector3 offset, Vector2 unitPixel)
    {
        if (StoreMeshSize.ContainsKey(meshSize))
        {
            int id = StoreMeshSize[meshSize];
            return id;
        }

        int idMesh = StoreMesh.Count + 1;
        Mesh mesh = MeshCreator.CreateSquareMesh(meshSize.X,meshSize.Y,unitPixel, offset);
        StoreMesh.Add(idMesh, mesh);
        StoreMeshSize.Add(meshSize,idMesh);
        StoreMeshVector.Add(idMesh, meshSize);
        return idMesh;
    }

}

struct MaterialTexture
{
    public bool availableSpace;
    public ShaderMaterial material;
    Godot.Collections.Array<Texture2D> arrayTexture;
    int currentIndex;
    int limit;
    public MaterialTexture(ShaderMaterial materialGeneric)
    {
        this.availableSpace = true;
        this.material = (ShaderMaterial)materialGeneric.Duplicate();
        this.arrayTexture = new Godot.Collections.Array<Texture2D>();
        this.material.SetShaderParameter("mtextures", arrayTexture);
        currentIndex = -1;
        limit = 10;
    }

    public bool IsAvailableSpace()
    {
        if (currentIndex + 1 == limit)
        {
            this.availableSpace = false;
            return false;
        }
        return true;
    }
    

    public int AddTexture(string pathTexture)
    {
        currentIndex++;
        Texture2D texture2D = GD.Load<Texture2D>(pathTexture);
        if (currentIndex == limit)
        {
            this.availableSpace = false;
            return -1;
        }        
        arrayTexture.Add(texture2D);
        return currentIndex;
    }

    public int AddTexture(Texture2D texture2D)
    {
        if (currentIndex == limit)
        {
            return -1;
        }
        currentIndex++;
        arrayTexture.Add(texture2D);
        return currentIndex;
    }

    public readonly Texture2D GetTexture2D(int index)
    {
        return arrayTexture[index];
    }
}
internal class SpriteManager:SingletonBase<SpriteManager>
{  
    SpriteSimple spritesStore;
    SpriteMultimesh spriteMultimeshStore;
    SpriteAnimation spriteAnimation;

    internal SpriteSimple SpritesStore { get => spritesStore; set => spritesStore = value; }
    internal SpriteMultimesh SpriteMultimeshStore { get => spriteMultimeshStore; set => spriteMultimeshStore = value; }
    internal SpriteAnimation SpriteAnimation { get => spriteAnimation; set => spriteAnimation = value; }

    protected override void Initialize()
    {
        SpritesStore = new SpriteSimple();
        SpriteMultimeshStore = new SpriteMultimesh();
        SpriteAnimation = new SpriteAnimation();
    }

    public void LoadTexture(string pathTexture, Vector3 tileSize, Vector3 offset, Vector2 unitPixel, bool atlas = false)
    {
        SpritesStore.LoadTexture(pathTexture, tileSize, offset, unitPixel ,atlas);
    }
    public int LoadTextureAnimation(string pathTexture, Vector3 tileSize, Vector3 offset, Vector2 unitPixel)
    {
        return  SpriteAnimation.LoadTexture(pathTexture, tileSize,offset,unitPixel);
    }
    public int CreateAnimation(int idAtlasAnimation, string nameAnimation, int countTypesAnimation)
    {
        return SpriteAnimation.CreateAnimation(idAtlasAnimation, nameAnimation, countTypesAnimation);
    }
    public int CreateAnimation(int idAtlasAnimation, int idAnimation, string nameAnimation, int countTypesAnimation)
    {
        return SpriteAnimation.CreateAnimation(idAtlasAnimation, idAnimation, nameAnimation, countTypesAnimation);
    }
    public AnimationIndividual GetAnimation(int idAnimationSpriteSheet)
    {
        return SpriteAnimation.GetAnimation(idAnimationSpriteSheet);
    }
    public void LoadTextureMultimesh(string pathTexture, Vector3 tileSize, Vector3 offset, Vector2 unitPixel)
    {       
        SpriteMultimeshStore.LoadTexture(pathTexture, tileSize, offset,unitPixel, true);      
    }

    protected override void Destroy()
    {
    
    }
   
    public Sprite3D CreateSpriteSimple(int spriteId)
    {
        TextureIndividual textureIndividual = SpritesStore.GetSprite(spriteId, out ShaderMaterial shaderMaterial, out Mesh mesh, out Texture2D texture2D, out Vector3 tileSize);

        Rid instanceRid = RenderingServer.InstanceCreate();
        Rid meshRid = mesh.GetRid();
        Rid materialRid = shaderMaterial.GetRid();
         
        RenderingServer.InstanceSetBase(instanceRid, meshRid);
        RenderingServer.InstanceSetScenario(instanceRid, EcsManager.Instance.RidWorld3D);      
        //RenderingServer.MeshAddSurfaceFromArrays(meshRid, RenderingServer.PrimitiveType.Triangles, quad);        
        RenderingServer.MeshSurfaceSetMaterial(meshRid, 0, materialRid);
        //RenderingServer.InstanceGeometrySetShaderParameter(instanceRid, "index", textureInternal.idTexture);
      
        
        RenderingServer.InstanceSetBase(instanceRid, meshRid);
        if (textureIndividual.idInternalSprite!=-1)
        {
            int segmentX = texture2D.GetWidth() / (int) tileSize.X;
            int segmentY = texture2D.GetHeight() / (int)tileSize.Y;
            Vector2[] uvs = GetUVSegment(textureIndividual.idInternalSprite, texture2D.GetWidth(), texture2D.GetHeight(), tileSize.X,tileSize.Y );
            RenderingServer.MeshSurfaceUpdateAttributeRegion(meshRid, 0, 0, ConvertUVsToBytes(uvs));        
        }

        // Asignar el transform y agregar al escenario

        Transform3D xform = new Transform3D(Basis.Identity, Vector3.Zero);       
        xform = xform.TranslatedLocal(new Vector3(2, 2, 1));
        RenderingServer.InstanceSetTransform(instanceRid, xform);

        Sprite3D sprite3D = new()
        {
            idRid = instanceRid,
            idSpriteOrAnimation = spriteId,
            idInstance = -1
        };
        return sprite3D;


    }
 
    public Sprite3D CreateSpriteMulti(int spriteId)
    {
        TextureIndividualMulti textureIndividualMulti = SpriteMultimeshStore.GetSprite(spriteId,out AtlasMultimesh atlasMultimesh);
      
        Rid multimeshRid = atlasMultimesh.multiMesh.GetRid();

        int currentInstance = atlasMultimesh.GetInstance();
     
        //RenderingServer.MeshSurfaceSetMaterial(meshRid, 0, materialRid);

        Transform3D xform = new Transform3D(Basis.Identity, Vector3.Zero);
        //xform = xform.Scaled(new Vector3(2f, 3f, 1f));
        xform = xform.Translated(new Vector3(1, 2, 1));
        RenderingServer.MultimeshInstanceSetTransform(multimeshRid, currentInstance, xform);
        RenderingServer.MultimeshInstanceSetCustomData(multimeshRid, currentInstance, new Color(textureIndividualMulti.idInternalSprite, 0, 0, 0));    
        Sprite3D sprite3D = new()
        {
            idRid = multimeshRid,
            idSpriteOrAnimation = spriteId,
            idInstance = currentInstance,
             spriteAnimation = 0
        };
        return sprite3D;
    }


    public Sprite3D CreateAnimationInstance(int idAnimation)
    {
        SpriteAnimation.GetAnimation(idAnimation, out AtlasMultimesh atlasMultimesh);      
        Rid multimeshRid = atlasMultimesh.multiMesh.GetRid();
        int currentInstance = atlasMultimesh.GetInstance();

        Sprite3D sprite3D = new()
        {
            idRid = multimeshRid,
            idSpriteOrAnimation = idAnimation,
            idInstance = currentInstance,
            spriteAnimation = 1
        };
        return sprite3D;
    }

    void CreateMultimesh(string pathTexture)
    {
        
    }
    public  byte[] ConvertUVsToBytes(Vector2[] uvs)
    {
        int byteCount = uvs.Length * sizeof(float) * 2; // Each Vector2 has 2 floats, each float is 4 bytes
        byte[] byteArray = new byte[byteCount];
        System.Buffer.ByteLength(byteArray); // Ensure correct size

        for (int i = 0; i < uvs.Length; i++)
        {
            Vector2 v = uvs[i];
            // Convert each float to bytes and copy to byteArray
            System.Buffer.BlockCopy(System.BitConverter.GetBytes(v.X), 0, byteArray, i * sizeof(float) * 2, sizeof(float));
            System.Buffer.BlockCopy(System.BitConverter.GetBytes(v.Y), 0, byteArray, i * sizeof(float) * 2 + sizeof(float), sizeof(float));
        }
        return byteArray;
    }

    private Vector2[] GetUVRegion(float textureWidth, float textureHeight, float regionX, float regionY, float regionWidth, float regionHeight)
    {
        return new Vector2[]
        {
        new Vector2(regionX / textureWidth, (regionY + regionHeight) / textureHeight), // Inferior izquierda
        new Vector2((regionX + regionWidth) / textureWidth, (regionY + regionHeight) / textureHeight), // Inferior derecha
        new Vector2((regionX + regionWidth) / textureWidth, regionY / textureHeight), // Superior derecha
        new Vector2(regionX / textureWidth, regionY / textureHeight)  // Superior izquierda
        };
    }
    public static Vector2[] GetUVs(int widthSegments, int heightSegments, int position)
    {
        
        float segmentWidth = 1.0f / widthSegments;
        float segmentHeight = 1.0f / heightSegments;


        int xIndex = position % widthSegments;
        int yIndex = position / widthSegments;

  
        float uMin = xIndex * segmentWidth;
        float vMin = yIndex * segmentHeight;
        float uMax = (xIndex + 1) * segmentWidth;
        float vMax = (yIndex + 1) * segmentHeight;

     
        return new Vector2[]
        {
            new Vector2(uMin, vMax), // Bottom left
            new Vector2(uMax, vMax), // Bottom right
            new Vector2(uMax, vMin), // Top right
            new Vector2(uMin, vMin)  // Top left
        };

    }
    private Vector2[] GetUVSegment(int row, int column,  float textureWidth, float textureHeight, float segmentWidth, float segmentHeight)
    {
        // Calcular las coordenadas de la region (UV)
        float regionX = column * segmentWidth;
        float regionY = row * segmentHeight;

        // Calcular las coordenadas UV normales
        return new Vector2[]
        {
        new Vector2(regionX / textureWidth, (regionY + segmentHeight) / textureHeight), // Inferior izquierda
        new Vector2((regionX + segmentWidth) / textureWidth, (regionY + segmentHeight) / textureHeight), // Inferior derecha
        new Vector2((regionX + segmentWidth) / textureWidth, regionY / textureHeight), // Superior derecha
        new Vector2(regionX / textureWidth, regionY / textureHeight)  // Superior izquierda
        };
    }

    private Vector2[] GetUVSegment(int position, float textureWidth, float textureHeight, float segmentWidth, float segmentHeight)
    {
        // Calcular el número de columnas basado en el ancho de la textura y los segmentos
        int columns = (int)(textureWidth / segmentWidth);

        // Calcular la fila y columna basándose en la posición
        int row = position / columns;
        int column = position % columns;

        // Calcular las coordenadas de la región (UV)
        float regionX = column * segmentWidth;
        float regionY = row * segmentHeight;

        // Calcular las coordenadas UV normales
        return new Vector2[]
        {
        new Vector2(regionX / textureWidth, (regionY + segmentHeight) / textureHeight), // Inferior izquierda
        new Vector2((regionX + segmentWidth) / textureWidth, (regionY + segmentHeight) / textureHeight), // Inferior derecha
        new Vector2((regionX + segmentWidth) / textureWidth, regionY / textureHeight), // Superior derecha
        new Vector2(regionX / textureWidth, regionY / textureHeight)  // Superior izquierda
        };
    }
}

