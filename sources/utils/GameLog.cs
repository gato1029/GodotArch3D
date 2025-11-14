using System;
using System.Collections.Generic;
using Godot;

namespace GodotEcsArch.sources.utils
{
    public sealed class GameLog
    {
        private static GameLog _instance;
        public static GameLog Instance => _instance ??= new GameLog();

        private GameLogConsole _console;
        private readonly Queue<string> _buffer = new();
        private int _maxLines = 200; // buffer interno

        private GameLog() { }

        private void EnsureConsole()
        {
            if (_console != null) return;

            // Mostrar la ventana si no existe
            GameLogConsole.ShowWindow();

            // Vincular la instancia creada al GameLog
            _console = GameLogConsole.Instance;

        }

        public void Log(string message)
        {
            string formatted = $"[{DateTime.Now:HH:mm:ss}] {message}";

            EnsureConsole();

            if (_console != null)
            {
                _console.AddLine(formatted);
            }
            else
            {
                // Buffer temporal si la consola no se puede crear todavía
                _buffer.Enqueue(formatted);
                if (_buffer.Count > _maxLines)
                    _buffer.Dequeue();
            }

            GD.Print(formatted);
        }

        public void AttachBufferedLines()
        {
            if (_console != null)
            {
                foreach (var line in _buffer)
                    _console.AddLine(line);
                _buffer.Clear();
            }
        }

        public static void LogCat(string message) => Instance.Log(message);
    }
}
