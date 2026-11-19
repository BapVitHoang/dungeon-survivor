using UnityEngine;

[CreateAssetMenu(fileName = "NewMultiShotUpgrade", menuName = "Upgrades/MultiShot")]
public class MultiShotUpgradeSO : UpgradeSO
{
    [Header("Multi-Shot Info")]
    public int extraProjectiles = 1;

    public override void Apply(GameObject player)
    {
        AutoShooter shooter = player.GetComponent<AutoShooter>();
        if (shooter != null)
        {
            shooter.AddProjectiles(extraProjectiles);
        }
        else
        {
            Debug.LogWarning("AutoShooter component not found on player.");
        }
    }
}
