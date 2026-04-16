using Godot;
using GodotEcsArch.sources.utils;
using System;
using System.Collections.Generic;

namespace GodotEcsArch.sources.managers.Multimesh;

public class MultiMeshGroup
{
    public Mesh BaseMesh;

    private List<MultimeshData> _multimeshes = new();

    private Action<MultimeshData> _onCreated;

    public MultiMeshGroup(Mesh mesh, Action<MultimeshData> onCreated)
    {
        BaseMesh = mesh;
        _onCreated = onCreated;
    }

    public (MultimeshData mm, int instanceId) CreateInstance()
    {
        // 🔍 Buscar uno con espacio
        foreach (var mm in _multimeshes)
        {
            if (mm.AvailableSpace())
            {
                int id = mm.CreateInstance();
                return (mm, id);
            }
        }

        var newMM = new MultimeshData(BaseMesh);

        _multimeshes.Add(newMM);
        _onCreated?.Invoke(newMM);
        int newId = newMM.CreateInstance();
        return (newMM, newId);
    }
}
public class MultimeshData
{
    public Rid multimeshRid;
    public Rid instanceRid;
    public MultiMesh multiMesh;
    public Stack<int> freePositions;
    public int currentPosition = 0;
    public int maxInstances;
    private HashSet<int> usedPositions = new HashSet<int>();
    public MultimeshData(Mesh meshBase, int pMaxInstances = 65500)
    {
        currentPosition = -1;
        maxInstances = pMaxInstances;
        freePositions = new Stack<int>();
        multiMesh = new MultiMesh();
        multiMesh.Mesh = meshBase;
        multiMesh.UseCustomData = true;
        multiMesh.UseColors = true;
        multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
        multiMesh.InstanceCount = maxInstances;
        multiMesh.VisibleInstanceCount = maxInstances;
        

        instanceRid = RenderingServer.InstanceCreate();
        multimeshRid = multiMesh.GetRid();
        RenderingServer.InstanceSetBase(instanceRid, multiMesh.GetRid());
        RenderingServer.InstanceSetScenario(instanceRid, NodeMainHelper.ridWorld3D);
                     
    }

    public bool AvailableSpace()
    {
        if (freePositions.Count>0)
        {
            return true;
        }
        if (currentPosition < maxInstances -1)
        {
            return true;
        }
        return false;
    }
    public int CreateInstance()
    {
        int id;
        if (freePositions.Count > 0)
        {
            id = freePositions.Pop();
        }
        else
        {
            currentPosition++;
            id = currentPosition;
        }

        if (usedPositions.Contains(id))
        {
            GD.PrintErr("BUG: intentando reutilizar instance en uso: " + id);
        }

        usedPositions.Add(id);
        return id;
    }
    public void FreeInstance(int idInstance)
    {
        if (!usedPositions.Contains(idInstance))
        {
            GD.PrintErr("BUG: intentando liberar instance que no está en uso: " + idInstance);
            return;
        }

        usedPositions.Remove(idInstance);

        RenderingServer.MultimeshInstanceSetTransform(multimeshRid, idInstance, Transform3D.Identity);
        RenderingServer.MultimeshInstanceSetCustomData(multimeshRid, idInstance, new Color(0, 0, 0, 0));
        RenderingServer.MultimeshInstanceSetColor(multimeshRid, idInstance, new Color(0, 0, 0, 0));

        freePositions.Push(idInstance);
    }
}
