using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComponentDisplay : MonoBehaviour {

    public Image comImage;
    public Text count;

    public void Init(Sprite sp,int cnt)
    {
        comImage.overrideSprite = sp;
        count.text = cnt.ToString();
    }
}
