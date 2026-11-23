using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewArenaTheme", menuName = "Arena/Theme")]
public class ArenaThemeSO : ScriptableObject
{
    public string themeName;

    [Header("Base Floor & Legacy Fallbacks")]
    public TileBase baseFloorTile;
    public TileBase[] variantFloorTiles;
    [Range(0f, 1f)] public float variantChance = 0.2f;
    public TileBase wallTile;
    public TileBase obstacleTile;
    public TileBase[] decorTiles;
    [Range(0f, 1f)] public float decorChance = 0.05f;

    [Header("2.5D Roof & Parapet Rims")]
    public TileBase roofBaseTile;
    public TileBase[] roofVariantTiles;
    public TileBase roofRimTop;
    public TileBase roofRimBottom;
    public TileBase roofRimLeft;
    public TileBase roofRimRight;
    public TileBase roofCornerOuterTL;
    public TileBase roofCornerOuterTR;
    public TileBase roofCornerOuterBL;
    public TileBase roofCornerOuterBR;
    public TileBase roofCornerInnerTL;
    public TileBase roofCornerInnerTR;
    public TileBase roofCornerInnerBL;
    public TileBase roofCornerInnerBR;

    [Header("2.5D South Brick Facade & Floor Shadows")]
    public TileBase facadeMidTile;
    public TileBase facadeLeftTile;
    public TileBase facadeRightTile;
    public TileBase facadeSingleTile;
    public TileBase[] facadeAccentTiles;
    [Range(0f, 1f)] public float facadeAccentChance = 0.22f;
    public TileBase floorShadowTile;
    public TileBase floorShadowVariantTile;

    [Header("Walkable Landmarks")]
    public TileBase pavedLeftTile;
    public TileBase pavedMidTile;
    public TileBase pavedRightTile;
    public TileBase graveDirtTile;
    public TileBase railHorizontalTile;
    public TileBase railVerticalTile;
    public TileBase railCornerTL;
    public TileBase railCornerTR;
    public TileBase railCornerBL;
    public TileBase railCornerBR;

    [Header("Solid Standing Props")]
    public TileBase propCrateTile;
    public TileBase propBarrelTile;
    public TileBase propChestTile;
    public TileBase propCrossGraveTile;
    public TileBase propStoneGraveTile;
    public TileBase propIronFenceTile;
    public TileBase propAnvilTile;
    public TileBase propTableTile;
    public TileBase propMinecartTile;

    [Header("Lighting & Tint")]
    public Color floorTint = Color.white;
    public Color ambientLightColor = Color.white;
    public Color wallTint = Color.white;
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
        return decorTiles[Random.Range(0, decorTiles.Length)];
    }

    public TileBase GetRandomRoofTile()
    {
        if (roofVariantTiles != null && roofVariantTiles.Length > 0 && Random.value < 0.12f)
        {
            return roofVariantTiles[Random.Range(0, roofVariantTiles.Length)];
        }
        if (roofBaseTile != null)
        {
            return roofBaseTile;
        }
        return wallTile;
    }

    public TileBase GetFacadeTile(bool hasWest, bool hasEast)
    {
        TileBase mid = (facadeMidTile != null) ? facadeMidTile : wallTile;
        if (!hasWest && !hasEast)
        {
            return (facadeSingleTile != null) ? facadeSingleTile : mid;
        }
        else if (!hasWest && hasEast)
        {
            return (facadeLeftTile != null) ? facadeLeftTile : mid;

        }
        else if (hasWest && !hasEast)
        {
            return (facadeRightTile != null) ? facadeRightTile : mid;
        }
        else
        {
            if (facadeAccentTiles != null && facadeAccentTiles.Length > 0 && Random.value < facadeAccentChance)
            {
                return facadeAccentTiles[Random.Range(0, facadeAccentTiles.Length)];
            }
            return mid;
        }

    }

    public TileBase GetFloorShadowTile()
    {
        if (floorShadowVariantTile != null && Random.value < 0.3f)
        {
            return floorShadowVariantTile;
        }
        return floorShadowTile;
    }

    public TileBase GetPropTile(PropType propType)
    {
        switch (propType)
        {
            case PropType.Crate: return propCrateTile;
            case PropType.Barrel: return propBarrelTile;
            case PropType.Chest: return propChestTile;
            case PropType.CrossGrave: return propCrossGraveTile;
            case PropType.IronFence: return propIronFenceTile;
            case PropType.Minecart: return propMinecartTile;
            case PropType.StoneGrave: return propStoneGraveTile;
            case PropType.Table: return propTableTile;
            case PropType.Anvil: return propAnvilTile;
            default: return null;
        }
        
    }
}
