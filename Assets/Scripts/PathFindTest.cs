using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class PathFindTest : MonoBehaviour
{
    // Start is called before the first frame update
    private PathfindingService _pathfindingService;
    private AGVAgent _agv;
    private List<int> _path = new List<int>();
    void Start()
    {
        _pathfindingService = new PathfindingService(new WarehouseGraph());
        _pathfindingService.GenerateTestGraph1();
        _path = _pathfindingService.FindPath(0, 22, _agv);
        for (int i = 0; i < _path.Count; i++)
        {
            Debug.Log(_path[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
