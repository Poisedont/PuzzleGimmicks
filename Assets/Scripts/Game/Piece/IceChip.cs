using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceChip : MonoBehaviour, IChipLogic
{
    Chip m_chip;
    public Chip chip { get { return m_chip; } }

    [Tooltip("ICE chip type: should only be 1 in 4 Ice types")]
    [SerializeField] EPieces m_chipType;
    [SerializeField] SpriteRenderer m_spriteRender;
    [SerializeField] SpriteRenderer m_iceRender;
    [SerializeField] Sprite[] m_iceSprites;

    int m_level;
    int m_eventBirth;
    WaitForSeconds m_waitForIceBreak = new WaitForSeconds(0.2f);
    ////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        m_chip = GetComponent<Chip>();
        int lv = m_chipType - EPieces.HalfIceChip + 1;
        if (lv > 0 && lv <= 2)
        {
            m_level = lv;
        }
        else
        {
            Debug.LogWarning("set Ice chip type wrong!!!");
        }
    }

    private void Start()
    {
        m_eventBirth = Session.Instance.eventCount;
    }

    public IEnumerator Destroying()
    {
        if (m_eventBirth == Session.Instance.eventCount)
        {
            chip.destroying = false;
            yield break;
        }

        m_eventBirth = Session.Instance.eventCount;

        m_level--;
        if (m_level >= 0)
        {
            chip.destroying = false;
            chip.IsBusy = true;

            GameObject Obj = ContentManager.Instance.GetItem("IceDropEffects");

            ParticleSystem iceEffects = Obj.GetComponent<ParticleSystem>();
            Obj.transform.position = transform.position;
            iceEffects.Play();

            if (m_iceRender)
            {
                if (m_level == 0) m_iceRender.gameObject.SetActive(false);
                else
                    m_iceRender.sprite = m_iceSprites[m_level - 1];
            }

            yield return new WaitForSeconds(iceEffects.duration);
            if(m_level == 0)
            {
                Session.Instance.CollectIce();
            }
            Destroy(Obj);
            //yield return m_waitForIceBreak; // wait for change color

            chip.IsBusy = false;
        }
        else
        {
            //work normally like a SimpleChip
            chip.ParentRemove();
            chip.IsBusy = true;

            int targetColor = LevelProfile.main.GetTargetCount(chip.id, FieldTarget.Color);

            if (chip.IsColored() && targetColor > 0)
            {
                TargetUI[] listO = GameObject.FindObjectsOfType<TargetUI>();

                TargetUI go = null;

                for (int i = 0; i < listO.Length; i++)
                {
                    if (listO[i].GetLvlTarget().GetTarget().Equals(FieldTarget.Color))
                    {
                        if (listO[i].GetLvlTarget().GetTargetCount(chip.id) > 0
                            && listO[i].GetIndexTaret() == chip.id
                            && listO[i].GetLvlTarget().GetTargetCount(chip.id) > listO[i].GetLvlTarget().GetCurrentCount(chip.id)
                            )
                            go = listO[i];
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

            // animation chip destroy and collect
            chip.PlayAnim("destroying");

            yield return new WaitForSeconds(chip.TimePlaying("destroying"));

            // chip.CompleteAnim("destroying");

            SoundManager.PlaySFX(SoundDefine.k_3m);
        }
    }

    public string GetChipType()
    {
        if (m_level == 0)
            return "SimpleChip";
        return "IceChip";
    }

    public List<Chip> GetDangeredChips(List<Chip> stack)
    {
        stack.Add(chip);
        return stack;
    }

    public int GetPotencial()
    {
        return 1;
    }

    public bool IsMatchable()
    {
        return true;
    }
}