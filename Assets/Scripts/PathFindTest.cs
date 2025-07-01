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
    void Awake()
    {
        _pathfindingService = PathfindingService.Instance;
        _pathfindingService.GenerateTestGraph1();
        _path = _pathfindingService.FindPath(1005, 4025, _agv);
        _pathfindingService._graph.UpdateNodeStatus(1003,true);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_pathfindingService != null)
        {
            _pathfindingService.DrawGraphGizmos();
        }
    }
#endif

}

