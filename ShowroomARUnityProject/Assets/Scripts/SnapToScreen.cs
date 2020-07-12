using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class SnapToScreen : MonoBehaviour
{
    public GameObject horizScroll;

    public void SnapTo(int target)
    {
        int currentPage = horizScroll.GetComponent<HorizontalScrollSnap>()._currentPage;
        int distance = target - currentPage;
        if (distance > 0)
        {
            while(distance != 0)
            {
                horizScroll.GetComponent<HorizontalScrollSnap>().NextScreen();
                distance--;
            }
        }
        else if (distance < 0)
        {
            while (distance != 0)
            {
                horizScroll.GetComponent<HorizontalScrollSnap>().PreviousScreen();
                distance++;
            }
        }
    }
}
