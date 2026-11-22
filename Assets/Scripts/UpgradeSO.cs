using UnityEngine;

public abstract class UpgradeSO : ScriptableObject
{
    [Header("Card Info")]
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;

    public abstract void Apply(GameObject player);

    public virtual bool CanApply(GameObject player)
    {
        return true;
    }

}
