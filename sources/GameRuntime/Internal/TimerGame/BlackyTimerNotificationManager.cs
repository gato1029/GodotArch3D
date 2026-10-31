using System;
using System.Collections.Generic;

namespace GodotEcsArch.sources.GameRuntime.Internal.TimerGame;

public sealed class BlackyTimerNotificationManager : IDisposable
{
    private sealed class Subscription
    {
        public long Id;
        public BlackyTimerNotificationType Type;
        public Action<BlackyTimerNotification> Callback;
    }

    private sealed class PendingDispatch
    {
        public BlackyTimerSchedule Schedule;
        public long ScheduledMinute;
        public long OccurrenceIndex;
    }

    private readonly List<Subscription>[] subscribers;

    private readonly Dictionary<long, Subscription> subscriptionsById = new();

    private readonly List<BlackyTimerSchedule> schedules = new();

    private readonly Dictionary<long, BlackyTimerSchedule> schedulesById = new();

    private readonly Func<long> getCurrentGameMinute;

    private long nextId = 1;

    private long notificationCursor;

    private bool disposed;

    public BlackyTimerNotificationManager(
        Func<long> getCurrentGameMinute)
    {
        this.getCurrentGameMinute =
            getCurrentGameMinute ?? throw new ArgumentNullException(
                nameof(getCurrentGameMinute));

        int count = Enum.GetValues(
            typeof(BlackyTimerNotificationType)).Length;

        subscribers = new List<Subscription>[count];

        for (int i = 0; i < count; i++)
            subscribers[i] = new List<Subscription>();

        notificationCursor = getCurrentGameMinute();
    }

    // =========================================================
    // SUBSCRIPTIONS
    // =========================================================

    public long Subscribe(
        BlackyTimerNotificationType type,
        Action<BlackyTimerNotification> callback)
    {
        ThrowIfDisposed();

        ValidateType(type);

        if (callback == null)
            throw new ArgumentNullException(nameof(callback));

        long id = nextId++;

        var subscription = new Subscription
        {
            Id = id,
            Type = type,
            Callback = callback
        };

        subscribers[(int)type].Add(subscription);

        subscriptionsById.Add(id, subscription);

        return id;
    }

    public bool Unsubscribe(long subscriptionId)
    {
        ThrowIfDisposed();

        if (!subscriptionsById.TryGetValue(
                subscriptionId,
                out Subscription subscription))
        {
            return false;
        }

        subscribers[(int)subscription.Type].Remove(subscription);

        subscriptionsById.Remove(subscriptionId);

        subscription.Callback = null;

        return true;
    }

    // =========================================================
    // PERIODIC SCHEDULES
    // =========================================================

    /// <summary>
    /// Programa una notificación cada determinada cantidad de días.
    ///
    /// firstDay representa el día absoluto del calendario.
    /// La notificación se ejecuta a las 00:00 de ese día.
    /// </summary>
    public long ScheduleEveryDays(
        BlackyTimerNotificationType type,
        long intervalDays,
        long firstDay)
    {
        ThrowIfDisposed();

        ValidateType(type);

        if (intervalDays <= 0)
            throw new ArgumentOutOfRangeException(nameof(intervalDays));

        if (firstDay <= 0)
            throw new ArgumentOutOfRangeException(nameof(firstDay));

        long firstMinute = checked((firstDay - 1) * 1440);

        long intervalMinutes = checked(intervalDays * 1440);

        return AddSchedule(
            type,
            BlackyTimerNotificationUnit.Day,
            firstMinute,
            intervalMinutes,
            false,
            null);
    }

    /// <summary>
    /// Programa una notificación cada determinada cantidad de meses.
    ///
    /// firstMonth representa el mes absoluto del calendario.
    /// La notificación se ejecuta a las 00:00 del primer día del mes.
    /// </summary>
    public long ScheduleEveryMonths(
        BlackyTimerNotificationType type,
        long intervalMonths,
        long firstMonth)
    {
        ThrowIfDisposed();

        ValidateType(type);

        if (intervalMonths <= 0)
            throw new ArgumentOutOfRangeException(nameof(intervalMonths));

        if (firstMonth <= 0)
            throw new ArgumentOutOfRangeException(nameof(firstMonth));

        const long daysPerMonth = 30;

        long firstMinute = checked(
            (firstMonth - 1) * daysPerMonth * 1440);

        long intervalMinutes = checked(
            intervalMonths * daysPerMonth * 1440);

        return AddSchedule(
            type,
            BlackyTimerNotificationUnit.Month,
            firstMinute,
            intervalMinutes,
            false,
            null);
    }

    // =========================================================
    // ONE SHOT SCHEDULES
    // =========================================================

    /// <summary>
    /// Programa una notificación después de cierta cantidad de minutos.
    /// </summary>
    public long ScheduleAfterMinutes(
        BlackyTimerNotificationType type,
        long minutes,
        Action<BlackyTimerNotification> callback = null)
    {
        ThrowIfDisposed();

        ValidateType(type);

        if (minutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(minutes));

        long targetMinute = checked(
            getCurrentGameMinute() + minutes);

        return ScheduleAtMinute(
            type,
            targetMinute,
            callback);
    }

    /// <summary>
    /// Programa una notificación para un minuto absoluto del juego.
    /// </summary>
    public long ScheduleAtMinute(
        BlackyTimerNotificationType type,
        long targetMinute,
        Action<BlackyTimerNotification> callback = null)
    {
        ThrowIfDisposed();

        ValidateType(type);

        if (targetMinute <= getCurrentGameMinute())
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetMinute),
                "La fecha programada debe ser futura.");
        }

        return AddSchedule(
            type,
            BlackyTimerNotificationUnit.Minute,
            targetMinute,
            0,
            true,
            callback);
    }

    // =========================================================
    // CANCEL
    // =========================================================

    public bool Cancel(long scheduleId)
    {
        ThrowIfDisposed();

        if (!schedulesById.TryGetValue(
                scheduleId,
                out BlackyTimerSchedule schedule))
        {
            return false;
        }

        schedules.Remove(schedule);

        schedulesById.Remove(scheduleId);

        return true;
    }

    // =========================================================
    // PROCESSING
    // =========================================================

    internal void Process(
        long destinationMinute,
        BlackyTimerAdvanceMode advanceMode)
    {
        ThrowIfDisposed();

        if (destinationMinute < notificationCursor)
        {
            notificationCursor = destinationMinute;
            return;
        }

        long fromMinute = notificationCursor;

        if (destinationMinute == fromMinute)
            return;

        var pending = new List<PendingDispatch>();

        var expiredSchedules = new List<long>();

        foreach (BlackyTimerSchedule schedule in schedules)
        {
            if (schedule.IsOneShot)
            {
                long target = schedule.FirstOccurrence;

                if (target > fromMinute &&
                    target <= destinationMinute)
                {
                    pending.Add(new PendingDispatch
                    {
                        Schedule = schedule,
                        ScheduledMinute = target,
                        OccurrenceIndex = 0
                    });

                    expiredSchedules.Add(schedule.Id);
                }

                continue;
            }

            if (schedule.FirstOccurrence > destinationMinute)
                continue;

            long firstIndex = 0;

            if (fromMinute >= schedule.FirstOccurrence)
            {
                firstIndex =
                    ((fromMinute - schedule.FirstOccurrence)
                     / schedule.Interval) + 1;
            }

            long occurrenceMinute = checked(
                schedule.FirstOccurrence +
                firstIndex * schedule.Interval);

            long occurrenceIndex = firstIndex;

            while (occurrenceMinute <= destinationMinute)
            {
                pending.Add(new PendingDispatch
                {
                    Schedule = schedule,
                    ScheduledMinute = occurrenceMinute,
                    OccurrenceIndex = occurrenceIndex
                });

                occurrenceIndex++;

                if (occurrenceMinute >
                    long.MaxValue - schedule.Interval)
                {
                    break;
                }

                occurrenceMinute += schedule.Interval;
            }
        }

        // Primero retiramos los one-shot vencidos.
        // Así no quedan almacenados durante la ejecución del callback.
        foreach (long id in expiredSchedules)
        {
            schedules.RemoveAll(x => x.Id == id);
            schedulesById.Remove(id);
        }

        // Orden cronológico.
        // En caso de empate, utilizamos el ID de programación.
        pending.Sort((a, b) =>
        {
            int comparison = a.ScheduledMinute.CompareTo(
                b.ScheduledMinute);

            if (comparison != 0)
                return comparison;

            return a.Schedule.Id.CompareTo(b.Schedule.Id);
        });

        // Avanzamos el cursor antes de ejecutar callbacks.
        // Si un callback programa una nueva notificación,
        // esta no se procesa retroactivamente en este mismo avance.
        notificationCursor = destinationMinute;

        foreach (PendingDispatch item in pending)
        {
            var notification = new BlackyTimerNotification(
                item.Schedule.Type,
                item.Schedule.Unit,
                item.ScheduledMinute,
                destinationMinute,
                item.OccurrenceIndex,
                advanceMode,
                item.Schedule.IsOneShot);

            Dispatch(item.Schedule, notification);
        }
    }

    private void Dispatch(
        BlackyTimerSchedule schedule,
        BlackyTimerNotification notification)
    {
        // Callback exclusivo de la programación.
        schedule.Callback?.Invoke(notification);

        // Suscriptores globales del tipo.
        List<Subscription> typeSubscribers =
            subscribers[(int)schedule.Type];

        // Snapshot para permitir que un callback se suscriba
        // o desuscriba durante la ejecución.
        Subscription[] snapshot = typeSubscribers.ToArray();

        foreach (Subscription subscription in snapshot)
        {
            if (subscription.Callback == null)
                continue;

            // Si fue desuscrito durante el dispatch,
            // no se ejecuta.
            if (!subscriptionsById.ContainsKey(subscription.Id))
                continue;

            subscription.Callback.Invoke(notification);
        }
    }

    // =========================================================
    // INTERNAL HELPERS
    // =========================================================

    private long AddSchedule(
        BlackyTimerNotificationType type,
        BlackyTimerNotificationUnit unit,
        long firstOccurrence,
        long interval,
        bool isOneShot,
        Action<BlackyTimerNotification> callback)
    {
        long id = nextId++;

        var schedule = new BlackyTimerSchedule(
            id,
            type,
            unit,
            firstOccurrence,
            interval,
            isOneShot,
            callback);

        schedules.Add(schedule);

        schedulesById.Add(id, schedule);

        return id;
    }

    private void ValidateType(
        BlackyTimerNotificationType type)
    {
        int index = (int)type;

        if (index <= 0 || index >= subscribers.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(type),
                "El tipo de notificación no es válido.");
        }
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
            throw new ObjectDisposedException(
                nameof(BlackyTimerNotificationManager));
    }

    // =========================================================
    // CLEAR
    // =========================================================

    /// <summary>
    /// Elimina todas las suscripciones y programaciones.
    ///
    /// IMPORTANTE:
    /// No modifica el tiempo del juego.
    /// El cursor se sincroniza con el minuto actual para
    /// evitar que se reproduzcan notificaciones históricas.
    ///
    /// El administrador continúa siendo utilizable.
    /// </summary>
    public void Clear()
    {
        ThrowIfDisposed();

        ClearInternal();
    }

    private void ClearInternal()
    {
        foreach (List<Subscription> list in subscribers)
        {
            foreach (Subscription subscription in list)
                subscription.Callback = null;

            list.Clear();
        }

        subscriptionsById.Clear();

        foreach (BlackyTimerSchedule schedule in schedules)
        {
            // Libera referencias a callbacks individuales.
            // La propiedad es readonly, así que al limpiar las
            // colecciones el objeto queda disponible para GC.
        }

        schedules.Clear();

        schedulesById.Clear();

        notificationCursor = getCurrentGameMinute();

        // No reiniciamos nextId.
        // Así evitamos que un ID antiguo pueda cancelar
        // accidentalmente un registro nuevo.
    }

    /// <summary>
    /// Descarta las notificaciones históricas hasta el tiempo actual,
    /// pero conserva todas las suscripciones y programaciones.
    /// </summary>
    public void ResynchronizeToCurrentTime()
    {
        ThrowIfDisposed();

        long currentMinute = getCurrentGameMinute();

        // Elimina one-shot que ya quedaron atrás.
        for (int i = schedules.Count - 1; i >= 0; i--)
        {
            BlackyTimerSchedule schedule = schedules[i];

            if (schedule.IsOneShot &&
                schedule.FirstOccurrence <= currentMinute)
            {
                schedulesById.Remove(schedule.Id);
                schedules.RemoveAt(i);
            }
        }

        notificationCursor = currentMinute;
    }

    // =========================================================
    // DISPOSE
    // =========================================================

    public void Dispose()
    {
        if (disposed)
            return;

        ClearInternal();

        disposed = true;
    }
}