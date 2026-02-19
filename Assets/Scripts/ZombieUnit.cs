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

    protected override void Awake()
    {
        base.Awake();
        team = Team.Zombie;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
        agent.stoppingDistance = attackRange * 0.8f;
        _updateTimer = Random.Range(0f, _updateInterval);
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
                float angle = GetAttackAngle();
                float radius = attackRange * 0.8f;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                agent.SetDestination(target.position + offset);
            }
        }
    }

    private Transform FindClosestPlayer()
    {
        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (Unit unit in playerUnits)
        {
            float dist = Vector3.Distance(transform.position, unit.transform.position);
            if (dist < closestDistance && dist < detectionRange)
            {
                closestDistance = dist;
                closest = unit.transform;
            }
        }

        return closest;
    }
    
    float GetAttackAngle()
    {
        // use this zombie's instance ID to get a unique but consistent angle
        float hash = GetInstanceID() * 0.618f; // golden ratio for nice distribution
        return hash % (2f * Mathf.PI);
    }

    private void Wander()
    {
        if (_timer >= wanderTimer)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * wanderRadius;
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
            _timer = 0;
        }
    }

    private void Attack(Transform target)
    {
        _attackTimer += Time.deltaTime;
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
