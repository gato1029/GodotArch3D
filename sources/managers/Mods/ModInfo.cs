namespace GodotEcsArch.sources.managers.Mods;

public struct ModInfo
{
    public string Name;        // mod_fire
    public string DbPath;      // .../mod_fire/data.db
    public string FolderPath;  // .../mod_fire/

    public ModInfo(string name, string dbPath, string folderPath)
    {
        Name = name;
        DbPath = dbPath;
        FolderPath = folderPath;
    }
}
