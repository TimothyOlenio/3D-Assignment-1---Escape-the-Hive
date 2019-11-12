using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointGizmos : MonoBehaviour
{
    public float size;
    private Transform[] waypoints;
    void OnDrawGizmos()
    {
        waypoints = gameObject.GetComponentsInChildren<Transform>();
        for (int i = 1; i < waypoints.Length; i++)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(waypoints[i].position, size);
        }
    }
}