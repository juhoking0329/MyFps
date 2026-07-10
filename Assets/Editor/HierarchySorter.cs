using UnityEngine;
using UnityEditor;
using System.Linq;

public class HierarchySorter
{
    // 유니티 상단 메뉴 [Tools] -> [Sort Children Alphabetically] 항목을 새로 만듭니다!
    [MenuItem("Tools/Sort Children Alphabetically")]
    private static void SortChildren()
    {
        // 현재 선택한 오브젝트를 가져옵니다.
        GameObject activeGO = Selection.activeGameObject;

        if (activeGO == null)
        {
            Debug.LogWarning("⚠️ 정렬할 부모 오브젝트(예: Environment)를 먼저 하이어라키에서 선택해 주세요!");
            return;
        }

        // 선택한 부모 아래의 모든 자식들을 이름 순(Natural Sort)으로 정렬합니다.
        var children = activeGO.transform.Cast<Transform>().ToList();

        // C# 기본 정렬법으로 이름 순 정렬
        children = children.OrderBy(t => t.name, new NaturalStringComparer()).ToList();

        // 정렬된 순서대로 유니티 하이어라키 순서를 재배치합니다.
        for (int i = 0; i < children.Count; i++)
        {
            children[i].SetSiblingIndex(i);
        }

        Debug.Log($"✅ '{activeGO.name}' 아래의 모든 자식 오브젝트가 이름 순으로 깔끔하게 정렬되었습니다!");
    }
}

// 숫자가 포함된 문자열(Ground (1), Ground (2)...)을 자연스럽게 1 -> 2 -> 10 순으로 정렬해 주는 비교기
public class NaturalStringComparer : System.Collections.Generic.IComparer<string>
{
    [System.Runtime.InteropServices.DllImport("shlwapi.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern int StrCmpLogicalW(string psz1, string psz2);

    public int Compare(string x, string y)
    {
        return StrCmpLogicalW(x, y);
    }
}