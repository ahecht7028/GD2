using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageTextController : MonoBehaviour
{
    public GameObject popupText;
    private static GameObject canvas;

    void Start()
    {
        canvas = GameObject.Find("PlayerCanvas");


        

    }

    public void CreateFloatingText(string text, Vector3 location, bool crit, int time)
    {
        GameObject instance = Instantiate(popupText);
        Vector2 screenPosition = new Vector2(Random.Range(-3f, 3f)+Screen.width/2, Random.Range(-3f, 3f) + Screen.height / 2);
        FloatingText itext = instance.GetComponent<FloatingText>();



        //TODO Scale font size based on how far away the collision was
        if (time >= 20)
        {
            itext.ResizeText(30, crit);
        }
        else if (time >= 10)
        {
            itext.ResizeText(20, crit);
        }
        else
        {
            itext.ResizeText(10, crit);
        }




        instance.transform.SetParent(canvas.transform, false);
        instance.transform.position = screenPosition;
        instance.GetComponent<FloatingText>().SetText(text);
    }
}
