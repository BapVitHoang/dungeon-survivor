using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewArenaLayout", menuName = "Arena/Layout")]
public class ArenaLayoutSO :ScriptableObject
{
    public string layoutName;

    [Header("Grid Cells (Playable Area:[-18..17], Y [-12..11]) ")]
    public List<Vector2Int> obstacleCells = new List<Vector2Int>();
    public List<Vector2Int> torchCells = new List<Vector2Int>();

    [Header("Symmetry Variations")]
    public bool allowMirrorX = true;
    public bool allowMirrorY = true;

    public Vector2Int GetMirroredCell(Vector2Int cell, int signX, int signY)
    {
        return new Vector2Int(cell.x * signX, cell.y * signY);
    }
}
