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
        _pathfindingService.GenerateTestGraph();
        _path = _pathfindingService.FindPath(3020, 4000, _agv); 
        for (int i = 0; i < _path.Count; i++)
        {
            Debug.Log(_path[i]);
        }
    }
    
    private void OnDrawGizmos()
    {
        if(_pathfindingService != null)
        {
            _pathfindingService.DrawGraphGizmos();
            if (_path != null)
            {
                _pathfindingService.DrawPathGizmos(_path);
            }
        }
    }
}
