using System.Collections;
using UnityEngine;

public class BookBlock : IBlock
{
    public Sprite[] sprites; // Images of blocks of different levels. The size of the array must be equal to max level

    SpriteRenderer spriteRender;
    int eventCountBorn;
    Animation anim;
    bool destroying = false;
    public string crush_effect;
    public GameObject m_bookFly;

    public override void Initialize(SlotSettings settings = null)
    {
        spriteRender = GetComponent<SpriteRenderer>();
        eventCountBorn = Session.Instance.eventCount;
        spriteRender.sprite = sprites[level - 1];
        anim = GetComponent<Animation>();
    }
    public override void BlockCrush(bool force)
    {
        if (destroying) return;

        if (eventCountBorn == Session.Instance.eventCount && !force) return;

        eventCountBorn = Session.Instance.eventCount;
        
        level--;
        if (level == 0)
        {
            slot.SetScore(1);
            slot.block = null;
            SlotGravity.Reshading();
            StartCoroutine(DestroyingRoutine());
            return;
        }
        if (level > 0)
        {
            if (anim) anim.Play("BlockCrush");
           
            // TODO: Audio play("BlockHit");
            spriteRender.sprite = sprites[level - 1];
            StartCoroutine(DestroyingRoutine());
        }
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
        return 3;
    }

    IEnumerator DestroyingRoutine()
    {
        //destroying = true;
        if (!string.IsNullOrEmpty(crush_effect))
        {
            GameObject o = ContentManager.Instance.GetItem(crush_effect);
            o.transform.position = transform.position;

            ParticleSystem effect = o.GetComponent<ParticleSystem>();

            effect.Play();

            int ptarget = LevelProfile.main.GetTargetCount(0, FieldTarget.FixBlock);

            if (ptarget > 0)
            {
                TargetUI[] listO = GameObject.FindObjectsOfType<TargetUI>();

                TargetUI go = null;

                for (int i = 0; i < listO.Length; i++)
                {
                    if (listO[i].GetLvlTarget().GetTarget().Equals(FieldTarget.FixBlock))
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

                    GameObject book = Instantiate(m_bookFly.gameObject);
                    book.SetActive(true);
                    if (level == 0)
                    {
                        gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    }

                    //sprite.sortingLayerName = "UI";
                    //sprite.sortingOrder = 10;

                    float time = 0;
                    float speed = Random.Range(1f, 1.8f);
                    Vector3 startPosition = transform.position;
                    Vector3 targetPosition = target.position;

                    while (time < 1)
                    {
                        book.transform.position = Vector3.Lerp(startPosition, targetPosition, EasingFunctions.easeInOutQuad(time));
                        time += Time.unscaledDeltaTime * speed;
                        yield return 0;
                    }

                    book.transform.position = target.position;
                    Destroy(book);
                }
            }
            else
            {
                if (level == 0)
                {
                    gameObject.GetComponent<SpriteRenderer>().enabled = false;
                }
                yield return new WaitForSeconds(effect.duration);
            }
            //yield return new WaitForSeconds(effect.duration);
            Session.Instance.CollectBook();
            effect.Stop();
            Destroy(o);
            //anim.Play("BlockDestroy");
            // TODO: Audio play("BlockCrush");
            //while (anim.isPlaying)
            //{
            //    yield return 0;
            //}

        }
        if (level == 0)
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

}