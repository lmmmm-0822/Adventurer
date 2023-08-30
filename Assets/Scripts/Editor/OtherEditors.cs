using UnityEngine;
using UnityEditor;

public class OtherEditors
{
    [MenuItem("Tools/重启项目")]
    static void ExcuteReopenProject()
    {
        EditorApplication.OpenProject(Application.dataPath.Replace("Assets", string.Empty));
    }
}