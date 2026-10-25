using System.Collections.Generic;
using Godot;
using GodotEcsArch.sources.utils;
namespace GodotEcsArch.sources.managers.Bars;


public class HealthBarMultimeshData
{
    public Rid multimeshRid;
    public Rid instanceRid;
    public MultiMesh multiMesh;
    public Stack<int> freePositions;
    public int currentPosition = -1;
    public int maxInstances;
    private HashSet<int> usedPositions = new HashSet<int>();

    public HealthBarMultimeshData(Mesh meshBase, int pMaxInstances = 65500)
    {
        maxInstances = pMaxInstances;
        freePositions = new Stack<int>();

        multiMesh = new MultiMesh();
        multiMesh.Mesh = meshBase;
        multiMesh.UseCustomData = true; // Clave para el % de vida
        multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
        multiMesh.InstanceCount = maxInstances;
        multiMesh.VisibleInstanceCount = maxInstances;

        multimeshRid = multiMesh.GetRid();

        instanceRid = RenderingServer.InstanceCreate();
        RenderingServer.InstanceSetBase(instanceRid, multimeshRid);
        RenderingServer.InstanceSetScenario(instanceRid, NodeMainHelper.ridWorld3D);
    }

    public bool AvailableSpace()
    {
        return freePositions.Count > 0 || currentPosition < maxInstances - 1;
    }

    public int CreateInstance()
    {
        int id = freePositions.Count > 0 ? freePositions.Pop() : ++currentPosition;

        if (usedPositions.Contains(id))
        {
            GD.PrintErr("BUG (HealthBar): intentando reutilizar instance en uso: " + id);
        }

        usedPositions.Add(id);
        return id;
    }

    public void UpdateHealth(int idInstance, float healthPercent, Transform3D transform)
    {
        if (!usedPositions.Contains(idInstance)) return;

        // 1. Actualizar transformación 3D (posición y rotación de cámara)
        RenderingServer.MultimeshInstanceSetTransform(multimeshRid, idInstance, transform);

        // 2. Enviar porcentaje de vida en el canal R de CustomData
        Color customData = new Color(Mathf.Clamp(healthPercent, 0.0f, 1.0f), 1, 0, 0);
        RenderingServer.MultimeshInstanceSetCustomData(multimeshRid, idInstance, customData);
    }

    public void FreeInstance(int idInstance)
    {
        if (!usedPositions.Contains(idInstance))
        {
            GD.PrintErr("BUG (HealthBar): intentando liberar instance que no está en uso: " + idInstance);
            return;
        }

        usedPositions.Remove(idInstance);

        RenderingServer.MultimeshInstanceSetTransform(multimeshRid, idInstance, Transform3D.Identity);
        RenderingServer.MultimeshInstanceSetCustomData(multimeshRid, idInstance, new Color(0, 0, 0, 0));

        freePositions.Push(idInstance);
    }
}
