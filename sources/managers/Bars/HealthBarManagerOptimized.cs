using System.Collections.Generic;
using Godot;
namespace GodotEcsArch.sources.managers.Bars;

public class HealthBarManagerOptimized
{
    private Mesh _barMesh;
    private HealthBarMultiMeshGroup _group;
    private Dictionary<Rid, HealthBarMultimeshData> _ridToMultimesh = new();

    public HealthBarManagerOptimized(float widthPixels = 32f, float heightPixels = 6f)
    {
        // 1. Cargar el Shader y asignar el material a la malla
        var shader = GD.Load<Shader>("res://resources/shaders/health_bar.gdshader");
        var barMaterial = new ShaderMaterial();
        barMaterial.Shader = shader;

        _barMesh = CreateBarMesh(widthPixels, heightPixels);
        _barMesh.SurfaceSetMaterial(0, barMaterial);

        // 2. Inicializar el grupo gestor de multimeshes dinámicos
        _group = new HealthBarMultiMeshGroup(_barMesh, (mm) =>
        {
            _ridToMultimesh[mm.multimeshRid] = mm;
        });
    }

    /// <summary>
    /// Crea una nueva instancia de barra de vida y devuelve su RID y su ID de slot.
    /// Si el multimesh actual llega a 65,500, creará uno nuevo automáticamente.
    /// </summary>
    public (Rid multimeshRid, int instanceId) CreateInstance()
    {
        var (mm, id) = _group.CreateInstance();
        return (mm.multimeshRid, id);
    }

    /// <summary>
    /// Actualiza la salud y la posición 3D de una barra específica.
    /// </summary>
    public void UpdateHealth(Rid multimeshRid, int instanceId, float healthPercent, Transform3D transform)
    {
        var mm = FindMultiMesh(multimeshRid);
        if (mm != null)
        {
            mm.UpdateHealth(instanceId, healthPercent, transform);
        }
    }

    /// <summary>
    /// Libera la barra de vida cuando la unidad muere o se destruye.
    /// </summary>
    public void FreeInstance(Rid multimeshRid, int instanceId)
    {
        if (instanceId < 0) return;

        var mm = FindMultiMesh(multimeshRid);
        if (mm == null)
        {
            GD.PrintErr("❌ HealthBar Multimesh no encontrado para liberar.");
            return;
        }

        mm.FreeInstance(instanceId);
    }

    private HealthBarMultimeshData FindMultiMesh(Rid rid)
    {
        if (_ridToMultimesh.TryGetValue(rid, out var mm))
            return mm;

        return null;
    }

    private Mesh CreateBarMesh(float widthPixels, float heightPixels)
    {
        ArrayMesh arrayMesh = new ArrayMesh();
        float pixelsToUnitsFactor = 1f / 32f;
        float halfWidth = 0.5f * (widthPixels * pixelsToUnitsFactor);
        float halfHeight = 0.5f * (heightPixels * pixelsToUnitsFactor);

        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-halfWidth, -halfHeight, 0),
            new Vector3(halfWidth, -halfHeight, 0),
            new Vector3(halfWidth, halfHeight, 0),
            new Vector3(-halfWidth, halfHeight, 0)
        };

        int[] indices = new int[] { 0, 2, 1, 0, 3, 2 };
        Vector3[] normals = new Vector3[]
        {
            new Vector3(0, 0, 1), new Vector3(0, 0, 1),
            new Vector3(0, 0, 1), new Vector3(0, 0, 1)
        };
        Vector2[] uvs = new Vector2[]
        {
            new Vector2(0, 1), new Vector2(1, 1),
            new Vector2(1, 0), new Vector2(0, 0)
        };

        Godot.Collections.Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)ArrayMesh.ArrayType.Max);
        arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices;
        arrays[(int)ArrayMesh.ArrayType.Index] = indices;
        arrays[(int)ArrayMesh.ArrayType.Normal] = normals;
        arrays[(int)ArrayMesh.ArrayType.TexUV] = uvs;

        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        return arrayMesh;
    }
}