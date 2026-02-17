using System.Collections.Generic;
using UnityEngine;

/*
 * SpotConnectionManager
 * ---------------------
 * Manages shortcut connections between board positions.
 * Bridges created by consumables last for one full round
 * and are removed automatically when their duration expires.
 */
public class SpotConnectionManager : MonoBehaviour
{
    public static SpotConnectionManager Instance { get; private set; }

    private Dictionary<int, List<int>> connections = new Dictionary<int, List<int>>();

    // Active bridges with their remaining round duration
    private List<BridgeData> activeBridges = new List<BridgeData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /*
     * Adds a one-way connection between two board positions.
     */
    public void AddConnection(int from, int to)
    {
        if (!connections.ContainsKey(from))
            connections[from] = new List<int>();

        if (!connections[from].Contains(to))
            connections[from].Add(to);
    }

    /*
     * Registers a new bridge that lasts for one full round.
     * Adds both directions of the connection.
     */
    public void RegisterBridge(int a, int b)
    {
        AddConnection(a, b);
        AddConnection(b, a);

        activeBridges.Add(new BridgeData(a, b));
    }

    /*
     * Returns all connections from a given board position.
     */
    public List<int> GetConnections(int from)
    {
        if (connections.TryGetValue(from, out var list))
            return list;

        return new List<int>();
    }

    /*
     * Called when a turn ends (player or enemy).
     * When a full round is completed, bridge durations decrease.
     */
    public void OnRoundStepCompleted()
    {
        foreach (var bridge in activeBridges)
            bridge.roundsLeft--;

        RemoveExpiredBridges();
    }

    /*
     * Removes bridges whose duration has reached zero.
     */
    private void RemoveExpiredBridges()
    {
        for (int i = activeBridges.Count - 1; i >= 0; i--)
        {
            if (activeBridges[i].roundsLeft <= 0)
            {
                RemoveConnection(activeBridges[i].a, activeBridges[i].b);
                RemoveConnection(activeBridges[i].b, activeBridges[i].a);
                activeBridges.RemoveAt(i);
            }
        }
    }

    /*
     * Removes a one-way connection.
     */
    private void RemoveConnection(int from, int to)
    {
        if (connections.ContainsKey(from))
            connections[from].Remove(to);
    }

    /*
     * Stores data for an active temporary bridge.
     */
    private class BridgeData
    {
        public int a;
        public int b;
        public int roundsLeft = 1;

        public BridgeData(int a, int b)
        {
            this.a = a;
            this.b = b;
        }
    }
}
