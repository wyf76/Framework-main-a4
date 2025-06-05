using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance { get; private set; }

    private Waypoint[] waypoints;

    void Awake()
    {
        Instance = this;
        waypoints = FindObjectsOfType<Waypoint>();
    }

    public List<Vector3> FindPath(Vector3 start, Vector3 target)
    {
        if (waypoints == null || waypoints.Length == 0)
            return new List<Vector3> { target };

        Waypoint startWp = GetClosestWaypoint(start);
        Waypoint endWp = GetClosestWaypoint(target);
        if (startWp == null || endWp == null)
            return new List<Vector3> { target };

        List<Waypoint> wps = BFS(startWp, endWp);
        List<Vector3> result = new List<Vector3>();
        foreach (var wp in wps)
            result.Add(wp.transform.position);
        result.Add(target);
        return result;
    }

    Waypoint GetClosestWaypoint(Vector3 pos)
    {
        Waypoint best = null;
        float bestDist = Mathf.Infinity;
        foreach (var wp in waypoints)
        {
            float d = (wp.transform.position - pos).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = wp;
            }
        }
        return best;
    }

    List<Waypoint> BFS(Waypoint start, Waypoint goal)
    {
        var queue = new Queue<Waypoint>();
        var came = new Dictionary<Waypoint, Waypoint>();
        queue.Enqueue(start);
        came[start] = null;
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == goal)
                break;
            foreach (var n in current.neighbors)
            {
                if (n == null || came.ContainsKey(n))
                    continue;
                came[n] = current;
                queue.Enqueue(n);
            }
        }
        var path = new List<Waypoint>();
        if (!came.ContainsKey(goal))
            return path;
        Waypoint w = goal;
        while (w != null)
        {
            path.Insert(0, w);
            w = came[w];
        }
        return path;
    }
}
