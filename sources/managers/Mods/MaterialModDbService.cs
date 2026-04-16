using Godot;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GodotEcsArch.sources.managers.Mods;




public class MaterialModDbService:SingletonBase<MaterialModDbService>
{
    private LiteDatabase _db;
    private ILiteCollection<MaterialModData> _collection;

   
    protected override void Initialize()
    {
        string dbPath = FileHelper.GetPathGameDB("ModsGame/db/OrchestMaterial.db");
        try
        {
            _db = new LiteDatabase(dbPath);
            _collection = _db.GetCollection<MaterialModData>("materials");
         
            _collection.EnsureIndex(x => x.idNameMod, true);
            _collection.EnsureIndex(x => x.idTextureAtlas);
        }
        catch (Exception e)
        {
            string errorMessage = $"❌ Error initializing MaterialModDbService at {dbPath}: {e.Message}";
            throw;
        }
    }

    // 🔹 Métodos CRUD
    public void Guardar(MaterialModData data)
    {        
        _collection.Upsert(data);
    }

    public MaterialModData Obtener(string idNameMod)
    {
        return _collection.FindById(idNameMod);
    }

    public List<MaterialModData> ObtenerTodos()
    {
        return _collection.FindAll().ToList();
    }

    public List<MaterialModData> ObtenerPorAtlas(int idTextureAtlas)
    {
        return _collection.Find(x => x.idTextureAtlas == idTextureAtlas).ToList();
    }

    public bool Eliminar(string idNameMod)
    {
        return _collection.Delete(idNameMod);
    }

    public bool Existe(string idNameMod)
    {
        return _collection.Exists(x => x.idNameMod == idNameMod);
    }

    //public void Dispose()
    //{
    //    _db?.Dispose();
    //}
}