using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Models.LocationDefinition Definition { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        this.transform.position = Camera.main.ScreenToWorldPoint(
            new Vector3((float)this.Definition.Position.X, (float)this.Definition.Position.Y, 10));
    }

    // Update is called once per frame
    void Update()
    {

    }
}
