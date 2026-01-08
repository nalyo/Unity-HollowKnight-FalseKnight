using UnityEngine;

public class AddExtraMaterial : MonoBehaviour
{
    [SerializeField] private Material extraMaterial;

    [ContextMenu("Add Material To All Renderers")]
    void AddMaterialToAll()
    {
        if (extraMaterial == null)
        {
            Debug.LogWarning("请先指定额外材质！");
            return;
        }

        // 找到场景中所有 Renderer
        Renderer[] renderers = FindObjectsOfType<Renderer>();

        foreach (var rend in renderers)
        {
            var mats = rend.sharedMaterials; // 原始材质数组
            bool alreadyAdded = false;

            // 避免重复添加
            foreach (var m in mats)
            {
                if (m == extraMaterial)
                {
                    alreadyAdded = true;
                    break;
                }
            }

            if (!alreadyAdded && rend.gameObject.name.Contains("hot_spring"))
            {
                var newMats = new Material[mats.Length + 1];
                mats.CopyTo(newMats, 0);
                newMats[mats.Length] = extraMaterial;
                rend.sharedMaterials = newMats; // 替换回去
            }
        }

        Debug.Log("已为所有对象添加额外材质: " + extraMaterial.name);
    }
}
