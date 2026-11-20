using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class ArenaGenerator : MonoBehaviour
{
    public const int PlayableWidth = 36;
    public const int PlayableHeight = 24;
    public const int WallThickness = 3;

    [Header("Data Pools")]
    [SerializeField] private ArenaLayoutSO[] layouts;
    [SerializeField] private ArenaThemeSO[] themes;

    [Header("Tilemap Layers")]
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap decorTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap obstacleTilemap;

    [Header("Lighting & Props")]
    [SerializeField] private Light2D globalLight;
    [SerializeField] private GameObject torchPrefab;
    [SerializeField] private Transform propsRoot;

    [Header("Walkability Check")]
    [SerializeField] private LayerMask obstacleLayerMask;

    public Bounds PlayableBounds { get; private set; }

    private void Awake()
    {
        GenerateArena();
    }

    public void GenerateArena()
    {
        PlayableBounds = new Bounds(Vector3Int.zero, new Vector3(PlayableWidth, PlayableHeight, 0f));

        if (layouts == null || layouts.Length == 0 || themes == null || themes.Length == 0)
        {
            Debug.LogWarning("ArenaGenerator: Missing layouts or themes!");
            return;
        }
        ArenaLayoutSO layout = layouts[Random.Range(0, layouts.Length)];
        ArenaThemeSO theme = themes[Random.Range(0, themes.Length)];
        int signX = (layout.allowMirrorX && Random.value < 0.5f) ? -1 : 1;
        int signY = (layout.allowMirrorY && Random.value < 0.5f) ? -1 : 1;

        floorTilemap?.ClearAllTiles();
        decorTilemap?.ClearAllTiles();
        wallTilemap?.ClearAllTiles();
        obstacleTilemap?.ClearAllTiles();
        ClearProps();

        int halfW = PlayableWidth / 2;
        int halfH = PlayableHeight / 2;

        for (int x = -halfW - WallThickness; x < halfW + WallThickness; x++)
        {
            for (int y = -halfH - WallThickness; y < halfH + WallThickness; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                floorTilemap.SetTile(pos, theme.GetRandomFloorTile());
                bool isWall = (x < -halfW || x >= halfW || y < -halfH || y >= halfH);
                if (isWall)
                {
                    wallTilemap.SetTile(pos, theme.wallTile);
                }
            }
        }
        if (floorTilemap != null) floorTilemap.color = theme.floorTint;
        if (globalLight != null)
        {
            globalLight.color = theme.ambientLightColor;
            globalLight.intensity = theme.ambientIntensity;
        }

        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
        foreach (Vector2Int cell in layout.obstacleCells)
        {
            Vector2Int mirrored = layout.GetMirroredCell(cell, signX, signY);
            obstacleTilemap.SetTile(new Vector3Int(mirrored.x, mirrored.y, 0), theme.obstacleTile);
            occupied.Add(mirrored);
        }

        for (int x = -halfW; x < halfW; x++)
        {
            for (int y = -halfH; y < halfH; y++)
            {
                if (Mathf.Abs(x) <= 3 && Mathf.Abs(y) <= 3) continue;

                if (occupied.Contains(new Vector2Int(x, y))) continue;

                if (Random.value < theme.decorChance)
                {
                    TileBase decor = theme.GetRandomDecorTile();
                    if (decor != null) decorTilemap.SetTile(new Vector3Int(x, y, 0), decor);
                }
            }
        }
        if (torchPrefab != null)
        {
            foreach (Vector2Int cell in layout.torchCells)
            {
                Vector2Int mirrored = layout.GetMirroredCell(cell, signX, signY);
                Vector3 worldPos = floorTilemap.GetCellCenterWorld(new Vector3Int(mirrored.x, mirrored.y, 0));
                GameObject torchObj = Instantiate(torchPrefab, worldPos, Quaternion.identity, propsRoot);
                TorchFlicker flicker = torchObj.GetComponent<TorchFlicker>();
                if (flicker != null) flicker.SetColor(theme.torchLightColor);
            }
        }
        Physics2D.SyncTransforms();
        Debug.Log($"[Arena] Generated Layout: {layout.layoutName} | Theme: {theme.themeName} | Mirror: ({signX},{signY})");
    }

    public bool IsPositionWalkable(Vector2 worldPos, float clearanceRadius)
    {
        if (worldPos.x < PlayableBounds.min.x + clearanceRadius ||
            worldPos.x > PlayableBounds.max.x - clearanceRadius ||
            worldPos.y < PlayableBounds.min.y + clearanceRadius ||
            worldPos.y > PlayableBounds.max.y - clearanceRadius) return false;
        return Physics2D.OverlapCircle(worldPos, clearanceRadius, obstacleLayerMask) == null;
    }

    private void ClearProps()
    {
        if (propsRoot == null) return;
        for (int i = propsRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(propsRoot.GetChild(i).gameObject);
        }
    }
}
