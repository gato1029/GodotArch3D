using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.TerrainBase;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.WorldGameProfile;

using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.managers.Mods;
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
    /// <summary>Niveles de altura posibles: 0 agua, 1 costa, 2..4 tierra.</summary>
    public const int NivelesTotales = 5;

    public PerfilGeografico Perfil { get; }
    public float Frecuencia { get; }
    public TipoForma Forma { get; }
    public float FormaPeso { get; }
    public float Sinuosidad { get; }

    /// <summary>Nivel máximo usado (1..4). Los niveles por encima deben tener proporción 0.</summary>
    public int AlturaMaxima { get; }

    public float Altura0 { get; }   // agua
    public float Altura1 { get; }   // costa
    public float Altura2 { get; }
    public float Altura3 { get; }
    public float Altura4 { get; }

    public ConfiguracionGeograficaData(PerfilGeografico perfil)
    {
        Perfil = perfil;

        switch (perfil)
        {
            case PerfilGeografico.Montanosa:
                Frecuencia = 0.008f;
                Forma = TipoForma.Ninguna; FormaPeso = 0.00f; Sinuosidad = 0.00f;
                AlturaMaxima = 4;
                Altura0 = 0.05f; Altura1 = 0.05f; Altura2 = 0.20f; Altura3 = 0.30f; Altura4 = 0.40f;
                break;

            case PerfilGeografico.Llanura:
                Frecuencia = 0.005f;
                Forma = TipoForma.Ninguna; FormaPeso = 0.00f; Sinuosidad = 0.00f;
                AlturaMaxima = 3;
                Altura0 = 0.05f; Altura1 = 0.05f; Altura2 = 0.75f; Altura3 = 0.15f; Altura4 = 0.00f;
                break;

            case PerfilGeografico.Valle:
                Frecuencia = 0.010f;
                Forma = TipoForma.Valle; FormaPeso = 0.70f; Sinuosidad = 0.25f;
                AlturaMaxima = 4;
                Altura0 = 0.08f; Altura1 = 0.07f; Altura2 = 0.35f; Altura3 = 0.30f; Altura4 = 0.20f;
                break;

            case PerfilGeografico.Meseta:
                Frecuencia = 0.006f;
                Forma = TipoForma.Meseta; FormaPeso = 0.60f; Sinuosidad = 0.00f;
                AlturaMaxima = 4;
                Altura0 = 0.03f; Altura1 = 0.04f; Altura2 = 0.08f; Altura3 = 0.60f; Altura4 = 0.25f;
                break;

            case PerfilGeografico.Cuenca:
                Frecuencia = 0.007f;
                Forma = TipoForma.Cuenca; FormaPeso = 0.65f; Sinuosidad = 0.00f;
                AlturaMaxima = 4;
                Altura0 = 0.15f; Altura1 = 0.08f; Altura2 = 0.40f; Altura3 = 0.25f; Altura4 = 0.12f;
                break;

            case PerfilGeografico.Costera:
                Frecuencia = 0.006f;
                Forma = TipoForma.Costera; FormaPeso = 0.45f; Sinuosidad = 0.00f;
                AlturaMaxima = 3;
                Altura0 = 0.25f; Altura1 = 0.08f; Altura2 = 0.50f; Altura3 = 0.17f; Altura4 = 0.00f;
                break;

            case PerfilGeografico.Escarpada:
                Frecuencia = 0.012f;
                Forma = TipoForma.Ninguna; FormaPeso = 0.00f; Sinuosidad = 0.00f;
                AlturaMaxima = 4;
                Altura0 = 0.10f; Altura1 = 0.05f; Altura2 = 0.15f; Altura3 = 0.20f; Altura4 = 0.50f;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(perfil), perfil, null);
        }

        float[] a = { Altura0, Altura1, Altura2, Altura3, Altura4 };

        float suma = 0f;
        foreach (float v in a) suma += v;
        if (MathF.Abs(suma - 1f) > 0.001f)
            throw new InvalidOperationException($"{perfil}: Altura0..4 suman {suma}, deben sumar 1.");

        if (AlturaMaxima < 1 || AlturaMaxima >= NivelesTotales)
            throw new InvalidOperationException($"{perfil}: AlturaMaxima {AlturaMaxima} fuera de 1..{NivelesTotales - 1}.");

        for (int i = AlturaMaxima + 1; i < NivelesTotales; i++)
            if (a[i] > 0f)
                throw new InvalidOperationException($"{perfil}: Altura{i} tiene peso pero AlturaMaxima es {AlturaMaxima}.");

        if (a[AlturaMaxima] <= 0f)
            throw new InvalidOperationException($"{perfil}: AlturaMaxima {AlturaMaxima} no tiene peso asignado.");
    }
}
public class BiomePlacementRuleData
{
    public long idBioma { get; set; }
    public bool isLowerBase { get; set; }
    public float alturaMinima { get; set; } 
    public float alturaMaxima { get; set; }

    public float peso { get; set; } = 1f;

    [BsonIgnore]
    public BiomaData bioma { get; set; }

    public BiomePlacementRuleData()
    {
        
    }
    [BsonCtor]
    public BiomePlacementRuleData(long idBioma)
    {
        bioma = AtlasModsManager.GetDirect<BiomaData>(idBioma);
    }
}

public class TemplateWorldProfileData : IdDataLong
{
    // recuerda esto solo es un template un perfilador, el mundo podria cambiar realmente por la semilla XD
    public BlackyWorldTypeDetail tipoMundo { get; set; }

    public PerfilGeografico perfilGeografico { get; set; }

    public List<BiomePlacementRuleData> reglasBiomas { get; set; } = new();

    public int IdMundo { get; set; } // lo usaremos para identificar que mundos paso, por el momento nuestro mundo es de 24 mundos

    public TemplateWorldProfileData()
    {
        id = EpochIdGenerator.NewId();
    }
}
