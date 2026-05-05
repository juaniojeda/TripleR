using System.Collections;
using UnityEngine;

public class SimplePatrolAI : MonoBehaviour
{
    private enum AIState
    {
        WaitingToStart,
        Patrol,
        ShowingCanvas,
        ReturningHome,
        Finished
    }

    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float arriveDistance = 0.1f;
    [SerializeField] private float rotationSpeed = 8f;

    [SerializeField] private GameObject worldCanvas;
    [SerializeField] private float canvasTime = 3f;
    [SerializeField] private float startDelay = 5f;

    private AIState state;
    private Vector3 homePosition;
    private Transform currentTarget;
    private Coroutine interactionRoutine;
    private Coroutine startRoutine;

    private void Awake()
    {
        homePosition = transform.position;
        currentTarget = pointB;
        state = AIState.WaitingToStart;

        if (worldCanvas != null)
            worldCanvas.SetActive(false);
    }

    private void Start()
    {
        startRoutine = StartCoroutine(StartDelayRoutine());
    }

    private void Update()
    {
        switch (state)
        {
            case AIState.Patrol:
                Patrol();
                break;

            case AIState.ReturningHome:
                ReturnHome();
                break;
        }
    }

    public void DetectPlayer()
    {
        if (state == AIState.ShowingCanvas || state == AIState.ReturningHome || state == AIState.Finished)
            return;

        if (startRoutine != null)
        {
            StopCoroutine(startRoutine);
            startRoutine = null;
        }

        if (interactionRoutine != null)
            StopCoroutine(interactionRoutine);

        interactionRoutine = StartCoroutine(ShowCanvasRoutine());
    }

    private IEnumerator StartDelayRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        if (state == AIState.WaitingToStart)
            state = AIState.Patrol;

        startRoutine = null;
    }

    private IEnumerator ShowCanvasRoutine()
    {
        state = AIState.ShowingCanvas;

        if (worldCanvas != null)
            worldCanvas.SetActive(true);

        yield return new WaitForSeconds(canvasTime);

        if (worldCanvas != null)
            worldCanvas.SetActive(false);

        state = AIState.ReturningHome;
        interactionRoutine = null;
    }

    private void Patrol()
    {
        if (pointA == null || pointB == null)
            return;

        MoveTo(currentTarget.position);

        if (Vector3.Distance(transform.position, currentTarget.position) <= arriveDistance)
            currentTarget = currentTarget == pointA ? pointB : pointA;
    }

    private void ReturnHome()
    {
        MoveTo(homePosition);

        if (Vector3.Distance(transform.position, homePosition) <= arriveDistance)
        {
            transform.position = homePosition;
            state = AIState.Finished;
        }
    }

    private void MoveTo(Vector3 targetPosition)
    {
        Vector3 currentPosition = transform.position;
        Vector3 direction = targetPosition - currentPosition;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        transform.position = Vector3.MoveTowards(
            currentPosition,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}
//using System.Collections;
//using UnityEngine;

//public class SimplePatrolAI : MonoBehaviour
//{
//    private enum AIState
//    {
//        Patrol,
//        ShowingCanvas,
//        ReturningHome
//    }

//    [Header("Patrulla")]
//    [SerializeField] private Transform pointA;
//    [SerializeField] private Transform pointB;
//    [SerializeField] private float moveSpeed = 1.5f;
//    [SerializeField] private float arriveDistance = 0.1f;
//    [SerializeField] private float rotationSpeed = 8f;

//    [Header("Canvas")]
//    [SerializeField] private GameObject worldCanvas;
//    [SerializeField] private float canvasTime = 10f;

//    private AIState state;
//    private Vector3 homePosition;
//    private Transform currentTarget;
//    private Coroutine interactionRoutine;

//    private void Awake()
//    {
//        homePosition = transform.position;
//        currentTarget = pointB;

//        if (worldCanvas != null)
//            worldCanvas.SetActive(false);
//    }

//    private void Update()
//    {
//        switch (state)
//        {
//            case AIState.Patrol:
//                Patrol();
//                break;

//            case AIState.ReturningHome:
//                ReturnHome();
//                break;
//        }
//    }

//    public void DetectPlayer()
//    {
//        if (state != AIState.Patrol)
//            return;

//        if (interactionRoutine != null)
//            StopCoroutine(interactionRoutine);

//        interactionRoutine = StartCoroutine(ShowCanvasRoutine());
//    }

//    private IEnumerator ShowCanvasRoutine()
//    {
//        state = AIState.ShowingCanvas;

//        if (worldCanvas != null)
//            worldCanvas.SetActive(true);

//        yield return new WaitForSeconds(canvasTime);

//        if (worldCanvas != null)
//            worldCanvas.SetActive(false);

//        state = AIState.ReturningHome;
//        interactionRoutine = null;
//    }

//    private void Patrol()
//    {
//        if (pointA == null || pointB == null)
//            return;

//        MoveTo(currentTarget.position);

//        if (Vector3.Distance(transform.position, currentTarget.position) <= arriveDistance)
//            currentTarget = currentTarget == pointA ? pointB : pointA;
//    }

//    private void ReturnHome()
//    {
//        MoveTo(homePosition);

//        if (Vector3.Distance(transform.position, homePosition) <= arriveDistance)
//        {
//            transform.position = homePosition;
//            currentTarget = pointB;
//            state = AIState.Patrol;
//        }
//    }

//    private void MoveTo(Vector3 targetPosition)
//    {
//        Vector3 currentPosition = transform.position;
//        Vector3 direction = targetPosition - currentPosition;
//        direction.y = 0f;

//        if (direction.sqrMagnitude <= 0.001f)
//            return;

//        transform.position = Vector3.MoveTowards(
//            currentPosition,
//            targetPosition,
//            moveSpeed * Time.deltaTime
//        );

//        Quaternion targetRotation = Quaternion.LookRotation(direction);
//        transform.rotation = Quaternion.Slerp(
//            transform.rotation,
//            targetRotation,
//            rotationSpeed * Time.deltaTime
//        );
//    }
//}