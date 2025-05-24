using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyController2 : MonoBehaviour
{
    public Transform[] waypoints;
    public float idleTime = 2f;
    public float walkSpeed = 2f;
    public float chaseSpeed = 4f;
    public float sightDistance = 10f;
    public float rageDistance = 5f;
    public float attackDistance = 2f;

    public AudioClip idleSound;
    public AudioClip walkingSound;
    public AudioClip chasingSound;
    public AudioClip rageSound;
    public AudioClip attackSound;

    private int currentWaypointIndex = 0;
    private NavMeshAgent agent;
    private Animator animator;
    private float idleTimer = 0f;
    private Transform player;
    private AudioSource audioSource;

    private enum EnemyState { Idle, Walk, Chase, Rage, Attack }
    private EnemyState currentState = EnemyState.Idle;
    private bool isRaging = false; // To avoid multiple coroutine calls

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        audioSource = GetComponent<AudioSource>();
        SetDestinationToWaypoint();
    }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                idleTimer += Time.deltaTime;
                SetAnim(false, false, false);
                PlaySound(idleSound);

                if (idleTimer >= idleTime)
                    NextWaypoint();

                CheckForPlayerDetection(distanceToPlayer);
                break;

            case EnemyState.Walk:
                idleTimer = 0f;
                SetAnim(true, false, false);
                PlaySound(walkingSound);

                if (agent.remainingDistance <= agent.stoppingDistance)
                    currentState = EnemyState.Idle;

                CheckForPlayerDetection(distanceToPlayer);
                break;

            case EnemyState.Chase:
                idleTimer = 0f;
                agent.speed = chaseSpeed;
                agent.SetDestination(player.position);
                SetAnim(false, true, false);
                PlaySound(chasingSound);

                if (distanceToPlayer <= rageDistance && !isRaging)
                {
                    currentState = EnemyState.Rage;
                }
                else if (distanceToPlayer > sightDistance)
                {
                    currentState = EnemyState.Walk;
                    agent.speed = walkSpeed;
                }
                break;

            case EnemyState.Rage:
                if (!isRaging)
                {
                    StartCoroutine(DoRageThenAttack());
                }
                break;

            case EnemyState.Attack:
                agent.SetDestination(transform.position); // Stop moving
                SetAnim(false, false, true);
                PlaySound(attackSound);

                if (distanceToPlayer > attackDistance && distanceToPlayer < sightDistance)
                {
                    currentState = EnemyState.Chase;
                    agent.speed = chaseSpeed;
                }
                else if (distanceToPlayer >= sightDistance)
                {
                    currentState = EnemyState.Walk;
                    agent.speed = walkSpeed;
                }
                break;
        }
    }

    private IEnumerator DoRageThenAttack()
    {
        isRaging = true;
        agent.isStopped = true;
        animator.SetTrigger("Rage");
        PlaySound(rageSound);

        yield return new WaitForSeconds(1.5f); // Adjust this to your Rage animation length

        agent.isStopped = false;
        currentState = EnemyState.Attack;
        isRaging = false;
    }

    private void CheckForPlayerDetection(float distanceToPlayer)
    {
        RaycastHit hit;
        Vector3 playerDirection = player.position - transform.position;

        if (Physics.Raycast(transform.position, playerDirection.normalized, out hit, sightDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                currentState = EnemyState.Chase;
                Debug.Log("Player detected!");
            }
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && (!audioSource.isPlaying || audioSource.clip != clip))
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    private void NextWaypoint()
    {
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        SetDestinationToWaypoint();
    }

    private void SetDestinationToWaypoint()
    {
        agent.speed = walkSpeed;
        agent.SetDestination(waypoints[currentWaypointIndex].position);
        currentState = EnemyState.Walk;
    }

    private void SetAnim(bool isWalking, bool isChasing, bool isAttacking)
    {
        animator.SetBool("IsWalking", isWalking);
        animator.SetBool("IsChasing", isChasing);
        animator.SetBool("IsAttacking", isAttacking);
    }

    private void OnDrawGizmos()
    {
        if (player == null) return;

        Gizmos.color = currentState == EnemyState.Chase || currentState == EnemyState.Rage ? Color.red : Color.green;
        Gizmos.DrawLine(transform.position, player.position);
    }
}
