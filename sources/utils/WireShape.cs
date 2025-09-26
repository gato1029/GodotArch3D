using Godot;
using System;
using System.Collections.Generic;

public class WireShape : SingletonBase<WireShape>
{
    public enum TypeDraw
    {
        PIXEL,
        NORMAL
    }

    private class ShapeData
    {
        public Rid MeshRid;
        public Rid InstanceRid;
        public Transform3D Transform;
        public Color Color;
        public Mesh Mesh; // 🔥 mantener referencia viva
        public float layer;
        // 🔹 Para flechas
        public Vector2? From;
        public Vector2? To;
    }

    private static Dictionary<int, ShapeData> _shapes = new Dictionary<int, ShapeData>();
    private Dictionary<Color, StandardMaterial3D> _materials = new Dictionary<Color, StandardMaterial3D>();

    public int DrawArrow(Vector2 from, Vector2 to, float layer, Color color, float headSize = 0.2f, TypeDraw typeDraw = TypeDraw.PIXEL)
    {
        int idUnico = UniqueIdGenerator.GetNextId<WireShape>();

        var instanceRid = RenderingServer.InstanceCreate();
        Mesh mesh = CreateWireArrowMesh(from, to, headSize, typeDraw);

        var mat = GetOrCreateMaterial(color);
        mesh.SurfaceSetMaterial(0, mat);

        var meshRid = mesh.GetRid();

        // Asignar al mundo
        RenderingServer.InstanceSetBase(instanceRid, meshRid);
        RenderingServer.InstanceSetScenario(instanceRid, EcsManager.Instance.RidWorld3D);

        // Posicionar en XY con layer como Z
        Transform3D xform = Transform3D.Identity;
        xform.Origin = new Vector3(0, 0, layer); // 👉 La posición está definida en los vértices (from/to), no en el transform
        RenderingServer.InstanceSetTransform(instanceRid, xform);

        // Guardar en el diccionario para actualizar después
        _shapes[idUnico] = new ShapeData
        {
            MeshRid = meshRid,
            InstanceRid = instanceRid,
            Transform = xform,
            Color = color,
            Mesh = mesh, // 🔥 evita que el GC lo libere
            From = from,
            To = to,
            layer = layer,
        };

        return idUnico;
    }

    public int DrawCircle(float radius, Vector2 position, float layer, Color color, int segments = 32, TypeDraw typeDraw = TypeDraw.PIXEL)
    {
        int idUnico = UniqueIdGenerator.GetNextId<WireShape>();

        var instanceRid = RenderingServer.InstanceCreate();

        Mesh mesh = CreateWireCircleMesh(radius, segments, typeDraw);

        // Reutilizamos materiales ya creados para evitar instancias innecesarias
        var mat = GetOrCreateMaterial(color);
        mesh.SurfaceSetMaterial(0, mat);

   

        var meshRid = mesh.GetRid();

        RenderingServer.InstanceSetBase(instanceRid, meshRid);
        RenderingServer.InstanceSetScenario(instanceRid, EcsManager.Instance.RidWorld3D);

        Transform3D xform = new Transform3D(Basis.Identity, Vector3.Zero);
        xform.Origin = new Vector3(position.X, position.Y, layer);
        RenderingServer.InstanceSetTransform(instanceRid, xform);

        // Guardamos en el diccionario para persistencia
        var shapeData = new ShapeData
        {
            
            InstanceRid = instanceRid,
            MeshRid = meshRid,        
            Color = color,
            Mesh = mesh, // 🔥 evita que el GC lo libere
            Transform = xform,
            layer = layer,
        };
        _shapes[idUnico] = shapeData;

        return idUnico;
    }
    // ===========================
    // Crear un cuadrado
    // ===========================
    public int DrawSquare(Vector2 size, Vector2 position, float layer, Color color, TypeDraw typeDraw = TypeDraw.PIXEL)
    {
        int idUnico = UniqueIdGenerator.GetNextId<WireShape>();

        var instanceRid = RenderingServer.InstanceCreate();
        Mesh mesh = CreateWireSquareMesh(size.X, size.Y, typeDraw);
       
        var mat = GetOrCreateMaterial(color);
        mesh.SurfaceSetMaterial(0, mat);

        var meshRid = mesh.GetRid();

        // Asignar al mundo
        RenderingServer.InstanceSetBase(instanceRid, meshRid);
        RenderingServer.InstanceSetScenario(instanceRid, EcsManager.Instance.RidWorld3D);

        // Posicionar en XY con layer como Z
        Transform3D xform = Transform3D.Identity;
        xform.Origin = new Vector3(position.X, position.Y, layer);
        RenderingServer.InstanceSetTransform(instanceRid, xform);

        // Guardamos en el diccionario
        _shapes[idUnico] = new ShapeData
        {
            MeshRid = meshRid,
            InstanceRid = instanceRid,
            Transform = xform,
            Color = color,
            Mesh = mesh, // 🔥 evita que el GC lo libere
            layer = layer
        };

        return idUnico;
    }
    // ===========================
    // Material cache
    // ===========================
    private StandardMaterial3D GetOrCreateMaterial(Color color)
    {
        if (_materials.TryGetValue(color, out var mat))
            return mat;

        mat = new StandardMaterial3D
        {
            AlbedoColor = color,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
        };

        _materials[color] = mat;
        return mat;
    }
    // ===========================
    // Actualizar posición por ID
    // ===========================
    public void UpdatePosition(int id, Vector2 newPos)
    {
        if (_shapes.TryGetValue(id, out var shape))
        {
            shape.Transform.Origin = new Vector3(newPos.X, newPos.Y, shape.layer);
            RenderingServer.InstanceSetTransform(shape.InstanceRid, shape.Transform);
        }
    }
    public void UpdateShapePositionPixel(int id, Vector2 newPositionPixels)
    {
        if (!_shapes.ContainsKey(id))
            return;

        var shape = _shapes[id];

        // Convertir a unidades de Godot
        Vector2 newPositionUnits = new Vector2(
            MeshCreator.PixelsToUnits(newPositionPixels.X),
            MeshCreator.PixelsToUnits(newPositionPixels.Y)
        );

        // Actualizar el Transform
        Transform3D xform = shape.Transform;
        xform.Origin = new Vector3(newPositionUnits.X, newPositionUnits.Y, shape.layer);

        // Aplicar al instance
        RenderingServer.InstanceSetTransform(shape.InstanceRid, xform);

        // Guardar el nuevo Transform en ShapeData
        shape.Transform = xform;
        _shapes[id] = shape;
    }
    // ===========================
    // Liberar un shape por ID
    // ===========================
    public void FreeShape(int id)
    {
        if (_shapes.TryGetValue(id, out var shape))
        {
            if (shape.InstanceRid.IsValid)
                RenderingServer.FreeRid(shape.InstanceRid);
            if (shape.MeshRid.IsValid)
                RenderingServer.FreeRid(shape.MeshRid);

            shape.Mesh = null; // 🔥 liberar referencia administrada

            _shapes.Remove(id);
            UniqueIdGenerator.ReleaseId<WireShape>(id);
        }
    }

    // ===========================
    // Crear malla de cuadrado en wireframe
    // ===========================
    public static ArrayMesh CreateWireSquareMesh(float widthPixels, float heightPixels, TypeDraw typeDraw = TypeDraw.PIXEL)
    {
        ArrayMesh arrayMesh = new ArrayMesh();
        float halfWidth;
        float halfHeight;

        if (typeDraw == TypeDraw.PIXEL)
        {
            halfWidth = 0.5f * MeshCreator.PixelsToUnits(widthPixels);
            halfHeight = 0.5f * MeshCreator.PixelsToUnits(heightPixels);
        }
        else
        {
            halfWidth = 0.5f * widthPixels;
            halfHeight = 0.5f * heightPixels;
        }

        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-halfWidth, -halfHeight, 0),  // Bottom left (0)
            new Vector3( halfWidth, -halfHeight, 0),  // Bottom right (1)
            new Vector3( halfWidth,  halfHeight, 0),  // Top right (2)
            new Vector3(-halfWidth,  halfHeight, 0)   // Top left (3)
        };

        int[] indices = new int[]
        {
            0, 1,
            1, 2,
            2, 3,
            3, 0
        };

        Godot.Collections.Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)ArrayMesh.ArrayType.Max);
        arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices;
        arrays[(int)ArrayMesh.ArrayType.Index] = indices;

        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Lines, arrays);

        return arrayMesh;
    }

    // ===========================
    // Crear malla de radio en wireframe
    // ===========================
    public static ArrayMesh CreateWireCircleMesh(float radius, int segments = 32, TypeDraw typeDraw = TypeDraw.PIXEL)
    {
        ArrayMesh arrayMesh = new ArrayMesh();

        float r = typeDraw == TypeDraw.PIXEL ? MeshCreator.PixelsToUnits(radius) : radius;

        Vector3[] vertices = new Vector3[segments];
        int[] indices = new int[segments * 2];

        for (int i = 0; i < segments; i++)
        {
            float angle = Mathf.Tau * i / segments;
            vertices[i] = new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0);

            // conectamos cada vértice con el siguiente (cierre con el primero)
            indices[i * 2] = i;
            indices[i * 2 + 1] = (i + 1) % segments;
        }

        Godot.Collections.Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)ArrayMesh.ArrayType.Max);
        arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices;
        arrays[(int)ArrayMesh.ArrayType.Index] = indices;

        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Lines, arrays);

        return arrayMesh;
    }

    // ===========================
    // Crear malla de la flecha
    // ===========================
    public static ArrayMesh CreateWireArrowMesh(
    Vector2 from,
    Vector2 to,
    float headSize = 0.2f,
    WireShape.TypeDraw typeDraw = WireShape.TypeDraw.PIXEL)
    {
        ArrayMesh arrayMesh = new ArrayMesh();

        // Escala en unidades
        Vector3 start, end;
        if (typeDraw == WireShape.TypeDraw.PIXEL)
        {
            start = new Vector3(
                MeshCreator.PixelsToUnits(from.X),
                MeshCreator.PixelsToUnits(from.Y),
                0);

            end = new Vector3(
                MeshCreator.PixelsToUnits(to.X),
                MeshCreator.PixelsToUnits(to.Y),
                0);
        }
        else
        {
            start = new Vector3(from.X, from.Y, 0);
            end = new Vector3(to.X, to.Y, 0);
        }

        // Dirección y normal perpendicular
        Vector3 dir = (end - start).Normalized();
        Vector3 perp = new Vector3(-dir.Y, dir.X, 0);

        // Punto base de la cabeza
        Vector3 headBase = end - dir * headSize;

        // Dos puntos de la V
        Vector3 left = headBase + perp * (headSize * 0.5f);
        Vector3 right = headBase - perp * (headSize * 0.5f);

        // Vértices de la flecha
        Vector3[] vertices = new Vector3[]
        {
        start, end,     // Línea principal
        end, left,      // V izquierda
        end, right      // V derecha
        };

        // Indices para líneas
        int[] indices = new int[]
        {
        0, 1, // Línea principal
        2, 3, // V izquierda
        4, 5  // V derecha
        };

        Godot.Collections.Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)ArrayMesh.ArrayType.Max);
        arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices;
        arrays[(int)ArrayMesh.ArrayType.Index] = indices;

        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Lines, arrays);

        return arrayMesh;
    }


    public int DrawIsometricGrid(int rows, int cols, float cellSize, Vector2 position, float layer, Color color, bool centered=false, TypeDraw typeDraw = TypeDraw.PIXEL)
    {
        int idUnico = UniqueIdGenerator.GetNextId<WireShape>();

        var instanceRid = RenderingServer.InstanceCreate();
        Mesh mesh = CreateWireIsometricGridMesh(rows, cols, cellSize, centered, typeDraw);

        var mat = GetOrCreateMaterial(color);
        mesh.SurfaceSetMaterial(0, mat);

        var meshRid = mesh.GetRid();

        // Asignar al mundo
        RenderingServer.InstanceSetBase(instanceRid, meshRid);
        RenderingServer.InstanceSetScenario(instanceRid, EcsManager.Instance.RidWorld3D);

        // Posicionar en XY con layer como Z
        Transform3D xform = Transform3D.Identity;
        xform.Origin = new Vector3(position.X, position.Y, layer);
        RenderingServer.InstanceSetTransform(instanceRid, xform);

        // Guardamos en el diccionario
        _shapes[idUnico] = new ShapeData
        {
            MeshRid = meshRid,
            InstanceRid = instanceRid,
            Transform = xform,
            Color = color,
            layer = layer,
            Mesh = mesh // 🔥 evitar GC
        };

        return idUnico;
    }

    private static ArrayMesh CreateWireIsometricGridMesh(int rows, int cols, float cellSize, bool centered, TypeDraw typeDraw)
    {
        ArrayMesh arrayMesh = new ArrayMesh();
        var vertices = new System.Collections.Generic.List<Vector3>();
        var indices = new System.Collections.Generic.List<int>();
        int index = 0;

        float unitCell = typeDraw == TypeDraw.PIXEL
            ? MeshCreator.PixelsToUnits(cellSize)
            : cellSize;

        float halfW = unitCell;
        float halfH = unitCell / 2f;

        // Offset para centrar
        float offsetX = 0;
        float offsetY = 0;
        if (centered)
        {
            offsetX = -((cols - 1 - (rows - 1)) * halfW) / 2f;
            offsetY = -((rows + cols - 2) * halfH) / 2f;
        }

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float cx = (c - r) * halfW + offsetX;
                float cy = (c + r) * halfH + offsetY;

                Vector3 top = new Vector3(cx, cy - halfH, 0);
                Vector3 right = new Vector3(cx + halfW, cy, 0);
                Vector3 bottom = new Vector3(cx, cy + halfH, 0);
                Vector3 left = new Vector3(cx - halfW, cy, 0);

                vertices.Add(top); vertices.Add(right); indices.Add(index++); indices.Add(index++);
                vertices.Add(right); vertices.Add(bottom); indices.Add(index++); indices.Add(index++);
                vertices.Add(bottom); vertices.Add(left); indices.Add(index++); indices.Add(index++);
                vertices.Add(left); vertices.Add(top); indices.Add(index++); indices.Add(index++);
            }
        }

        Godot.Collections.Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)ArrayMesh.ArrayType.Max);
        arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices.ToArray();
        arrays[(int)ArrayMesh.ArrayType.Index] = indices.ToArray();

        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Lines, arrays);
        return arrayMesh;
    }

    public int DrawGrid(int rows, int cols, float cellSize, Vector2 position, float layer, Color color, TypeDraw typeDraw = TypeDraw.PIXEL)
    {
        int idUnico = UniqueIdGenerator.GetNextId<WireShape>();

        var instanceRid = RenderingServer.InstanceCreate();
        Mesh mesh = CreateWireGridMesh(rows, cols, cellSize, typeDraw);

        var mat = GetOrCreateMaterial(color);
        mesh.SurfaceSetMaterial(0, mat);

        var meshRid = mesh.GetRid();

        // Asignar al mundo
        RenderingServer.InstanceSetBase(instanceRid, meshRid);
        RenderingServer.InstanceSetScenario(instanceRid, EcsManager.Instance.RidWorld3D);

        // Posicionar en XY con layer como Z
        Transform3D xform = Transform3D.Identity;
        xform.Origin = new Vector3(position.X, position.Y, layer);
        RenderingServer.InstanceSetTransform(instanceRid, xform);

        // Guardamos en el diccionario
        _shapes[idUnico] = new ShapeData
        {
            MeshRid = meshRid,
            InstanceRid = instanceRid,
            Transform = xform,
            Color = color,
            layer = layer,
            Mesh = mesh // 🔥 evita que el GC lo libere
        };

        return idUnico;
    }

    private static ArrayMesh CreateWireGridMesh(int rows, int cols, float cellSize, TypeDraw typeDraw)
    {
        ArrayMesh arrayMesh = new ArrayMesh();

        var vertices = new System.Collections.Generic.List<Vector3>();
        var indices = new System.Collections.Generic.List<int>();

        int index = 0;

        // 🔥 aplicar conversión si es PIXEL
        float unitCell = typeDraw == TypeDraw.PIXEL
            ? MeshCreator.PixelsToUnits(cellSize)
            : cellSize;

        float width = cols * unitCell;
        float height = rows * unitCell;

        // Líneas verticales
        for (int c = 0; c <= cols; c++)
        {
            float x = c * unitCell - width / 2f;
            vertices.Add(new Vector3(x, -height / 2f, 0));
            vertices.Add(new Vector3(x, height / 2f, 0));

            indices.Add(index++);
            indices.Add(index++);
        }

        // Líneas horizontales
        for (int r = 0; r <= rows; r++)
        {
            float y = r * unitCell - height / 2f;
            vertices.Add(new Vector3(-width / 2f, y, 0));
            vertices.Add(new Vector3(width / 2f, y, 0));

            indices.Add(index++);
            indices.Add(index++);
        }

        Godot.Collections.Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)ArrayMesh.ArrayType.Max);
        arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices.ToArray();
        arrays[(int)ArrayMesh.ArrayType.Index] = indices.ToArray();

        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Lines, arrays);

        return arrayMesh;
    }

    public void UpdateArrowPosition(int id, Vector2 from, Vector2 to, float headSize = 0.2f, TypeDraw typeDraw = TypeDraw.PIXEL)
    {
        if (!_shapes.ContainsKey(id))
            return;

        var shape = _shapes[id];

        // 🔹 Crear un nuevo mesh para la flecha
        Mesh newMesh = CreateWireArrowMesh(from, to, headSize, typeDraw);

        // Reusar el mismo material
        newMesh.SurfaceSetMaterial(0, GetOrCreateMaterial(shape.Color));

        // Obtener el rid nuevo
        var newMeshRid = newMesh.GetRid();

        // Asignar el mesh al instance ya existente
        RenderingServer.InstanceSetBase(shape.InstanceRid, newMeshRid);

        // Mantener el transform actual (solo capa/layer en Z)
        RenderingServer.InstanceSetTransform(shape.InstanceRid, shape.Transform);

        // Actualizar ShapeData para evitar GC
        shape.MeshRid = newMeshRid;
        shape.Mesh = newMesh;
        _shapes[id] = shape;
    }
    public void RotateArrow(int id, float degrees, float headSize = 0.2f, TypeDraw typeDraw = TypeDraw.PIXEL)
    {
        if (!_shapes.ContainsKey(id))
            return;

        var shape = _shapes[id];

        // 🔹 Recuperar posiciones actuales
        // Como no guardamos explícitamente `from` y `to`, necesitamos calcularlos desde el mesh
        // Para simplificar, guardaremos from/to en ShapeData
        if (shape.From == null || shape.To == null)
            return;

        Vector2 from = shape.From.Value;
        Vector2 to = shape.To.Value;

        // Vector dirección actual
        Vector2 dir = to - from;

        // Rotar el vector por los grados dados
        float radians = Mathf.DegToRad(-degrees); // 👈 invertir signo        
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        Vector2 rotatedDir = new Vector2(
            dir.X * cos - dir.Y * sin,
            dir.X * sin + dir.Y * cos
        );

        // Nuevo punto final
        Vector2 newTo = from + rotatedDir;

        // 🔹 Actualizar la flecha con la nueva dirección
        UpdateArrowPosition(id, from, newTo, headSize, typeDraw);

        // Guardar nueva posición en ShapeData
        shape.To = newTo;
        _shapes[id] = shape;
    }
    public int DrawFilledSquare(Vector2 size, Vector2 position, float layer, Color color, float alpha = 1f, TypeDraw typeDraw = TypeDraw.PIXEL)
    {
        int idUnico = UniqueIdGenerator.GetNextId<WireShape>();

        var instanceRid = RenderingServer.InstanceCreate();
        Mesh mesh = CreateFilledSquareMesh(size.X, size.Y, typeDraw);

        Color matColor = new Color(color.R, color.G, color.B, alpha);
        var mat = GetOrCreateMaterial(matColor);
        mat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
        mesh.SurfaceSetMaterial(0, mat);

        var meshRid = mesh.GetRid();

        // Asignar al mundo
        RenderingServer.InstanceSetBase(instanceRid, meshRid);
        RenderingServer.InstanceSetScenario(instanceRid, EcsManager.Instance.RidWorld3D);

        // Posicionar en XY con layer como Z
        Transform3D xform = Transform3D.Identity;
        xform.Origin = new Vector3(position.X, position.Y, layer);
        RenderingServer.InstanceSetTransform(instanceRid, xform);

        // Guardar en el diccionario
        _shapes[idUnico] = new ShapeData
        {
            MeshRid = meshRid,
            InstanceRid = instanceRid,
            Transform = xform,
            Color = color,
            layer = layer,
            Mesh = mesh // 🔥 evita GC
        };

        return idUnico;
    }

    private static ArrayMesh CreateFilledSquareMesh(float width, float height, TypeDraw typeDraw)
    {
        ArrayMesh arrayMesh = new ArrayMesh();

        float halfWidth;
        float halfHeight;
        if (typeDraw == TypeDraw.PIXEL)
        {
            halfWidth = 0.5f * MeshCreator.PixelsToUnits(width);
            halfHeight = 0.5f * MeshCreator.PixelsToUnits(height);
        }
        else
        {
            halfWidth = 0.5f * width;
            halfHeight = 0.5f * height;
        }

        Vector3[] vertices = new Vector3[]
        {
        new Vector3(-halfWidth, -halfHeight, 0), // 0 bottom left
        new Vector3( halfWidth, -halfHeight, 0), // 1 bottom right
        new Vector3( halfWidth,  halfHeight, 0), // 2 top right
        new Vector3(-halfWidth,  halfHeight, 0), // 3 top left
        };

        int[] indices = new int[]
       {
            0, 2, 1,  // First triangle
            0, 3, 2   // Second triangle
       };

        Vector3[] normals = new Vector3[]
        {
            new Vector3(0, 0, 1),  // Normal pointing out of the screen
            new Vector3(0, 0, 1),
            new Vector3(0, 0, 1),
            new Vector3(0, 0, 1)
        };

        // Define the UV coordinates (optional, used for texturing)
        Vector2[] uvs = new Vector2[]
        {
            new Vector2(0, 1),  // Bottom left
            new Vector2(1, 1),  // Bottom right
            new Vector2(1, 0),  // Top right
            new Vector2(0, 0)   // Top left
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
    // ===========================
    // Liberar todo
    // ===========================
    public void FreeAll()
    {
        foreach (var kv in _shapes)
        {
            var shape = kv.Value;
            if (shape.InstanceRid.IsValid)
                RenderingServer.FreeRid(shape.InstanceRid);
            if (shape.MeshRid.IsValid)
                RenderingServer.FreeRid(shape.MeshRid);

            UniqueIdGenerator.ReleaseId<WireShape>(kv.Key);
        }
        _shapes.Clear();
        _materials.Clear();
    }

    protected override void Initialize() { }
    protected override void Destroy() { FreeAll(); }
}
