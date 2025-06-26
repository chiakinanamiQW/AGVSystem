using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeGameObject : MonoBehaviour
{
    [SerializeField] public WarehouseGraph.Node Node;
    private Material material;

    private void Start()
    {
        material = GetComponent<MeshRenderer>().material;
    }

    private void Update()
    {
        UpdateNodeObject();
    }

    private void UpdateNodeObject()
    {
        if(Node == null)
            return;
        
        if (Node.Type == WarehouseGraph.NodeType.Shelf)
        {
            material.SetColor("_Color", GetColor(Node.Weight / Node.WeightLimit));
        }
            
    }
    private Color GetColor(float t)
    {
        t = Mathf.SmoothStep(0, 1, t);
        return Color.Lerp(Color.red, Color.green, t);
    }
}
