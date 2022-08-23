using System.Collections;
using UnityEngine;

public class Curtain : IBlock
{

    public Sprite[] m_sprites; // Images of blocks of different type: begin, middle, end
    SpriteRenderer m_spriteRender;
    int m_eventCountBorn = 0;
    Animation anim;
    private bool destroying;

    ////////////////////////////////////////////////////////////////////////////////

    public override void BlockCrush(bool force)
    {
        if (m_eventCountBorn == Session.Instance.eventCount && !force) return;
        if (destroying) return;

        m_eventCountBorn = Session.Instance.eventCount;
        level--;

        if (level == 0)
        {
            slot.block = null;
            SlotGravity.Reshading();

            StartCoroutine(DestroyingRoutine());
            return;
        }
        if (level > 0)
        {
            if (anim) anim.Play("CurtainOpen");
            // TODO: Audio play("BlockHit");
            m_spriteRender.sprite = m_sprites[level - 1];
        }
    }

    IEnumerator DestroyingRoutine()
    {

        destroying = true;

        int ptarget = LevelProfile.main.GetTargetCount(0, FieldTarget.Curtain);

        if (ptarget > 0)
        {
            TargetUI[] listO = GameObject.FindObjectsOfType<TargetUI>();

            TargetUI go = null;

            for (int i = 0; i < listO.Length; i++)
            {
                if (listO[i].GetLvlTarget().GetTarget().Equals(FieldTarget.Curtain))
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
        Session.Instance.CollectCurtain();
        Destroy(gameObject);
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
        m_spriteRender = GetComponent<SpriteRenderer>();
        m_eventCountBorn = Session.Instance.eventCount;
        m_spriteRender.sprite = m_sprites[level - 1];
        anim = GetComponent<Animation>();
    }

    public override bool IsCastShadow()
    {
        return false;
    }

}