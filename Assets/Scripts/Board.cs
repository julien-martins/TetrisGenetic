using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public TileBase[] Tiles;

    private Tilemap _tilemap;

    private Piece piece;

    // Start is called before the first frame update
    void Start()
    {
        _tilemap = GetComponentInChildren<Tilemap>();
        _tilemap.SetTile(new Vector3Int(0, 0, 0), Tiles[0]);
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
