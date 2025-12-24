using UnityEngine;
using System.Collections.Generic;

/*
 * StressTestShop
 * --------------
 * Performs thousands of shop rerolls to ensure no duplicates
 * ever appear across pedestals.
 * Runs in chunks so Unity does not freeze.
 */
public class StressTestShop : MonoBehaviour
{
    [SerializeField] private ShopPedestalRandomizer[] pedestals;
    [SerializeField] private int iterations = 10000;
    [SerializeField] private int batchSize = 100;

    private void Start()
    {
        StartCoroutine(RunTest());
    }

    private System.Collections.IEnumerator RunTest()
    {
        Debug.Log("Starting shop stress test...");

        int batches = iterations / batchSize;

        for (int b = 0; b < batches; b++)
        {
            for (int i = 0; i < batchSize; i++)
            {
                // Reset pedestals
                foreach (var p in pedestals)
                    p.ResetForNextVisit();

                // Clear used items once per reroll
                typeof(ShopPedestalRandomizer)
                    .GetField("usedItemsThisVisit", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                    .SetValue(null, new HashSet<BaseItemSO>());

                // Generate items
                foreach (var p in pedestals)
                    p.GenerateIfNeeded();

                // Check duplicates
                HashSet<string> names = new HashSet<string>();

                foreach (var p in pedestals)
                {
                    BaseItemSO item = p.GetChosenItem();
                    if (item == null) continue;

                    if (names.Contains(item.itemName))
                    {
                        Debug.LogError("DUPLICATE FOUND in batch " + b + ": " + item.itemName);
                        yield break;
                    }

                    names.Add(item.itemName);
                }
            }

            Debug.Log("Progress: " + (b * batchSize) + " iterations...");

            // Let Unity breathe
            yield return null;
        }

        Debug.Log("Stress test passed. No duplicates found in " + iterations + " iterations.");
    }
}
