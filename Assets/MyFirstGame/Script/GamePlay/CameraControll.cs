using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CameraControll : MonoBehaviour {

    public CharacterUnitController unitController;
    public float dragSpeed = 0.5f;
    private Vector3 dragOrigin;

    public int horizontalMax;
    public int verticalMax;
    bool isOnMoving = false;

    private void OnEnable()
    {
        GamePlayManager.OnGameplayEvent += OnGameplayEventReceive;
    }
    private void OnDisable()
    {
        GamePlayManager.OnGameplayEvent -= OnGameplayEventReceive;
    }
    private void OnGameplayEventReceive(string message, GamePlayManager.GameplayEvent type)
    {
        if(type == GamePlayManager.GameplayEvent.StartQuest)
        {
            horizontalMax = PF_GamePlay.ActiveQuest.Acts.ToList()[0].Value.MapData.Values.ToList()[0].Count;
            verticalMax = PF_GamePlay.ActiveQuest.Acts.ToList()[0].Value.MapData.Keys.ToList().Count;
        }

        if (type == GamePlayManager.GameplayEvent.PreMoveState)
        {
            isOnMoving = true;
        }
    }

    // Update is called once per frame
    void Update ()
    {
        if (isOnMoving)
        {
            MoveToUnit(unitController.currentUnit.gameObject);
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
            return;
        }

        if (!Input.GetMouseButton(0)) return;

        Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
        Vector3 move = new Vector3(-pos.x * dragSpeed, -pos.y * dragSpeed, 0);

        if (transform.position.x <= 0 && move.x <= 0)
            return;
        if (transform.position.x >= horizontalMax && move.x >= 0)
            return;
        if (transform.position.y <= 0 && move.y <= 0)
            return;
        if (transform.position.y >= verticalMax && move.y >= 0)
            return;

        transform.position += new Vector3(move.x, move.y, 0) * Time.deltaTime * 20;
        //Camera.main.transform.Translate(move, Space.World);
    }

    void MoveToUnit(GameObject go)
    {
        Vector2 dis = go.transform.position - transform.position ;
        transform.position += new Vector3(dis.x, dis.y, 0) * Time.deltaTime * 2;

        if (dis.magnitude <= 0.5f)
        {
            isOnMoving = false;
        }
    }
}
