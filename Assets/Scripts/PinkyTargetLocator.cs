using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinkyTargetLocator : MonoBehaviour {

    private GameObject pacMan;
    private Movement move;
    private int lastDir = 0;

    // Start is called before the first frame update
    void Start() {
        pacMan = GameObject.Find("PacMan(Clone)") ? GameObject.Find("PacMan(Clone)") : GameObject.Find("PacMan 1(Clone)");
        move = pacMan.GetComponent<Movement>();
    }

    // Update is called once per frame
    void Update() {
        Vector2 dir = num2vec((int)move._dir - 1);
        if ((int)move._dir - 1 != -1) {
            lastDir = (int)move._dir - 1;
        }
        dir *= 4;
        transform.SetPositionAndRotation(pacMan.transform.position + new Vector3(dir.x, dir.y, 0), transform.rotation);
    }

    Vector2 num2vec(int n) {
        switch (n) {
            case -1:
                return num2vec(lastDir);
            case 0:
                return new Vector2(0, 1);
            case 1:
                return new Vector2(0, -1);
            case 2:
                return new Vector2(-1, 0);
            case 3:
                return new Vector2(1, 0);
            default:    // should never happen
                return new Vector2(0, 0);
        }
    }
}
