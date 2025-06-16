using UnityEngine;
using System.Collections.Generic;

public class WarehouseVisualizer : MonoBehaviour
{
   /* public GameObject AGVPrefab;
    public Material[] StatusMaterials;

    private Dictionary<int, GameObject> _nodeObjects = new Dictionary<int, GameObject>();

    void Start()
    {
        // 根据数据层生成3D场景
        foreach (var node in DataLayer.Graph.Nodes)
        {
            var obj = Instantiate(GetPrefabByType(node.Type), node.Position, Quaternion.identity);
            _nodeObjects.Add(node.ID, obj);
        }
    }

    void Update()
    {
        foreach (var agv in DataLayer.AGVAgents)
        {
            // 实时更新AGV位置
            var path = DataLayer.GetAGVPath(agv.AGVID);
            if (path != null && path.Count > 0)
            {
                AGVPrefab.transform.position = _nodeObjects[path[0]].transform.position;
            }

            // 更新货架状态（故障、占用）
            var node = DataLayer.Graph.Nodes[agv.CurrentNode];
            _nodeObjects[node.ID].GetComponent<Renderer>().material = StatusMaterials[(int)node.IsBlocked];
        }
    }

    private GameObject GetPrefabByType(WarehouseGraph.NodeType type)
    {
        // 根据节点类型返回对应的Prefab
        return new GameObject();
    }*/
}
