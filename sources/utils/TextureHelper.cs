using Godot;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.utils
{
    internal class TextureHelper
    {
        public static Image LoadImageLocal(string filePath)
        {
            // Crear una instancia de Image para cargar el archivo
            Image image = new Image();
            Error result = image.Load(filePath);

            if (result == Error.Ok)
            {
               
                return image;
            }

            GD.PrintErr($"Error al cargar la imagen desde: {filePath}. Error: {result}");
            return null;
        }
        public static Texture LoadTextureLocal(string filePath)
        {
            string rutaCarpeta = Path.GetDirectoryName(filePath) +"/DDS/";
            string nombreSinExtension = Path.GetFileNameWithoutExtension(filePath);
            string nuevaRuta = rutaCarpeta+ nombreSinExtension+".dds";

            // Crear una instancia de Image para cargar el archivo

            //   if (!File.Exists(nuevaRuta))
            //   {
            //       if (!Directory.Exists(rutaCarpeta))
            //       {
            //           Directory.CreateDirectory(rutaCarpeta);
            //       }
            //       Image image = new Image();
            //       image.Load(filePath);
            //       image.GenerateMipmaps();
            //       Error result1 = image.Compress(Image.CompressMode.S3Tc, Image.CompressSource.Generic);
            //       if (result1 == Error.Ok)
            //       {
            //           image.SaveDds(nuevaRuta);
            //       }
            //       else
            //       {
            //           GD.PrintErr($"Error al convertir: {filePath}. Error: {result1}");
            //       }

            //   }
            //   var bytes = File.ReadAllBytes(nuevaRuta);
            //   Image imageCompressed = new Image();
            ////  Error result = imageCompressed.LoadDdsFromBuffer(bytes);
            Image imageCompressed = new Image();
            Error result = imageCompressed.Load(filePath);
            result = imageCompressed.GenerateMipmaps();

            if (result == Error.Ok)
            {
                // Convertir la imagen cargada en una textura
                ImageTexture texture = ImageTexture.CreateFromImage(imageCompressed);
                    
                return texture;
            }

            GD.PrintErr($"Error al cargar la imagen desde: {filePath}. Error: {result}");
            return null;
        }
        public static ImageTexture LoadImageTextureLocal(string filePath)
        {
            // Crear una instancia de Image para cargar el archivo
            Image image = new Image();
            Error result = image.Load(filePath);

            if (result == Error.Ok)
            {
                // Convertir la imagen cargada en una textura
                ImageTexture texture = ImageTexture.CreateFromImage(image);
                
                return texture;
            }

            GD.PrintErr($"Error al cargar la imagen desde: {filePath}. Error: {result}");
            return null;
        }
        public static Texture GetSubTextureFromImage(Image image, Vector2I cellSize, int index)
        {
            // Verificar que la imagen esté cargada correctamente
            if (image == null || image.GetWidth() == 0 || image.GetHeight() == 0)
            {
                GD.PrintErr("Imagen inválida.");
                return null;
            }

            // Obtener el tamaño total de la imagen
            Vector2 imageSize = new Vector2(image.GetWidth(), image.GetHeight());

            // Calcular la posición de la celda (basado en el índice)
            int columns = (int)(imageSize.X / cellSize.X); // Número de columnas
            int row = index / columns; // Fila correspondiente al índice
            int column = index % columns; // Columna correspondiente al índice

            // Calcular las coordenadas de la subimagen a partir del índice
            int x = column * cellSize.X;
            int y = row * cellSize.Y;

            // Verificar si la subimagen está dentro de los límites de la imagen
            if (x + cellSize.X > imageSize.X || y + cellSize.Y > imageSize.Y)
            {
                GD.PrintErr("Índice fuera de los límites de la imagen.");
                return null;
            }

            // Extraer la región de la imagen correspondiente a la celda
            Rect2I rect = new Rect2I(new Vector2I(x, y), cellSize);
            Image subImage = image.GetRegion(rect);

            // Convertir la subimagen en una textura
            ImageTexture texture = ImageTexture.CreateFromImage(subImage);

            return texture;
        }
        public static List<Texture> SplitTexture(string texturePath, Vector2I cellSize)
        {
            List<Texture> textures = new List<Texture>();

            // Cargar la textura como Image
            Image image = new Image();
            Error result = image.Load(texturePath);

            if (result != Error.Ok)
            {
                GD.PrintErr($"Error al cargar la textura: {texturePath}. Error: {result}");
                return textures;
            }

            // Obtener el tamaño total de la imagen
            Vector2 imageSize = new Vector2(image.GetWidth(), image.GetHeight());

            // Iterar sobre las celdas de la imagen
            for (int y = 0; y < imageSize.Y; y += cellSize.Y)
            {
                for (int x = 0; x < imageSize.X; x += cellSize.X)
                {
                    // Extraer la región de la imagen
                    Rect2I rect = new Rect2I(new Vector2I(x, y), cellSize);
                    Image subImage = image.GetRegion(rect);

                    // Convertir la subimagen en una textura
                    ImageTexture texture = ImageTexture.CreateFromImage(subImage);


                    // Añadir a la lista de texturas
                    textures.Add(texture);
                }
            }

            return textures;
        }

        public static bool IsTextureEmpty(Texture texture)
        {
            ImageTexture imageTexture = texture as ImageTexture;
            // Convertir la textura a una instancia de Image
            Image image = imageTexture.GetImage();

            // Recorrer los píxeles y verificar si alguno no es completamente transparente
            for (int y = 0; y < image.GetHeight(); y++)
            {
                for (int x = 0; x < image.GetWidth(); x++)
                {
                    // Obtener el color del píxel en (x, y)
                    Color color = image.GetPixel(x, y);

                    // Si el píxel no es completamente transparente
                    if (color.A > 0.0f) // Cambia la condición según tu criterio
                    {
                        return false; // La textura no está vacía
                    }
                }
            }


            return true; // La textura está vacía
        }

        public static Rect2 CalculatePixelUVInAtlas(Rect2 originalUV, int idMaterial)
        {
            var mat = MaterialManager.Instance.GetMaterial(idMaterial);
            Vector2 placement = new Vector2(mat.originXTextureMaster, mat.originYTextureMaster);
            return new Rect2(
                placement.X + originalUV.Position.X,
                placement.Y + originalUV.Position.Y,
                originalUV.Size.X,
                originalUV.Size.Y
            );
        }
        public static void RecalulateUVFormat(TileDynamicData tile)
        {
            var material = MaterialManager.Instance.GetMaterial(tile.idMaterial);
            var offset = new Vector2(material.originXTextureMaster, material.originYTextureMaster);

            // Calcular nueva posición y tamaño
            float newX = offset.X + tile.x;
            float newY = offset.Y + tile.y;
            float newWidth = tile.widht;
            float newHeight = tile.height;

            // Aplicar posición formateada
            tile.xFormat = newX;
            tile.yFormat = newY;

            // Aplicar formato con espejo si corresponde
            tile.widhtFormat = tile.mirrorX ? -newWidth : newWidth;
            tile.heightFormat = tile.mirrorY ? -newHeight : newHeight;
        }

        public static void RecalulateUVFormat(SpriteData spriteData)
        {
            var material = MaterialManager.Instance.GetMaterial(spriteData.idMaterial);
            var offset = new Vector2(material.originXTextureMaster, material.originYTextureMaster);

            // Calcular nueva posición y tamaño
            float newX = offset.X + spriteData.x;
            float newY = offset.Y + spriteData.y;
            float newWidth = spriteData.widht;
            float newHeight = spriteData.height;

            // Aplicar posición formateada
            spriteData.xFormat = newX;
            spriteData.yFormat = newY;

            // Aplicar formato con espejo si corresponde
            spriteData.widhtFormat = spriteData.mirrorX ? -newWidth : newWidth;
            spriteData.heightFormat = spriteData.mirrorY ? -newHeight : newHeight;
        }

        public static bool AreAtlasTexturesFullyEqual(AtlasTexture texA, AtlasTexture texB)
        {
            if (texA == null || texB == null)
                return false;

            // 1. Comparar atlas y región
            if (texA.Atlas != texB.Atlas || texA.Region != texB.Region)
                return false;

            // 2. Obtener imágenes de ambas regiones
            Image imageA = GetAtlasRegionImage(texA);
            Image imageB = GetAtlasRegionImage(texB);

            if (imageA == null || imageB == null)
                return false;

            // 3. Comparar tamaños
            if (imageA.GetSize() != imageB.GetSize())
                return false;

            // 4. Comparar píxeles
            
            for (int y = 0; y < imageA.GetHeight(); y++)
            {
                for (int x = 0; x < imageA.GetWidth(); x++)
                {
                    if (imageA.GetPixel(x, y) != imageB.GetPixel(x, y))
                    {
            
                        return false;
                    }
                }
            }            
            return true;
        }
        private static Image GetAtlasRegionImage(AtlasTexture atlasTexture)
        {
            if (atlasTexture?.Atlas is not Texture2D atlas)
                return null;

            Image fullImage = atlas.GetImage();
            if (fullImage == null)
                return null;

            Rect2I region = (Rect2I)atlasTexture.Region;
            // Asegúrate de no salirte del atlas
            if (region.Position.X + region.Size.X > fullImage.GetWidth() ||
                region.Position.Y + region.Size.Y > fullImage.GetHeight())
                return null;

            return fullImage.GetRegion(region);
        }
    }
}
