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

    [Header("Movimiento")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float arriveDistance = 0.1f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Tutorial")]
    [SerializeField] private TutorialPagesUI tutorialUI;
    [SerializeField] private float startDelay = 5f;

    [Header("Animaciones")]
    [SerializeField] private Animator animator;
    [SerializeField] private string isMovingParameter = "IsMoving";
    [SerializeField] private string isTalkingParameter = "IsTalking";

    private AIState state;
    private Vector3 homePosition;
    private Transform currentTarget;
    private Coroutine startRoutine;

    private void Awake()
    {
        homePosition = transform.position;
        currentTarget = pointB;
        state = AIState.WaitingToStart;

        if (tutorialUI != null)
            tutorialUI.gameObject.SetActive(false);

        SetMoving(false);
        SetTalking(false);
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

        ShowTutorial();
    }

    private IEnumerator StartDelayRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        if (state == AIState.WaitingToStart)
        {
            state = AIState.Patrol;
            SetTalking(false);
            SetMoving(true);
        }

        startRoutine = null;
    }

    private void ShowTutorial()
    {
        state = AIState.ShowingCanvas;

        SetMoving(false);
        SetTalking(true);

        if (tutorialUI != null)
        {
            tutorialUI.Open(this);
        }
        else
        {
            Debug.LogWarning("No asignaste Tutorial UI en el SimplePatrolAI.");
            SetTalking(false);
            state = AIState.ReturningHome;
            SetMoving(true);
        }
    }

    public void FinishTutorial()
    {
        if (state != AIState.ShowingCanvas)
            return;

        SetTalking(false);
        state = AIState.ReturningHome;
        SetMoving(true);
    }

    private void Patrol()
    {
        if (pointA == null || pointB == null)
        {
            SetMoving(false);
            return;
        }

        bool moved = MoveTo(currentTarget.position);
        SetMoving(moved);

        if (Vector3.Distance(transform.position, currentTarget.position) <= arriveDistance)
            currentTarget = currentTarget == pointA ? pointB : pointA;
    }

    private void ReturnHome()
    {
        bool moved = MoveTo(homePosition);
        SetMoving(moved);

        if (Vector3.Distance(transform.position, homePosition) <= arriveDistance)
        {
            transform.position = homePosition;
            state = AIState.Finished;
            SetMoving(false);
            SetTalking(false);
        }
    }

    private bool MoveTo(Vector3 targetPosition)
    {
        Vector3 currentPosition = transform.position;
        Vector3 direction = targetPosition - currentPosition;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return false;

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

        return true;
    }

    private void SetMoving(bool value)
    {
        if (animator == null)
            return;

        animator.SetBool(isMovingParameter, value);
    }

    private void SetTalking(bool value)
    {
        if (animator == null)
            return;

        animator.SetBool(isTalkingParameter, value);
    }
}

//using System.Collections;
//using UnityEngine;

//public class SimplePatrolAI : MonoBehaviour
//{
//    private enum AIState
//    {
//        WaitingToStart,
//        Patrol,
//        ShowingCanvas,
//        ReturningHome,
//        Finished
//    }

//    [Header("Movimiento")]
//    [SerializeField] private Transform pointA;
//    [SerializeField] private Transform pointB;
//    [SerializeField] private float moveSpeed = 1.5f;
//    [SerializeField] private float arriveDistance = 0.1f;
//    [SerializeField] private float rotationSpeed = 8f;

//    [Header("Tutorial")]
//    [SerializeField] private TutorialPagesUI tutorialUI;
//    [SerializeField] private float startDelay = 5f;

//    private AIState state;
//    private Vector3 homePosition;
//    private Transform currentTarget;
//    private Coroutine startRoutine;

//    private void Awake()
//    {
//        homePosition = transform.position;
//        currentTarget = pointB;
//        state = AIState.WaitingToStart;

//        if (tutorialUI != null)
//            tutorialUI.gameObject.SetActive(false);
//    }

//    private void Start()
//    {
//        startRoutine = StartCoroutine(StartDelayRoutine());
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
//        if (state == AIState.ShowingCanvas || state == AIState.ReturningHome || state == AIState.Finished)
//            return;

//        if (startRoutine != null)
//        {
//            StopCoroutine(startRoutine);
//            startRoutine = null;
//        }

//        ShowTutorial();
//    }

//    private IEnumerator StartDelayRoutine()
//    {
//        yield return new WaitForSeconds(startDelay);

//        if (state == AIState.WaitingToStart)
//            state = AIState.Patrol;

//        startRoutine = null;
//    }

//    private void ShowTutorial()
//    {
//        state = AIState.ShowingCanvas;

//        if (tutorialUI != null)
//        {
//            tutorialUI.Open(this);
//        }
//        else
//        {
//            Debug.LogWarning("No asignaste Tutorial UI en el SimplePatrolAI.");
//            state = AIState.ReturningHome;
//        }
//    }

//    public void FinishTutorial()
//    {
//        if (state != AIState.ShowingCanvas)
//            return;

//        state = AIState.ReturningHome;
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
//            state = AIState.Finished;
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