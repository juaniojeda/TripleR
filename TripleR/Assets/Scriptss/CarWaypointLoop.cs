using UnityEngine;

public class CarWaypointLoop : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints;

    [Header("Movimiento")]
    public float speed = 8f;
    public float rotationSpeed = 5f;
    public float waypointDistance = 0.5f;

    private int currentWaypointIndex = 0;

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];

        // Dirección hacia el waypoint
        Vector3 direction = targetWaypoint.position - transform.position;
        direction.y = 0f;

        // Movimiento
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetWaypoint.position,
            speed * Time.deltaTime
        );

        // Rotación suave hacia el waypoint
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Pasar al siguiente waypoint
        float distance = Vector3.Distance(transform.position, targetWaypoint.position);

        if (distance < waypointDistance)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0;
            }
        }
    }
}