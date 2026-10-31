namespace GodotEcsArch.sources.WindowsDataBase.ResourceMaterial;

// estos son los materiales que existiran en el mundo, madera, piedra etc

public class ResourceMaterialData:IdDataLong
{
    public string description { get; set; }
    public long idTileSpriteData { get; set; }
}