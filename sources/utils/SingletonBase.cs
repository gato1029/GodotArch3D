using System;

public class SingletonBase<T> where T : class, new()
{
    private static readonly Lazy<T> _instance = new Lazy<T>(() =>
    {
        T instance = new T();
        (instance as SingletonBase<T>)?.Initialize();
        return instance;
    });

    public static T Instance => _instance.Value;

    // Constructor protegido para evitar la creacion de instancias fuera de la clase
    protected SingletonBase()
    {
    }

    // Metodo de inicializacion, puede ser sobrescrito por las clases derivadas
    protected virtual void Initialize()
    {
    }    
    protected virtual void Destroy()
    {
    }
}
