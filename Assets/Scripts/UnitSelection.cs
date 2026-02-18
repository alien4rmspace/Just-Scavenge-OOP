using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

public class UnitSelection : MonoBehaviour
{
    public RectTransform selectionBox;
    public LayerMask unitLayer;
    public LayerMask groundLayer;

    private Vector2 _startScreenPos;
    private bool _isDragging;
    private float _dragThreshold = 10f;
    
    private List<PlayerUnit> _selectedUnits = new List<PlayerUnit>();
    private Camera _cam;
    void OnEnable()
    {
        Unit.OnUnitDied += HandleUnitDied;
    }

    void OnDisable()
    {
        Unit.OnUnitDied -= HandleUnitDied;
    }
    void Start()
    {
        _cam = Camera.main;
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
            _startScreenPos = Mouse.current.position.ReadValue();
            _isDragging = false;
        }

        if (Mouse.current.leftButton.isPressed)
        {
            if (!_isDragging &&
                Vector2.Distance(_startScreenPos, Mouse.current.position.ReadValue()) > _dragThreshold)
            {
                _isDragging = true;
                selectionBox.gameObject.SetActive(true);
            }

            if (_isDragging)
                UpdateSelectionBox();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (_isDragging)
                SelectUnitsInBox();
            else
                ClickSelect();

            selectionBox.gameObject.SetActive(false);
            _isDragging = false;
        }
    }
    
    void ClickSelect()
    {
        Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, unitLayer))
        {
            PlayerUnit unit = hit.collider.GetComponent<PlayerUnit>();
            if (unit != null)
            {
                if (!Keyboard.current.leftShiftKey.isPressed)
                    DeselectAll();

                if (unit.isSelected)
                {
                    unit.SetSelected(false);
                    _selectedUnits.Remove(unit);
                }
                else
                {
                    unit.SetSelected(true);
                    _selectedUnits.Add(unit);
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
        Vector2 min = Vector2.Min(_startScreenPos, currentPos);
        Vector2 max = Vector2.Max(_startScreenPos, currentPos);

        selectionBox.anchoredPosition = min;
        selectionBox.sizeDelta = max - min;
    }

    void SelectUnitsInBox()
    {
        if (!Keyboard.current.leftShiftKey.isPressed)
            DeselectAll();

        Vector2 min = Vector2.Min(_startScreenPos, Mouse.current.position.ReadValue());
        Vector2 max = Vector2.Max(_startScreenPos, Mouse.current.position.ReadValue());
        Rect selectionRect = new Rect(min, max - min);

        foreach (PlayerUnit unit in Unit.playerUnits.OfType<PlayerUnit>())
        {
            // check two points: feet and head
            Vector2 feet = _cam.WorldToScreenPoint(unit.transform.position);
            Vector2 head = _cam.WorldToScreenPoint(unit.transform.position + Vector3.up * 2f);

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
                if (!_selectedUnits.Contains(unit))
                    _selectedUnits.Add(unit);
            }
        }
    }

    void DeselectAll()
    {
        foreach (PlayerUnit unit in _selectedUnits)
            unit.SetSelected(false);
        _selectedUnits.Clear();
    }

    // ─── MOVEMENT ────────────────────────────────

    void HandleMovement()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame && _selectedUnits.Count > 0)
        {
            Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            int layerMask = groundLayer;
            
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                Vector3 target = hit.point;

                for (int i = 0; i < _selectedUnits.Count; i++)
                {
                    // simple offset so units don't all stack
                    Vector3 offset = GetFormationOffset(i, _selectedUnits.Count);
                    _selectedUnits[i].MoveTo(target + offset);
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
    void HandleUnitDied(Unit unit)
    {
        PlayerUnit playerUnit = unit as PlayerUnit; // Safe cast as PlayerUnit
        if (playerUnit != null && _selectedUnits.Contains(playerUnit))
        {
            _selectedUnits.Remove(playerUnit);
        }
    }
}
