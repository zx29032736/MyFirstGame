using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableMask : MonoBehaviour {

    public ClickableTile ct;

    private void Awake()
    {
        ct = GetComponentInParent<ClickableTile>();
    }

    private void OnMouseDown()
    {
        if (ct != null)
            ct.Clicked();
        
    }
}
