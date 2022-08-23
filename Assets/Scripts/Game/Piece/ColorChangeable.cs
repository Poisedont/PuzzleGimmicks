using UnityEngine;

public class ColorChangeable : MonoBehaviour, IChipColorChangeable
{
    [Tooltip("Main sprite render")]
    [SerializeField] SpriteRenderer m_render;

    [Tooltip("Use for change color. Need match with Chip.chiptypes")]
    [SerializeField] Sprite[] m_colorSprites;
    ////////////////////////////////////////////////////////////////////////////////
    
    Chip chip;
    ////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        chip = GetComponent<Chip>();
    }
    public int ChangeColor()
    {
        if (!m_render) return chip.id;

        int color = chip.id;
        int rnd = color;
        do
        {
            rnd = LevelProfile.main.GetColorRandom();
        } while (rnd == color);

        Sprite[] colorSprites = m_colorSprites;

        if (colorSprites != null && rnd < colorSprites.Length)
        {
            m_render.sprite = colorSprites[rnd];
            return rnd;
        }

        return color;
    }
}