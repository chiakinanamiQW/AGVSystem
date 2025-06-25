using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneCreatCtrl : MonoBehaviour
{
    // Start is called before the first frame update
    private WarehouseGraph _graph;
    
    [Header("Prefabs")]
    public GameObject chargerPrefab;
    public GameObject doorPrefab;
    public GameObject elevatorPrefab;
    public GameObject goodsPrefab;
    public GameObject shelfPrefab;
    void Start()
    {
        _graph = PathfindingService.Instance._graph;
        
        GenerateGraphScene();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GenerateGraphScene()
    {
        foreach (WarehouseGraph.Node node in _graph.Nodes)
        {
            GameObject prefab = null;
            switch (node.Type)
            {
                case WarehouseGraph.NodeType.Shelf:
                    if (shelfPrefab == null && goodsPrefab == null)
                        break;
                    prefab = shelfPrefab;
                    break;
                case WarehouseGraph.NodeType.Elevator:
                    if(elevatorPrefab == null)
                        break;
                    prefab = elevatorPrefab;
                    break;
                case WarehouseGraph.NodeType.Charger:
                    if(chargerPrefab == null)
                        break;
                    prefab = chargerPrefab;
                    break;
                case WarehouseGraph.NodeType.OutPort:
                    if(doorPrefab == null)
                        break;
                    prefab = doorPrefab;
                    break;
                case WarehouseGraph.NodeType.InPort:
                    if(doorPrefab == null)
                        break;
                    prefab = doorPrefab;
                    break;
            }

            if (prefab != null)
            {
                GameObject obj;
                obj = Instantiate(prefab, node.Position, Quaternion.identity);
                obj.transform.parent = transform;
                obj.name = obj.name + "_" + node.ID;
                NodeGameObject ngo = obj.GetComponent<NodeGameObject>();
                if (ngo != null)
                {
                    ngo.Node = node;
                }
            }
        }
    }
}
