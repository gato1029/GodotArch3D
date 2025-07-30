using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.CharacterCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.Resources.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using LiteDB;
using RectangleBinPacking;
using System;
using System.Collections.Generic;



public partial class WindowNewMaterial :  Window, IFacadeWindow<MaterialData>
{
    FileDialog fileDialog;
    int mode = 0;
    MaterialData objectData;
    bool ChangeTexture = false;
    public event IFacadeWindow<MaterialData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI                                
        ButtonDividir.Pressed+= TextureSplit_Clik; 
        ButtonSearchFile.Pressed+= FileSearch;
        ButtonSave.Pressed += Guardar_Click;
        ButtonDelete.Pressed += ButtonDelete_Pressed;
        fileDialog = new FileDialog();
        fileDialog.Title = "Buscar Imagen";

        // Configurar propiedades del FileDialog
        fileDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
        fileDialog.Access = FileDialog.AccessEnum.Filesystem; // Acceso al sistema de archivos
        fileDialog.Filters = new string[] { "*.png ; PNG Images" }; // Filtros de archivo
        fileDialog.AlwaysOnTop = true;
        fileDialog.ForceNative = true;
        // Conectar la señal "file_selected"
        fileDialog.FileSelected += OnFileSelected;
        // Agregar el nodo a la escena
        AddChild(fileDialog);
        objectData = new MaterialData();

        foreach (MaterialType value in Enum.GetValues(typeof(MaterialType)))
        {
            OptionButtonType.AddItem(value.ToString());
        }

        OptionButtonType.ItemSelected += OptionButtonType_ItemSelected;
    }

    private void OptionButtonType_ItemSelected(long index)
    {
        ChangeTexture = true;
    }

    public void SetData(MaterialData data)
    {
        objectData = data;

        LineEditName.Text = objectData.name;
        LineEditId.Text = objectData.id.ToString();
        LineEditPath.Text = objectData.pathTexture;
        SpinBoxX.Value = objectData.divisionPixelX;
        SpinBoxY.Value = objectData.divisionPixelY;
        LineEditCategory.Text = objectData.category;
        OptionButtonType.Selected = OptionButtonType.GetItemIndex(objectData.type);
        mode = 1;
        TextureSplit_Clik();
    }

    private void ButtonDelete_Pressed()
    {
        DataBaseManager.Instance.RemoveById<MaterialData>(objectData.id);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
        if (objectData.type <= 4)
        {
            GenerateMasterAtlas();
        }
    }
    private void FileSearch()
    {
        fileDialog.PopupCentered();
    }

    private void Guardar_Click()
    {
        if (LineEditPath.Text.StartsWith("AssetExternals"))
        {
            mode = 1;
        }
        else
        {
            mode = 0;
        }

        Image image;
        if (mode == 0)
        {
             image = TextureHelper.LoadImageLocal(LineEditPath.Text);
        }
        else
        {
            image = TextureHelper.LoadImageLocal(FileHelper.GetPathGameDB(LineEditPath.Text));
        }

        objectData.pathTexture = LineEditPath.Text;


        objectData.name = LineEditName.Text;
        objectData.heightTexture = image.GetHeight();
        objectData.widhtTexture = image.GetWidth();
        objectData.divisionPixelX = (int)SpinBoxX.Value;
        objectData.divisionPixelY = (int)SpinBoxY.Value;
        objectData.category = LineEditCategory.Text;
        objectData.type = OptionButtonType.GetItemId(OptionButtonType.GetSelectedId());
        if (LineEditId.Text == string.Empty)
        {
            int idnext = DataBaseManager.Instance.NextID<MaterialData>();
            string path = FileHelper.CopyFileToAssetExternals(LineEditPath.Text,"Material", idnext.ToString());
            objectData.pathTexture = path;
            DataBaseManager.Instance.InsertUpdateLog(objectData);
            
        }
        else
        {
            string path = FileHelper.CopyFileToAssetExternals(LineEditPath.Text, "Material", objectData.id.ToString());
            objectData.pathTexture = path;
            DataBaseManager.Instance.InsertUpdateLog(objectData, int.Parse(LineEditId.Text));
            
        }
       
        if (objectData.type <= 4 && ChangeTexture)
        {
            GenerateMasterAtlas();
        }
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }

    private void GenerateMasterAtlas()
    {
        MaterialType materialType = (MaterialType)objectData.type;
        BsonExpression bsonExpression = BsonExpression.Create("type = @0", objectData.type);
        var listData = DataBaseManager.Instance.FindAllFilter<MaterialData>(bsonExpression);

        int atlasSize = 4096;
        int idAtlas = 1;

        MaxRectsBinPack<int> maxRectsBinPack = new MaxRectsBinPack<int>(atlasSize, atlasSize, FreeRectChoiceHeuristic.RectBottomLeftRule, false);
        List<Texture2D> textures = new List<Texture2D>();
        List<InsertResult> placements = new List<InsertResult>();
        List<MaterialData> materialesIds = new List<MaterialData>();

        foreach (var item in listData)
        {
            InsertResult result = maxRectsBinPack.Insert(item.id, item.widhtTexture, item.heightTexture);

            // Si no cabe, guarda el atlas actual y empieza uno nuevo
            if (result == null)
            {
                // Guardar el atlas actual
                if (textures.Count > 0)
                {
                    CreateTextureAtlas(idAtlas, materialType, textures, placements, materialesIds, new Vector2I(atlasSize, atlasSize));
                    idAtlas++;
                }

                // Resetear para nuevo atlas
                maxRectsBinPack = new MaxRectsBinPack<int>(atlasSize, atlasSize, FreeRectChoiceHeuristic.RectBottomLeftRule, false);
                textures.Clear();
                placements.Clear();
                materialesIds.Clear();
                // Reintentar insertar en el nuevo atlas
                result = maxRectsBinPack.Insert(item.id, item.widhtTexture, item.heightTexture);
                if (result == null)
                {
                    GD.PrintErr($"Error: La textura ID {item.id} ({item.widhtTexture}x{item.heightTexture}) no cabe ni siquiera en un atlas vacío.");
                    continue;
                }
            }

            textures.Add((Texture2D)item.textureMaterial);
            placements.Add(result);
            materialesIds.Add(item);
        }

        // Guardar el último atlas si hay datos
        if (textures.Count > 0)
        {          
            CreateTextureAtlas(idAtlas, materialType,textures, placements, materialesIds, new Vector2I(atlasSize, atlasSize));
        }
    }
    public bool AreListsEqual(List<int> listA, List<int> listB)
    {
        if (listA == null || listB == null)
            return false;

        if (listA.Count != listB.Count)
            return false;

        for (int i = 0; i < listA.Count; i++)
        {
            if (listA[i] != listB[i])
                return false;
        }

        return true;
    }
    public  ImageTexture CreateTextureAtlas(int idAtlas, MaterialType materialType, List<Texture2D> textures, List<InsertResult> placements, List<MaterialData> materiales, Vector2I atlasSize)
    {
        string pathSave = "AssetExternals/Atlas/atlas_" + materialType.ToString() + "_" + idAtlas.ToString() + ".png";
        string path = FileHelper.GetPathGameDB(pathSave);
        int idAtlasHash = StableHash.FromString(pathSave);

        List<int> idMaterialsInternal = new List<int>();
        foreach (var item in materiales)
        {
            idMaterialsInternal.Add(item.id);
        }
        var textureAtlas = DataBaseManager.Instance.FindById<TextureMasterData>(idAtlasHash);
        //if (AreListsEqual(textureAtlas.listMaterials, idMaterialsInternal))
        //{
        //    GD.Print("Texturas en la misma posicion, no se realiza reestructuracion");
        //    return null;
        //}     

        if (textures.Count != placements.Count)
            throw new ArgumentException("Textures and placements count mismatch.");

        Image atlasImage = Image.CreateEmpty(atlasSize.X, atlasSize.Y, false, Image.Format.Rgba8);
        atlasImage.Fill(new Color(0, 0, 0, 0)); // fondo transparente

        for (int i = 0; i < textures.Count; i++)
        {
            Texture2D texture = textures[i];
            InsertResult placement = placements[i];

            Image image = texture.GetImage();
            image.Convert(Image.Format.Rgba8);

            Vector2I size = image.GetSize();            
            atlasImage.BlitRect(image, new Rect2I(Vector2I.Zero, size), new Vector2I(placement.X, placement.Y));
        }
       
        atlasImage.SavePng(path);
        
       
        List<int> idMaterials = new List<int>();
        for (int i = 0; i < materiales.Count; i++)
        {
            InsertResult placement = placements[i];
            MaterialData mat = materiales[i];
            idMaterials.Add(mat.id);
            mat.idTextureMaster = idAtlasHash;
            mat.originXTextureMaster = placement.X;
            mat.originYTextureMaster = placement.Y;
            DataBaseManager.Instance.InsertUpdateLog(mat, mat.id);

            NormalizeSpriteTiles(mat, placement);
        }

        TextureMasterData textureMasterData = new TextureMasterData();
        textureMasterData.id = idAtlasHash;
        textureMasterData.pathTexture = pathSave;
        textureMasterData.listMaterials = idMaterials;
        textureMasterData.materialType = materialType;
        DataBaseManager.Instance.InsertUpdate(textureMasterData);
        GD.Print("Textura nueva Generada:" + pathSave);
        ImageTexture atlasTexture =  ImageTexture.CreateFromImage(atlasImage);        
        return atlasTexture;
    }

    private void NormalizeSpriteTiles(MaterialData idMaterial, InsertResult placement)
    {
        NormalizeTiles(idMaterial, placement);
        NormalizeTilesAnimated(idMaterial, placement);

        NormalizeSingleSpriteGeneric<ResourceData>("spriteData", idMaterial, placement);
        NormalizeSingleSpriteGeneric<ResourceData>("miniatura", idMaterial, placement);
        NormalizeAnimationDataGeneric<ResourceData>(idMaterial, placement);

        NormalizeSingleSpriteGeneric<BuildingData>("spriteData", idMaterial, placement);
        NormalizeSingleSpriteGeneric<BuildingData>("miniatura", idMaterial, placement);
        NormalizeAnimationDataGeneric<BuildingData>(idMaterial, placement);


        NormalizeSingleSpriteGeneric<AccessoryData>("miniatura", idMaterial, placement);
        NormalizeSpriteAnimation(idMaterial, placement);
        NormalizeAnimationCharacter(idMaterial, placement);
    }

    private void NormalizeSingleSpriteGeneric<T>(string fieldName, MaterialData idMaterial, InsertResult placement) where T : class
    {
        var expression = BsonExpression.Create($"{fieldName}.idMaterial = @0", idMaterial.id);
        var listData = DataBaseManager.Instance.FindAllFilter<T>(expression);

        foreach (var item in listData)
        {
            dynamic sprite = fieldName == "spriteData" ? ((dynamic)item).spriteData : ((dynamic)item).miniatura;

            var newRect = CalculatePixelUVInAtlas(new Rect2(sprite.x, sprite.y, sprite.widht, sprite.height), placement);

            sprite.xFormat = newRect.Position.X;
            sprite.yFormat = newRect.Position.Y;
            sprite.widhtFormat = sprite.mirrorX ? -newRect.Size.X : newRect.Size.X;
            sprite.heightFormat = sprite.mirrorY ? -newRect.Size.Y : newRect.Size.Y;

            DataBaseManager.Instance.InsertUpdate(item);
        }
    }

    private void NormalizeAnimationDataGeneric<T>(MaterialData idMaterial, InsertResult placement) where T : class
    {
        var expression = BsonExpression.Create("ANY(animationData.idMaterial) = @0", idMaterial.id);
        var listData = DataBaseManager.Instance.FindAllFilter<T>(expression);

        foreach (var item in listData)
        {
            dynamic dynamicItem = item;
            bool updated = false;

            foreach (var animationSet in dynamicItem.animationData)
            {
                if (animationSet.idMaterial != idMaterial.id)
                    continue;

                foreach (var anim in animationSet.animationData)
                {
                    foreach (var frame in anim.frameDataArray)
                    {
                        var newRect = CalculatePixelUVInAtlas(
                            new Rect2(frame.x, frame.y, frame.widht, frame.height),
                            placement
                        );

                        float newWidth = frame.widhtFormat < 0 ? -newRect.Size.X : newRect.Size.X;
                        float newHeight = frame.heightFormat < 0 ? -newRect.Size.Y : newRect.Size.Y;

                        frame.xFormat = newRect.Position.X;
                        frame.yFormat = newRect.Position.Y;
                        frame.widhtFormat = newWidth;
                        frame.heightFormat = newHeight;

                        updated = true;
                    }
                }
            }

            if (updated)
            {
                DataBaseManager.Instance.InsertUpdate(dynamicItem);
            }
        }
    }

    private void NormalizeAnimationCharacter(MaterialData idMaterial, InsertResult placement) 
    {
        var expression = BsonExpression.Create("ANY(animationDataArray.idMaterial) = @0", idMaterial.id);
        var listData = DataBaseManager.Instance.FindAllFilter<AnimationCharacterBaseData>(expression);

        foreach (var item in listData)
        {
           
            bool updated = false;

            foreach (var animationSet in item.animationDataArray)
            {
                if (animationSet.idMaterial != idMaterial.id)
                    continue;

                foreach (var anim in animationSet.animationData)
                {
                    foreach (var frame in anim.frameDataArray)
                    {
                        var newRect = CalculatePixelUVInAtlas(
                            new Rect2(frame.x, frame.y, frame.widht, frame.height),
                            placement
                        );

                        float newWidth = frame.widhtFormat < 0 ? -newRect.Size.X : newRect.Size.X;
                        float newHeight = frame.heightFormat < 0 ? -newRect.Size.Y : newRect.Size.Y;

                        frame.xFormat = newRect.Position.X;
                        frame.yFormat = newRect.Position.Y;
                        frame.widhtFormat = newWidth;
                        frame.heightFormat = newHeight;

                        updated = true;
                    }
                }
            }

            if (updated)
            {
                DataBaseManager.Instance.InsertUpdate(item);
            }
        }

         expression = BsonExpression.Create("ANY(animationExtraDataArray.idMaterial) = @0", idMaterial.id);
         listData = DataBaseManager.Instance.FindAllFilter<AnimationCharacterBaseData>(expression);

        foreach (var item in listData)
        {

            bool updated = false;

            foreach (var animationSet in item.animationExtraDataArray)
            {
                if (animationSet.idMaterial != idMaterial.id)
                    continue;

                foreach (var anim in animationSet.animationData)
                {
                    foreach (var frame in anim.frameDataArray)
                    {
                        var newRect = CalculatePixelUVInAtlas(
                            new Rect2(frame.x, frame.y, frame.widht, frame.height),
                            placement
                        );

                        float newWidth = frame.widhtFormat < 0 ? -newRect.Size.X : newRect.Size.X;
                        float newHeight = frame.heightFormat < 0 ? -newRect.Size.Y : newRect.Size.Y;

                        frame.xFormat = newRect.Position.X;
                        frame.yFormat = newRect.Position.Y;
                        frame.widhtFormat = newWidth;
                        frame.heightFormat = newHeight;

                        updated = true;
                    }
                }
            }

            if (updated)
            {
                DataBaseManager.Instance.InsertUpdate(item);
            }
        }
    }

    private void NormalizeSpriteAnimation(MaterialData idMaterial, InsertResult placement) 
    {
        var expression = BsonExpression.Create("animationTilesData.idMaterial = @0", idMaterial.id);
        var listData = DataBaseManager.Instance.FindAllFilter<AccessoryData>(expression);

        foreach (var item in listData)
        {
            
            bool updated = false;


            if (item.animationTilesData.idMaterial != idMaterial.id)
                continue;

            foreach (var frame in item.animationTilesData.framesArray)
            {
                var newRect = CalculatePixelUVInAtlas(
                    new Rect2(frame.x, frame.y, frame.widht, frame.height),
                    placement
                );

                float newWidth = frame.widhtFormat < 0 ? -newRect.Size.X : newRect.Size.X;
                float newHeight = frame.heightFormat < 0 ? -newRect.Size.Y : newRect.Size.Y;

                frame.xFormat = newRect.Position.X;
                frame.yFormat = newRect.Position.Y;
                frame.widhtFormat = newWidth;
                frame.heightFormat = newHeight;

                updated = true;
            }


            if (updated)
            {
                DataBaseManager.Instance.InsertUpdate(item);
            }
        }
    }
    private void NormalizeTiles(MaterialData idMaterial, InsertResult placement)
    {        
        BsonExpression bsonExpression = BsonExpression.Create("idMaterial = @0", idMaterial.id);
        List<TileDynamicData> listData = DataBaseManager.Instance.FindAllFilter<TileDynamicData>(bsonExpression);
        foreach (var item in listData)
        {
            var newRect = CalculatePixelUVInAtlas(new Rect2(item.x,item.y, item.widht,item.height), placement);

            item.xFormat = newRect.Position.X;
            item.yFormat= newRect.Position.Y;
            if (item.mirrorX)
            {
                item.widhtFormat = newRect.Size.X * (-1);
            }
            else
            {
                item.widhtFormat = newRect.Size.X;
            }
            if (item.mirrorY)
            {
                item.heightFormat = newRect.Size.Y * (-1);
            }
            else
            {
                item.heightFormat = newRect.Size.Y;
            }
                
            DataBaseManager.Instance.InsertUpdate(item);
        }
    }
    private void NormalizeTilesAnimated(MaterialData idMaterial, InsertResult placement)
    {
        BsonExpression bsonExpression = BsonExpression.Create("idMaterial = @0", idMaterial.id);
        List<TileAnimateData> listData = DataBaseManager.Instance.FindAllFilter<TileAnimateData>(bsonExpression);
        foreach (var item in listData)
        {
            if (item.framesArray!=null)
            {
                foreach (var itemFrame in item.framesArray)
                {

                    var newRect = CalculatePixelUVInAtlas(new Rect2(itemFrame.x, itemFrame.y, itemFrame.widht, itemFrame.height), placement);

                    itemFrame.xFormat = newRect.Position.X;
                    itemFrame.yFormat = newRect.Position.Y;
                    if (item.mirrorX)
                    {
                        itemFrame.widhtFormat = newRect.Size.X * (-1);
                    }
                    else
                    {
                        itemFrame.widhtFormat = newRect.Size.X;
                    }
                    if (item.mirrorY)
                    {
                        itemFrame.heightFormat = newRect.Size.Y * (-1);
                    }
                    else
                    {
                        itemFrame.heightFormat = newRect.Size.Y;
                    }
                }
                DataBaseManager.Instance.InsertUpdate(item);
            }                               
        }
    }
    public Rect2 CalculatePixelUVInAtlas(Rect2 originalUV, InsertResult placement)
    {
        return new Rect2(
            placement.X + originalUV.Position.X,
            placement.Y + originalUV.Position.Y,
            originalUV.Size.X,
            originalUV.Size.Y
        );
    }
    private void OnFileSelected(string path)
    {
        LineEditPath.Text = path;
        TextureSplit_Clik();
        ChangeTexture = true;
    }

    private void TextureSplit_Clik()
    {
        ItemListTiles.Clear();
        List<Texture> list;
        if (mode == 0)
        {
             list = TextureHelper.SplitTexture(LineEditPath.Text, new Vector2I((int)SpinBoxX.Value, (int)SpinBoxY.Value));
        }
        else
        {
             list = TextureHelper.SplitTexture(FileHelper.GetPathGameDB(LineEditPath.Text), new Vector2I((int)SpinBoxX.Value, (int)SpinBoxY.Value));
        }
        
        for (int i = 0; i < list.Count; i++)
        {
            Texture item = list[i];
            if (!TextureHelper.IsTextureEmpty(item))
            {
                ItemListTiles.AddItem("ID:" + i, (Texture2D)item);
            }
        }
    }
}
