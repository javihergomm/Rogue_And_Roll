using UnityEngine;
using System.Collections.Generic;

/*
 * StressTestLootBox
 * -----------------
 * Performs thousands of loot box openings to ensure:
 * - Only items from the assigned pool are returned
 * - Returned items match the loot box polarity
 * - No loot boxes appear inside loot boxes
 * - No null rewards
 */
public class StressTestLootBox : MonoBehaviour
{
    [SerializeField] private LootBoxSO lootBox;
    [SerializeField] private LootBoxItemPool pool;
    [SerializeField] private int iterations = 50000;

    private void Start()
    {
        RunTest();
    }

    private void RunTest()
    {
        Debug.Log("Starting loot box stress test...");

        HashSet<BaseItemSO> poolSet = new HashSet<BaseItemSO>(pool.items);

        for (int i = 0; i < iterations; i++)
        {
            BaseItemSO reward = lootBox.Open();

            if (reward == null)
            {
                Debug.LogError("NULL REWARD at iteration " + i);
                return;
            }

            if (!poolSet.Contains(reward))
            {
                Debug.LogError("ITEM NOT IN POOL: " + reward.ItemName);
                return;
            }

            if (reward is LootBoxSO)
            {
                Debug.LogError("LOOT BOX INSIDE LOOT BOX at iteration " + i);
                return;
            }

            if (reward.Polarity != lootBox.Polarity)
            {
                Debug.LogError("POLARITY MISMATCH at iteration " + i +
                               " Reward: " + reward.Polarity +
                               " Box: " + lootBox.Polarity);
                return;
            }
        }

        Debug.Log("Loot box stress test passed. No issues found in " + iterations + " iterations.");
    }
}
