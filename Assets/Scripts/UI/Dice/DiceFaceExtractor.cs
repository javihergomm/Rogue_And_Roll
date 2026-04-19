using UnityEngine;
using System.Collections.Generic;

public static class DiceFaceExtractor
{
    // Define filas y columnas por dado
    private static readonly Dictionary<int, (int rows, int cols)> layout =
        new()
        {
            { 4, (2, 2) }, // D4
            { 6, (3, 2) }, // D6
            // Cuando tengas más:
            // { 8, (X, Y) },
            // { 10, (X, Y) },
            // { 12, (X, Y) },
            // { 20, (X, Y) },
        };

    public static Sprite GetFace(int maxRoll, int face)
    {
        // Cargar imagen del dado correspondiente
        var sheet = Resources.Load<Sprite>($"Sprites/Dado {maxRoll} 2D");
        if (sheet == null)
            return null;

        Texture2D tex = sheet.texture;

        if (!layout.TryGetValue(maxRoll, out var cfg))
            return null;

        int rows = cfg.rows;
        int cols = cfg.cols;

        // Convertir número de cara a índice de rejilla
        int index = face - 1;
        int row = index / cols;
        int col = index % cols;

        // Tamaño de celda
        int cellWidth = tex.width / cols;
        int cellHeight = tex.height / rows;

        // Y invertida
        int x = col * cellWidth;
        int y = tex.height - ((row + 1) * cellHeight);

        Rect rect = new(x, y, cellWidth, cellHeight);

        return Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), sheet.pixelsPerUnit);
    }
}
