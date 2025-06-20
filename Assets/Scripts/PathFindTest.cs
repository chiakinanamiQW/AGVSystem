using System;
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
        _pathfindingService = PathfindingService.Instance;
        _pathfindingService.GenerateTestGraph1();
        _path = _pathfindingService.FindPath(1005, 4025, _agv);
        _pathfindingService._graph.UpdateNodeStatus(1003,true);
        for (int i = 0; i < _path.Count; i++)
        {
            Debug.Log(_path[i]);
        }
    }

    private void OnDrawGizmos()
    {
        if (_pathfindingService != null)
        {
            _pathfindingService.DrawGraphGizmos();
        }
    }
}
