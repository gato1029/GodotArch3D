using Godot;
using System.Collections.Generic;

namespace GodotEcsArch.sources.managers.Multimesh;

public class MultimeshData
{
    public Rid rid;
    public MultiMesh multiMesh;
    public Stack<int> freePositions;
    public int currentPosition;
    public int maxInstances;
    public MultimeshData(Mesh meshBase, int pMaxInstances = 65500)
    {
        currentPosition = -1;
        maxInstances = pMaxInstances;
        freePositions = new Stack<int>();
        multiMesh = new MultiMesh();
        multiMesh.Mesh = meshBase;
        multiMesh.UseCustomData = true;
        multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
        multiMesh.InstanceCount = maxInstances;
        multiMesh.VisibleInstanceCount = maxInstances;
        

        Rid instanceRid = RenderingServer.InstanceCreate();
        RenderingServer.InstanceSetBase(instanceRid, multiMesh.GetRid());
        RenderingServer.InstanceSetScenario(instanceRid, EcsManager.Instance.RidWorld3D);
        rid =multiMesh.GetRid();
        //rid = instanceRid;

    }

    public bool AvailbleSpace()
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
        if (freePositions.Count>0)
        {
            return freePositions.Pop();
        }
        else
        {
            currentPosition++;
            return currentPosition;
        }        
    }
    public void FreeInstance(int idInstance)
    { 
        freePositions.Push(idInstance); 
    }
}
