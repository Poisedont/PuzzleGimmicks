using UnityEngine;
using UnityEditor;

public class LevelMakerEditor : EditorWindow
{

    Vector2 editorScroll, tabsScroll = new Vector2();
    // MetaEditor editor = null;

    string editorTitle = "";
    Color selectionColor;
    Color bgColor;

    System.Action editorRender;
    MetaEditor editor;
    Color defaultColor;

    ////////////////////////////////////////////////////////////////////////////////

    [MenuItem("puzzlegimmicksunity/LevelMaker")]
    private static void ShowWindow()
    {
        var window = GetWindow<LevelMakerEditor>();
        window.titleContent = new GUIContent("LevelMaker");
        window.Show();
    }

    private void OnFocus()
    {

        selectionColor = Color.Lerp(Color.red, Color.white, 0.7f);
        bgColor = Color.Lerp(GUI.backgroundColor, Color.black, 0.3f);

        Initialize();

    }

    private void Initialize()
    {

    }

    private void OnGUI()
    {
        defaultColor = GUI.backgroundColor;
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUI.backgroundColor = bgColor;
        EditorGUILayout.BeginVertical(EditorStyles.textArea, GUILayout.Width(150), GUILayout.ExpandHeight(true));
        GUI.backgroundColor = defaultColor;
        tabsScroll = EditorGUILayout.BeginScrollView(tabsScroll);

        DrawTabs();

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        GUI.backgroundColor = bgColor;
        EditorGUILayout.BeginVertical(EditorStyles.textArea, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUI.backgroundColor = defaultColor;
        editorScroll = EditorGUILayout.BeginScrollView(editorScroll);

        if (editor != null)
        {
            editorRender.Invoke();
        }
        else
        {
            GUILayout.Label("Nothing selected");
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        GUILayout.Label("Puzzle gimmick. Late 2019");
    }

    private void DrawTabs()
    {
        DrawTabTitle("General");

        if (DrawTabButton("Content"))
        {
            editor = CreateInstance<ContentManagerEditor>();
            editor.onRepaint += Repaint;
            editorRender = editor.OnInspectorGUI;
        }

        DrawTabTitle("Levels");
        #region Level
        if (DrawTabButton("Level Editor"))
        {
            editor = CreateInstance<LevelEditor>();
            editor.onRepaint += Repaint;
            editorRender = editor.OnInspectorGUI;
        }
        #endregion
    }


    void DrawTabTitle(string text)
    {
        GUILayout.Label(text, EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandWidth(true));
    }

    bool DrawTabButton(string text)
    {
        Color color = GUI.backgroundColor;
        if (editorTitle == text)
        {
            GUI.backgroundColor = selectionColor;
        }
        bool result = GUILayout.Button(text, EditorStyles.miniButton, GUILayout.ExpandWidth(true));
        GUI.backgroundColor = color;

        if (string.IsNullOrEmpty(editorTitle) || (editorTitle == text && editorRender == null))
        {
            result = true;
        }

        if (result)
        {
            EditorGUI.FocusTextInControl("");
            editorTitle = text;
        }

        return result;
    }

}