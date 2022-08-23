using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Stone tile
public class BackLayer : MonoBehaviour
{
    [SerializeField] int level = 0; // Level of stone. From 0 to 2. Each "StoneCrush"-call fall level by one. If it becomes -1, this stone will be destroyed.
    [SerializeField] Sprite[] sprites; // Images of stone of different levels. The size of the array must be equal to 3
    SpriteRenderer spriteRender;
    Animation anim;
    public string crush_effect;

    public static List<BackLayer> s_allStones = new List<BackLayer>();

    void Start()
    {
        spriteRender = GetComponentInChildren<SpriteRenderer>();
        spriteRender.sprite = sprites[level];
        anim = GetComponent<Animation>();
        // AnimationSpeed speed = GetComponentInChildren<AnimationSpeed>();
        // speed.speed = Random.Range(0.4f, 0.8f);
        // speed.offset = Random.Range(0f, 1f);

        s_allStones.Add(this);
    }

    public void SetLevel(int lv)
    {
        if (lv >= 0 && lv < sprites.Length)
        {
            level = lv;
            if (spriteRender)
            {
                spriteRender.sprite = sprites[level];
            }
        }
    }

    // Crush block funtion
    public void StoneCrush()
    {
        SoundManager.PlaySFX(SoundDefine.stone_crash);

        if (level < 0)
            return;

        StartCoroutine(PlayEffectRockLayer(transform.position));

        if (level == 0)
        {
            // Audio play("StoneCrush");
            //Destroy(gameObject);
            s_allStones.Remove(this);
            level = -1;
            return;
        }

        level--;
        spriteRender.sprite = sprites[level];



        // anim.Play("Crush");
        // Audio play("StoneHit");
    }

    IEnumerator PlayEffectRockLayer(Vector3 pos)
    {
        GameObject Obj;

        if(level == 2)
        {
            Obj = ContentManager.Instance.GetItem("RocksDropEffects3");
        }else if(level == 1)
        {
            Obj = ContentManager.Instance.GetItem("RocksDropEffects2");
        }
        else
        {
            Obj = ContentManager.Instance.GetItem("RocksDropEffects");
        }

        if (level == 0)
        {
            spriteRender.gameObject.SetActive(false);
        }

        ParticleSystem rockEffects = Obj.GetComponent<ParticleSystem>();
        Obj.transform.position = pos;
        rockEffects.Play();

        yield return new WaitForSeconds(rockEffects.duration );

        rockEffects.Stop();
        Destroy(rockEffects.gameObject);
        Destroy(Obj);
        if (!spriteRender.gameObject.active)
        {
            // Audio play("StoneCrush");
            Destroy(gameObject);
        }
    } 
        //IEnumerator DestroyingRoutine()
        //{
        //    if (!string.IsNullOrEmpty(crush_effect))
        //    {
        //        GameObject o = ContentManager.Instance.GetItem(crush_effect);
        //        o.transform.position = transform.position;

        //        anim.Play("StoneDestroy");
        //        while (anim.isPlaying)
        //        {
        //            yield return 0;
        //        }
        //    }

        //    StartCoroutine(PlayEffectRockLayer(transform.position));
        //    Destroy(gameObject);
        //}

    private void OnDestroy()
    {
        s_allStones.Remove(this);
    }
}