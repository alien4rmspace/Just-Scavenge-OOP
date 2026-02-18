using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    public enum Team
    {
        Player,
        Zombie
    }

    public Team team;
    public bool isSelected;
    public GameObject selectionIndicator;
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(selected);
        }
    }

    public void MoveTo(Vector3 destination)
    {
        agent.SetDestination(destination);
    }
}
