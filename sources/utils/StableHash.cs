using System;
using System.Text;

public static class StableHash
{
    /// <summary>
    /// Genera un hash FNV-1a de 32 bits desde un string. Estable entre ejecuciones.
    /// </summary>
    public static int FromString(string input)
    {
        const uint fnvPrime = 0x01000193;    // 16777619
        const uint offsetBasis = 0x811C9DC5; // 2166136261

        uint hash = offsetBasis;

        byte[] data = Encoding.UTF8.GetBytes(input);
        foreach (byte b in data)
        {
            hash ^= b;
            hash *= fnvPrime;
        }

        return unchecked((int)hash); // permite overflow a valor negativo
    }
}
