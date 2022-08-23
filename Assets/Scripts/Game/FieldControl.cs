using UnityEngine;
using UnityEngine.EventSystems;

public class FieldControl : Singleton<FieldControl>
{
    RaycastHit2D hit;
    public Camera controlCamera;

    Slot pressedSlot;
    Vector2 pressPoint;

    public static System.Action<Chip, Side> swap = delegate { };

    private void Start()
    {
        controlCamera = Camera.main;

        swap += Chip.Swap;
    }
    private void Update()
    {
        if (Time.timeScale == 0f) return;
        TouchUpdate();

    }

    void TouchUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject(-1)) return;
            if (EventSystem.current.IsPointerOverGameObject(0)) return;
            Vector2 point = controlCamera.ScreenPointToRay(Input.mousePosition).origin;
            hit = Physics2D.Raycast(point, Vector2.zero);
            if (!hit.transform) return;
            pressedSlot = hit.transform.GetComponent<Slot>();
            pressPoint = Input.mousePosition;

        }
        if (Input.GetMouseButton(0) && pressedSlot != null)
        {
            Vector2 move = Input.mousePosition;
            move -= pressPoint;
            if (move.magnitude > Screen.height * 0.05f)
            {
                foreach (Side side in Utils.straightSides)
                {
                    if (Vector2.Angle(move, Utils.SideOffsetX(side) * Vector2.right + Utils.SideOffsetY(side) * Vector2.up) <= 45)
                    {
                        if (pressedSlot.chip)
                        {
                            swap.Invoke(pressedSlot.chip, side);
                        }
                    }
                }

                pressedSlot = null;
            }
        }
    }

    public Slot GetSlotFromTouch()
    {
        Vector2 point;
        point = controlCamera.ScreenPointToRay(Input.mousePosition).origin;

        hit = Physics2D.Raycast(point, Vector2.zero);
        if (!hit.transform) return null;
        return hit.transform.GetComponent<Slot>();
    }
}