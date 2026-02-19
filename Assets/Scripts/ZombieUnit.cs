using UnityEngine;
using UnityEngine.AI;

public class ZombieUnit : Unit
{
    public float detectionRange = 15f;
    public float attackRange = 1.5f;
    public float attackDamage = 10f;
    public float attackCooldown = 2f;
    public float wanderRadius = 10f;
    public float wanderTimer = 3f;

    private float _timer;
    private float _attackTimer;
    private float _updateInterval = 0.25f;
    private float _updateTimer;
    private float _attackAngle;

    private LayerMask _playerLayerMask;
    private readonly Collider[] _detectionResults = new Collider[20];

    protected override void Awake()
    {
        base.Awake();
        team = Team.Zombie;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
        agent.stoppingDistance = attackRange * 0.8f;
        _updateTimer = Random.Range(0f, _updateInterval);
        
        float hash = GetInstanceID() * 0.618f;
        _attackAngle = hash % (2f * Mathf.PI);

        _playerLayerMask = LayerMask.GetMask("Player");
    }
    
    protected override void Update()
    {
        base.Update();

        _timer += Time.deltaTime;
        _updateTimer += Time.deltaTime;
        _attackTimer += Time.deltaTime;
        if (_updateTimer < _updateInterval)
        {
            return;
        }
        _updateTimer = 0f;

        Transform target = FindClosestPlayer();
        
        if (target == null)
        {
            Wander();
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        if (distanceToTarget < detectionRange)
        {
            if (distanceToTarget <= attackRange)
            {
                agent.ResetPath();
                Attack(target);
            }
            else
            {
                float radius = attackRange * 0.8f;
                Vector3 offset = new Vector3(Mathf.Cos(_attackAngle) * radius, 0, Mathf.Sin(_attackAngle) * radius);
                agent.SetDestination(target.position + offset);
            }
        }
    }

    private Transform FindClosestPlayer()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, detectionRange, _detectionResults, _playerLayerMask);

        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        for (int i = 0; i < count; i++)
        {
            float dist = (transform.position - _detectionResults[i].transform.position).sqrMagnitude;
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = _detectionResults[i].transform;
            }
        }

        return closest;
    }
    
    private void Wander()
    {
        if (_timer >= wanderTimer)
        {
            float chanceThreshold = 0.1f;
            if (Random.value < chanceThreshold)
            {
                Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
                Vector3 randomPoint = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                    agent.SetDestination(hit.position);
            }
            _timer = 0;
        }
    }

    private void Attack(Transform target)
    {
        if (_attackTimer >= attackCooldown)
        {
            Unit targetUnit = target.GetComponent<Unit>();
            if (targetUnit != null)
            {
                targetUnit.TakeDamage(attackDamage);
                _attackTimer = 0f;
            }
        }
    }
}
