using Godot;
using System.Collections.Generic;

/// <summary>
/// Pool de CanvasItems (RenderingServer puro, sin nodos) para dibujar texto de debug
/// posicionado en el MUNDO 3D y proyectado a pantalla usando una Camera3D.
///
/// Reserva una cantidad base de RIDs al iniciar y los reutiliza mediante ids (int).
///
/// Uso típico:
///   var pool = new DebugTextPool();
///   pool.Init(_debugCanvasLayer.GetCanvas(), camera, 10000);
///
///   int id = pool.DrawText(new Vector3(x, 0, z), "3"); // posición de MUNDO, no de pantalla
///   pool.UpdateText(id, "7");                          // cambia el VALOR (caro, redibuja glyphs)
///   pool.UpdateWorldPosition(id, nuevaPosMundo);        // cambia dónde vive esa celda en el mundo
///   pool.ReleaseText(id);                              // libera el slot para otra celda
///
///   // una vez por frame (ej. en tu _Process):
///   pool.Update(); // reproyecta todas las posiciones SOLO si la cámara se movió
///
///   // al cerrar / cambiar de escena:
///   pool.Cleanup();
/// </summary>
public partial class DebugTextPool
{
    // Un RID de CanvasItem por slot. El índice en esta lista ES el "id" que se entrega afuera.
    private List<Rid> _items = new();

    // Posición de MUNDO de cada slot (paralelo a _items). Solo válido si _inUse[id] es true.
    private List<Vector3> _worldPositions = new();

    // Ids liberados, listos para reutilizar (se entregan antes de crear un CanvasItem nuevo).
    private Stack<int> _freeIds = new();

    // Marca qué slots están actualmente en uso (para validar releases dobles, debug, etc).
    private List<bool> _inUse = new();

    private Rid _canvasParent;
    private Font _font;
    private int _fontSize;
    private Color _defaultColor;

    private Camera3D _camera;
    private Transform3D _lastCamTransform;
    private bool _hasLastCamTransform;

    // Soporte de zoom para cámara ortogonal (Camera3D.Size).
    private float _lastCamSize;
    private float _referenceOrthoSize; // Size de la cámara al momento de Init/SetCamera (factor de escala = 1 en ese punto)
    private bool _scaleWithZoom;

    /// <summary>
    /// Inicializa el pool. Llamar una sola vez.
    /// </summary>
    /// <param name="canvasParent">RID del canvas donde se dibuja, ej. canvasLayer.GetCanvas()</param>
    /// <param name="camera">Cámara usada para proyectar posiciones de mundo a pantalla</param>
    /// <param name="initialCapacity">Cantidad de CanvasItems a pre-crear (ej. 10000)</param>
    /// <param name="font">Fuente a usar. Si es null, usa ThemeDB.FallbackFont</param>
    /// <param name="fontSize">Tamaño de fuente por defecto</param>
    /// <param name="defaultColor">Color por defecto del texto</param>
    /// <param name="scaleWithZoom">
    /// Si es true, el texto escala junto con el zoom de la cámara ortogonal (Size), manteniendo
    /// su tamaño relativo al mundo. Si es false (default), el texto siempre mide lo mismo en
    /// píxeles de pantalla sin importar el zoom (más legible para debug puro).
    /// </param>
    public void Init(Rid canvasParent, Camera3D camera, int initialCapacity = 10000, Font font = null,
        int fontSize = 14, Color? defaultColor = null, bool scaleWithZoom = true)
    {
        _canvasParent = canvasParent;
        _camera = camera;
        _font = font ?? ThemeDB.FallbackFont;
        _fontSize = fontSize;
        _defaultColor = defaultColor ?? Colors.White;
        _scaleWithZoom = scaleWithZoom;

        if (_camera != null)
        {
            _referenceOrthoSize = _camera.Size;
            _lastCamSize = _camera.Size;
        }

        _items.Capacity = initialCapacity;
        _inUse.Capacity = initialCapacity;
        _worldPositions.Capacity = initialCapacity;

        for (int i = 0; i < initialCapacity; i++)
        {
            var ci = RenderingServer.CanvasItemCreate();
            RenderingServer.CanvasItemSetParent(ci, _canvasParent);
            _items.Add(ci);
            _inUse.Add(false);
            _worldPositions.Add(Vector3.Zero);
            _freeIds.Push(i);
        }
    }

    /// <summary>
    /// Cambia la cámara usada para proyectar. Útil si cambias de cámara activa en runtime.
    /// Fuerza una re-proyección completa en el próximo Update(). El Size actual de la nueva
    /// cámara pasa a ser la referencia de escala 1.0 (si scaleWithZoom está activo).
    /// </summary>
    public void SetCamera(Camera3D camera)
    {
        _camera = camera;
        _hasLastCamTransform = false; // fuerza recálculo la próxima vez
        if (_camera != null)
        {
            _referenceOrthoSize = _camera.Size;
            _lastCamSize = _camera.Size;
        }
    }

    /// <summary>
    /// Activa o desactiva el escalado del texto según el zoom de la cámara. Al activarlo, el
    /// Size actual de la cámara pasa a ser la nueva referencia de escala 1.0.
    /// </summary>
    public void SetScaleWithZoom(bool scaleWithZoom)
    {
        _scaleWithZoom = scaleWithZoom;
        if (_camera != null)
            _referenceOrthoSize = _camera.Size;
        _hasLastCamTransform = false; // fuerza recálculo
    }

    /// <summary>
    /// Dibuja un texto en una posición de MUNDO y devuelve un id para actualizarlo o liberarlo luego.
    /// Reutiliza un slot libre del pool si hay uno; si no, crea un CanvasItem nuevo (el pool crece).
    ///
    /// Internamente el texto se dibuja en el origen local (0,0) del CanvasItem, y la posición en
    /// pantalla (calculada proyectando worldPosition con la cámara) se aplica como transform.
    /// Esto permite reposicionar después SIN volver a generar los glyphs (barato).
    /// </summary>
    public int DrawText(Vector3 worldPosition, string text, Color? color = null, int? fontSize = null)
    {
        int id = GetFreeId();
        _inUse[id] = true;
        _worldPositions[id] = worldPosition;

        ApplyProjectedTransform(id, worldPosition);
        RedrawContent(_items[id], text, color, fontSize);

        return id;
    }

    /// <summary>
    /// Cambia el CONTENIDO del texto de un id existente (caro: re-tesela los glyphs).
    /// Usa esto solo cuando el VALOR mostrado cambia. La posición actual se conserva.
    /// </summary>
    public void UpdateText(int id, string text, Color? color = null, int? fontSize = null)
    {
        if (!IsValid(id))
        {
            GD.PushError($"DebugTextPool.UpdateText: id {id} inválido o no está en uso.");
            return;
        }

        RedrawContent(_items[id], text, color, fontSize);
    }

    /// <summary>
    /// Cambia la posición de MUNDO de un id existente (ej. la celda se movió) y re-proyecta
    /// inmediatamente. Barato: solo mueve el transform, no toca los glyphs.
    /// </summary>
    public void UpdateWorldPosition(int id, Vector3 worldPosition)
    {
        if (!IsValid(id))
        {
            GD.PushError($"DebugTextPool.UpdateWorldPosition: id {id} inválido o no está en uso.");
            return;
        }

        _worldPositions[id] = worldPosition;
        ApplyProjectedTransform(id, worldPosition);
    }

    /// <summary>
    /// Llamar UNA VEZ POR FRAME. Si la cámara se movió, rotó, o hizo zoom (Size, en ortogonal)
    /// desde el último Update(), re-proyecta la posición de pantalla de TODOS los ids activos.
    /// Si la cámara sigue exactamente igual, no hace nada (evita trabajo innecesario).
    ///
    /// IMPORTANTE: el zoom de una cámara ortogonal se controla con Camera3D.Size, NO con su
    /// transform (posición/rotación). Si solo comparáramos el transform, un zoom sin mover la
    /// cámara pasaría desapercibido y el texto quedaría desalineado de su celda. Por eso Size
    /// se chequea aparte.
    /// </summary>
    public void Update()
    {
        if (_camera == null) return;

        Transform3D camTransform = _camera.GlobalTransform;
        float camSize = _camera.Size;

        bool cameraMoved = !_hasLastCamTransform
            || !camTransform.Origin.IsEqualApprox(_lastCamTransform.Origin)
            || !camTransform.Basis.GetRotationQuaternion().IsEqualApprox(_lastCamTransform.Basis.GetRotationQuaternion())
            || !Mathf.IsEqualApprox(camSize, _lastCamSize);

        if (!cameraMoved) return;

        _lastCamTransform = camTransform;
        _lastCamSize = camSize;
        _hasLastCamTransform = true;

        for (int id = 0; id < _items.Count; id++)
        {
            if (!_inUse[id]) continue;
            ApplyProjectedTransform(id, _worldPositions[id]);
        }
    }

    private void ApplyProjectedTransform(int id, Vector3 worldPosition)
    {
        var ci = _items[id];

        if (_camera == null)
        {
            GD.PushError("DebugTextPool: no hay cámara asignada (usa Init con camera o SetCamera).");
            return;
        }

        if (_camera.IsPositionBehind(worldPosition))
        {
            RenderingServer.CanvasItemSetVisible(ci, false);
            return;
        }

        RenderingServer.CanvasItemSetVisible(ci, true);
        Vector2 screenPos = _camera.UnprojectPosition(worldPosition);

        if (_scaleWithZoom)
        {
            // Escala uniforme (misma en X e Y): el texto NUNCA se deforma, solo crece o
            // encoge como un todo, en proporción inversa al Size actual vs. el de referencia.
            // Zoom in (Size baja) -> scale > 1 (texto más grande, igual que las celdas).
            // Zoom out (Size sube) -> scale < 1 (texto más chico, igual que las celdas).
            float safeSize = Mathf.Max(_camera.Size, 0.0001f);
            float scale = _referenceOrthoSize / safeSize;
            var xform = new Transform2D(new Vector2(scale, 0f), new Vector2(0f, scale), screenPos);
            RenderingServer.CanvasItemSetTransform(ci, xform);
        }
        else
        {
            // Tamaño fijo en pantalla: siempre legible, sin importar el zoom.
            RenderingServer.CanvasItemSetTransform(ci, new Transform2D(0, screenPos));
        }
    }

    private void RedrawContent(Rid ci, string text, Color? color, int? fontSize)
    {
        RenderingServer.CanvasItemClear(ci);
        if (!string.IsNullOrEmpty(text))
        {
            _font.DrawString(
                ci,
                Vector2.Zero, // siempre en el origen local; la posición real la da el transform
                text,
                HorizontalAlignment.Left,
                -1,
                fontSize ?? _fontSize,
                color ?? _defaultColor
            );
        }
    }

    /// <summary>
    /// Libera un id: borra su contenido visual y lo devuelve al pool para ser reutilizado
    /// por un futuro DrawText. El CanvasItem (RID) NO se destruye, solo se limpia y se recicla.
    /// </summary>
    public void ReleaseText(int id)
    {
        if (!IsValid(id))
        {
            GD.PushError($"DebugTextPool.ReleaseText: id {id} inválido o ya estaba liberado.");
            return;
        }

        RenderingServer.CanvasItemClear(_items[id]);
        RenderingServer.CanvasItemSetTransform(_items[id], Transform2D.Identity);
        _inUse[id] = false;
        _worldPositions[id] = Vector3.Zero;
        _freeIds.Push(id);
    }

    /// <summary>
    /// Oculta o muestra un id manualmente sin borrar su contenido. Nota: Update() también
    /// controla visibilidad automáticamente según si la posición quedó detrás de cámara.
    /// </summary>
    public void SetVisible(int id, bool visible)
    {
        if (!IsValid(id))
        {
            GD.PushError($"DebugTextPool.SetVisible: id {id} inválido.");
            return;
        }
        RenderingServer.CanvasItemSetVisible(_items[id], visible);
    }

    /// <summary>
    /// Libera TODOS los RIDs del pool. Llamar al cerrar el juego o destruir el sistema
    /// que usa este pool (ej. en _ExitTree si lo envuelves en un Node).
    /// </summary>
    public void Cleanup()
    {
        foreach (var ci in _items)
        {
            if (ci.IsValid)
                RenderingServer.FreeRid(ci);
        }
        _items.Clear();
        _inUse.Clear();
        _worldPositions.Clear();
        _freeIds.Clear();
    }

    // ---- privados ----

    private int GetFreeId()
    {
        if (_freeIds.Count > 0)
            return _freeIds.Pop();

        // Pool agotado: crece de a uno (podrías crecer de a bloques si esperas esto seguido).
        var ci = RenderingServer.CanvasItemCreate();
        RenderingServer.CanvasItemSetParent(ci, _canvasParent);
        _items.Add(ci);
        _inUse.Add(false);
        _worldPositions.Add(Vector3.Zero);
        return _items.Count - 1;
    }

    private bool IsValid(int id)
    {
        return id >= 0 && id < _items.Count && _inUse[id];
    }
}