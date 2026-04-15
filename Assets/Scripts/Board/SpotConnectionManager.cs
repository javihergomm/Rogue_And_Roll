using System.Collections.Generic;
using UnityEngine;

/*
 * SpotConnectionManager
 * ---------------------
 * Manages temporary bridges and board shortcuts.
 * Provides connection lookup and helper methods for movement logic.
 */
public class SpotConnectionManager : MonoBehaviour
{
    public static SpotConnectionManager Instance { get; private set; }

    private Dictionary<int, List<int>> connections = new();

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterBridge(int from, int to)
    {
        if (!connections.ContainsKey(from))
            connections[from] = new List<int>();

        if (!connections[from].Contains(to))
            connections[from].Add(to);
    }

    public void UnregisterBridge(int from, int to)
    {
        if (!connections.ContainsKey(from))
            return;

        connections[from].Remove(to);

        if (connections[from].Count == 0)
            connections.Remove(from);
    }

    public List<int> GetConnections(int spot)
    {
        if (connections.ContainsKey(spot))
            return connections[spot];

        return new List<int>();
    }

    public bool WouldBridgeMoveAway(int startPos, int steps, int targetPos)
    {
        Movement anyMovement = FindFirstObjectByType<Movement>();

        if (anyMovement == null || anyMovement.Positions == null || anyMovement.Positions.Length == 0)
        {
            Debug.LogError("SpotConnectionManager: Cannot calculate total spots. No Movement found.");
            return false;
        }

        int total = anyMovement.Positions.Length;

        int predicted = (startPos + steps - 1 + total) % total + 1;

        var con = GetConnections(predicted);

        if (con.Count == 0)
            return false;

        int bridgeTarget = con[0];

        int distBefore = Mathf.Abs(targetPos - predicted);
        int distAfter = Mathf.Abs(targetPos - bridgeTarget);

        return distAfter > distBefore;
    }
}
