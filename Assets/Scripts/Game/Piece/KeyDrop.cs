using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyDrop : MonoBehaviour, IChipLogic
{
    private Chip m_chip;
    private bool m_live = false;
    public Chip chip { get { return m_chip; } }
    public bool alive
    {
        get { return m_live; }
        set
        {
            if (m_live == value) return;
            m_live = value;
            if (m_live)
            {
                ++s_alive_key_count;
            }
            else
            {
                --s_alive_key_count;
            }
        }
    }

    public static int s_alive_key_count = 0;
    ////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        m_chip = GetComponent<Chip>();
        chip.destroyable = false;
        alive = true;
        //TODO: audio play Key appear
    }

    private void Update()
    {
        if (chip.destroying) return;
        if (!chip.slot) return;
        if (!Session.Instance.CanIWait()) return;
        if (chip.slot.sugarDropSlot && transform.localPosition == Vector3.zero)
        {
            chip.destroyable = true;
            chip.DestroyChip();
        }
    }
    public IEnumerator Destroying()
    {
        chip.IsBusy = true;
        // Audio play("Key collect");
        SoundManager.PlaySFX(SoundDefine.k_key_drop);

        yield return new WaitForSeconds(0.2f);
        chip.IsBusy = false;

        chip.ParentRemove();

        float velocity = 0;
        Vector3 impuls = new Vector3(Random.Range(-3f, 3f), Random.Range(1f, 5f), 0);
        impuls += chip.impulse;
        chip.impulse = Vector3.zero;
        foreach (SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>())
        {
            sprite.sortingLayerName = "UI";
        }

        float rotationSpeed = Random.Range(-30f, 30f);
        float growSpeed = Random.Range(0.2f, 0.8f);


        int ptarget = LevelProfile.main.GetTargetCount(0, FieldTarget.KeyDrop);

        if (ptarget > 0)
        {
            TargetUI[] listO = GameObject.FindObjectsOfType<TargetUI>();

            TargetUI go = null;

            for (int i = 0; i < listO.Length; i++)
            {
                if (listO[i].GetLvlTarget().GetTarget().Equals(FieldTarget.KeyDrop))
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
            else
            {
                while (transform.position.y > -10)
                {
                    velocity += Time.deltaTime * 20;
                    velocity = Mathf.Min(velocity, 40); //limit speed 
                    transform.position += impuls * Time.deltaTime * transform.localScale.x;
                    transform.position -= Vector3.up * Time.deltaTime * velocity * transform.localScale.x;
                    transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
                    transform.localScale += Vector3.one * growSpeed * Time.deltaTime;
                    yield return 0;
                }
            }
        }
        else
        {
            while (transform.position.y > -10)
            {
                velocity += Time.deltaTime * 20;
                velocity = Mathf.Min(velocity, 40); //limit speed 
                transform.position += impuls * Time.deltaTime * transform.localScale.x;
                transform.position -= Vector3.up * Time.deltaTime * velocity * transform.localScale.x;
                transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
                transform.localScale += Vector3.one * growSpeed * Time.deltaTime;
                yield return 0;
            }
        }
        Session.Instance.CollectKey();
        Destroy(gameObject);
    }

    public string GetChipType()
    {
        return "Key";
    }

    public List<Chip> GetDangeredChips(List<Chip> stack)
    {
        stack.Add(m_chip);
        return stack;
    }

    public int GetPotencial()
    {
        return 0;
    }

    public bool IsMatchable()
    {
        return false;
    }

    private void OnDestroy()
    {
        alive = false;
    }
}