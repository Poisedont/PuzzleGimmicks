using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smoke : IBlock
{
    public static List<Smoke> all = new List<Smoke>();

    int eventCountBorn;

    bool destroying = false;

    Animation anim;
    public string crush_effect;

    public static int seed = 0; // count for smoke should be spread
    public static int lastSmokeCrush = -1;
    public override void BlockCrush(bool force)
    {
        if (eventCountBorn == Session.Instance.eventCount && !force) return;
        if (destroying) return;

        lastSmokeCrush = Session.Instance.swapEvent;

        eventCountBorn = Session.Instance.eventCount;

        slot.SetScore(1);
        StartCoroutine(DestroyingRoutine());
    }

    public override bool CanBeCrushedByNearSlot(Chip near = null)
    {
        return true;
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
        eventCountBorn = Session.Instance.eventCount;
        StartCoroutine(PlayAnimSmoke());
        all.Add(this);
        if (lastSmokeCrush == -1)
        {
            lastSmokeCrush = Session.Instance.swapEvent;
        }
    }

    IEnumerator PlayAnimSmoke()
    {
        GameObject Obj = ContentManager.Instance.GetItem("SmokeBlue");
        ParticleSystem blueEffects = Obj.GetComponent<ParticleSystem>();
        Obj.transform.position = transform.position;
        blueEffects.Play();
        yield return new WaitForSeconds(blueEffects.duration);
        blueEffects.Stop();
        Destroy(blueEffects.gameObject);
        Destroy(Obj);
    }

    private void OnDestroy()
    {
        all.Remove(this);
    }

    IEnumerator DestroyingRoutine()
    {
        destroying = true;

        GameObject o = ContentManager.Instance.GetItem(crush_effect);
        if (o) o.transform.position = transform.position;

        int ptarget = LevelProfile.main.GetTargetCount(0, FieldTarget.Smoke);

        if (ptarget > 0)
        {
            TargetUI[] listO = GameObject.FindObjectsOfType<TargetUI>();

            TargetUI go = null;

            for (int i = 0; i < listO.Length; i++)
            {
                if (listO[i].GetLvlTarget().GetTarget().Equals(FieldTarget.Smoke))
                {
                    if (listO[i].GetLvlTarget().GetTargetCount(0) > 0
                       && listO[i].GetIndexTaret() == 0
                       && listO[i].GetLvlTarget().GetTargetCount(0) > listO[i].GetLvlTarget().GetCurrentCount(0)
                       )
                    {
                        go = listO[i];
                    }
                }
            }

            if (go)
            {
                Transform target = go.transform;

                //sprite.sortingLayerName = "UI";
                //sprite.sortingOrder = 10;

                float time = 0;
                float speed = Random.Range(1f, 1.8f);
                Vector3 startPosition = transform.position;
                Vector3 targetPosition = target.position;

                while (time < 1)
                {
                    transform.position = Vector3.Lerp(startPosition, targetPosition, EasingFunctions.easeInOutQuad(time));
                    time += Time.unscaledDeltaTime * speed;
                    yield return 0;
                }

                transform.position = target.position;
            }
        }

        // TODO: Audio play("SmokeCrush");
        if (anim)
        {
            anim.Play("Destroy");
            while (anim.isPlaying)
            {
                yield return 0;
            }
        }

        slot.block = null;
        SlotGravity.Reshading();
        Destroy(gameObject);
    }

    public static void Spread()
    {
        List<Slot> slots = new List<Slot>();

        foreach (Smoke smoke in all)
        {
            foreach (Side side in Utils.straightSides)
            {
                if (smoke.slot[side] && !smoke.slot[side].block
                    && !(smoke.slot[side].chip &&
                            (smoke.slot[side].chip.chipType == "Key" // can't eat 'Key'
                            || smoke.slot[side].chip.chipType == EPieces.SmokePot.ToString()
                            || smoke.slot[side].chip.chipType == EPieces.Compass.ToString()
                            )
                        )
                    )
                {
                    slots.Add(smoke.slot[side]);
                }
            }
        }

        while (seed > 0)
        {
            if (slots.Count == 0) return;

            Slot target = slots.GetRandom();
            slots.Remove(target);

            if (target.chip)
            {
                target.chip.HideChip(false);
            }

            Smoke newSmoke = ContentManager.Instance.GetItem<Smoke>("Smoke");
            newSmoke.transform.position = target.transform.position;
            newSmoke.name = "newSmoke";
            newSmoke.transform.parent = target.transform;
            target.block = newSmoke;
            newSmoke.slot = target;
            // TODO: Audio play("SmokeAdd");
            newSmoke.Initialize();

            seed--;
        }

    }

    public static void Cleanup()
    {
        if (all != null)
        {
            all.Clear();
        }
    }
}