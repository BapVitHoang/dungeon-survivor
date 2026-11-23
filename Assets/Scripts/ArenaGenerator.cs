using System.Collections.Generic;
using UnityEngine;
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

        if (floorTilemap != null) floorTilemap.color = theme.floorTint;
        if (wallTilemap != null) wallTilemap.color = theme.wallTint;
        if (obstacleTilemap != null) obstacleTilemap.color = theme.wallTint;
        if (globalLight != null)
        {
            globalLight.color = theme.ambientLightColor;
            globalLight.intensity = theme.ambientIntensity;
        }

        int halfW = PlayableWidth / 2;
        int halfH = PlayableHeight / 2;

        PaintBaseFloor(theme, halfW, halfH);
        PaintPerimeterWalls(theme, halfW, halfH);
        HashSet<Vector2Int> occupied = PaintInteriorIslands(theme, layout, signX, signY, halfW, halfH);
        PaintLandmarks(theme, layout, signX, signY, occupied);
        PaintSolidProps(theme, layout, signX, signY, occupied, halfW, halfH);
        ScatterFloorDecor(theme,occupied,halfW, halfH);
        SpawnTorches(theme, layout, signX, signY);
   
        Physics2D.SyncTransforms();
        Debug.Log($"[Arena] Generated Layout: {layout.layoutName} | Theme: {theme.themeName} | Mirror: ({signX},{signY})");
    }

    private void PaintBaseFloor(ArenaThemeSO theme, int halfW, int halfH)
    {
        for (int x = -halfW - 1; x <= halfW; x++)
        {
            for (int y = -halfH - 1; y <= halfH; y++)
            {
                floorTilemap.SetTile(new Vector3Int(x, y, 0), theme.GetRandomFloorTile());
            }
        }
    }

    private void PaintPerimeterWalls(ArenaThemeSO theme, int halfW, int halfH)
    {
        for (int x = -halfW - WallThickness; x <= halfW + WallThickness; x++)
        {
            for (int y = -halfH - WallThickness; y <= halfH + WallThickness; y++)
            {
                bool insidePlayable = (x >= -halfW && x < halfW && y >= -halfH && y < halfH);
                if (insidePlayable)
                {
                    continue;
                }
                TileBase tile = ResolvePerimeterTile(theme, x, y, halfW, halfH, out bool isNorthFacade);
                wallTilemap.SetTile(new Vector3Int(x, y, 0), tile);

                if (isNorthFacade)
                {
                    TileBase shadowTile = theme.GetFloorShadowTile();
                    if (shadowTile != null)
                    {
                        floorTilemap.SetTile(new Vector3Int(x, y - 1, 0), shadowTile);
                    }
                }

            }
        }
    }

    private HashSet<Vector2Int> PaintInteriorIslands(ArenaThemeSO theme, ArenaLayoutSO layout, int signX, int signY, int halfW, int halfH)
    {
        HashSet<Vector2Int> solidSet = new HashSet<Vector2Int>();
        foreach (Vector2Int cell in layout.obstacleCells)
        {
            Vector2Int mirrored = layout.GetMirroredCell(cell, signX, signY);
            if (mirrored.x >= -halfW && mirrored.x < halfW && mirrored.y >= -halfH && mirrored.y < halfH)
            {
                solidSet.Add(mirrored);
            }
        }

        foreach (Vector2Int pos in solidSet)
        {
            TileBase tile = ResolveInteriorTile(theme, pos, solidSet, out bool isSouthFacade);
            obstacleTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), tile);

            if (isSouthFacade)
            {
                Vector2Int below = new Vector2Int(pos.x, pos.y - 1);
                if (!solidSet.Contains(below) && below.y >= -halfH)
                {
                    TileBase shadowTile = theme.GetFloorShadowTile();
                    if (shadowTile != null)
                    {
                        floorTilemap.SetTile(new Vector3Int(below.x, below.y, 0), shadowTile);
                    }
                }
            }
        }
        return solidSet;
    }

    private TileBase ResolveInteriorTile(ArenaThemeSO theme, Vector2Int pos, HashSet<Vector2Int> solidSet, out bool isSouthFacade)
    {
        isSouthFacade = false;
        bool hasN = solidSet.Contains(new Vector2Int(pos.x, pos.y + 1));
        bool hasS = solidSet.Contains(new Vector2Int(pos.x, pos.y - 1));
        bool hasS2 = solidSet.Contains(new Vector2Int(pos.x, pos.y - 2));
        bool hasW = solidSet.Contains(new Vector2Int(pos.x - 1, pos.y));
        bool hasE = solidSet.Contains(new Vector2Int(pos.x + 1, pos.y));

        if (!hasS)
        {
            isSouthFacade = true;
            return theme.GetFacadeTile(hasW, hasE);
        }
        if (!hasN)
        {
            if (!hasW) return theme.roofCornerOuterTL;
            if (!hasE) return theme.roofCornerOuterTR;
            return theme.roofRimTop;
        }
        if (!hasS2)
        {
            if (!hasW) return theme.roofCornerOuterBL;
            if (!hasE) return theme.roofCornerOuterBR;
            return theme.roofRimBottom;
        }
        if (!hasW) return theme.roofRimLeft;
        if (!hasE) return theme.roofRimRight;
        return theme.GetRandomRoofTile();
    }
    private TileBase ResolvePerimeterTile(ArenaThemeSO theme, int x, int y, int halfW, int halfH, out bool isNorthFacade)
    {
        isNorthFacade = false;

        if (y == halfH && x >= -halfW && x < halfW)
        {
            isNorthFacade = true;
            return theme.GetFacadeTile(true, true);
        }
        if (y == halfH + 1 && x >= -halfW && x < halfW)
        {
            return theme.roofRimBottom;
        }
        if (y == halfH + 1 && x == -halfW - 1)
        {
            return theme.roofCornerInnerTL;
        }
        if (y == halfH + 1 && x == halfW)
        {
            return theme.roofCornerInnerTR;
        }
        if (x == -halfW - 1 && y >= -halfH && y <= halfH)
        {
            return theme.roofRimRight;
        }
        if (x == halfW && y >= -halfH && y <= halfH)
        {
            return theme.roofRimLeft;
        }
        if (y == -halfH - 1 && x >= -halfW && x < halfW)
        {
            return theme.roofRimTop;
        }
        if (y == -halfH - 1 && x == -halfW - 1)
        {
            return theme.roofCornerInnerBL;
        }
        if (y == -halfH - 1 && x == halfW)
        {
            return theme.roofCornerInnerBR;
        }
        return theme.GetRandomRoofTile();
    }

    private void PaintLandmarks(ArenaThemeSO theme, ArenaLayoutSO layout, int signX, int signY, HashSet<Vector2Int> occupied)
    {
        HashSet<Vector2Int> pavedSet = new HashSet<Vector2Int>();
        foreach (Vector2Int cell in layout.pavedCells)
        {
            Vector2Int mirrored = layout.GetMirroredCell(cell, signX, signY);
            if (!occupied.Contains(mirrored))
            {
                pavedSet.Add(mirrored);
            }
        }
        foreach (Vector2Int pos in pavedSet)
        {
            bool hasLeft = pavedSet.Contains(new Vector2Int(pos.x - 1, pos.y));
            bool hasRight = pavedSet.Contains(new Vector2Int(pos.x + 1, pos.y));
            TileBase pavedTile = theme.pavedMidTile;
            if (!hasLeft && hasRight && theme.pavedLeftTile != null) pavedTile = theme.pavedLeftTile;
            else if (hasLeft && !hasRight && theme.pavedRightTile != null) pavedTile = theme.pavedRightTile;
            if (pavedTile != null)
            {
                decorTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), pavedTile);
            }
        }
        HashSet<Vector2Int> railSet = new HashSet<Vector2Int>();
        foreach (Vector2Int cell in layout.railCells)
        {
            Vector2Int mirrored = layout.GetMirroredCell(cell, signX, signY);
            if (!occupied.Contains(mirrored))
            {
                railSet.Add(mirrored);
            }
        }
        foreach (Vector2Int pos in railSet)
        {
            TileBase railTile = ResolveRailTile(theme, pos, railSet);
            if (railTile != null)
            {
                decorTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), railTile);
            }
        }
    }
    private TileBase ResolveRailTile(ArenaThemeSO theme, Vector2Int pos, HashSet<Vector2Int> railSet)
    {
        bool n = railSet.Contains(new Vector2Int(pos.x, pos.y + 1));
        bool s = railSet.Contains(new Vector2Int(pos.x, pos.y - 1));
        bool w = railSet.Contains(new Vector2Int(pos.x - 1, pos.y));
        bool e = railSet.Contains(new Vector2Int(pos.x + 1, pos.y));

        if (s && e && !n && !w) return theme.railCornerTL;
        if(s && w && !n && !e) return theme.railCornerTR;
        if (n  && e && !s && !w) return theme.railCornerBL;
        if (n && w && !s && !e) return theme.railCornerBR;
        if (n||s) return theme.railVerticalTile;
        return theme.railHorizontalTile;
    }

    private void PaintSolidProps(ArenaThemeSO theme, ArenaLayoutSO layout, int signX, int signY, HashSet<Vector2Int> occupied, int halfW, int halfH)
    {
        foreach (ArenaPropPlacement prop in layout.props)
        {
            Vector2Int pos = layout.GetMirroredCell(prop.cell, signX, signY);
            if (pos.x < -halfW || pos.x >= halfW || pos.y < -halfH || pos.y >= halfH) continue;
            if (Mathf.Abs(pos.x) <= 3 && Mathf.Abs(pos.y) <= 3) continue;
            if (occupied.Contains(pos)) continue;
            TileBase propTile = theme.GetPropTile(prop.type);
            if (propTile == null) continue;
            obstacleTilemap.SetTile(new Vector3Int(pos.x,pos.y,0), propTile);
            occupied.Add(pos);
            if (prop.type == PropType.CrossGrave || prop.type == PropType.StoneGrave)
            {
                Vector2Int dirtPos = new Vector2Int(pos.x,pos.y-1);
                if (!occupied.Contains(dirtPos) && dirtPos.y >= -halfH && theme.graveDirtTile !=null)
                {
                    decorTilemap.SetTile(new Vector3Int(dirtPos.x, dirtPos.y, 0), theme.graveDirtTile);
                }
            }
        }

    }
    private void ScatterFloorDecor(ArenaThemeSO theme, HashSet<Vector2Int> occupied, int halfW, int halfH)
    {
        for (int x = -halfW + 1; x < halfW - 1; x++)
        {
            for (int y = -halfH + 1; y < halfH - 1; y++)
            {
                if (Mathf.Abs(x) <= 3 && Mathf.Abs(y) <= 3) continue;
                Vector2Int cell = new Vector2Int(x, y);
                if (occupied.Contains(cell)) continue;
                if (decorTilemap.HasTile(new Vector3Int(x, y, 0))) continue;
                if (Random.value < theme.decorChance)
                {
                    TileBase decor = theme.GetRandomDecorTile();
                    if (decor != null) decorTilemap.SetTile(new Vector3Int(x, y, 0), decor);
                }
            }
        }
    }
    public bool IsPositionWalkable(Vector2 worldPos, float clearanceRadius)
    {
        if (worldPos.x < PlayableBounds.min.x + clearanceRadius ||
            worldPos.x > PlayableBounds.max.x - clearanceRadius ||
            worldPos.y < PlayableBounds.min.y + clearanceRadius ||
            worldPos.y > PlayableBounds.max.y - clearanceRadius) return false;
        return Physics2D.OverlapCircle(worldPos, clearanceRadius, obstacleLayerMask) == null;
    }
    private void SpawnTorches(ArenaThemeSO theme, ArenaLayoutSO layout, int signX, int signY)
    {
        if (torchPrefab == null) return;
        foreach (Vector2Int cell in layout.torchCells)
        {
            Vector2Int mirrored = layout.GetMirroredCell(cell, signX, signY);
            Vector3 worldPos = floorTilemap.GetCellCenterWorld(new Vector3Int(mirrored.x, mirrored.y, 0));
            GameObject torchObj = Instantiate(torchPrefab, worldPos, Quaternion.identity, propsRoot);
            TorchFlicker flicker = torchObj.GetComponent<TorchFlicker>();
            if (flicker != null) flicker.SetColor(theme.torchLightColor);
        }
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
