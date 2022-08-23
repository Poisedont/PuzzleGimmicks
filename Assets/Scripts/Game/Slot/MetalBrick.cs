using System.Collections;
using UnityEngine;

public class MetalBrick : IBlock
{
    private bool destroying;
    public override void BlockCrush(bool force)
    {
        if (!force) return;
        if (destroying) return;

        destroying = true;

        slot.block = null;
        SlotGravity.Reshading();

        StartCoroutine(PlayEffectRockLayer(transform.position));
        //GameObject effect = ContentManager.Instance.GetItem("metalCrush");
        //if (effect) effect.transform.position = transform.position;

        //Destroy(gameObject);
    }

    IEnumerator PlayEffectRockLayer(Vector3 pos)
    {
        gameObject.transform.Find("image").gameObject.SetActive(false);
        GameObject Obj = ContentManager.Instance.GetItem("MetalDropEffects");

        ParticleSystem rockEffects = Obj.GetComponent<ParticleSystem>();
        Obj.transform.position = pos;
        rockEffects.Play();

        yield return new WaitForSeconds(rockEffects.duration);

        rockEffects.Stop();
        Destroy(Obj);
        Session.Instance.CollectMetal();
        Destroy(gameObject);
    }

    public override bool CanBeCrushedByNearSlot(Chip near = null)
    {
        return false;
    }

    public override bool CanItContainChip()
    {
        return false;
    }

    public override int GetLevels()
    {
        return 1;
    }

    public override void Initialize(SlotSettings settings = null)
    {
    }

    public override bool IsCastShadow()
    {
        return base.IsCastShadow();
    }

}