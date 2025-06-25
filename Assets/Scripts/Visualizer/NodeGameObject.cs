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
            material.SetColor("_Color", GetColor( 1 - (Node.Weight / Node.WeightLimit)));
        }
            
    }
    private Color GetColor(float t)
    {
        int R = (int)(255 * (1 - t));
        int G = (int)(255 * t);
        int B = 0;
        return new Color(R, G, B);
    }
}
