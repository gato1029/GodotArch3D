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
        // Crear la ruta completa para la carpeta AssetExternals
        string destinationFolder = Path.Combine(gameDirectory, finalCarpet );

            // Asegurarse de que la carpeta de destino exista, si no, crearla
            
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            // Crear la ruta de destino completa (copiar el archivo con su nombre original)
            string fileName = idName +"." +sourcePath.GetExtension(); // Obtener solo el nombre del archivo
            string destinationPath = Path.Combine(destinationFolder, fileName); // Ruta completa de destino

        // Usar la clase File para copiar el archivo

        try
        {
            File.Copy(sourcePath, destinationPath,true);
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

