using Godot;
using System.Collections.Generic;

namespace GodotEcsArch.sources.managers.Multimesh;

public class MultimeshData
{
    public Rid multimeshRid;
    public Rid instanceRid;
    public MultiMesh multiMesh;
    public Stack<int> freePositions;
    public int currentPosition = 0;
    public int maxInstances;
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
        RenderingServer.InstanceSetScenario(instanceRid, EcsManager.Instance.RidWorld3D);
                     
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
        if (freePositions.Count > 0)
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
        // Limpia el slot por completo
        RenderingServer.MultimeshInstanceSetTransform(multimeshRid, idInstance, Transform3D.Identity);
        RenderingServer.MultimeshInstanceSetCustomData(multimeshRid, idInstance, new Color(0, 0, 0, 0));
        RenderingServer.MultimeshInstanceSetColor(multimeshRid, idInstance, new Color(0, 0, 0, 0));
        
        //RenderingServer.MultimeshInstanceSetCustomData(rid, idInstance, new Color(-1, -1, -1, -1));
        freePositions.Push(idInstance); 
    }
}
