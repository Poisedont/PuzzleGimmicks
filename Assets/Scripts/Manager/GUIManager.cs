using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIManager : Singleton<GUIManager>
{

    public MenuBase[] m_menuArray;

    private Dictionary<string, MenuBase> m_menuList;
    public MenuBase MenuReadyOpen { get; set; }

    Stack<MenuBase> m_screenStacks = new Stack<MenuBase>();


    // Start is called before the first frame update
    void Start()
    {
        m_menuList = new Dictionary<string, MenuBase>();

        for (int i = 0; i < m_menuArray.Length; i++)
        {
            if (m_menuArray[i] != null)
            {
                m_menuList.Add(m_menuArray[i].GetMenuName(), m_menuArray[i]);
            }
        }

        StartOpenMenu();
    }

    public void UpdateLanguage()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (m_screenStacks.Count > 0)
            {
                m_screenStacks.Peek().OnBack();
            }
        }


    }

    public MenuBase GetMenuInstance(string name)
    {
        if (m_menuList.ContainsKey(name))
        {
            return m_menuList[name];
        }
        return null;
    }

    public void OpenMenu<T>()
    {
        string name = typeof(T).ToString();
        OpenMenu(name);
    }
    /// <summary>
    /// Open the menu with name of menu type
    /// </summary>
    public void OpenMenu(string pName)
    {
        if (m_menuList.ContainsKey(pName))
        {
            MenuBase menu = m_menuList[pName];
            OpenMenu(menu);
        }
        else
        {
            Debug.Log("Open menu [" + pName + "] but no instance to show");
        }
    }

    public void OpenMenu(MenuBase pMenu)
    {
        //deactive last menu
        if (m_screenStacks.Count > 0 && !pMenu.KeepMenuUnderneath)
        {
            var menu = m_screenStacks.Peek();
            menu.Hide();
            MenuReadyOpen = pMenu;
        }
        else //in case need to keep menu underneath, last menu will not hide so need to call open menu directly
        {
            MenuReadyOpen = pMenu;
            StartOpenMenu();
        }
    }

    public void StartOpenMenu()
    {
        if (MenuReadyOpen != null)
        {
            MenuReadyOpen.Show();
            m_screenStacks.Push(MenuReadyOpen);

            MenuReadyOpen = null;
        }
    }

    public void CloseMenu(string pName)
    {
        if (m_screenStacks.Count == 0)
        {
            Debug.LogWarning("No menu to close, stack empty");
            return;
        }
        var menu = m_screenStacks.Pop();
        menu.Hide();

        //active top menu in stack
        if (m_screenStacks.Count > 0)
        {
            m_screenStacks.Peek().Show();
        }
    }


    public void AddScreentoStack(MenuBase menu)
    {
        m_screenStacks.Push(menu);
    }

    public void GotoHome()
    {
        //call hide on top menu
        var menu = m_screenStacks.Pop();
        menu.Hide();
        // go to bottom menu in stack
        while (m_screenStacks.Count > 1)
        {
            menu = m_screenStacks.Pop();
            // menu.Hide();
        }

        if (m_screenStacks.Count == 1)
        {
            MenuReadyOpen = m_screenStacks.Peek();
           // m_screenStacks.Peek().Show();
        }
    }
}
