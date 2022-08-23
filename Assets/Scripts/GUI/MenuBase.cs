using UnityEngine;
using System.Collections;

/// <summary>
/// Generic class of MenuBase that can work with GUIManager
/// </summary>
public abstract class MenuBase<T> : MenuBase where T : MenuBase
{
    protected static string s_menuName = null;
    public T GetMenu()
    {
        return GetComponent<T>();
    }

    public static T GetMenuInstance()
    {
        if (s_menuName == null)
        {
            s_menuName = typeof(T).ToString();
        }
        var menu = GUIManager.Instance.GetMenuInstance(s_menuName);
        if (menu)
        {
            return menu as T;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Static function call to open menu instance of menu T
    /// </summary>
    public static void Open()
    {
        if (s_menuName == null)
        {
            s_menuName = typeof(T).ToString();
        }
        GUIManager.Instance.OpenMenu(s_menuName);

    }

    /// <summary>
    /// Static function call to close menu instance of menu T
    /// </summary>
    public static void Close()
    {
        if (s_menuName == null)
        {
            s_menuName = typeof(T).ToString();
        }
        GUIManager.Instance.CloseMenu(s_menuName);
       
    }

    public override string GetMenuName()
    {
        return typeof(T).ToString();
    }

    public override void OnBack()
    {
        SoundManager.PlaySFX(SoundDefine.k_ButtonClickSFXName);
        Close();
    }
}
public class MenuBase : MonoBehaviour
{
    protected Animator m_animator;

    protected CanvasGroup m_canvasGroup;

    [SerializeField] bool m_keepMenuUnderneath = false;
    public bool KeepMenuUnderneath { get { return m_keepMenuUnderneath; } }

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_canvasGroup = GetComponent<CanvasGroup>();
    }
    public virtual string GetMenuName()
    {
        return gameObject.name;
    }

    protected virtual void OnMenuOpening()
    {
        SetCanvasTouchable(true);

        //gameObject.SetActive(true);
        //SoundManager.PlaySoundEffect(SoundDefine.k_panel_sfx);
        if (m_animator)
        {
            m_animator.Play("ShowPanelAnim", -1, 0f);
        }
    }

    protected virtual void OnMenuClosing()
    {
        SetCanvasTouchable(false);

       // gameObject.SetActive(false);
        if (gameObject.activeSelf && m_animator)
        {
            m_animator.Play("ClosePanelAnim", -1, 0f);
        }
    }

    // event call in ClosePanelAnim
    IEnumerator WaitingInVisibleObj()
    {
        yield return null;
        this.gameObject.SetActive(false);
        GUIManager.Instance.StartOpenMenu();
    }

    /// <summary>
    /// Show a menu. Should call in GUIManager. Avoid to call it without notice GUIManager
    /// </summary>
    public void Show()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        OnMenuOpening();
    }

    /// <summary>
    /// Hide a menu. Should call in GUIManager. Avoid to call it without notice GUIManager
    /// </summary>
    public void Hide()
    {
        OnMenuClosing();
    }

    /// <summary>
    /// Use to enable/disable a menu interactive function. 
    /// </summary>
    /// <param name="isTouch">True if menu enable touch. Otherwise is false</param>
    public void SetCanvasTouchable(bool isTouch)
    {
        if (m_canvasGroup)
        {
            m_canvasGroup.blocksRaycasts = isTouch;
        }
        else
        {
            Debug.LogWarning("Canvas groups missing for this function");
        }
    }

    public virtual void OnBack()
    {
        Debug.Log("Menu not implement back function");
    }


}