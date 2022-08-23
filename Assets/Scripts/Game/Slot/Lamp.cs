using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lamp : IBlock
{
    [SerializeField] GameObject m_fireObject;
    [SerializeField] Animation m_animFire;

    bool m_isLight;
    static List<Lamp> s_allLamps;
    public override void BlockCrush(bool force)
    {
        if (m_isLight) return;

        if (m_fireObject)
        {
            m_fireObject.SetActive(true);
            if (m_animFire)
            {
                m_animFire.Play();
            }
        }

        m_isLight = true;

        if (IsAllLampLight())
        {
            StartCoroutine(DestroyAllLamps());
        }
    }

    public override bool CanBeCrushedByNearSlot(Chip near = null)
    {
        if (near == null) { return false; }
        if (m_isLight) return false;

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
        if (s_allLamps == null)
        {
            s_allLamps = new List<Lamp>();
        }
        s_allLamps.Add(this);
        m_isLight = false;
        if (!m_fireObject)
        {
            Debug.LogError("Lamp has no fire object");
        }
        else
        {
            m_fireObject.SetActive(false);
        }
    }

    public override bool IsCastShadow()
    {
        return base.IsCastShadow();
    }
    ////////////////////////////////////////////////////////////////////////////////
    bool IsAllLampLight()
    {
        var notLight = s_allLamps.Find(a => !a.m_isLight);
        if (notLight)
        {
            return false;
        }
        return true;
    }

    IEnumerator DestroyAllLamps()
    {
        yield return null;

        List<GameObject> listEffectLamp = new List<GameObject>();
        ParticleSystem effect = new ParticleSystem();

        foreach (var lamp in s_allLamps)
        {
            lamp.slot.block = null;
            GameObject temp = ContentManager.Instance.GetItem("SmokeInSlot");
            temp.transform.position = lamp.transform.position;
            effect = temp.GetComponent<ParticleSystem>();
            effect.Play();
            listEffectLamp.Add(temp);
            lamp.gameObject.SetActive(false);

           
        }
        SlotGravity.Reshading();
        yield return new WaitForSeconds(effect.duration);

        for (int i = 0; i < listEffectLamp.Count; i++)
        {
            Destroy(listEffectLamp[i].gameObject);
        }

        foreach (var lamp in s_allLamps)
        {
            Destroy(lamp.gameObject);
        }
    }

    public static void Cleanup()
    {
        if (s_allLamps != null)
        {
            s_allLamps.Clear();
        }
    }
}