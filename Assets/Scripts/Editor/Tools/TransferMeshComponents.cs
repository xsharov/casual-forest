using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TransferMeshComponents : EditorWindow
{
    // Добавляем пункт меню для обработки выбранных префабов
    [MenuItem("Tools/Transfer Mesh Components in Selected Prefabs")]
    static void ProcessSelectedPrefabs()
    {
        // Получаем все выбранные ассеты типа GameObject (префабы)
        GameObject[] prefabs = Selection.GetFiltered<GameObject>(SelectionMode.Assets);
        if (prefabs.Length == 0)
        {
            Debug.LogWarning("Нет выбранных префабов для обработки.");
            return;
        }

        foreach (GameObject prefab in prefabs)
        {
            // Получаем путь к префабу
            string path = AssetDatabase.GetAssetPath(prefab);
            // Загружаем содержимое префаба для редактирования
            GameObject prefabInstance = PrefabUtility.LoadPrefabContents(path);
            
            // Рекурсивно обрабатываем префаб
            bool modified = ProcessGameObject(prefabInstance);
            if (modified)
            {
                // Если изменения были, сохраняем префаб
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, path);
                Debug.Log("Обработан префаб: " + path);
            }
            // Выгружаем содержимое префаба из памяти
            PrefabUtility.UnloadPrefabContents(prefabInstance);
        }
        AssetDatabase.Refresh();
        Debug.Log("Обработка завершена.");
    }

    // Рекурсивная функция обработки GameObject и его дочерних объектов
    static bool ProcessGameObject(GameObject go)
    {
        bool modified = false;
        // Собираем список прямых дочерних трансформов,
        // чтобы не модифицировать коллекцию во время итерации
        List<Transform> children = new List<Transform>();
        foreach (Transform child in go.transform)
        {
            children.Add(child);
        }

        // Обрабатываем каждого ребенка
        foreach (Transform child in children)
        {
            // Рекурсивно обрабатываем дочерние объекты
            if (ProcessGameObject(child.gameObject))
                modified = true;

            // Если у объекта есть MeshFilter, начинаем обработку
            MeshFilter mf = child.GetComponent<MeshFilter>();
            if (mf != null)
            {
                // Если меш отсутствует – просто удаляем объект
                if (mf.sharedMesh == null)
                {
                    DestroyImmediate(child.gameObject);
                    modified = true;
                }
                else
                {
                    // Если меш присутствует – переносим компоненты в родительский объект
                    Transform parent = child.parent;
                    if (parent != null)
                    {
                        // Если у родителя ещё нет MeshFilter, добавляем его и копируем меш
                        if (parent.GetComponent<MeshFilter>() == null)
                        {
                            MeshFilter newMF = parent.gameObject.AddComponent<MeshFilter>();
                            newMF.sharedMesh = mf.sharedMesh;
                        }
                        // Если у дочернего объекта есть MeshRenderer и у родителя его ещё нет – переносим материалы
                        MeshRenderer mr = child.GetComponent<MeshRenderer>();
                        if (mr != null && parent.GetComponent<MeshRenderer>() == null)
                        {
                            MeshRenderer newMR = parent.gameObject.AddComponent<MeshRenderer>();
                            newMR.sharedMaterials = mr.sharedMaterials;
                        }
                    }
                    // Удаляем обработанный дочерний объект
                    DestroyImmediate(child.gameObject);
                    modified = true;
                }
            }
        }
        return modified;
    }
}
