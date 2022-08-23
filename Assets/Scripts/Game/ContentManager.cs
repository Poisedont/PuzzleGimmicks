using System.Collections.Generic;
using UnityEngine;

public class ContentManager : Singleton<ContentManager>
{

    public List<ContentAssistantItem> contentItems;

    // Dictionary prefabs for quick lookup
    Dictionary<string, GameObject> content = new Dictionary<string, GameObject>();

    GameObject zObj;

    public override void Awake()
    {
        base.Awake();

        Init();
    }

    void Init()
    {
        content.Clear();
        foreach (ContentAssistantItem item in contentItems)
        {
            content.Add(item.item.name, item.item);
        }
    }

    public T GetItem<T>(string key) where T : Component
    {
        GameObject obj = GetItem(key);
        if (obj)
            return obj.GetComponent<T>();
        return null;
    }

    public T GetPrefab<T>(string key) where T : Component
    {
        GameObject obj = GetPrefab(key);
        if (obj)
            return obj.GetComponent<T>();
        return null;
    }

    public GameObject GetItem(string key)
    {
        if (content.ContainsKey(key))
            return Instantiate(content[key]);
        return null;
    }

    public GameObject GetPrefab(string key)
    {
        if (content.ContainsKey(key))
            return content[key];
        return null;
    }

    public T GetItem<T>(string key, Vector3 position) where T : Component
    {
        zObj = GetItem(key);
        zObj.transform.position = position;
        return zObj.GetComponent<T>();
    }

    public GameObject GetItem(string key, Vector3 position)
    {
        zObj = GetItem(key);
        zObj.transform.position = position;
        return zObj;
    }

    [System.Serializable]
    public struct ContentAssistantItem
    {
        public GameObject item;
        public string category;

        public ContentAssistantItem(GameObject _item, string _category) : this()
        {
            item = _item;
            category = _category;
        }
    }
}