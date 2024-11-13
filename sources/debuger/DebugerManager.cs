using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Relationships;
using Arch.System;
using Godot;
using ImGuiNET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;


internal class DebugerManager : BaseSystem<World, float>
{
    public static Dictionary<int, EntityDebug> entities;
  

    private CommandBuffer commandBuffer;
    private QueryDescription query = new QueryDescription();

    public DebugerManager(World world) : base(world)
    {
        commandBuffer = new CommandBuffer();
     
        entities = new Dictionary<int, EntityDebug>();   
    }
    public void RenderGui()
    {
        //for (int i = 0; i < entities.Count; i++)
        //{
        //    renderEntity(entities[i], entitiesChecked[i]);
        //}
        
    }
    public void renderEntity(EntityDebug entity)
    {

        ImGui.Checkbox(entity.entity.Id.ToString(), ref entity.isChecked);
    
    }
    public void renderDetail(EntityDebug entity)
    {
        if (entity.isChecked)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 10.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 1.5f);
            ImGui.BeginChild(entity.entity.Id.ToString() + " Child Window", new System.Numerics.Vector2(0, 600), ImGuiChildFlags.Border);
            ImGui.Text($"Detalle Entidad: {entity.entity.Id.ToString()}");
            ImGui.Separator();
          
            foreach (var component in entity.entity.GetAllComponents())
            {              
                DetailEntity(component);          
            }

            ImGui.EndChild();
            ImGui.PopStyleVar(2);
        }
    }

    public void DetailEntity(object entityObject)
    {
        ImGui.Button(entityObject.GetType().Name);
        ImGui.Separator();
        Type type = entityObject.GetType();
        FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            var data = field.GetValue(entityObject);
            if (data != null)
            {
             
                if (data is IEnumerable enumerable && !(data is string))
                {
                    foreach (var item in enumerable)
                    {
                        if (item != null && item.GetType().IsGenericType &&
                         item.GetType().GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                        {
                            
                            var key = item.GetType().GetProperty("Key")?.GetValue(item, null);
                            var value = item.GetType().GetProperty("Value")?.GetValue(item, null);
                            if (value.ToString().Trim()== "Arch.Relationships.InRelationship")
                            {
                                ImGui.Text("\t" + "Entidad Padre:" + key.ToString().Trim());                                
                            }
                            else
                            {                                
                                ImGui.Text("\t" + "Entidad Hija:" + key.ToString().Trim());
                                ImGui.Text("\t" + "Relacion:" + value.ToString().Trim());
                            }
                           
                            
                            
                        }
                        else
                        {
                            DetailEntity(item);
                        }
                        
                    }
                }
                else
                {
                    ImGui.Text("\t" + field.Name + ":" + data.ToString().Trim());
                }
                
            }
            else
            {
                ImGui.Text("\t" + field.Name + ": Sin definir");
            }
        }
    }
    public override void Update(in float t)
    {
        ImGui.Begin("Entitys");
        ImGui.Columns(2, "TreeAndDetailsColumns", true);
        ImGui.SetColumnWidth(0, 100);
        ImGui.Text("Total:"+ entities.Count);
        foreach (var archetype in World)
        {
            foreach (ref Chunk chunk in archetype)
            {
                foreach (int index in chunk)
                {
                    Entity entity = chunk.Entities[index];
                    if (!entities.ContainsKey(entity.Id))
                    {
                        bool flag = false;                        
                        entities.Add(entity.Id, new EntityDebug(flag,entity));
                    }
                    
                }
            }
        }
        foreach (var item in entities)
        {
            renderEntity(item.Value);
        }
        ImGui.NextColumn();
        foreach (var item in entities)
        {                       
            renderDetail(item.Value);
        }
        ImGui.End();

    }
}

