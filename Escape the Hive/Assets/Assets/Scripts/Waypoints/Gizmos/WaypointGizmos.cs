using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointGizmos : MonoBehaviour
{
    //Create Variables
    //A float that controls the size of the object
    public float size;
    //Create an array of transform components called waypoints
    private Transform[] waypoints;
    
    //This function creates a sphere aand colours it yellow around the objects, then draw a line between them1
    void OnDrawGizmos()
    {
        //set the waypoints to the children of the attached object
        waypoints = gameObject.GetComponentsInChildren<Transform>();
        //Create a Vector3  that holds the last position of the previous waypoint
        Vector3 last = waypoints[waypoints.Length - 1].position;
        //Cycle through the waypoints array and draw a sphere around them and colour them yellow
        for (int i = 1; i < waypoints.Length; i++)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(waypoints[i].position, size);
            Gizmos.DrawLine(last, waypoints[i].position);

            last = waypoints[i].position;
        }
    }
}