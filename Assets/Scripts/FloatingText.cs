using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour
{
    public Animator animator;
    private Text damageText;

    void OnEnable()
    {
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        Destroy(gameObject, clipInfo[0].clip.length);
        damageText = animator.GetComponent<Text>();
    }

    public void SetText(string text)
    {
        damageText.text = text;
    }

    public void ResizeText(int size, bool crit)
    {
        if (crit)
        {
            damageText.fontSize = size + 10;
            damageText.color = Color.red;
        }
        else
            damageText.fontSize = size;
    }
}
