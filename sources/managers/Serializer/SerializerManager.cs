using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace GodotEcsArch.sources.managers.serializer;
internal class SerializerManager
{
    public static void SaveToFileJson<T>(T data,string carpet , string file)
    {
        try
        {
            file = file + ".json";
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            if (!Directory.Exists(carpet))
            {
                Directory.CreateDirectory(carpet);
            }
            string fullPath = carpet + "/" + file;
            File.WriteAllText(fullPath, json);
        }
        catch (IOException ex)
        {
            // Manejo básico de error, puedes expandirlo según tu contexto
            Console.WriteLine($"Error al guardar el archivo: {ex.Message}");
        }
    }

    public static T LoadFromFileJson<T>(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Archivo no encontrado: {filePath}");
            }

            string json = File.ReadAllText(filePath);
            T data = JsonConvert.DeserializeObject<T>(json);
            return data;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error al leer el archivo: {ex.Message}");
            return default;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error al deserializar JSON: {ex.Message}");
            return default;
        }
    }

    public static void SaveToFileBin<T>(T data, string carpet, string filePath)
    {
        try
        {
            filePath = filePath + ".bin";
            if (!Directory.Exists(carpet))
            {
                Directory.CreateDirectory(carpet);
            }
            string fullPath = carpet + "/" + filePath;

            using (var file = File.Create(fullPath))
            {

                ProtoBuf.Serializer.Serialize(file, data);                
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error al guardar archivo binario: {ex.Message}");
        }
    }
    public static T LoadFromFileBin<T>(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Archivo no encontrado: {filePath}");
            }

            using (var file = File.OpenRead(filePath))
            {
                return ProtoBuf.Serializer.Deserialize<T>(file);
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error al leer archivo binario: {ex.Message}");
            return default;
        }
    }
}
