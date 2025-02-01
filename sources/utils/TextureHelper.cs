using Godot;
using System;
using System.Collections.Generic;
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
    }
}
