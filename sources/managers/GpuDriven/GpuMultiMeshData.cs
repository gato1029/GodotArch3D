namespace GodotEcsArch.sources.managers.GpuDriven;
using System.Threading;
using Godot;
using GodotEcsArch.sources.utils;
using System.Collections.Generic;
public static class BufferSlot
{
    /// <summary>
    /// Slot 0: Transformaciones de las instancias.
    /// Shader uniform: transform_texture.
    /// RGBA: X, Y, Z, Escala.
    /// </summary>
    public const int Transform = 0;
    /// <summary>
    /// Slot 1: Banderas de control de las instancias.
    /// Shader uniform: flags_texture.
    /// RGBA: MirrorX, MirrorY, IsVisible, LayerTexture.
    /// </summary>
    public const int Flags = 1;

    /// <summary>
    /// Slot 2: Datos estáticos de las instancias.
    /// Shader uniform: static_data_texture.
    /// RGBA: AtlasX, AtlasY, Width, Height (en Pixeles).
    /// </summary>
    public const int StaticData = 2;

    /// <summary>
    /// Slot 3: Estado de animación de las instancias.
    /// Shader uniform: anim_state_texture.
    /// RGBA: Índice LUT, Tiempo inicial, Reservado, Reservado.
    /// </summary>
    public const int AnimState = 3;

    /// <summary>
    /// Slot 4: Buffer personalizado para shaders especializados.
    /// Uso libre según el pipeline.
    /// </summary>
    public const int Custom0 = 4;

    /// <summary>
    /// Slot 5: Buffer personalizado para shaders especializados.
    /// Uso libre según el pipeline.
    /// </summary>
    public const int Custom1 = 5;

    /// <summary>
    /// Cantidad máxima de slots disponibles.
    /// </summary>
    public const int MaxSlots = 4;
}
public struct BufferEntry
{
    public string UniformName;
    public Image Image;
    public ImageTexture Texture;
    public byte[] Buffer;
    public bool IsActive;
    public bool IsDirty; // 🔹 Marca si el búfer sufrió cambios en CPU y requiere subida a VRAM
}
public unsafe class GpuMultiMeshData
{
    private int _initializedTransformCount = 0;
    public Rid MultimeshRid { get; private set; }
    public Rid InstanceRid { get; private set; }
    public MultiMesh MultiMeshRef { get; private set; }
    public Stack<int> FreePositions { get; private set; }
    public int CurrentPosition { get; private set; } = -1;
    public int MaxInstances { get; private set; }

    private readonly HashSet<int> _usedPositions = new();
    public ShaderMaterial Material { get; private set; }

    private readonly int _textureWidth;
    private readonly int _textureHeight;

    private readonly BufferEntry[] _buffers = new BufferEntry[BufferSlot.MaxSlots];
    private readonly int[] _activeBufferIndices;

    public int GpuDataIndex { get; internal set; } = -1;
    
    // 🔹 Bandera rápida para saber si al menos un búfer interno del Chunk está sucio
    private int _hasDirtyBuffers = 0;

    public bool HasDirtyBuffers => Volatile.Read(ref _hasDirtyBuffers) != 0;
    public bool RequiresAnimationTime => _buffers[BufferSlot.AnimState].IsActive;
    public GpuMultiMeshData(
        Mesh baseMesh, 
        ShaderMaterial baseMaterial, 
        Dictionary<int, string> requiredSlotUniforms, 
        int maxInstances = 65536, 
        int textureWidth = 256)
    {
        MaxInstances = maxInstances;
        FreePositions = new Stack<int>();

        _textureWidth = textureWidth;
        _textureHeight = Mathf.CeilToInt((float)MaxInstances / _textureWidth);

        Material = (ShaderMaterial)baseMaterial.Duplicate();

        Mesh localMesh = (Mesh)baseMesh.Duplicate();
        localMesh.SurfaceSetMaterial(0, Material);

        MultiMeshRef = new MultiMesh
        {
            Mesh = localMesh,
            UseCustomData = false,
            UseColors = true,
            TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
            InstanceCount = MaxInstances,
            VisibleInstanceCount = MaxInstances
        };
   
        MultiMeshRef.CustomAabb = new Aabb(
            new Vector3(-2048, -2048, -100),
            new Vector3(4096, 4096, 200)
        );
        InstanceRid = RenderingServer.InstanceCreate();
        MultimeshRid = MultiMeshRef.GetRid();
        RenderingServer.InstanceSetBase(InstanceRid, MultiMeshRef.GetRid());
        RenderingServer.InstanceSetScenario(InstanceRid, NodeMainHelper.ridWorld3D);

        var activeIndicesList = new List<int>();
 
        foreach (var (slotId, uniformName) in requiredSlotUniforms)
        {
            if (slotId < 0 || slotId >= BufferSlot.MaxSlots) continue;

            var image = Image.CreateEmpty(_textureWidth, _textureHeight, false, Image.Format.Rgbaf);
            var texture = ImageTexture.CreateFromImage(image);
            var buffer = image.GetData();

            _buffers[slotId] = new BufferEntry
            {
                UniformName = uniformName,
                Image = image,
                Texture = texture,
                Buffer = buffer,
                IsActive = true,
                IsDirty = true // Marca inicial para subir los datos base en el primer Flush
            };

            Material.SetShaderParameter(uniformName, texture);
            activeIndicesList.Add(slotId);
        }

        _activeBufferIndices = activeIndicesList.ToArray();
      

        if (Material.GetShaderParameter("texture_width").Obj == null)
        {
            Material.SetShaderParameter("texture_width", _textureWidth);
        }
    }

    public bool HasAvailableSpace() => FreePositions.Count > 0 || CurrentPosition < MaxInstances - 1;

    public int CreateInstance()
    {
        int id = (FreePositions.Count > 0)
            ? FreePositions.Pop()
            : ++CurrentPosition;

        if (_usedPositions.Contains(id))
        {
            GD.PrintErr($"BUG: Intentando reutilizar instancia en uso: {id}");
            return id;
        }

        _usedPositions.Add(id);

        // Inicializar el transform interno únicamente la primera vez.
        if (id > _initializedTransformCount - 1)
        {
            MultiMeshRef.SetInstanceTransform(id, Transform3D.Identity);
            _initializedTransformCount = id + 1;
        }

        return id;
    }

    public void FreeInstance(int instanceId)
    {
        if (!_usedPositions.Contains(instanceId))
        {
            GD.PrintErr($"BUG: Intentando liberar instancia no asignada: {instanceId}");
            return;
        }

        _usedPositions.Remove(instanceId);

        // Ocultar la instancia poniendo su visibilidad en 0 en Flags (Slot 1)
        SetBufferData(BufferSlot.Flags, instanceId, 0.0f, 0.0f, 0.0f, 0.0f);

        FreePositions.Push(instanceId);
    }

    /// <summary>
    /// Escribe datos instantáneamente O(1) usando punteros y marca la ranura como 'Dirty' para el siguiente Flush.
    /// </summary>
    public void SetBufferData(int slotId, int instanceId, float r, float g, float b, float a)
    {
        if (instanceId < 0 || instanceId >= MaxInstances) return;
        ref var entry = ref _buffers[slotId];
        if (!entry.IsActive) return;

        fixed (byte* ptr = entry.Buffer)
        {
            float* floatPtr = (float*)ptr;
            int baseIndex = instanceId * 4;

            floatPtr[baseIndex + 0] = r;
            floatPtr[baseIndex + 1] = g;
            floatPtr[baseIndex + 2] = b;
            floatPtr[baseIndex + 3] = a;
        }

        // 🔹 Marcamos la ranura y el Chunk como modificados
        entry.IsDirty = true;
        Interlocked.Exchange(ref _hasDirtyBuffers, 1);
    }
    public void SetBufferData(int slotId, int instanceId, float r)
    {
        ref var entry = ref _buffers[slotId];
        if (!entry.IsActive) return;

        fixed (byte* ptr = entry.Buffer)
        {
            ((float*)ptr)[instanceId * 4] = r;
        }

        entry.IsDirty = true;
        Interlocked.Exchange(ref _hasDirtyBuffers, 1);
    }
    public void SetBufferData(int slotId, int instanceId, float r, float g)
    {
        ref var entry = ref _buffers[slotId];
        if (!entry.IsActive) return;

        fixed (byte* ptr = entry.Buffer)
        {
            float* p = (float*)ptr + instanceId * 4;
            p[0] = r;
            p[1] = g;
        }

        entry.IsDirty = true;
        Interlocked.Exchange(ref _hasDirtyBuffers, 1);
    }
    public void SetBufferData(int slotId, int instanceId, float r, float g, float b)
    {
        ref var entry = ref _buffers[slotId];
        if (!entry.IsActive) return;

        fixed (byte* ptr = entry.Buffer)
        {
            float* p = (float*)ptr + instanceId * 4;
            p[0] = r;
            p[1] = g;
            p[2] = b;
        }

        entry.IsDirty = true;
        Interlocked.Exchange(ref _hasDirtyBuffers, 1);
    }
    /// <summary>
    /// Escribe datos instantáneamente O(1) usando punteros y marca la ranura como 'Dirty' para el siguiente Flush.
    /// </summary>
    public void SetBufferData(int slotId, int instanceId, Godot.Color color)
    {
        if (instanceId < 0 || instanceId >= MaxInstances) return;
        ref var entry = ref _buffers[slotId];
        if (!entry.IsActive) return;

        fixed (byte* ptr = entry.Buffer)
        {
            float* floatPtr = (float*)ptr;
            int baseIndex = instanceId * 4;

            floatPtr[baseIndex + 0] = color.R;
            floatPtr[baseIndex + 1] = color.G;
            floatPtr[baseIndex + 2] = color.B;
            floatPtr[baseIndex + 3] = color.A;
        }

        // 🔹 Marcamos la ranura y el Chunk como modificados
        entry.IsDirty = true;
        Interlocked.Exchange(ref _hasDirtyBuffers, 1);
    }
    public void UpdateGlobalTime(float time)
    {
        if (!RequiresAnimationTime)
            return;

        Material.SetShaderParameter("global_time", time);
    }

    /// <summary>
    /// Sube ÚNICAMENTE las imágenes que sufrieron cambios en CPU hacia la VRAM.
    /// Si ningún dato cambió, no realiza ninguna llamada a la GPU.
    /// </summary>
    public void FlushToGPU()
    {
        if (Interlocked.Exchange(ref _hasDirtyBuffers, 0) == 0)
            return;

        for (int i = 0; i < _activeBufferIndices.Length; i++)
        {
            int slotId = _activeBufferIndices[i];
            ref var entry = ref _buffers[slotId];

            if (!entry.IsDirty) continue;

            entry.Image.SetData(
                _textureWidth,
                _textureHeight,
                false,
                Image.Format.Rgbaf,
                entry.Buffer
            );

            entry.Texture.Update(entry.Image);
            
            entry.IsDirty = false;
        }
    }
}