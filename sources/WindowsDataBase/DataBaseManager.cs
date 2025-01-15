
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase
{
    internal class DataBaseManager: SingletonBase<DataBaseManager>
    {
        LiteDatabase db;
       string dataBasePath = "MiBaseDeDatos.db";
        private Dictionary<Type, string> collectionNameMap = new Dictionary<Type, string>();

        protected override void Initialize()
        {
            db = new LiteDatabase(@dataBasePath);

            collectionNameMap[typeof(MaterialData)] = "Materiales";
          

            ILiteCollection<MaterialData> MaterialDataCollection = db.GetCollection<MaterialData>("Materiales");            
            MaterialDataCollection.EnsureIndex(x => x.idMaterial, unique: true);        
            
        }

        /// <summary>
        /// Método genérico para insertar datos en una colección específica
        /// </summary>
        /// <typeparam name="T">Tipo del dato que se insertará</typeparam>
        /// <param name="data">Instancia del objeto a insertar</param>
        public void Insert<T>(T data)
        {           
            if (!collectionNameMap.TryGetValue(typeof(T), out var collectionName))
            {
                throw new InvalidOperationException($"No se ha configurado un nombre de colección para el tipo {typeof(T).Name}");
            }
            var collection = db.GetCollection<T>(collectionName);           
            collection.Insert(data);
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
            // Obtén el nombre de la colección basado en el tipo T
            if (!collectionNameMap.TryGetValue(typeof(T), out var collectionName))
            {
                throw new InvalidOperationException($"No se ha configurado un nombre de colección para el tipo {typeof(T).Name}");
            }

            // Obtén la colección correspondiente
            var collection = db.GetCollection<T>(collectionName);

            // Busca el documento por ID
            return collection.FindById(id); // Devuelve el documento o null si no se encuentra
        }
    }
}
