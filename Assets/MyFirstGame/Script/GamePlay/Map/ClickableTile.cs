using UnityEngine;
using System.Collections;

public class ClickableTile : MonoBehaviour {

	public int tileX;
	public int tileY;
	public TileMap map;

    public GameObject mask;
    public GameObject atkMask;

    public bool isCharacter = false;

	//void OnMouseUp() {
	//	Debug.Log ("Click!");
	//	//map.GeneratePathTo(tileX, tileY);
	//}
 //   private void OnMouseDown()
 //   {
 //       Debug.Log("asd");
 //       //map.DisableTileMask();
 //   }
   
    public void Clicked()
    {
        map.playManager.OnMovingTileClicked(tileX, tileY);
    }
}
