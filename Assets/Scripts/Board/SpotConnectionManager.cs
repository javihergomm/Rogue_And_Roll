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
        Debug.Log("[BRIDGE] SpotConnectionManager inicializado.");
    }

    // ---------------------------------------------------------
    // REGISTRAR PUENTE (BIDIRECCIONAL)
    // ---------------------------------------------------------
    public void RegisterBridge(int from, int to)
    {
        Debug.Log("[BRIDGE] Registrando puente: " + from + " <-> " + to);

        // A -> B
        if (!connections.ContainsKey(from))
            connections[from] = new List<int>();

        if (!connections[from].Contains(to))
        {
            connections[from].Add(to);
            Debug.Log("[BRIDGE] Puente añadido: " + from + " -> " + to);
        }

        // B -> A
        if (!connections.ContainsKey(to))
            connections[to] = new List<int>();

        if (!connections[to].Contains(from))
        {
            connections[to].Add(from);
            Debug.Log("[BRIDGE] Puente añadido: " + to + " -> " + from);
        }

        PrintAllConnections();
    }

    // ---------------------------------------------------------
    // ELIMINAR PUENTE (BIDIRECCIONAL)
    // ---------------------------------------------------------
    public void UnregisterBridge(int from, int to)
    {
        Debug.Log("[BRIDGE] Eliminando puente: " + from + " <-> " + to);

        // A -> B
        if (connections.ContainsKey(from))
        {
            connections[from].Remove(to);
            if (connections[from].Count == 0)
            {
                connections.Remove(from);
                Debug.Log("[BRIDGE] Eliminada lista vacia para " + from);
            }
        }

        // B -> A
        if (connections.ContainsKey(to))
        {
            connections[to].Remove(from);
            if (connections[to].Count == 0)
            {
                connections.Remove(to);
                Debug.Log("[BRIDGE] Eliminada lista vacia para " + to);
            }
        }

        PrintAllConnections();
    }

    // ---------------------------------------------------------
    // CONSULTAR CONEXIONES
    // ---------------------------------------------------------
    public List<int> GetConnections(int spot)
    {
        Debug.Log("[BRIDGE] Consultando conexiones en " + spot);

        if (connections.ContainsKey(spot))
        {
            Debug.Log("[BRIDGE] Conexiones encontradas en " + spot + ": " + string.Join(",", connections[spot]));
            return connections[spot];
        }

        Debug.Log("[BRIDGE] No hay conexiones en " + spot);
        return new List<int>();
    }

    // ---------------------------------------------------------
    // LOGICA DE MOVIMIENTO (NO SE TOCA)
    // ---------------------------------------------------------
    public bool WouldBridgeMoveAway(int startPos, int steps, int targetPos)
    {
        Debug.Log("[BRIDGE] WouldBridgeMoveAway? start=" + startPos + ", steps=" + steps + ", target=" + targetPos);

        Movement anyMovement = FindFirstObjectByType<Movement>();

        if (anyMovement == null || anyMovement.Positions == null || anyMovement.Positions.Length == 0)
        {
            Debug.LogError("[BRIDGE] ERROR: No hay Movement o Positions.");
            return false;
        }

        int total = anyMovement.Positions.Length;

        int predicted = (startPos + steps - 1 + total) % total + 1;

        Debug.Log("[BRIDGE] Posicion predicha tras movimiento: " + predicted);

        var con = GetConnections(predicted);

        if (con.Count == 0)
        {
            Debug.Log("[BRIDGE] No hay puente en la posicion predicha.");
            return false;
        }

        int bridgeTarget = con[0];

        int distBefore = Mathf.Abs(targetPos - predicted);
        int distAfter = Mathf.Abs(targetPos - bridgeTarget);

        Debug.Log("[BRIDGE] distBefore=" + distBefore + ", distAfter=" + distAfter);

        bool result = distAfter > distBefore;

        Debug.Log("[BRIDGE] ¿Moverse por el puente aleja del objetivo? " + result);

        return result;
    }

    // ---------------------------------------------------------
    // DEBUG
    // ---------------------------------------------------------
    private void PrintAllConnections()
    {
        Debug.Log("========== [BRIDGE] LISTA COMPLETA DE CONEXIONES ==========");

        if (connections.Count == 0)
        {
            Debug.Log("[BRIDGE] (vacio)");
            return;
        }

        foreach (var kvp in connections)
        {
            Debug.Log("[BRIDGE] " + kvp.Key + " -> " + string.Join(",", kvp.Value));
        }

        Debug.Log("===========================================================");
    }
}
