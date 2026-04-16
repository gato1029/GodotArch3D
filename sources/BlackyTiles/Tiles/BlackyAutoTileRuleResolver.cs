

using Godot;
using GodotEcsArch.sources.WindowsDataBase.TileSprite;
using GodotFlecs.sources.KuroTiles;
using System;
using System.Collections.Generic;
using System.Linq;
namespace GodotEcsArch.sources.BlackyTiles.Tiles;


public sealed class BlackyAutoTileRuleResolver
{
    private struct BlackyAutoTileCell
    {
        public bool HasTile;
        public long IdTile;
        public int IdAgrupador;
    }

    private readonly BlackyAutoTileDataProvider provider;

    private static readonly (int x, int y)[] NeighborOffsets =
    {
        (0, 1),    // Arriba
        (1, 0),    // Derecha
        (0, -1),   // Abajo
        (-1, 0),   // Izquierda
        (1, 1),    // ArribaDerecha
        (1, -1),   // AbajoDerecha
        (-1, -1),  // AbajoIzquierda
        (-1, 1)    // ArribaIzquierda
    };

    public BlackyAutoTileRuleResolver(
        BlackyAutoTileDataProvider provider)
    {
        this.provider = provider;
    }

    #region ===== PUBLIC API =====

    public void ResolveSingle(int altura,
        int layer,
        Vector2I worldPos,
        AutoTileSpriteData rule)
    {
        if (!provider.TryGetTileData(altura,
            layer,
            worldPos.X,
            worldPos.Y,
            out _,
            out _))
            return;

        var env = BuildEnvironment(altura,layer, worldPos);

        ApplyFromEnvironment(altura,layer, worldPos, env, rule);
    }

    public void ResolveRegion(int altura,
    int layer,
    List<Vector2I> positions,
    AutoTileSpriteData rule)
    {
        if (positions == null || positions.Count == 0)
            return;

        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        foreach (var pos in positions)
        {
            if (pos.X < minX) minX = pos.X;
            if (pos.Y < minY) minY = pos.Y;
            if (pos.X > maxX) maxX = pos.X;
            if (pos.Y > maxY) maxY = pos.Y;
        }

        minX -= 1;
        minY -= 1;
        maxX += 1;
        maxY += 1;

        BuildAndResolveRegion(altura,layer, minX, minY, maxX, maxY, rule);
    }
    #endregion

    #region ===== INTERNAL =====

    private void BuildAndResolveRegion(int altura,
    int layer,
    int minX,
    int minY,
    int maxX,
    int maxY,
    AutoTileSpriteData rule)
    {
        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        var buffer = new BlackyAutoTileCell[width, height];

        // 🔥 UNA SOLA PASADA AL PROVIDER
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int worldX = minX + x;
                int worldY = minY + y;

                if (provider.TryGetTileData(altura,
                    layer,
                    worldX,
                    worldY,
                    out long idTile,
                    out int idAgrupador))
                {
                    buffer[x, y].HasTile = true;
                    buffer[x, y].IdTile = idTile;
                    buffer[x, y].IdAgrupador = idAgrupador;
                }
            }
        }

        ResolveBuffer(altura,layer, buffer, minX, minY, width, height, rule);
    }
    private void ResolveBuffer(int altura,
    int layer,
    BlackyAutoTileCell[,] buffer,
    int minX,
    int minY,
    int width,
    int height,
    AutoTileSpriteData rule)
    {
        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                var center = buffer[x, y];

                if (!center.HasTile)
                    continue;

                TileEnvironment env = new();

                for (int i = 0; i < 8; i++)
                {
                    var offset = NeighborOffsets[i];
                    var neighbor = buffer[x + offset.x, y + offset.y];

                    if (neighbor.HasTile)
                    {
                        env.Set(
                            (NeighborPosition)i,
                            neighbor.IdTile,
                            neighbor.IdAgrupador);
                    }
                }
                int worldX =  x;
                int worldY =  y;
                // 🔥 TILE DEBAJO
                if (layer > 0 && provider.TryGetTileData(altura,
                    layer - 1,
                    worldX,
                    worldY,
                    out long idTileUnder,
                    out int idAgrupadorUnder))
                {
                    env.SetUnderCenter(idTileUnder, idAgrupadorUnder);
                }
                else
                {
                    env.SetUnderCenter(0, 0);
                }

                ApplyFromEnvironment(altura,
                    layer,
                    new Vector2I(minX + x, minY + y),
                    env,
                    rule);
            }
        }
    }
    private TileEnvironment BuildEnvironment(int altura,
        int layer,
        Vector2I worldPos)
    {
        TileEnvironment env = new();

        for (int i = 0; i < 8; i++)
        {
            var offset = NeighborOffsets[i];

            if (provider.TryGetTileData(altura,
                layer,
                worldPos.X + offset.x,
                worldPos.Y + offset.y,
                out long idTile,
                out int idAgrupador))
            {
                env.Set(
                    (NeighborPosition)i,
                    idTile,
                    idAgrupador);
            }
            else
            {
                env.Set((NeighborPosition)i, 0, 0);
            }
        }
        if (layer > 0 && provider.TryGetTileData(altura,layer-1, worldPos.X, worldPos.Y, out long idTileUnder, out int idAgrupadorUnder))
        {
            env.SetUnderCenter(idTileUnder, idAgrupadorUnder);
        }
        else
        {
            env.SetUnderCenter(0, 0);
        }

        
        return env;
    }

    private void ApplyFromEnvironment(int altura,
        int layer,
        Vector2I worldPos,
        TileEnvironment env,
        AutoTileSpriteData rule)
    {
        TileRuleTemplate bestRule = null;

        foreach (var template in rule.tileRuleTemplates)
        {
            if (template.MatchesEnvironment(env))
            {
                bestRule = template;
                break;
            }
        }

        if (bestRule == null)
        {
            provider.ApplyTileTemplate(altura,
            layer,
            worldPos.X,
            worldPos.Y,
            null, true);
            // No se encontró ninguna regla que coincida con el entorno, no se aplica ningún cambio eliminando tile si es necesario

            return;
        }


        //bestRule.neighborConditionTemplateCenter.UnderNeighborType
        TileTemplate templateToApply;

        if (bestRule.IsRandomTiles)
        {
            int index = (int)(GD.Randi() % bestRule.RandomTiles.Count);
            templateToApply = bestRule.RandomTiles[index];
        }
        else
        {
            templateToApply = bestRule.TileCentral;
        }

        //if (provider.TryGetTileData(altura, layer, worldPos.X, worldPos.Y, out long currentTileId, out _))
        //{
        //    if (currentTileId == templateToApply.idTileSprite)
        //    {
        //        return; // El tile actual ya es el correcto, no se aplica ningún cambio
        //    }
        //}

        provider.ApplyTileTemplate(altura,
        layer,
        worldPos.X,
        worldPos.Y,
        templateToApply);        
    }

    #endregion
}