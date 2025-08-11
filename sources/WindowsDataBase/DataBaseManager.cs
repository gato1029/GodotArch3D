
using Godot;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.CharacterCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.Resources.DataBase;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Weapons;
using LiteDB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TileData = GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileData;

namespace GodotEcsArch.sources.WindowsDataBase
{
    
    public class DataBaseFree
    {
        [BsonId]
        public string id { get; set; }
        public List<int> freeIds { get; set; }
    }
    internal class DataBaseManager: SingletonBase<DataBaseManager>
    {
        LiteDatabase db;
        string dataBasePath = FileHelper.GetPathGameDB("AssetExternals/db/MiBaseDeDatos2.db");
        private Dictionary<Type, string> collectionNameMap = new Dictionary<Type, string>();

        protected override void Initialize()
        {
            db = new LiteDatabase(dataBasePath);
            var mapper = BsonMapper.Global;

            collectionNameMap[typeof(MaterialSimpleData)] = "Materiales";

            collectionNameMap[typeof(MaterialData)] = "Materiales";
            collectionNameMap[typeof(TileData)] = "Tiles";
            collectionNameMap[typeof(AutoTileData)] = "AutoTiles";
            collectionNameMap[typeof(AnimationCharacterBaseData)] = "AnimationCharacterBaseData";
            collectionNameMap[typeof(TerrainData)] = "Terrain";
            collectionNameMap[typeof(AnimationBaseData)] = "AnimationBase";
            collectionNameMap[typeof(AccessoryData)] = "AccessoryData";
            collectionNameMap[typeof(CharacterModelBaseData)] = "CharacterModelBaseData";
            collectionNameMap[typeof(AccesoryAnimationBodyData)] = "AccesoryAnimationBodyData";
            collectionNameMap[typeof(ResourceData)] = "ResourceData";
            collectionNameMap[typeof(BuildingData)] = "BuildingData";
            collectionNameMap[typeof(DataBaseFree)] = "DataBaseFree";
            collectionNameMap[typeof(TextureMasterData)] = "TextureMasterData";
            collectionNameMap[typeof(ResourceSourceData)] = "ResourceSourceData";

            // RegisterCollection<BuildingData>("DataBaseFree");

            ILiteCollection<TextureMasterData> TextureMasterDataCollection = db.GetCollection<TextureMasterData>("TextureMasterData");
            TextureMasterDataCollection.EnsureIndex(x => x.id, unique: true);

            ILiteCollection<DataBaseFree> DataBaseFreeCollection = db.GetCollection<DataBaseFree>("DataBaseFree");
            DataBaseFreeCollection.EnsureIndex(x => x.id, unique: true);

            ILiteCollection<MaterialData> MaterialDataCollection = db.GetCollection<MaterialData>("Materiales");            
            MaterialDataCollection.EnsureIndex(x => x.id, unique: true);

            ILiteCollection<ResourceSourceData> ResourceSourceDataCollection = db.GetCollection<ResourceSourceData>("ResourceSourceData");
            ResourceSourceDataCollection.EnsureIndex(x => x.id, unique: true);

            ILiteCollection<TileData> TileDataCollection = db.GetCollection<TileData>("Tiles");
            TileDataCollection.EnsureIndex(x => x.id, unique: true);

            ILiteCollection<AutoTileData> AutoTileDataCollection = db.GetCollection<AutoTileData>("AutoTiles");
            AutoTileDataCollection.EnsureIndex(x => x.id, unique: true);

            ILiteCollection<AnimationCharacterBaseData> AnimationCharacterBaseDataCollection = db.GetCollection<AnimationCharacterBaseData>("AnimationCharacterBaseData");
            AnimationCharacterBaseDataCollection.EnsureIndex(x => x.id, unique: true);

            ILiteCollection<TerrainData> TerrainDataCollection = db.GetCollection<TerrainData>("Terrain");
            TerrainDataCollection.EnsureIndex(x => x.id, unique: true);

            ILiteCollection<AnimationBaseData> WeaponBaseDataCollection = db.GetCollection<AnimationBaseData>("AnimationBase");
            WeaponBaseDataCollection.EnsureIndex(x => x.id, unique: true);

            ILiteCollection<AccessoryData> AccessoryDataCollection = db.GetCollection<AccessoryData>("AccessoryData");
            AccessoryDataCollection.EnsureIndex(x => x.id, unique: true);

            ILiteCollection<CharacterModelBaseData> CharacterModelBaseDataCollection = db.GetCollection<CharacterModelBaseData>("CharacterModelBaseData");
            CharacterModelBaseDataCollection.EnsureIndex(x => x.id, unique: true);

            ILiteCollection<AccesoryAnimationBodyData> AccesoryAnimationBodyDataCollection = db.GetCollection<AccesoryAnimationBodyData>("AccesoryAnimationBodyData");
            AccesoryAnimationBodyDataCollection.EnsureIndex(x => x.id, unique: true);

            ILiteCollection<ResourceData> ResourceDataCollection = db.GetCollection<ResourceData>("ResourceData");
            ResourceDataCollection.EnsureIndex(x => x.id, unique: true);

            ILiteCollection<BuildingData> BuildingDataCollection = db.GetCollection<BuildingData>("BuildingData");
            BuildingDataCollection.EnsureIndex(x => x.id, unique: true);
        }

        public void RegisterCollection<T>(string collectionName) where T : class
        {
            if (!collectionNameMap.ContainsKey(typeof(T)))
            {
                collectionNameMap.Add(typeof(T), collectionName);
                ILiteCollection<T> BuildingDataCollection = db.GetCollection<T>(collectionName);
                BuildingDataCollection.EnsureIndex("_id", unique: true);
            }
            else
            {
                collectionNameMap[typeof(T)] = collectionName;
            }
        }
        public int NextID<T>()
        {
            var baseType = typeof(T).BaseType;
            string collectionName;
            if (baseType != null && baseType != typeof(object) && baseType !=typeof(IdData))
            {
                if (!collectionNameMap.TryGetValue(baseType, out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección Padre para el tipo {baseType.Name}");
                }
            }
            else
            {
                if (!collectionNameMap.TryGetValue(typeof(T), out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección para el tipo {typeof(T).Name}");
                }
            }

            var collection = db.GetCollection<BsonDocument>(collectionName);

            int id = 0;
            var FreeData = FindById<DataBaseFree>(collectionName);
            if (FreeData != null)
            {
                if (FreeData.freeIds.Count > 0)
                {
                    var freeid = FreeData.freeIds.Min();             
                    id = freeid;
                    return id;
                }

            }
            
            // Obtener el último ID de la colección
            var lastItem = collection.FindAll().OrderByDescending(item => item["_id"]).FirstOrDefault();
            if (lastItem == default)
            {
                return 1;
            }
            else
            {
                int aa = lastItem["_id"].AsInt32;
                return aa + 1;
            }
            

        }
        /// <summary>
        /// Método genérico para insertar datos en una colección específica
        /// </summary>
        /// <typeparam name="T">Tipo del dato que se insertará</typeparam>
        /// <param name="data">Instancia del objeto a insertar</param>
        public void Insert<T>(T data)
        {
            // Obtener el tipo base de T
            var baseType = typeof(T).BaseType;
            string collectionName;
            if (baseType != null && baseType != typeof(object) && baseType != typeof(IdData))
            {
                if (!collectionNameMap.TryGetValue(baseType, out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección Padre para el tipo {baseType.Name}");
                }                               
            }
            else
            {
                if (!collectionNameMap.TryGetValue(typeof(T), out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección para el tipo {typeof(T).Name}");
                }
            }
            
            var collection = db.GetCollection<T>(collectionName);           
            collection.Insert(data);
        }

        public bool InsertUpdateGeneric<T>(T data, int id = -1) 
        {
            var baseType = typeof(T).BaseType;
            string collectionName;
            if (baseType != null && baseType != typeof(object) && baseType != typeof(IdData))
            {
                if (!collectionNameMap.TryGetValue(baseType, out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección Padre para el tipo {baseType.Name}");
                }
            }
            else
            {
                if (!collectionNameMap.TryGetValue(typeof(T), out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección para el tipo {typeof(T).Name}");
                }
            }
         


            var collection = db.GetCollection<T>(collectionName);
            bool resultado = false;
            if (id == -1)
            {
                resultado = collection.Upsert(data);
            }
            else
            {
                resultado = collection.Upsert(id, data);
            }
            return resultado;
        }

        public bool InsertUpdate<T>(T data, int id = -1) 
        {
            var baseType = typeof(T).BaseType;
            string collectionName;
            if (baseType != null && baseType != typeof(object) && baseType != typeof(IdData))
            {
                if (!collectionNameMap.TryGetValue(baseType, out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección Padre para el tipo {baseType.Name}");
                }
            }
            else
            {
                if (!collectionNameMap.TryGetValue(typeof(T), out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección para el tipo {typeof(T).Name}");
                }
            }
         
            var collection = db.GetCollection<T>(collectionName);
            bool resultado = false;
            if (id == -1)
            {
                resultado = collection.Upsert( data);
            }
            else
            {
                resultado = collection.Upsert(id,data);
            }
            return resultado;
        }

        public bool InsertUpdateLog<T>(T data, int id = -1)
        {
            var baseType = typeof(T).BaseType;
            string collectionName;
            if (baseType != null && baseType != typeof(object) && baseType != typeof(IdData))
            {
                if (!collectionNameMap.TryGetValue(baseType, out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección Padre para el tipo {baseType.Name}");
                }
            }
            else
            {
                if (!collectionNameMap.TryGetValue(typeof(T), out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección para el tipo {typeof(T).Name}");
                }
            }
            if (id == -1)
            {
                var FreeData = FindById<DataBaseFree>(collectionName);
                if (FreeData != null)
                {
                    if (FreeData.freeIds.Count > 0)
                    {
                        var freeid = FreeData.freeIds.Min();
                        FreeData.freeIds.Remove(freeid);
                        id = freeid;
                        InsertUpdateGeneric(FreeData);
                    }

                }
            }

            var collection = db.GetCollection<T>(collectionName);
            bool resultado = false;
            if (id == -1)
            {
                resultado = collection.Upsert(data);
            }
            else
            {
                resultado = collection.Upsert(id, data);
            }
            return resultado;
        }

        protected override void Destroy()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Método genérico para encontrar un documento por su ID
        /// </summary>
        /// <typeparam name="T">Tipo del dato que se buscará</typeparam>
        /// <param name="id">El ID del documento que se quiere encontrar</param>
        /// <returns>El documento si se encuentra, o null si no</returns>
        public T FindById<T>(int id) where T : class
        {
            var currentType = typeof(T);
            var baseType = typeof(T).BaseType;
            string collectionName;
            if (baseType != null && baseType != typeof(object) && baseType != typeof(IdData))
            {
                if (!collectionNameMap.TryGetValue(baseType, out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección Padre para el tipo {baseType.Name}");
                }
                else
                {
                    // Obtén la colección correspondiente
                    var collectionBson = db.GetCollection<BsonDocument>(collectionName);
                    var filteredDocuments = collectionBson.Query() .Where(x => x["_id"] == id && x["type"] == currentType.Name).ToList();

                    var result = filteredDocuments.Select(BsonMapper.Global.ToObject<T>).FirstOrDefault();
                    // Busca el documento por ID
                    return result; // Devuelve el documento o null si no se encuentra
                }
            }
            else
            {
                if (!collectionNameMap.TryGetValue(typeof(T), out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección para el tipo {typeof(T).Name}");
                }
            }

            var collection = db.GetCollection<T>(collectionName);

            // Busca el documento por ID
            return collection.FindById(id); // Devuelve el documento o null si no se encuentra

        }
        public T FindById<T>(string id) where T : class
        {
            var currentType = typeof(T);
            var baseType = typeof(T).BaseType;
            string collectionName;
            if (baseType != null && baseType != typeof(object) && baseType != typeof(IdData))
            {
                if (!collectionNameMap.TryGetValue(baseType, out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección Padre para el tipo {baseType.Name}");
                }
                else
                {
                    // Obtén la colección correspondiente
                    var collectionBson = db.GetCollection<BsonDocument>(collectionName);
                    var filteredDocuments = collectionBson.Query().Where(x => x["_id"] == id && x["type"] == currentType.Name).ToList();

                    var result = filteredDocuments.Select(BsonMapper.Global.ToObject<T>).FirstOrDefault();
                    // Busca el documento por ID
                    return result; // Devuelve el documento o null si no se encuentra
                }
            }
            else
            {
                if (!collectionNameMap.TryGetValue(typeof(T), out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección para el tipo {typeof(T).Name}");
                }
            }

            var collection = db.GetCollection<T>(collectionName);

            // Busca el documento por ID
            return collection.FindById(id); // Devuelve el documento o null si no se encuentra

        }
        public bool ExistTile(int idMaterial, int idInternal)
        {

            string collectionName;

            if (!collectionNameMap.TryGetValue(typeof(TileData), out collectionName))
            {
                throw new InvalidOperationException($"No se ha configurado un nombre de colección para el tipo");
            }

            var collectionBson = db.GetCollection<BsonDocument>(collectionName);
            var filteredDocuments = collectionBson.Query().Where(x => x["idMaterial"] == idMaterial && x["idInternalPosition"] == idInternal);
            bool result= filteredDocuments.ToArray().Any();            
            return result; // Devuelve el documento o null si no se encuentra

        }
        public List<T> FindAllByMaterial<T>(int idMaterial) where T : class
        {
            var currentType = typeof(T);
            var baseType = typeof(T).BaseType;
            string collectionName;
            if (baseType != null && baseType != typeof(object) && baseType != typeof(IdData))
            {
                if (!collectionNameMap.TryGetValue(baseType, out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección Padre para el tipo {baseType.Name}");
                }
                else
                {
                    // Obtén la colección correspondiente
                    var collectionBson = db.GetCollection<BsonDocument>(collectionName);
                    var filteredDocuments = collectionBson.Query().Where(x => x["type"] == currentType.Name && x["idMaterial"] == idMaterial).ToList();

                    var result2 = filteredDocuments.Select(BsonMapper.Global.ToObject<T>);
                    // Busca el documento por ID
                    return result2.ToList(); // Devuelve el documento o null si no se encuentra
                }
            }
            else
            {
                if (!collectionNameMap.TryGetValue(typeof(T), out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección para el tipo {typeof(T).Name}");
                }
            }

            var collection = db.GetCollection<BsonDocument>(collectionName);
            var filteredDocuments2 = collection.Query().Where(x => x["idMaterial"] == idMaterial).ToList();
            var result = filteredDocuments2.Select(BsonMapper.Global.ToObject<T>);
            // Busca el documento por ID
            return result.ToList(); // Devuelve el documento o null si no se encuentra
        }
        public List<T> FindAllByName<T>(string name) where T : class
        {
            var currentType = typeof(T);
            var baseType = typeof(T).BaseType;
            string collectionName;
            if (baseType != null && baseType != typeof(object) && baseType != typeof(IdData))
            {
                if (!collectionNameMap.TryGetValue(baseType, out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección Padre para el tipo {baseType.Name}");
                }
                else
                {
                    // Obtén la colección correspondiente
                    var collectionBson = db.GetCollection<BsonDocument>(collectionName);
                    var filteredDocuments = collectionBson.Query().Where(x => x["type"] == currentType.Name && x["name"].AsString.Contains(name)).ToList();

                    var result2 = filteredDocuments.Select(BsonMapper.Global.ToObject<T>);
                    // Busca el documento por ID
                    return result2.ToList(); // Devuelve el documento o null si no se encuentra
                }
            }
            else
            {
                if (!collectionNameMap.TryGetValue(typeof(T), out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección para el tipo {typeof(T).Name}");
                }
            }
            var regex = new System.Text.RegularExpressions.Regex(name, RegexOptions.IgnoreCase);

            var collection = db.GetCollection<BsonDocument>(collectionName);
            var filteredDocuments2 = collection.Query().Where(x => x["name"].AsString.Contains(name)).ToList();
            var result = filteredDocuments2.Select(BsonMapper.Global.ToObject<T>);
            // Busca el documento por ID
            return result.ToList(); // Devuelve el documento o null si no se encuentra
        }

        public List<T> FindAll<T>() where T : class
        {
            var currentType = typeof(T);
            var baseType = typeof(T).BaseType;
            string collectionName;
            if (baseType != null && baseType != typeof(object) && baseType != typeof(IdData))
            {
                if (!collectionNameMap.TryGetValue(baseType, out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección Padre para el tipo {baseType.Name}");
                }
                else
                {
                    // Obtén la colección correspondiente
                    var collectionBson = db.GetCollection<BsonDocument>(collectionName);
                    var filteredDocuments = collectionBson.Query().Where(x => x["type"] == currentType.Name).ToList();

                    var result = filteredDocuments.Select(BsonMapper.Global.ToObject<T>);
                    // Busca el documento por ID
                    return result.ToList(); // Devuelve el documento o null si no se encuentra
                }
            }
            else
            {
                if (!collectionNameMap.TryGetValue(typeof(T), out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección para el tipo {typeof(T).Name}");
                }
            }

            var collection = db.GetCollection<T>(collectionName);

            // Busca el documento por ID
            return collection.FindAll().ToList(); // Devuelve el documento o null si no se encuentra
        }
        public List<T> FindAllFilter<T>(BsonExpression bsonExpression) where T : class
        {
            var currentType = typeof(T);
            var baseType = typeof(T).BaseType;
            string collectionName;
            if (baseType != null && baseType != typeof(object) && baseType != typeof(IdData))
            {
                if (!collectionNameMap.TryGetValue(baseType, out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección Padre para el tipo {baseType.Name}");
                }        
            }
            else
            {
                if (!collectionNameMap.TryGetValue(typeof(T), out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección para el tipo {typeof(T).Name}");
                }
            }

            var collection = db.GetCollection<BsonDocument>(collectionName);
            var filteredDocuments2 = collection.Query().Where(bsonExpression).ToList();
            var result = filteredDocuments2.Select(BsonMapper.Global.ToObject<T>);

            // Busca el documento por ID
            return result.ToList(); // Devuelve el documento o null si no se encuentra
        }

        public bool RemoveById<T>(int id) where T : class
        {
            var currentType = typeof(T);
            var baseType = typeof(T).BaseType;
            string collectionName;
            if (baseType != null && baseType != typeof(object) && baseType != typeof(IdData))
            {
                if (!collectionNameMap.TryGetValue(baseType, out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección Padre para el tipo {baseType.Name}");
                }
            }
            else
            {
                if (!collectionNameMap.TryGetValue(typeof(T), out collectionName))
                {
                    throw new InvalidOperationException($"No se ha configurado un nombre de colección para el tipo {typeof(T).Name}");
                }
            }

            var collection = db.GetCollection<BsonDocument>(collectionName);

            var FreeData = FindById<DataBaseFree>(collectionName);
            if (FreeData == null)
            {
                FreeData = new DataBaseFree();
                FreeData.id = collectionName;
                FreeData.freeIds = new List<int>();
            }
            FreeData.freeIds.Add(id);
            InsertUpdateGeneric(FreeData);
            return collection.Delete(id);
  
        }

    }
}
