using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase;
public class FreeIdBucket
{
    [BsonId]
    public string Id { get; set; } // collection_grouping

    public string CollectionName { get; set; }
    public ushort GroupingSave { get; set; }

    public List<ushort> FreeValues { get; set; } = new();
}