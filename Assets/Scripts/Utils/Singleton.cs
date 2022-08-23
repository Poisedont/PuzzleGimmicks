using UnityEngine;

public enum ESingletonType
{
    DESTROY_ON_LOAD,
    DONT_DESTROY
}
/// <summary>
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
/// 
/// To set singleton dont destroy on load, call SetDontDestroyOnLoad(true);
///
/// As a note, this is made as MonoBehaviour because we need Coroutines.
/// @ OnInitialize override this function to process some task(s) on init
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{

    private static T s_instance;
    // private static bool s_appIsQuit = false;

    [SerializeField]
    private bool m_dontDestroyOnLoad = false;

    public static T Instance
    {
        get
        {
            //if (s_appIsQuit)
            //{
            //	return null;
            //}
            return s_instance;
        }
    }
    public static void CreateInstance(ESingletonType dontDestroyOnLoad)
    {
        if (s_instance == null)
        {
            GameObject _emptyObject = new GameObject();
            _emptyObject.name = "(singleton)" + typeof(T).ToString();
            s_instance = _emptyObject.AddComponent<T>();
            _emptyObject.isStatic = true;
            // s_appIsQuit = false;
            if (dontDestroyOnLoad == ESingletonType.DONT_DESTROY)
            {
                DontDestroyOnLoad(_emptyObject);
            }
        }
    }

    //=== in case add instance in scene
    public virtual void Awake()
    {
        if (s_instance == null)
        {
            Debug.Log("Instance created in scene for " + typeof(T));
            s_instance = gameObject.GetComponent<T>();
            if (m_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
        // s_appIsQuit = false;
    }

    public void SetDontDestroyOnLoad(bool dontDestroy)
    {
        if (dontDestroy)
        {
            DontDestroyOnLoad(s_instance.gameObject);
        }
    }

    /// <summary>
    /// When Unity quits, it destroys objects in a random order.
    /// In principle, a Singleton is only destroyed when application quits.
    /// If any script calls Instance after it have been destroyed, 
    ///   it will create a buggy ghost object that will stay on the Editor scene
    ///   even after stopping playing the Application. Really bad!
    /// So, this was made to be sure we're not creating that buggy ghost object.
    /// </summary>
    protected virtual void OnDestroy()
    {
        // s_appIsQuit = true;
    }

}
