using System;

namespace GodotEcsArch.sources.GameRuntime.Internal.TimerGame;

internal sealed class BlackyTimerSchedule
{
    public long Id { get; }

    public BlackyTimerNotificationType Type { get; }

    public BlackyTimerNotificationUnit Unit { get; }

    /// <summary>
    /// Minuto absoluto de la primera ocurrencia.
    /// </summary>
    public long FirstOccurrence { get; }

    /// <summary>
    /// Intervalo entre ocurrencias.
    /// Es 0 para una notificación de una sola ejecución.
    /// </summary>
    public long Interval { get; }

    public bool IsOneShot { get; }

    /// <summary>
    /// Callback exclusivo de esta programación.
    /// Puede ser null si se utilizan únicamente suscriptores globales.
    /// </summary>
    public Action<BlackyTimerNotification> Callback { get; }

    public BlackyTimerSchedule(
        long id,
        BlackyTimerNotificationType type,
        BlackyTimerNotificationUnit unit,
        long firstOccurrence,
        long interval,
        bool isOneShot,
        Action<BlackyTimerNotification> callback)
    {
        Id = id;
        Type = type;
        Unit = unit;
        FirstOccurrence = firstOccurrence;
        Interval = interval;
        IsOneShot = isOneShot;
        Callback = callback;
    }
}