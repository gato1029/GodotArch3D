using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Godot.HttpRequest;


    internal class FileHelper
    {
    public static void DeleteFile(string filePath)
    {
        // Soporta rutas absolutas y rutas internas del proyecto (res:// o user://)
        string resolvedPath = ProjectSettings.GlobalizePath(filePath);

        if (File.Exists(resolvedPath))
        {
            File.Delete(resolvedPath);
           
        }
        else
        {
            GD.PrintErr($"El archivo no existe: {resolvedPath}");
        }
    }
    public static List<string> GetAllFiles(string folderPath)
        {
            string path = ProjectSettings.GlobalizePath(folderPath);
            List<string> fileList = new List<string>();


            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    fileList.Add(file);
                }
            }
            else
            {
                GD.PrintErr("No se pudo abrir la carpeta: " + folderPath);
            }

            return fileList;
        }

        public static string CopyFileToAssetExternals(string sourcePath,string carpet, string idName)
        {
        string gameDirectory = OS.GetExecutablePath().GetBaseDir();
#if DEBUG
        gameDirectory = "D:\\GitKraken";
#endif
        // Obtener la ruta del directorio actual del juego (directorio donde se encuentra el ejecutable)
        string finalCarpet = "AssetExternals" + "/" + carpet;
        string finalCarpetCompressed = "AssetExternals" + "/" + carpet+"/DDS";
        // Crear la ruta completa para la carpeta AssetExternals
        string destinationFolder = Path.Combine(gameDirectory, finalCarpet );
        string destinationFolderCompressed = Path.Combine(gameDirectory, finalCarpetCompressed);
        // Asegurarse de que la carpeta de destino exista, si no, crearla
        if (!Directory.Exists(finalCarpetCompressed))
        {
            Directory.CreateDirectory(finalCarpetCompressed);
        }
        if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            // Crear la ruta de destino completa (copiar el archivo con su nombre original)
            string fileName = idName +"." +sourcePath.GetExtension(); // Obtener solo el nombre del archivo
            string destinationPath = Path.Combine(destinationFolder, fileName); // Ruta completa de destino

            string destinationPathCompressed = Path.Combine(destinationFolder, idName + ".dds");
        // Usar la clase File para copiar el archivo

        try
        {
            if (!sourcePath.StartsWith("AssetExternals"))
            {
                File.Copy(sourcePath, destinationPath, true);

                // Crear una instancia de Image para cargar el archivo
                Image image = new Image();
                Error result = image.Load(destinationPath);
               
                image.Compress(Image.CompressMode.S3Tc);            
                image.SaveDds(destinationPathCompressed);
            }
            return finalCarpet+"/"+fileName;
        }
        catch (Exception ex)
        {
            GD.PrintErr("Error al copiar el archivo: " + ex.Message);
            throw ;
        }
                        
        }

    public static string GetPathGameDB(string sourcePathDB)
    {
        string gameDirectory = OS.GetExecutablePath().GetBaseDir();
        #if DEBUG
                gameDirectory = "D:\\GitKraken";
        #endif
        string destinationFolder = Path.Combine(gameDirectory, sourcePathDB);


        return destinationFolder;
    }
}

