
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using LiteDB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase
{
    internal class DataBaseManager: SingletonBase<DataBaseManager>
    {
        LiteDatabase db;
        string dataBasePath = FileHelper.GetPathGameDB("AssetExternals/db/MiBaseDeDatos2.db");
        private Dictionary<Type, string> collectionNameMap = new Dictionary<Type, string>();

        protected override void Initialize()
        {
            db = new LiteDatabase(dataBasePath);
            var mapper = BsonMapper.Global;


            collectionNameMap[typeof(MaterialData)] = "Materiales";
            collectionNameMap[typeof(TileData)] = "Tiles";
            collectionNameMap[typeof(AutoTileData)] = "AutoTiles";
            collectionNameMap[typeof(CharacterBaseData)] = "CharacterBase";

            ILiteCollection<MaterialData> MaterialDataCollection = db.GetCollection<MaterialData>("Materiales");            
            MaterialDataCollection.EnsureIndex(x => x.id, unique: true);

            ILiteCollection<TileData> TileDataCollection = db.GetCollection<TileData>("Tiles");
            TileDataCollection.EnsureIndex(x => x.id, unique: true);

            ILiteCollection<AutoTileData> AutoTileDataCollection = db.GetCollection<AutoTileData>("AutoTiles");
            AutoTileDataCollection.EnsureIndex(x => x.id, unique: true);

            ILiteCollection<AutoTileData> CharacterBaseDataCollection = db.GetCollection<AutoTileData>("CharacterBase");
            CharacterBaseDataCollection.EnsureIndex(x => x.id, unique: true);
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
        public void InsertUpdate<T>(T data, int id = -1)
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
            if (id == -1)
            {
                collection.Upsert( data);
            }
            else
            {
                collection.Upsert(id,data);
            }
            
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

    }
}
