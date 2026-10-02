using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.TerrainBase;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.WorldGameProfile;

using GodotEcsArch.sources.WindowsDataBase.Biomas;
using System;

public enum PerfilGeografico
{
    // Grandes elevaciones y desniveles; predominan pendientes, montañas y zonas altas.
    Montanosa,

    // Terreno predominantemente bajo y relativamente plano, con pocas variaciones de altura.
    Llanura,

    // Zona baja alargada entre áreas elevadas; las alturas aumentan progresivamente hacia los laterales.
    Valle,

    // Superficie extensa y relativamente plana situada a una altura elevada respecto a su entorno.
    Meseta,

    // Terreno hundido o rodeado por zonas más elevadas; concentra las áreas de menor altura.
    Cuenca,

    // Terreno cercano a una masa de agua, generalmente con transición entre zonas terrestres bajas y agua.
    Costera,

    // Relieve abrupto con cambios de altura fuertes y frecuentes; predominan pendientes pronunciadas y desniveles.
    Escarpada
}

/// <summary>
/// Estructura que se le suma al ruido para darle al terreno una forma que el ruido solo no produce.
/// </summary>
public enum TipoForma
{
    /// <summary>Solo ruido (manchas de relieve sin estructura global).</summary>
    Ninguna,
    /// <summary>Franja baja alargada (eje serpenteante) que sube hacia los laterales.</summary>
    Valle,
    /// <summary>Hueco central rodeado de zonas más altas.</summary>
    Cuenca,
    /// <summary>Un lado del mapa es agua (nivel bajo) y sube gradualmente hacia tierra adentro.</summary>
    Costera,
    /// <summary>Zona central elevada y amplia que cae hacia los bordes.</summary>
    Meseta
}

public struct ConfiguracionGeograficaData
{
    public PerfilGeografico Perfil { get; }

    // ---- Ruido ----
    /// <summary>Ciclos de relieve por celda (0.008 => un ciclo cada ~125 celdas).</summary>
    public float Frecuencia { get; }

    // ---- Forma ----
    public TipoForma Forma { get; }

    /// <summary>Peso de la forma frente al ruido: 0 = solo ruido, 1 = solo forma.</summary>
    public float FormaPeso { get; }

    /// <summary>Solo Valle: cuánto serpentea el eje, como fracción del ancho del mapa (0 = recto).</summary>
    public float Sinuosidad { get; }

    // ---- Distribución de alturas (proporciones del mundo completo; deben sumar 1) ----
    public int AlturaMaxima { get; }

    public float Altura0 { get; }
    public float Altura1 { get; }
    public float Altura2 { get; }
    public float Altura3 { get; }

    public ConfiguracionGeograficaData(PerfilGeografico perfil)
    {
        Perfil = perfil;

        switch (perfil)
        {
            case PerfilGeografico.Montanosa:
                Frecuencia = 0.008f;
                Forma = TipoForma.Ninguna; FormaPeso = 0.00f; Sinuosidad = 0.00f;
                AlturaMaxima = 3;
                Altura0 = 0.10f; Altura1 = 0.20f; Altura2 = 0.30f; Altura3 = 0.40f;
                break;

            case PerfilGeografico.Llanura:
                Frecuencia = 0.005f;
                Forma = TipoForma.Ninguna; FormaPeso = 0.00f; Sinuosidad = 0.00f;
                AlturaMaxima = 2;
                Altura0 = 0.05f; Altura1 = 0.80f; Altura2 = 0.15f; Altura3 = 0.00f;
                break;

            case PerfilGeografico.Valle:
                Frecuencia = 0.010f;
                Forma = TipoForma.Valle; FormaPeso = 0.70f; Sinuosidad = 0.25f;
                AlturaMaxima = 3;
                Altura0 = 0.10f; Altura1 = 0.45f; Altura2 = 0.30f; Altura3 = 0.15f;
                break;

            case PerfilGeografico.Meseta:
                Frecuencia = 0.006f;
                Forma = TipoForma.Meseta; FormaPeso = 0.60f; Sinuosidad = 0.00f;
                AlturaMaxima = 3;
                Altura0 = 0.05f; Altura1 = 0.10f; Altura2 = 0.70f; Altura3 = 0.15f;
                break;

            case PerfilGeografico.Cuenca:
                Frecuencia = 0.007f;
                Forma = TipoForma.Cuenca; FormaPeso = 0.65f; Sinuosidad = 0.00f;
                AlturaMaxima = 3;
                Altura0 = 0.20f; Altura1 = 0.50f; Altura2 = 0.20f; Altura3 = 0.10f;
                break;

            case PerfilGeografico.Costera:
                Frecuencia = 0.006f;
                Forma = TipoForma.Costera; FormaPeso = 0.45f; Sinuosidad = 0.00f;
                AlturaMaxima = 2;
                Altura0 = 0.25f; Altura1 = 0.60f; Altura2 = 0.15f; Altura3 = 0.00f;
                break;

            case PerfilGeografico.Escarpada:
                Frecuencia = 0.012f;
                Forma = TipoForma.Ninguna; FormaPeso = 0.00f; Sinuosidad = 0.00f;
                AlturaMaxima = 3;
                // Extremos fuertes y centro delgado: saltos cortos entre bajo y alto = relieve abrupto.
                Altura0 = 0.20f; Altura1 = 0.15f; Altura2 = 0.15f; Altura3 = 0.50f;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(perfil), perfil, null);
        }

        // Validación: detecta errores de datos al construir el perfil.
        float suma = Altura0 + Altura1 + Altura2 + Altura3;
        if (MathF.Abs(suma - 1f) > 0.001f)
            throw new InvalidOperationException($"{perfil}: Altura0..3 suman {suma}, deben sumar 1.");

        float[] a = { Altura0, Altura1, Altura2, Altura3 };
        for (int i = 0; i < 4; i++)
        {
            bool fueraDeRango = i > AlturaMaxima && a[i] > 0f;
            if (fueraDeRango)
                throw new InvalidOperationException($"{perfil}: Altura{i} tiene peso pero AlturaMaxima es {AlturaMaxima}.");
        }
        if (a[AlturaMaxima] <= 0f)
            throw new InvalidOperationException($"{perfil}: AlturaMaxima {AlturaMaxima} no tiene peso asignado.");
    }
}
public class BiomePlacementRuleData
{
    public BiomaData bioma { get; set; }

    public float alturaMinima { get; set; } // Por ejemplo, altura normalizada entre 0 y 1
    public float alturaMaxima { get; set; }

    public float peso { get; set; } = 1f;
}

public class TemplateWorldProfileData : IdDataLong
{
    public ConfiguracionGeograficaData configuracionGeografica { get; set; }

    public List<BiomePlacementRuleData> reglasBiomas { get; set; } = new();

    public TemplateWorldProfileData()
    {
        id = EpochIdGenerator.NewId();
    }
}
