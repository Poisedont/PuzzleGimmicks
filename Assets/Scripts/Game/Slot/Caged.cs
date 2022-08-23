using System.Collections;
using UnityEngine;

public class Caged : IBlock
{
    int eventCountBorn = 0;
    bool destroying = false;

    SlotGravity gravity;
    public Sprite[] sprites; // Images of blocks of different levels. The size of the array must be equal to max level
    SpriteRenderer spriteRender;

    public override void BlockCrush(bool force)
    {
        if (eventCountBorn == Session.Instance.eventCount && !force) return;
        if (destroying) return;

        eventCountBorn = Session.Instance.eventCount;
        level--;
        
        if (level == 0)
        {


            slot.SetScore(1);
            SlotGravity.Reshading();

            StartCoroutine(DestroyingRoutine());

            return;
        }
        if (level > 0)
        {
            // TODO: Audio play("BlockHit");
            spriteRender.sprite = sprites[level - 1];
        }
    }

    IEnumerator DestroyingRoutine()
    {
        GameObject o = ContentManager.Instance.GetItem("CageBreak");
        if (o) o.transform.position = transform.position;

        int ptarget = LevelProfile.main.GetTargetCount(0, FieldTarget.Cage);

        if (ptarget > 0)
        {
            TargetUI[] listO = GameObject.FindObjectsOfType<TargetUI>();

            TargetUI go = null;

            for (int i = 0; i < listO.Length; i++)
            {
                if (listO[i].GetLvlTarget().GetTarget().Equals(FieldTarget.Cage))
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
        Session.Instance.CollectCaged();
        Destroy(gameObject);
    }

    public override bool CanBeCrushedByNearSlot(Chip near = null)
    {
        return false;
    }

    public override bool CanItContainChip()
    {
        return true;
    }

    public override int GetLevels()
    {
        return level;
    }

    public override void Initialize(SlotSettings settings = null)
    {
        gravity = slot.GetComponent<SlotGravity>();
        gravity.enabled = false;
        eventCountBorn = Session.Instance.eventCount;
        level = 1;
    }

    private void OnDestroy()
    {
        gravity.enabled = true;
    }
}