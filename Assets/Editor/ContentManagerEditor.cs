using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(ContentManager))]
public class ContentManagerEditor : MetaEditor
{
    ContentManager main;
    private GameObject obj = null;
    private string category = "";
    private Dictionary<string, AnimBool> categories = new Dictionary<string, AnimBool>();

    public override Object FindTarget()
    {
        if (main == null)
        {
            main = FindObjectOfType<ContentManager>();
        }
        return main;
    }
    public override void OnInspectorGUI()
    {
        if (metaTarget == null)
        {
            EditorGUILayout.HelpBox("ContentManager is missing", MessageType.Error);
            return;
        }
        GUILayout.Label("ContentManager");
        main = (ContentManager)metaTarget;
        

        if (main.contentItems == null)
        {
            main.contentItems = new List<ContentManager.ContentAssistantItem>();
        }

        foreach (ContentManager.ContentAssistantItem i in main.contentItems)
        {
            if (!categories.ContainsKey(i.category))
            {
                categories.Add(i.category, new AnimBool(false));
                categories[i.category].valueChanged.AddListener(RepaintIt);
            }
        }

        foreach (var key in categories.Keys)
        {
            categories[key].target = GUILayout.Toggle(categories[key].target, key, EditorStyles.foldout);

            if (EditorGUILayout.BeginFadeGroup(categories[key].faded))
            {
                foreach (ContentManager.ContentAssistantItem j in main.contentItems.FindAll(x => x.category == key))
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        obj = j.item;
                        this.category = j.category;
                        main.contentItems.Remove(j);
                        return;
                    }
                    GameObject _obj = (GameObject)EditorGUILayout.ObjectField(j.item, typeof(GameObject), false, GUILayout.Width(150), GUILayout.ExpandWidth(true));
                    if (j.item != _obj)
                        main.contentItems[main.contentItems.IndexOf(j)] = new ContentManager.ContentAssistantItem(_obj, key);
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndFadeGroup();
        }

        #region Add item
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add", GUILayout.Width(50)))
        {
            if (obj == null || category == null) return;

            Undo.RecordObject(main, "content change");

            if (category == "") category = "Others";

            main.contentItems.Add(new ContentManager.ContentAssistantItem(obj, category));
            if (!categories.ContainsKey(category))
                categories.Add(category, new AnimBool(true));
            else
                categories[category].target = true;
            obj = null;
            category = "";

            EditorUtility.SetDirty(main);
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(main.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(main.gameObject.scene);
        }
        obj = (GameObject)EditorGUILayout.ObjectField(obj, typeof(GameObject), false, GUILayout.Width(150));
        GUILayout.Label("in", GUILayout.Width(30));
        category = GUILayout.TextField(category, GUILayout.Width(150));
        EditorGUILayout.EndHorizontal();
        #endregion
    }
}