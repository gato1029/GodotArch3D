using System;

namespace GodotEcsArch.sources.GameRuntime.Internal.TimerGame;

public enum BlackyTimerAdvanceMode
{
    Normal,
    Sleep,
    FastForward,
    Load
}

public enum BlackyTimerNotificationPolicy
{
    CatchUp,
    Skip
}

public enum BlackyTimerNotificationUnit
{
    Minute,
    Day,
    Month
}

public enum BlackyTimerNotificationType
{
    None = 0,
    
    NacimientoHuevo,
    
    ConstruccionCompleta
}

public enum BlackyDayPeriod
{
    Dawn,
    Morning,
    Noon,
    Afternoon,
    Dusk,
    Night
}

public sealed class BlackyTimerGame : IDisposable
{
    // =========================================================
    // CALENDAR CONSTANTS
    // =========================================================

    public const int MinutesPerHour = 60;

    public const int HoursPerDay = 24;

    public const int MinutesPerDay =
        MinutesPerHour * HoursPerDay;

    public const int DaysPerWeek = 7;

    public const int DaysPerMonth = 30;

    public const int MonthsPerYear = 12;

    public const int DaysPerYear =
        DaysPerMonth * MonthsPerYear;

    public const long MinutesPerWeek =
        MinutesPerDay * DaysPerWeek;

    public const long MinutesPerMonth =
        MinutesPerDay * DaysPerMonth;

    public const long MinutesPerYear =
        MinutesPerMonth * MonthsPerYear;

    // =========================================================
    // STATE
    // =========================================================

    private long totalGameMinutes;

    private double fractionalMinutes;

    private bool disposed;

    // =========================================================
    // SETTINGS
    // =========================================================

    /// <summary>
    /// Minutos de juego que transcurren por segundo real.
    /// </summary>
    public double MinutesPerSecond { get; set; } = 1.0;

    /// <summary>
    /// Multiplicador adicional de velocidad.
    /// </summary>
    public double TimeScale { get; set; } = 1.0;

    public bool IsPaused { get; private set; }

    // =========================================================
    // NOTIFICATIONS
    // =========================================================

    public BlackyTimerNotificationManager Notifications { get; }

    // =========================================================
    // CONSTRUCTOR
    // =========================================================

    public BlackyTimerGame(long initialGameMinutes = 0)
    {
        if (initialGameMinutes < 0)
            throw new ArgumentOutOfRangeException(
                nameof(initialGameMinutes));

        totalGameMinutes = initialGameMinutes;

        Notifications = new BlackyTimerNotificationManager(
            () => totalGameMinutes);
    }

    // =========================================================
    // ABSOLUTE TIME
    // =========================================================

    public long TotalGameMinutes
    {
        get
        {
            ThrowIfDisposed();
            return totalGameMinutes;
        }
    }

    public long TotalGameDays
    {
        get
        {
            ThrowIfDisposed();
            return totalGameMinutes / MinutesPerDay + 1;
        }
    }

    public long TotalGameHours
    {
        get
        {
            ThrowIfDisposed();
            return totalGameMinutes / MinutesPerHour;
        }
    }

    // =========================================================
    // DATE
    // =========================================================

    public long Year
    {
        get
        {
            ThrowIfDisposed();

            return totalGameMinutes / MinutesPerYear + 1;
        }
    }

    public int Month
    {
        get
        {
            ThrowIfDisposed();

            long minuteInYear =
                totalGameMinutes % MinutesPerYear;

            return (int)(
                minuteInYear / MinutesPerMonth) + 1;
        }
    }

    public int Day
    {
        get
        {
            ThrowIfDisposed();

            long minuteInMonth =
                totalGameMinutes % MinutesPerMonth;

            return (int)(
                minuteInMonth / MinutesPerDay) + 1;
        }
    }

    public int DayOfYear
    {
        get
        {
            ThrowIfDisposed();

            long minuteInYear =
                totalGameMinutes % MinutesPerYear;

            return (int)(
                minuteInYear / MinutesPerDay) + 1;
        }
    }

    public int DayOfWeek
    {
        get
        {
            ThrowIfDisposed();

            return (int)(
                (TotalGameDays - 1) % DaysPerWeek) + 1;
        }
    }

    // =========================================================
    // TIME
    // =========================================================

    public int Hour
    {
        get
        {
            ThrowIfDisposed();

            return (int)(
                (totalGameMinutes % MinutesPerDay)
                / MinutesPerHour);
        }
    }

    public int Minute
    {
        get
        {
            ThrowIfDisposed();

            return (int)(
                totalGameMinutes % MinutesPerHour);
        }
    }

    public BlackyDayPeriod DayPeriod
    {
        get
        {
            ThrowIfDisposed();

            if (Hour >= 5 && Hour < 7)
                return BlackyDayPeriod.Dawn;

            if (Hour >= 7 && Hour < 11)
                return BlackyDayPeriod.Morning;

            if (Hour >= 11 && Hour < 13)
                return BlackyDayPeriod.Noon;

            if (Hour >= 13 && Hour < 17)
                return BlackyDayPeriod.Afternoon;

            if (Hour >= 17 && Hour < 20)
                return BlackyDayPeriod.Dusk;

            return BlackyDayPeriod.Night;
        }
    }

    // =========================================================
    // PAUSE
    // =========================================================

    public void Pause()
    {
        ThrowIfDisposed();

        IsPaused = true;
    }

    public void Continue()
    {
        ThrowIfDisposed();

        IsPaused = false;
    }

    public void TogglePause()
    {
        ThrowIfDisposed();

        IsPaused = !IsPaused;
    }

    // =========================================================
    // AUTOMATIC UPDATE
    // =========================================================

    /// <summary>
    /// Avance automático del reloj.
    /// Se puede llamar desde el Update del juego.
    /// </summary>
    public void Update(double deltaSeconds)
    {
        ThrowIfDisposed();

        if (IsPaused)
            return;

        if (deltaSeconds <= 0)
            return;

        if (MinutesPerSecond < 0)
            throw new InvalidOperationException(
                "MinutesPerSecond no puede ser negativo.");

        if (TimeScale < 0)
            throw new InvalidOperationException(
                "TimeScale no puede ser negativo.");

        double elapsedMinutes =
            deltaSeconds *
            MinutesPerSecond *
            TimeScale;

        fractionalMinutes += elapsedMinutes;

        if (fractionalMinutes < 1.0)
            return;

        long wholeMinutes = (long)fractionalMinutes;

        fractionalMinutes -= wholeMinutes;

        AdvanceMinutes(
            wholeMinutes,
            true,
            BlackyTimerAdvanceMode.Normal,
            BlackyTimerNotificationPolicy.CatchUp);
    }

    // =========================================================
    // MANUAL ADVANCE
    // =========================================================

    public void AdvanceMinutes(
        long minutes,
        bool notify = true,
        BlackyTimerAdvanceMode mode =
            BlackyTimerAdvanceMode.Normal,
        BlackyTimerNotificationPolicy policy =
            BlackyTimerNotificationPolicy.CatchUp)
    {
        ThrowIfDisposed();

        if (minutes < 0)
            throw new ArgumentOutOfRangeException(nameof(minutes));

        if (minutes == 0)
            return;

        long destinationMinute = checked(
            totalGameMinutes + minutes);

        totalGameMinutes = destinationMinute;

        if (notify)
        {
            Notifications.Process(
                destinationMinute,
                mode);
        }
        else if (policy == BlackyTimerNotificationPolicy.Skip)
        {
            Notifications.ResynchronizeToCurrentTime();
        }

        // Con CatchUp no movemos el cursor.
        // Las notificaciones quedan pendientes para el siguiente
        // avance que tenga notify = true.
    }

    public void AdvanceHours(
        long hours,
        bool notify = true,
        BlackyTimerAdvanceMode mode =
            BlackyTimerAdvanceMode.Normal,
        BlackyTimerNotificationPolicy policy =
            BlackyTimerNotificationPolicy.CatchUp)
    {
        if (hours < 0)
            throw new ArgumentOutOfRangeException(nameof(hours));

        AdvanceMinutes(
            checked(hours * MinutesPerHour),
            notify,
            mode,
            policy);
    }

    public void AdvanceDays(
        long days,
        bool notify = true,
        BlackyTimerAdvanceMode mode =
            BlackyTimerAdvanceMode.Normal,
        BlackyTimerNotificationPolicy policy =
            BlackyTimerNotificationPolicy.CatchUp)
    {
        if (days < 0)
            throw new ArgumentOutOfRangeException(nameof(days));

        AdvanceMinutes(
            checked(days * MinutesPerDay),
            notify,
            mode,
            policy);
    }

    public void AdvanceMonths(
        long months,
        bool notify = true,
        BlackyTimerAdvanceMode mode =
            BlackyTimerAdvanceMode.Normal,
        BlackyTimerNotificationPolicy policy =
            BlackyTimerNotificationPolicy.CatchUp)
    {
        if (months < 0)
            throw new ArgumentOutOfRangeException(nameof(months));

        AdvanceMinutes(
            checked(months * MinutesPerMonth),
            notify,
            mode,
            policy);
    }

    public void AdvanceYears(
        long years,
        bool notify = true,
        BlackyTimerAdvanceMode mode =
            BlackyTimerAdvanceMode.Normal,
        BlackyTimerNotificationPolicy policy =
            BlackyTimerNotificationPolicy.CatchUp)
    {
        if (years < 0)
            throw new ArgumentOutOfRangeException(nameof(years));

        AdvanceMinutes(
            checked(years * MinutesPerYear),
            notify,
            mode,
            policy);
    }

    // =========================================================
    // DISPATCH PENDING
    // =========================================================

    /// <summary>
    /// Procesa las notificaciones pendientes sin avanzar el reloj.
    ///
    /// Útil cuando se utilizó notify:false con CatchUp.
    /// </summary>
    public void DispatchPendingNotifications(
        BlackyTimerAdvanceMode mode =
            BlackyTimerAdvanceMode.Normal)
    {
        ThrowIfDisposed();

        Notifications.Process(
            totalGameMinutes,
            mode);
    }

    // =========================================================
    // SET DATE
    // =========================================================

    /// <summary>
    /// Establece una fecha absoluta.
    ///
    /// Las notificaciones históricas se descartan.
    /// Las suscripciones y programaciones futuras se conservan.
    /// </summary>
    public void SetDateTime(
        long year,
        int month,
        int day,
        int hour,
        int minute)
    {
        ThrowIfDisposed();

        if (year < 1)
            throw new ArgumentOutOfRangeException(nameof(year));

        if (month < 1 || month > MonthsPerYear)
            throw new ArgumentOutOfRangeException(nameof(month));

        if (day < 1 || day > DaysPerMonth)
            throw new ArgumentOutOfRangeException(nameof(day));

        if (hour < 0 || hour >= HoursPerDay)
            throw new ArgumentOutOfRangeException(nameof(hour));

        if (minute < 0 || minute >= MinutesPerHour)
            throw new ArgumentOutOfRangeException(nameof(minute));

        long total = checked(
            (year - 1) * MinutesPerYear);

        total = checked(
            total + (month - 1) * MinutesPerMonth);

        total = checked(
            total + (day - 1) * MinutesPerDay);

        total = checked(
            total + hour * MinutesPerHour);

        total = checked(total + minute);

        totalGameMinutes = total;

        fractionalMinutes = 0;

        Notifications.ResynchronizeToCurrentTime();
    }

    // =========================================================
    // RESET FRACTIONAL TIME
    // =========================================================

    public void ResetFractionalTime()
    {
        ThrowIfDisposed();

        fractionalMinutes = 0;
    }

    // =========================================================
    // DISPOSE
    // =========================================================

    private void ThrowIfDisposed()
    {
        if (disposed)
            throw new ObjectDisposedException(
                nameof(BlackyTimerGame));
    }

    public void Dispose()
    {
        if (disposed)
            return;

        Notifications.Dispose();

        disposed = true;
    }
}