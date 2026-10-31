namespace GodotEcsArch.sources.GameRuntime.Internal.TimerGame;

public readonly struct BlackyTimerNotification
{
    public BlackyTimerNotificationType Type { get; }

    public BlackyTimerNotificationUnit Unit { get; }

    /// <summary>
    /// Minuto absoluto del juego en el que debía ejecutarse.
    /// </summary>
    public long ScheduledGameMinute { get; }

    /// <summary>
    /// Minuto absoluto del juego en el que se ejecutó realmente.
    /// </summary>
    public long DispatchGameMinute { get; }

    /// <summary>
    /// Índice de ocurrencia.
    /// La primera ocurrencia utiliza el índice 0.
    /// </summary>
    public long OccurrenceIndex { get; }

    /// <summary>
    /// Motivo por el que avanzó el reloj.
    /// </summary>
    public BlackyTimerAdvanceMode AdvanceMode { get; }

    public bool IsOneShot { get; }

    public BlackyTimerNotification(
        BlackyTimerNotificationType type,
        BlackyTimerNotificationUnit unit,
        long scheduledGameMinute,
        long dispatchGameMinute,
        long occurrenceIndex,
        BlackyTimerAdvanceMode advanceMode,
        bool isOneShot)
    {
        Type = type;
        Unit = unit;
        ScheduledGameMinute = scheduledGameMinute;
        DispatchGameMinute = dispatchGameMinute;
        OccurrenceIndex = occurrenceIndex;
        AdvanceMode = advanceMode;
        IsOneShot = isOneShot;
    }
}