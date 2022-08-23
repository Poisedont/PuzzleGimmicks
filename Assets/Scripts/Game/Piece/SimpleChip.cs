using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleChip : MonoBehaviour, IChipLogic
{
    Chip m_chip;
    public Chip chip { get { return m_chip; } }

    ////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        m_chip = GetComponent<Chip>();
    }
    public List<Chip> GetDangeredChips(List<Chip> stack)
    {
        stack.Add(chip);
        return stack;
    }

    public string GetChipType()
    {
        return "SimpleChip";
    }

    public int GetPotencial()
    {
        return 1;
    }

    public bool IsMatchable()
    {
        return true;
    }

    public IEnumerator Destroying()
    {
        chip.IsBusy = true;
        yield return null;

        chip.PlayAnim("StartDestroy");
        yield return new WaitForSeconds(chip.TimePlaying("StartDestroy"));

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
        Destroy(gameObject);
    }

}