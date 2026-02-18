using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class UnitSelection : MonoBehaviour
{
    public RectTransform selectionBox;
    public LayerMask unitLayer;
    public LayerMask groundLayer;

    private Vector2 startScreenPos;
    private bool isDragging;
    private float dragThreshold = 10f;
    
    private List<Unit> selectedUnits = new List<Unit>();
    private Camera cam;
    void Start()
    {
        cam = Camera.main;
        selectionBox.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        HandleSelection();
        HandleMovement();
    }

    void HandleSelection()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            startScreenPos = Mouse.current.position.ReadValue();
            isDragging = false;
        }

        if (Mouse.current.leftButton.isPressed)
        {
            if (!isDragging &&
                Vector2.Distance(startScreenPos, Mouse.current.position.ReadValue()) > dragThreshold)
            {
                isDragging = true;
                selectionBox.gameObject.SetActive(true);
            }

            if (isDragging)
                UpdateSelectionBox();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (isDragging)
                SelectUnitsInBox();
            else
                ClickSelect();

            selectionBox.gameObject.SetActive(false);
            isDragging = false;
        }
    }
    
    void ClickSelect()
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, unitLayer))
        {
            Unit unit = hit.collider.GetComponent<Unit>();
            if (unit != null)
            {
                if (!Keyboard.current.leftShiftKey.isPressed)
                    DeselectAll();

                if (unit.isSelected)
                {
                    unit.SetSelected(false);
                    selectedUnits.Remove(unit);
                }
                else
                {
                    unit.SetSelected(true);
                    selectedUnits.Add(unit);
                }
                return;
            }
        }

        // clicked empty ground
        if (!Keyboard.current.leftShiftKey.isPressed)
            DeselectAll();
    }
    // ─── BOX SELECT ──────────────────────────────

    void UpdateSelectionBox()
    {
        Vector2 currentPos = Mouse.current.position.ReadValue();
        Vector2 min = Vector2.Min(startScreenPos, currentPos);
        Vector2 max = Vector2.Max(startScreenPos, currentPos);

        selectionBox.anchoredPosition = min;
        selectionBox.sizeDelta = max - min;
    }

    void SelectUnitsInBox()
    {
        if (!Keyboard.current.leftShiftKey.isPressed)
            DeselectAll();

        Vector2 min = Vector2.Min(startScreenPos, (Vector2)Mouse.current.position.ReadValue());
        Vector2 max = Vector2.Max(startScreenPos, (Vector2)Mouse.current.position.ReadValue());
        Rect selectionRect = new Rect(min, max - min);

        Unit[] allUnits = FindObjectsOfType<Unit>();
        foreach (Unit unit in allUnits)
        {
            // check two points: feet and head
            Vector2 feet = cam.WorldToScreenPoint(unit.transform.position);
            Vector2 head = cam.WorldToScreenPoint(unit.transform.position + Vector3.up * 2f);

            // build a small screen rect for the unit
            Vector2 unitMin = Vector2.Min(feet, head);
            Vector2 unitMax = Vector2.Max(feet, head);
            // give it some width
            unitMin.x -= 20f;
            unitMax.x += 20f;

            Rect unitRect = new Rect(unitMin, unitMax - unitMin);

            if (selectionRect.Overlaps(unitRect))
            {
                unit.SetSelected(true);
                if (!selectedUnits.Contains(unit))
                    selectedUnits.Add(unit);
            }
        }
    }

    void DeselectAll()
    {
        foreach (Unit unit in selectedUnits)
            unit.SetSelected(false);
        selectedUnits.Clear();
    }

    // ─── MOVEMENT ────────────────────────────────

    void HandleMovement()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame && selectedUnits.Count > 0)
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                Vector3 target = hit.point;

                for (int i = 0; i < selectedUnits.Count; i++)
                {
                    // simple offset so units don't all stack
                    Vector3 offset = GetFormationOffset(i, selectedUnits.Count);
                    Vector3 destination = target + offset;

                    // if using NavMesh:
                    // unit.GetComponent<NavMeshAgent>().SetDestination(destination);

                    // placeholder - you'd replace with your movement system
                    selectedUnits[i].transform.position = destination;
                }
            }
        }
    }

    Vector3 GetFormationOffset(int index, int total)
    {
        if (total <= 1) return Vector3.zero;

        int columns = Mathf.CeilToInt(Mathf.Sqrt(total));
        float spacing = 1.5f;
        int row = index / columns;
        int col = index % columns;

        float xOffset = (col - columns / 2f) * spacing;
        float zOffset = (row - columns / 2f) * spacing;

        return new Vector3(xOffset, 0, zOffset);
    }
}
