using Arch.Core;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;

public partial class MaterialManager: SingletonBase<MaterialManager>
{
    public Dictionary<string, Material> materials;
    protected override void Initialize()
    {
        materials = new Dictionary<string, Material>();

        var collection = FileHelper.GetAllFiles("res://resources/Material/");

        foreach (var item in collection)
        {
            var pathGodot = ProjectSettings.LocalizePath(item);
            Material resource = (Material)ResourceLoader.Load(pathGodot);
            if (resource != null)
            {
                string fileName = Path.GetFileNameWithoutExtension(pathGodot);
                materials.Add(fileName, resource);
            }
        }
    }

    public void RegisterMaterial(string name, Material material)
    {
        materials.Add(name, material);
    }

    public Material GetMaterial(string name)
    {
        return materials[name];
    }
    public void RegisterAlterMaterial(string nameOriginalMaterial, string name, Material material)
    {
        if (materials.ContainsKey(nameOriginalMaterial))
        {
            Material mat = materials[nameOriginalMaterial];
            mat.Set("Color", new Color(""));
            materials.Add(name, mat);
        }
    }

}
