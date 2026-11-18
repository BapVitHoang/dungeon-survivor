using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<UpgradeSO> allUpgrades = new List<UpgradeSO>();
    [SerializeField] private GameObject player;

    private void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

    }

    public List<UpgradeSO> GetRandomUpgrades(int count)
    {
        List<UpgradeSO> pool = new List<UpgradeSO>(allUpgrades);
        List<UpgradeSO> result = new List<UpgradeSO>();

        for (int i = 0; i < count; i++)
        {
           if (pool.Count == 0)
            {
                break;
            }

            int randomIndex = Random.Range(0, pool.Count);
            UpgradeSO selectedUpgrade = pool[randomIndex];
            result.Add(selectedUpgrade);
            pool.RemoveAt(randomIndex);
        }

        return result;
    }

    public void ApplyUpgrade(UpgradeSO selectedUpgrade)
    {
        if (selectedUpgrade != null && player != null)
        {
            selectedUpgrade.Apply(player);
        }
    }
}
