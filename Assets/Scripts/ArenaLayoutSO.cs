using System;
using System.Collections.Generic;
using UnityEngine;
public enum PropType
{
    Crate,
    Barrel,
    Chest,
    CrossGrave,
    StoneGrave,
    IronFence,
    Anvil,
    Table,
    Minecart
}
[Serializable]
public struct ArenaPropPlacement
{
    public Vector2Int cell;
    public PropType type;
}

[CreateAssetMenu(fileName = "NewArenaLayout", menuName = "Arena/Layout")]
public class ArenaLayoutSO :ScriptableObject
{
    public string layoutName;

    [Header("Grid Cells (Playable Area:[-18..17], Y [-12..11]) ")]
    public List<Vector2Int> obstacleCells = new List<Vector2Int>();
    public List<Vector2Int> torchCells = new List<Vector2Int>();

    [Header("2.5D Landmarks & Solid Props")]
    public List<Vector2Int> pavedCells = new List<Vector2Int>();
    public List<Vector2Int> railCells = new List<Vector2Int>();
    public List<ArenaPropPlacement> props = new List<ArenaPropPlacement>();

    [Header("Symmetry Variations")]
    public bool allowMirrorX = true;
    public bool allowMirrorY = true;

    public Vector2Int GetMirroredCell(Vector2Int cell, int signX, int signY)
    {
        return new Vector2Int(cell.x * signX, cell.y * signY);
    }
}
