using UnityEngine;
using UnityEngine.AI;

// This is controller for level 2 enemies. 
// They don't work as weeping angels, but instead patrol randomly and chase the player if they hear a sound.
public class EnemyController : MonoBehaviour
{
    enum EnemyState
    {
        Patrol,
        Chase,
        Flee,
        Attack
    }
    private EnemyState currentState;
    [Header("AI settings")]
    [SerializeField] private Transform player;
    [SerializeField] private float hearingRange = 15f;

    [Header("Patrol State")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float patrolRadius = 10f;
    [SerializeField] private float patrolWaitTime = 3f;

    [Header("Chase State")]
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float losePlayerTime = 5f;
    [SerializeField] private float losePlayerDistance = 20f;

    [Header("Flee State")]
    [SerializeField] private Light playerFlashlight;
    [SerializeField] private float timeToFlee = 1.5f;  
    [SerializeField] private float fleeDistance = 20.0f;

    [Header("Attack State")]
    [SerializeField] private float attackRange = 2f;

    private NavMeshAgent agent;
    private float patrolWaitTimer;
    private float currentLoseTimer;
    private float currentFleeTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentState = EnemyState.Patrol;
        agent.speed = patrolSpeed;
        patrolWaitTimer = patrolWaitTime;
        currentLoseTimer = losePlayerTime;
        currentFleeTimer = timeToFlee;
        WanderToNewLocation();

        if (player == null)
        {
            Debug.LogError("ENEMY AI: Player Transform is not set! AI will not be able to chase.");
        }
    }
    void Update()
    {
        CheckForFlashlight();

        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                Chase();
                break;
                /*case EnemyState.Flee:
                      Flee();         // unsure if we want flee to despawn enemy or have it return to patrol
                      break;*/

        }
    }
    private void OnEnable()
    {
        SoundManager.OnSoundMade += HearSound;
    }

    private void OnDisable()
    {
        SoundManager.OnSoundMade -= HearSound;
    }
    private void Patrol()
    {
        // Check if the agent has reached its destination
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            // If it has, start the wait timer
            patrolWaitTimer -= Time.deltaTime;
            if (patrolWaitTimer <= 0)
            {
                // Timer is up, find a new random spot and reset the timer
                WanderToNewLocation();
                patrolWaitTimer = patrolWaitTime;
            }
        }
    }
    private void Chase()
    {
        Debug.Log("Chasing");
        if (player == null)
        {
            currentState = EnemyState.Patrol;
            agent.speed = patrolSpeed;
            return;
        }
        // Check if the player is too far away
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            Debug.Log("Attacking the player");
            Attack();
            return;
        }
        if (distanceToPlayer <= losePlayerDistance)
        {
            agent.SetDestination(player.position);

            // Constantly reset the "give up" timer as long as the player is close
            currentLoseTimer = losePlayerTime;
        }
        else
        {
            // Player is "lost" (too far away).
            // The agent will continue to its last set destination.
            // Now, we start the "give up" timer.
            currentLoseTimer -= Time.deltaTime;

            if (currentLoseTimer <= 0)
            {
                // Timer has run out AND the player is still far away.
                // Give up and go back to patrolling.
                currentState = EnemyState.Patrol;
                agent.speed = patrolSpeed;
                patrolWaitTimer = 0;
            }
        }
    }

    private void WanderToNewLocation()
    {
        // Find a random point on the NavMesh within our wander radius
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randomDirection, out navHit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(navHit.position);
        }
    }
    private void HearSound(Vector3 soundPosition)
    {
        // Can't chase if we don't have a reference to the player
        if (player == null) return;

        // Check if the sound is within our hearing range
        float distanceToSound = Vector3.Distance(transform.position, soundPosition);
        if (distanceToSound > hearingRange)
        {
            return; // Too far away, ignore it.
        }

        // Switch to the Chase state and increase speed
        currentState = EnemyState.Chase;
        agent.speed = chaseSpeed;

        currentLoseTimer = losePlayerTime;
    }
    private void Flee()
    {
        if (agent.remainingDistance < agent.stoppingDistance)
        {
            Destroy(gameObject);
        }
    }
    private void CheckForFlashlight()
    {
        if (currentState == EnemyState.Flee) return;

        if (IsVisibleToFlashlight())
        {
            currentFleeTimer -= Time.deltaTime;
            if (currentFleeTimer <= 0)
            {
                TriggerFleeState();
            }
        }
        else
        {
            currentFleeTimer = timeToFlee;
        }
    }

    private bool IsVisibleToFlashlight()
    {
        // Can't be seen if there's no light reference or it's off
        if (playerFlashlight == null || !playerFlashlight.enabled)
            return false;

        // Get direction and distance from the light to us
        Vector3 dirToAI = (transform.position - playerFlashlight.transform.position);
        float distance = dirToAI.magnitude;

        if (distance > playerFlashlight.range)
            return false;

        float angle = Vector3.Angle(playerFlashlight.transform.forward, dirToAI);
        if (angle > playerFlashlight.spotAngle / 2)
            return false;

        if (Physics.Raycast(playerFlashlight.transform.position, dirToAI.normalized, out RaycastHit hit, distance))
        {
            if (hit.transform.root != transform.root)
            {
                // Hit a wall or another object first
                return false;
            }
        }
        return true;
    }

    private void TriggerFleeState()
    {
        currentState = EnemyState.Flee;
        agent.speed = chaseSpeed; // Use chase speed to run away

        Vector3 dirFromPlayer = (transform.position - player.position).normalized;
        Vector3 fleeTarget = transform.position + (dirFromPlayer * fleeDistance);

        // Find the closest valid NavMesh point to this target
        if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit navHit, fleeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(navHit.position);
        }
        else
        {
            // Failsafe: Couldn't find a far point, just pick a random one
            WanderToNewLocation();
        }
    }
    private void Attack()
    {
        // To be implemented: Attack logic
    }
}
