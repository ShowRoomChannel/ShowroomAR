using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPivotToMiddle : MonoBehaviour
{
    // Start is called before the first frame update
    void FixedUpdate()
    {
        this.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
    }
}
