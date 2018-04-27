using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemViewerDisplayItem : MonoBehaviour {

    public Image itemImage;
    public Text itemName;
    public Text counts;

	public void BtnInitialize(Sprite sp,string name, int count)
    {
        itemImage.overrideSprite = sp;
        itemName.text = name;
        counts.text = count.ToString();
    }
}
