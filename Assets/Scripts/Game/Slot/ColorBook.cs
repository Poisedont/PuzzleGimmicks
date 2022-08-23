using System.Collections;
using UnityEngine;

public class ColorBook : IBlock
{
    int eventCountBorn;
    Animation anim;
    bool destroying = false;
    [SerializeField] string crush_effect;

    [SerializeField] Chip.EChipType m_bookColor;
    ////////////////////////////////////////////////////////////////////////////////

    public override void Initialize(SlotSettings settings = null)
    {
        eventCountBorn = Session.Instance.eventCount;
        anim = GetComponent<Animation>();
    }
    public override void BlockCrush(bool force)
    {
        if (destroying) return;

        if (eventCountBorn == Session.Instance.eventCount && !force) return;

        slot.SetScore(1);
        slot.block = null;
        SlotGravity.Reshading();
        StartCoroutine(DestroyingRoutine());
    }

    public override bool CanBeCrushedByNearSlot(Chip near = null)
    {
        if (near)
        {
            return near.id == (int)m_bookColor;
        }
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

    IEnumerator DestroyingRoutine()
    {
        destroying = true;
        if (!string.IsNullOrEmpty(crush_effect))
        {
            GameObject o = ContentManager.Instance.GetItem(crush_effect);
            if (o) o.transform.position = transform.position;

            int ptarget = LevelProfile.main.GetTargetCount(0, FieldTarget.ColorBlocker);

            if (ptarget > 0)
            {
                TargetUI[] listO = GameObject.FindObjectsOfType<TargetUI>();

                TargetUI go = null;

                for (int i = 0; i < listO.Length; i++)
                {
                    if (listO[i].GetLvlTarget().GetTarget().Equals(FieldTarget.ColorBlocker))
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
            Session.Instance.CollectColorBook();
            Destroy(o);
            //anim.Play("BlockDestroy");
            // TODO: Audio play("BlockCrush");
            //while (anim.isPlaying)
            //{
            //    yield return 0;
            //}
            
        }

        Destroy(gameObject);
    }

}