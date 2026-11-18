using UnityEngine;

public enum StatType
{
    FireRate,
    BulletDamage,
    MoveSpeed,
    MaxHealth,
    MagnetRadius
}

[CreateAssetMenu(fileName = "New Stat Upgrade", menuName = "Upgrades/Stat")]
public class StatUpgradeSO: UpgradeSO
{
    [Header("Stat Info")]
    public StatType statType;
    public float value;

    public override void Apply(GameObject player)
    {
        switch (statType)
        {
            case StatType.FireRate:
                AutoShooter shooterRate = player.GetComponent<AutoShooter>();
                if (shooterRate != null)
                {
                    shooterRate.UpgradeFireRate(value);
                }
                break;
            case StatType.BulletDamage:
                AutoShooter shooterDamage = player.GetComponent<AutoShooter>();
                if (shooterDamage != null)
                {
                    shooterDamage.UpgradeDamage(Mathf.RoundToInt(value));
                }
                break;
            case StatType.MoveSpeed:
                PlayerMovement movement = player.GetComponent<PlayerMovement>();
                if (movement != null)
                {
                    movement.UpgradeMoveSpeed(value);
                }
                break;
            case StatType.MaxHealth:
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.UpgradeMaxHealth(Mathf.RoundToInt(value));
                }
                break;
            case StatType.MagnetRadius:
                PlayerLevel radius = player.GetComponent<PlayerLevel>();
                if (radius != null)
                {
                    radius.UpgradeMagnetRadius(value);
                }
                break;
        }

    }
}