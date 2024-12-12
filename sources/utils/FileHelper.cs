using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
    }

