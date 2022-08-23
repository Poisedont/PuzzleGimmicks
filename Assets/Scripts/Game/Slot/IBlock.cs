using UnityEngine;
using System.Collections;

public abstract class IBlock : MonoBehaviour {
    public Slot slot;

    public int level = 1;
    abstract public void BlockCrush(bool force);
    abstract public bool CanBeCrushedByNearSlot(Chip near = null);
    abstract public void Initialize(SlotSettings settings = null);
    abstract public bool CanItContainChip();
    abstract public int GetLevels();
    virtual public bool IsCastShadow() { return true; }
}
