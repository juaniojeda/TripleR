using UnityEngine;

public class CarWaypointLoop : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints;

    [Header("Movimiento")]
    public float speed = 8f;
    public float rotationSpeed = 5f;
    public float waypointDistance = 0.5f;

    private int currentWaypointIndex;

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];

        if (targetWaypoint == null)
        {
            AdvanceToNextWaypoint();
            return;
        }

        Vector3 direction = targetWaypoint.position - transform.position;
        direction.y = 0f;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetWaypoint.position,
            speed * Time.deltaTime
        );

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        float distance = Vector3.Distance(transform.position, targetWaypoint.position);

        if (distance < waypointDistance)
            AdvanceToNextWaypoint();
    }

    private void AdvanceToNextWaypoint()
    {
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }
}
