using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewArenaTheme", menuName = "Arena/Theme")]
public class ArenaThemeSO : ScriptableObject
{
    public string themeName;

    [Header("Tiles")]
    public TileBase baseFloorTile;
    public TileBase[] variantFloorTiles;
    [Range(0f, 1f)] public float variantChance = 0.2f;
    public TileBase wallTile;
    public TileBase obstacleTile;
    public TileBase[] decorTiles;
    [Range(0f, 1f)] public float decorChance = 0.05f;

    [Header("Lighting & Tint")]
    public Color floorTint = Color.white;
    public Color ambientLightColor = Color.white;
    [Range(0.2f, 1.5f)] public float ambientIntensity = 0.55f;
    public Color torchLightColor = new Color(1f, 0.65f, 0.32f, 1f);

    public TileBase GetRandomFloorTile()
    {
        if (variantFloorTiles != null && variantFloorTiles.Length > 0 && Random.value < variantChance)
        {
           int random = Random.Range(0, variantFloorTiles.Length);
            return variantFloorTiles[random];
        }
        else
        {
            return baseFloorTile;
        }
    }
    public TileBase GetRandomDecorTile()
    {
        if (decorTiles == null || decorTiles.Length == 0) return null;
        return decorTiles[Random.Range(0,decorTiles.Length)];
    }
}
