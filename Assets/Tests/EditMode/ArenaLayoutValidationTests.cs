using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class ArenaLayoutValidationTests
{
    private List<ArenaLayoutSO> LoadAllLayouts()
    {
        List<ArenaLayoutSO> layouts = new List<ArenaLayoutSO>();
        string[] guids = AssetDatabase.FindAssets("t:ArenaLayoutSO");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ArenaLayoutSO layout = AssetDatabase.LoadAssetAtPath<ArenaLayoutSO>(path);
            if (layout != null)
            {
                layouts.Add(layout);
            }
        }

        return layouts;
    }

    [Test]
    public void AllArenaLayouts_KeepCenterSafeZoneClear()
    {
        List<ArenaLayoutSO> layouts = LoadAllLayouts();
        Assert.GreaterOrEqual(layouts.Count, 4, "Expected at least 4 ArenaLayoutSO assets in project.");

        foreach (ArenaLayoutSO layout in layouts)
        {
            foreach (Vector2Int cell in layout.obstacleCells)
            {
                bool inSafeZone = Mathf.Abs(cell.x) <= 3 && Mathf.Abs(cell.y) <= 3;
                Assert.IsFalse(inSafeZone, $"{layout.name}: obstacle at {cell} overlaps center safe zone!");
            }

            foreach (ArenaPropPlacement prop in layout.props)
            {
                bool inSafeZone = Mathf.Abs(prop.cell.x) <= 3 && Mathf.Abs(prop.cell.y) <= 3;
                Assert.IsFalse(inSafeZone, $"{layout.name}: prop {prop.type} at {prop.cell} overlaps center safe zone!");

            }
        }
    }

    [Test]
    public void AllArenaLayouts_StayWithinPlayableBounds()
    {
        List<ArenaLayoutSO> layouts = LoadAllLayouts();
        int halfW = ArenaGenerator.PlayableWidth / 2;
        int halfH = ArenaGenerator.PlayableHeight / 2;

        foreach (ArenaLayoutSO layout in layouts)
        {
            AssertCellsInBounds(layout.name, "obstacleCells", layout.obstacleCells, halfW, halfH);
            AssertCellsInBounds(layout.name, "torchCells", layout.torchCells, halfW, halfH);
            AssertCellsInBounds(layout.name, "pavedCells", layout.pavedCells, halfW, halfH);
            AssertCellsInBounds(layout.name, "railCells", layout.railCells, halfW, halfH);

            foreach (ArenaPropPlacement prop in layout.props)
            {
                bool inBounds = prop.cell.x >= -halfW && prop.cell.x < halfW && prop.cell.y >= -halfH && prop.cell.y < halfH;
                Assert.IsTrue(inBounds, $"{layout.name}: prop {prop.type} at {prop.cell} is out of playable bounds!");
            }
        }
    }

    private void AssertCellsInBounds(string layoutName, string listName, List<Vector2Int> cells, int halfW, int halfH)
    {
        foreach (Vector2Int cell in cells)
        {
            bool inBounds = cell.x >= -halfW && cell.x < halfW && cell.y >= -halfH && cell.y < halfH;
            Assert.IsTrue(inBounds, $"{layoutName}: {listName} cell {cell} is out of playable bounds!");
        }
    }

    [Test]
    public void GetMirroredCell_FlipsCoordinatesBySigns()
    {
        ArenaLayoutSO tempLayout = ScriptableObject.CreateInstance<ArenaLayoutSO>();
        try
        {
            Vector2Int original = new Vector2Int(5, -3);

            Assert.AreEqual(new Vector2Int(-5, -3), tempLayout.GetMirroredCell(original, -1, 1));
            Assert.AreEqual(new Vector2Int(5, 3), tempLayout.GetMirroredCell(original, 1, -1));
            Assert.AreEqual(new Vector2Int(-5, 3), tempLayout.GetMirroredCell(original, -1, -1));

        }
        finally
        {
            Object.DestroyImmediate(tempLayout);
        }
    }
}