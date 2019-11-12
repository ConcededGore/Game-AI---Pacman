using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomTargetLocator : MonoBehaviour {

    public int ghostId = -1;

    GameObject pacMan;

    int BLINKY = 0;
    int PINKY = 1;
    int INKY = 2;
    int CLYDE = 3;

    // Start is called before the first frame update
    void Start() {
        pacMan = GameObject.Find("PacMan(Clone)") ? GameObject.Find("PacMan(Clone)") : GameObject.Find("PacMan 1(Clone)");
    }

    // Update is called once per frame
    void Update() {
        transform.SetPositionAndRotation(pacMan.transform.position + new Vector3(num2vec(ghostId).x, num2vec(ghostId).y, 0), transform.rotation);
    }

    Vector2 num2vec(int n) {
        switch (n) {
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
