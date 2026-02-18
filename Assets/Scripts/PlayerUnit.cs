using UnityEngine;
using UnityEngine.AI;

public class PlayerUnit : Unit
{
    public bool isSelected;
    public GameObject selectionIndicator;
    protected override void Awake()
    {
        base.Awake();
        team = Team.Player;
    }

    protected override void Update()
    {
        base.Update();
    }
    
    public void SetSelected(bool selection)
    {
        isSelected = selection;
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(selection);
        }
    }
    
    public void MoveTo(Vector3 destination)
    {
        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            Debug.Log("Can't reach that location");
        }
    }
}
