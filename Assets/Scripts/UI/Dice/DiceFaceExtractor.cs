using UnityEngine;
using System.Collections.Generic;

public static class DiceFaceExtractor
{
    // Defines how many rows and columns each dice sheet has.
    // Key: max face value (4, 6, 8, 20, etc.)
    private static readonly Dictionary<int, (int rows, int cols)> layout =
        new Dictionary<int, (int rows, int cols)>()
        {
            { 4, (2, 2) },  // D4: 2 rows, 2 columns
            { 6, (3, 2) },  // D6: 3 rows, 2 columns
            { 8, (2, 4) },  // D8: 2 rows, 4 columns
            { 20, (4, 5) }, // D20: 4 rows, 5 columns 
        };

    public static Sprite GetFace(int maxRoll, int face)
    {
        // Load the sprite sheet for this dice type.
        // Expected file name: "Resources/Sprites/Dado {maxRoll} 2D.png"
        Sprite sheet = Resources.Load<Sprite>("Sprites/Dado " + maxRoll + " 2D");
        if (sheet == null)
            return null;

        Texture2D tex = sheet.texture;

        // Check if we have a layout for this dice type.
        if (!layout.TryGetValue(maxRoll, out var cfg))
            return null;

        int rows = cfg.rows;
        int cols = cfg.cols;

        // Convert face number (1-based) to grid index (0-based).
        int index = face - 1;
        int row = index / cols;
        int col = index % cols;

        // Compute cell size.
        int cellWidth = tex.width / cols;
        int cellHeight = tex.height / rows;

        // Unity's texture Y-axis is inverted.
        int x = col * cellWidth;
        int y = tex.height - ((row + 1) * cellHeight);

        Rect rect = new Rect(x, y, cellWidth, cellHeight);

        // Create and return the sprite.
        return Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), sheet.pixelsPerUnit);
    }
}
