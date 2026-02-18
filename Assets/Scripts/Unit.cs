using UnityEngine;

public class Unit : MonoBehaviour
{
    public bool isSelected;
    public GameObject selectionIndicator;

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(selected);
        }
    }
}
