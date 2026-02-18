using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    public enum Team
    {
        Player,
        Zombie
    }
    public static event Action<Unit> OnUnitDied;

    public Team team;
    public float baseSpeed = 3.0f;
    public float baseMaxHealth = 100.0f;
    public float currentHealth;
    
    protected NavMeshAgent agent;
    protected Animator animator;

    public static List<Unit> playerUnits = new List<Unit>();
    public static List<Unit> zombieUnits = new List<Unit>();

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.speed = baseSpeed;
        currentHealth = baseMaxHealth;
    }

    protected virtual void OnEnable()
    {
        if (team == Team.Zombie)
        {
            zombieUnits.Add(this);
        } else if (team == Team.Player)
        {
            playerUnits.Add(this);
        }
    }

    protected void OnDisable()
    {
        playerUnits.Remove(this);
        zombieUnits.Remove(this);
    }

    protected virtual void Update()
    {
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);
    }

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        OnUnitDied?.Invoke(this);
        Destroy(gameObject);
    }

}
