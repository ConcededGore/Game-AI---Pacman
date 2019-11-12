using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*****************************************************************************
 * IMPORTANT NOTES - PLEASE READ
 * 
 * This is where all the code needed for the Ghost AI goes. There should not
 * be any other place in the code that needs your attention.
 * 
 * There are several sets of variables set up below for you to use. Some of
 * those settings will do much to determine how the ghost behaves. You don't
 * have to use this if you have some other approach in mind. Other variables
 * are simply things you will find useful, and I am declaring them for you
 * so you don't have to.
 * 
 * If you need to add additional logic for a specific ghost, you can use the
 * variable ghostID, which is set to 1, 2, 3, or 4 depending on the ghost.
 * 
 * Similarly, set ghostID=ORIGINAL when the ghosts are doing the "original" 
 * PacMan ghost behavior, and to CUSTOM for the new behavior that you supply. 
 * Use ghostID and ghostMode in the Update() method to control all this.
 * 
 * You could if you wanted to, create four separate ghost AI modules, one per
 * ghost, instead. If so, call them something like BlinkyAI, PinkyAI, etc.,
 * and bind them to the correct ghost prefabs.
 * 
 * Finally there are a couple of utility routines at the end.
 * 
 * Please note that this implementation of PacMan is not entirely bug-free.
 * For example, player does not get a new screenful of dots once all the
 * current dots are consumed. There are also some issues with the sound 
 * effects. By all means, fix these bugs if you like.
 * 
 *****************************************************************************/

public class GhostAI : MonoBehaviour
{

    const int BLINKY = 1;   // These are used to set ghostID, to facilitate testing.
    const int PINKY = 2;
    const int INKY = 3;
    const int CLYDE = 4;
    public int ghostID;     // This variable is set to the particular ghost in the prefabs,

    const int ORIGINAL = 1; // These are used to set ghostMode, needed for the assignment.
    const int CUSTOM = 2;
    public int ghostMode;   // ORIGINAL for "original" ghost AI; CUSTOM for your unique new AI

    Movement move;
    private Vector3 startPos;
    private bool[] dirs = new bool[4];
    private bool[] prevDirs = new bool[4];

    public float releaseTime = 0f;          // This could be a tunable number
    private float releaseTimeReset = 0f;
    public float waitTime = 0f;             // This could be a tunable number
    private const float ogWaitTime = .1f;
    public int range = 0;                   // This could be a tunable number

    public bool dead = false;               // state variables
    public bool fleeing = false;

    //Default: base value of likelihood of choice for each path
    public float Dflt = 1f;

    //Available: Zero or one based on whether a path is available
    int A = 0;

    //Value: negative 1 or 1 based on direction of pacman
    int V = 1;

    //Fleeing: negative if fleeing
    int F = 1;

    //Priority: calculated preference based on distance of target in one direction weighted by the distance in others (dist/total)
    float P = 0f;

    // Variables to hold distance calcs
    float distX = 0f;
    float distY = 0f;
    float total = 0f;

    // Percent chance of each coice. order is: up < right < 0 < down < left for random choice
    // These could be tunable numbers. You may or may not find this useful.
    public float[] directions = new float[4];

    //remember previous choice and make inverse illegal!
    private int[] prevChoices = new int[4] { 1, 1, 1, 1 };

    // This will be PacMan when chasing, or Gate, when leaving the Pit
    public GameObject target;
    GameObject gate;
    GameObject pacMan;

    public bool chooseDirection = true;
    public int[] choices;
    public float choice;

    public int frameLockout;
    int previousTick;
    int tick = 0;

    public Vector3 prevTile = new Vector3(-100, -100, -100);
    private bool justScared = true;

    public float fleeTime = 5f;
    private float startedFleeing;

    public enum State
    {
        waiting,
        entering,
        leaving,
        active,
        fleeing,
        scatter         // Optional - This is for more advanced ghost AI behavior
    }

    public State _state = State.waiting;

    // Use this for initialization
    private void Awake()
    {
        startPos = this.gameObject.transform.position;

    }

    void Start()
    {
        move = GetComponent<Movement>();
        gate = GameObject.Find("Gate(Clone)");
        pacMan = GameObject.Find("PacMan(Clone)") ? GameObject.Find("PacMan(Clone)") : GameObject.Find("PacMan 1(Clone)");
        releaseTimeReset = releaseTime;
        //tileTarget = startPos;
    }

    public void restart()
    {
        releaseTime = releaseTimeReset;
        transform.position = startPos;
        _state = State.waiting;
    }

    /// <summary>
    /// This is where most of the work will be done. A switch/case statement is probably 
    /// the first thing to test for. There can be additional tests for specific ghosts,
    /// controlled by the GhostID variable. But much of the variations in ghost behavior
    /// could be controlled by changing values of some of the above variables, like
    /// 
    /// </summary>
    void Update()
    {
        //Debug.Log("got here 1");
        tick++;
        if(fleeing && _state == State.active)
        {
            _state = State.fleeing;
        }
        switch (_state)
        {
            case (State.waiting):
                //Debug.Log("got here 2");
                // below is some sample code showing how you deal with animations, etc.
                justScared = true;
                move._dir = Movement.Direction.still;
                if (releaseTime <= 0f)
                {
                    chooseDirection = true;
                    gameObject.GetComponent<Animator>().SetBool("Dead", false);
                    gameObject.GetComponent<Animator>().SetBool("Running", false);
                    gameObject.GetComponent<Animator>().SetInteger("Direction", 0);
                    gameObject.GetComponent<Movement>().MSpeed = 5f;

                    _state = State.leaving;

                    // etc.
                }
                gameObject.GetComponent<Animator>().SetBool("Dead", false);
                gameObject.GetComponent<Animator>().SetBool("Running", false);
                gameObject.GetComponent<Animator>().SetInteger("Direction", 0);
                gameObject.GetComponent<Movement>().MSpeed = 5f;
                releaseTime -= Time.deltaTime;
                // etc.
                break;


            case (State.leaving):
                move._dir = Movement.Direction.still;
                justScared = true;
                //Debug.Log("got here 4");
                if (transform.position.x < 13.48f || transform.position.x > 13.52)
                {
                    //print ("GOING LEFT OR RIGHT");
                    transform.position = Vector3.Lerp(transform.position, new Vector3(13.5f, transform.position.y, transform.position.z), 3f * Time.deltaTime);
                }
                else
                {
                    gameObject.GetComponent<Animator>().SetInteger("Direction", 2);
                    transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, -11f, transform.position.z), 3f * Time.deltaTime);
                }
                if (transform.position.y < -10.8 && transform.position.y > -11.2)
                {
                    fleeing = false;
                    dead = false;
                    gameObject.GetComponent<Animator>().SetBool("Running", false);
                    _state = State.active;
                }
                break;

            case (State.active):
                justScared = true;
                if (!dead)
                {
                    // etc.
                    // most of your AI code will be placed here!

                    //remember what direction reverse is
                    Vector2 reverseDir = new Vector2(0, 0);
                    reverseDir = move.direc * -1;
                    if (ghostID == BLINKY && /*tick - previousTick >= frameLockout*/ (Mathf.Abs(Mathf.Abs(transform.position.x) - Mathf.Abs(prevTile.x)) >= 1.5f ||
                        Mathf.Abs(Mathf.Abs(transform.position.y) - Mathf.Abs(prevTile.y)) >= 1.5f || !move.checkDirectionClear(num2vec((int)move._dir - 1))))
                    {
                        //Debug.Break();
                        if (ghostMode == ORIGINAL)
                        {
                            target = pacMan;
                            Vector2[] movements = new Vector2[4];
                            Movement.Direction[] dirs = new Movement.Direction[4];
                            //Figure out where each possible movement would place the ghost
                            movements[0] = new Vector2(transform.position.x + 1.5f, transform.position.y);
                            dirs[0] = Movement.Direction.right;
                            movements[1] = new Vector2(transform.position.x, transform.position.y + 1.5f);
                            dirs[1] = Movement.Direction.up;
                            movements[2] = new Vector2(transform.position.x - 1.5f, transform.position.y);
                            dirs[2] = Movement.Direction.left;
                            movements[3] = new Vector2(transform.position.x, transform.position.y - 1.5f);
                            dirs[3] = Movement.Direction.down;

                            //Sort by distance to target
                            for (int x = 0; x < 4 - 1; x++)
                            {
                                for (int y = 0; y < 4 - x - 1; y++)
                                {
                                    if (Vector2.Distance(movements[y], new Vector2(target.transform.position.x, target.transform.position.y)) >
                                       Vector2.Distance(movements[y + 1], new Vector2(target.transform.position.x, target.transform.position.y)))
                                    {
                                        Vector2 temp = movements[y];
                                        movements[y] = movements[y + 1];
                                        movements[y + 1] = temp;

                                        Movement.Direction temp1 = dirs[y];
                                        dirs[y] = dirs[y + 1];
                                        dirs[y + 1] = temp1;
                                    }
                                }
                            }
                            //Starting with the movement that brings the ghost the closes, check if the movements are valid, using the first one that is
                            for (int i = 0; i < 4; i++)
                            {
                                if (move.checkDirectionClear(num2vec((int)dirs[i] - 1)) && !compareDirectionsVectors(num2vec((int)dirs[i] - 1), reverseDir))
                                {
                                    move._dir = dirs[i];
                                    //Debug.Log("chosen direction is entry " + i + ", " + dirs[i]);
                                    prevTile = transform.position;
                                    previousTick = tick;
                                    i = 10;

                                }
                            }
                        }
                        else if (ghostMode == CUSTOM)
                        {
                            target = GameObject.Find("BlinkyCustomTarget");
                            target = pacMan;
                            Vector2[] movements = new Vector2[4];
                            Movement.Direction[] dirs = new Movement.Direction[4];
                            //Figure out where each possible movement would place the ghost
                            movements[0] = new Vector2(transform.position.x + 1.5f, transform.position.y);
                            dirs[0] = Movement.Direction.right;
                            movements[1] = new Vector2(transform.position.x, transform.position.y + 1.5f);
                            dirs[1] = Movement.Direction.up;
                            movements[2] = new Vector2(transform.position.x - 1.5f, transform.position.y);
                            dirs[2] = Movement.Direction.left;
                            movements[3] = new Vector2(transform.position.x, transform.position.y - 1.5f);
                            dirs[3] = Movement.Direction.down;

                            //Sort by distance to target
                            for (int x = 0; x < 4 - 1; x++) {
                                for (int y = 0; y < 4 - x - 1; y++) {
                                    if (Vector2.Distance(movements[y], new Vector2(target.transform.position.x, target.transform.position.y)) >
                                       Vector2.Distance(movements[y + 1], new Vector2(target.transform.position.x, target.transform.position.y))) {
                                        Vector2 temp = movements[y];
                                        movements[y] = movements[y + 1];
                                        movements[y + 1] = temp;

                                        Movement.Direction temp1 = dirs[y];
                                        dirs[y] = dirs[y + 1];
                                        dirs[y + 1] = temp1;
                                    }
                                }
                            }
                            //Starting with the movement that brings the ghost the closes, check if the movements are valid, using the first one that is
                            for (int i = 0; i < 4; i++) {
                                if (move.checkDirectionClear(num2vec((int)dirs[i] - 1)) && !compareDirectionsVectors(num2vec((int)dirs[i] - 1), reverseDir)) {
                                    move._dir = dirs[i];
                                    //Debug.Log("chosen direction is entry " + i + ", " + dirs[i]);
                                    prevTile = transform.position;
                                    previousTick = tick;
                                    i = 10;

                                }
                            }
                        }
                    }
                    else if (ghostID == CLYDE && (Mathf.Abs(Mathf.Abs(transform.position.x) - Mathf.Abs(prevTile.x)) >= 1.5f || Mathf.Abs(Mathf.Abs(transform.position.y) - Mathf.Abs(prevTile.y)) >= 1.5f || !move.checkDirectionClear(num2vec((int)move._dir - 1))))
                    {
                        if (ghostMode == ORIGINAL)
                        {
                            if (Vector2.Distance(new Vector2(transform.position.x, transform.position.y), new Vector2(pacMan.transform.position.x, pacMan.transform.position.y)) >= 8)
                            {
                                target = pacMan;
                            }
                            else
                            {
                                target = GameObject.Find("BottomLeft");
                            }

                            Vector2[] movements = new Vector2[4];
                            Movement.Direction[] dirs = new Movement.Direction[4];
                            //Figure out where each possible movement would place the ghost
                            movements[0] = new Vector2(transform.position.x + 1, transform.position.y);
                            dirs[0] = Movement.Direction.right;
                            movements[1] = new Vector2(transform.position.x, transform.position.y + 1);
                            dirs[1] = Movement.Direction.up;
                            movements[2] = new Vector2(transform.position.x - 1, transform.position.y);
                            dirs[2] = Movement.Direction.left;
                            movements[3] = new Vector2(transform.position.x, transform.position.y - 1);
                            dirs[3] = Movement.Direction.down;

                            //Sort by distance to target
                            for (int x = 0; x < 4 - 1; x++)
                            {
                                for (int y = 0; y < 4 - x - 1; y++)
                                {
                                    if (Vector2.Distance(movements[y], new Vector2(target.transform.position.x, target.transform.position.y)) >
                                       Vector2.Distance(movements[y + 1], new Vector2(target.transform.position.x, target.transform.position.y)))
                                    {
                                        Vector2 temp = movements[y];
                                        movements[y] = movements[y + 1];
                                        movements[y + 1] = temp;

                                        Movement.Direction temp1 = dirs[y];
                                        dirs[y] = dirs[y + 1];
                                        dirs[y + 1] = temp1;
                                    }
                                }
                            }
                            //Starting with the movement that brings the ghost the closes, check if the movements are valid, using the first one that is
                            for (int i = 0; i < 4; i++)
                            {
                                if (move.checkDirectionClear(num2vec((int)dirs[i] - 1)) && !compareDirectionsVectors(num2vec((int)dirs[i] - 1), reverseDir))
                                {
                                    move._dir = dirs[i];
                                    //Debug.Log("chosen direction is entry " + i + ", " + dirs[i]);
                                    prevTile = transform.position;
                                    previousTick = tick;
                                    i = 10;

                                }
                            }
                        } else if (ghostMode == CUSTOM) {
                            target = GameObject.Find("ClydeCustomTarget");
                            target = pacMan;
                            Vector2[] movements = new Vector2[4];
                            Movement.Direction[] dirs = new Movement.Direction[4];
                            //Figure out where each possible movement would place the ghost
                            movements[0] = new Vector2(transform.position.x + 1.5f, transform.position.y);
                            dirs[0] = Movement.Direction.right;
                            movements[1] = new Vector2(transform.position.x, transform.position.y + 1.5f);
                            dirs[1] = Movement.Direction.up;
                            movements[2] = new Vector2(transform.position.x - 1.5f, transform.position.y);
                            dirs[2] = Movement.Direction.left;
                            movements[3] = new Vector2(transform.position.x, transform.position.y - 1.5f);
                            dirs[3] = Movement.Direction.down;

                            //Sort by distance to target
                            for (int x = 0; x < 4 - 1; x++) {
                                for (int y = 0; y < 4 - x - 1; y++) {
                                    if (Vector2.Distance(movements[y], new Vector2(target.transform.position.x, target.transform.position.y)) >
                                       Vector2.Distance(movements[y + 1], new Vector2(target.transform.position.x, target.transform.position.y))) {
                                        Vector2 temp = movements[y];
                                        movements[y] = movements[y + 1];
                                        movements[y + 1] = temp;

                                        Movement.Direction temp1 = dirs[y];
                                        dirs[y] = dirs[y + 1];
                                        dirs[y + 1] = temp1;
                                    }
                                }
                            }
                            //Starting with the movement that brings the ghost the closes, check if the movements are valid, using the first one that is
                            for (int i = 0; i < 4; i++) {
                                if (move.checkDirectionClear(num2vec((int)dirs[i] - 1)) && !compareDirectionsVectors(num2vec((int)dirs[i] - 1), reverseDir)) {
                                    move._dir = dirs[i];
                                    //Debug.Log("chosen direction is entry " + i + ", " + dirs[i]);
                                    prevTile = transform.position;
                                    previousTick = tick;
                                    i = 10;

                                }
                            }
                        }
                    } else if (ghostID == PINKY && (Mathf.Abs(Mathf.Abs(transform.position.x) - Mathf.Abs(prevTile.x)) >= 1.5f ||  Mathf.Abs(Mathf.Abs(transform.position.y) - Mathf.Abs(prevTile.y)) >= 1.5f || !move.checkDirectionClear(num2vec((int)move._dir - 1)))) {
                        //Debug.Break();
                        if (ghostMode == ORIGINAL) {
                            target = GameObject.Find("PinkyTarget");
                            Vector2[] movements = new Vector2[4];
                            Movement.Direction[] dirs = new Movement.Direction[4];
                            //Figure out where each possible movement would place the ghost
                            movements[0] = new Vector2(transform.position.x + 1.5f, transform.position.y);
                            dirs[0] = Movement.Direction.right;
                            movements[1] = new Vector2(transform.position.x, transform.position.y + 1.5f);
                            dirs[1] = Movement.Direction.up;
                            movements[2] = new Vector2(transform.position.x - 1.5f, transform.position.y);
                            dirs[2] = Movement.Direction.left;
                            movements[3] = new Vector2(transform.position.x, transform.position.y - 1.5f);
                            dirs[3] = Movement.Direction.down;

                            //Sort by distance to target
                            for (int x = 0; x < 4 - 1; x++) {
                                for (int y = 0; y < 4 - x - 1; y++) {
                                    if (Vector2.Distance(movements[y], new Vector2(target.transform.position.x, target.transform.position.y)) >
                                       Vector2.Distance(movements[y + 1], new Vector2(target.transform.position.x, target.transform.position.y))) {
                                        Vector2 temp = movements[y];
                                        movements[y] = movements[y + 1];
                                        movements[y + 1] = temp;

                                        Movement.Direction temp1 = dirs[y];
                                        dirs[y] = dirs[y + 1];
                                        dirs[y + 1] = temp1;
                                    }
                                }
                            }
                            //Starting with the movement that brings the ghost the closes, check if the movements are valid, using the first one that is
                            for (int i = 0; i < 4; i++) {
                                if (move.checkDirectionClear(num2vec((int)dirs[i] - 1)) && !compareDirectionsVectors(num2vec((int)dirs[i] - 1), reverseDir)) {
                                    move._dir = dirs[i];
                                    //Debug.Log("chosen direction is entry " + i + ", " + dirs[i]);
                                    prevTile = transform.position;
                                    previousTick = tick;
                                    i = 10;

                                }
                            }
                        } 
                        else if (ghostMode == CUSTOM) {
                            target = GameObject.Find("PinkyCustomTarget");
                            target = pacMan;
                            Vector2[] movements = new Vector2[4];
                            Movement.Direction[] dirs = new Movement.Direction[4];
                            //Figure out where each possible movement would place the ghost
                            movements[0] = new Vector2(transform.position.x + 1.5f, transform.position.y);
                            dirs[0] = Movement.Direction.right;
                            movements[1] = new Vector2(transform.position.x, transform.position.y + 1.5f);
                            dirs[1] = Movement.Direction.up;
                            movements[2] = new Vector2(transform.position.x - 1.5f, transform.position.y);
                            dirs[2] = Movement.Direction.left;
                            movements[3] = new Vector2(transform.position.x, transform.position.y - 1.5f);
                            dirs[3] = Movement.Direction.down;

                            //Sort by distance to target
                            for (int x = 0; x < 4 - 1; x++) {
                                for (int y = 0; y < 4 - x - 1; y++) {
                                    if (Vector2.Distance(movements[y], new Vector2(target.transform.position.x, target.transform.position.y)) >
                                       Vector2.Distance(movements[y + 1], new Vector2(target.transform.position.x, target.transform.position.y))) {
                                        Vector2 temp = movements[y];
                                        movements[y] = movements[y + 1];
                                        movements[y + 1] = temp;

                                        Movement.Direction temp1 = dirs[y];
                                        dirs[y] = dirs[y + 1];
                                        dirs[y + 1] = temp1;
                                    }
                                }
                            }
                            //Starting with the movement that brings the ghost the closes, check if the movements are valid, using the first one that is
                            for (int i = 0; i < 4; i++) {
                                if (move.checkDirectionClear(num2vec((int)dirs[i] - 1)) && !compareDirectionsVectors(num2vec((int)dirs[i] - 1), reverseDir)) {
                                    move._dir = dirs[i];
                                    //Debug.Log("chosen direction is entry " + i + ", " + dirs[i]);
                                    prevTile = transform.position;
                                    previousTick = tick;
                                    i = 10;

                                }
                            }
                        }
                    }
                    else if (ghostID == INKY && (Mathf.Abs(Mathf.Abs(transform.position.x) - Mathf.Abs(prevTile.x)) >= 1.5f || Mathf.Abs(Mathf.Abs(transform.position.y) - Mathf.Abs(prevTile.y)) >= 1.5f || !move.checkDirectionClear(num2vec((int)move._dir - 1)))) {
                        //Debug.Break();
                        if (ghostMode == ORIGINAL) {
                            target = GameObject.Find("InkyTarget");
                            Vector2[] movements = new Vector2[4];
                            Movement.Direction[] dirs = new Movement.Direction[4];
                            //Figure out where each possible movement would place the ghost
                            movements[0] = new Vector2(transform.position.x + 1.5f, transform.position.y);
                            dirs[0] = Movement.Direction.right;
                            movements[1] = new Vector2(transform.position.x, transform.position.y + 1.5f);
                            dirs[1] = Movement.Direction.up;
                            movements[2] = new Vector2(transform.position.x - 1.5f, transform.position.y);
                            dirs[2] = Movement.Direction.left;
                            movements[3] = new Vector2(transform.position.x, transform.position.y - 1.5f);
                            dirs[3] = Movement.Direction.down;

                            //Sort by distance to target
                            for (int x = 0; x < 4 - 1; x++) {
                                for (int y = 0; y < 4 - x - 1; y++) {
                                    if (Vector2.Distance(movements[y], new Vector2(target.transform.position.x, target.transform.position.y)) >
                                       Vector2.Distance(movements[y + 1], new Vector2(target.transform.position.x, target.transform.position.y))) {
                                        Vector2 temp = movements[y];
                                        movements[y] = movements[y + 1];
                                        movements[y + 1] = temp;

                                        Movement.Direction temp1 = dirs[y];
                                        dirs[y] = dirs[y + 1];
                                        dirs[y + 1] = temp1;
                                    }
                                }
                            }
                            //Starting with the movement that brings the ghost the closes, check if the movements are valid, using the first one that is
                            for (int i = 0; i < 4; i++) {
                                if (move.checkDirectionClear(num2vec((int)dirs[i] - 1)) && !compareDirectionsVectors(num2vec((int)dirs[i] - 1), reverseDir)) {
                                    move._dir = dirs[i];
                                    //Debug.Log("chosen direction is entry " + i + ", " + dirs[i]);
                                    prevTile = transform.position;
                                    previousTick = tick;
                                    i = 10;

                                }
                            }
                        }
                        else if (ghostMode == CUSTOM) {
                            target = GameObject.Find("InkyCustomTarget");
                            target = pacMan;
                            Vector2[] movements = new Vector2[4];
                            Movement.Direction[] dirs = new Movement.Direction[4];
                            //Figure out where each possible movement would place the ghost
                            movements[0] = new Vector2(transform.position.x + 1.5f, transform.position.y);
                            dirs[0] = Movement.Direction.right;
                            movements[1] = new Vector2(transform.position.x, transform.position.y + 1.5f);
                            dirs[1] = Movement.Direction.up;
                            movements[2] = new Vector2(transform.position.x - 1.5f, transform.position.y);
                            dirs[2] = Movement.Direction.left;
                            movements[3] = new Vector2(transform.position.x, transform.position.y - 1.5f);
                            dirs[3] = Movement.Direction.down;

                            //Sort by distance to target
                            for (int x = 0; x < 4 - 1; x++) {
                                for (int y = 0; y < 4 - x - 1; y++) {
                                    if (Vector2.Distance(movements[y], new Vector2(target.transform.position.x, target.transform.position.y)) >
                                       Vector2.Distance(movements[y + 1], new Vector2(target.transform.position.x, target.transform.position.y))) {
                                        Vector2 temp = movements[y];
                                        movements[y] = movements[y + 1];
                                        movements[y + 1] = temp;

                                        Movement.Direction temp1 = dirs[y];
                                        dirs[y] = dirs[y + 1];
                                        dirs[y + 1] = temp1;
                                    }
                                }
                            }
                            //Starting with the movement that brings the ghost the closes, check if the movements are valid, using the first one that is
                            for (int i = 0; i < 4; i++) {
                                if (move.checkDirectionClear(num2vec((int)dirs[i] - 1)) && !compareDirectionsVectors(num2vec((int)dirs[i] - 1), reverseDir)) {
                                    move._dir = dirs[i];
                                    //Debug.Log("chosen direction is entry " + i + ", " + dirs[i]);
                                    prevTile = transform.position;
                                    previousTick = tick;
                                    i = 10;

                                }
                            }
                        }
                    }
                }
                // etc.

                break;

            case State.entering:

                // Leaving this code in here for you.
                move._dir = Movement.Direction.still;
                if (transform.position.x < 13.48f || transform.position.x > 13.52)
                {
                    transform.position = Vector3.Lerp(transform.position, new Vector3(13.5f, transform.position.y, transform.position.z), 3f * Time.deltaTime);
                }
                else if (transform.position.y > -13.99f || transform.position.y < -14.01f)
                {
                    gameObject.GetComponent<Animator>().SetInteger("Direction", 2);
                    transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, -14f, transform.position.z), 3f * Time.deltaTime);
                }
                else
                {
                    fleeing = false;
                    dead = false;
                    gameObject.GetComponent<Animator>().SetBool("Running", true);
                    _state = State.waiting;
                }

                break;
            case State.fleeing:
                Vector2 reverseDir1 = new Vector2(0, 0);
                reverseDir1 = move.direc * -1;
                if (justScared)
                {
                    if (move._dir == Movement.Direction.right)
                        move._dir = Movement.Direction.left;
                    else if (move._dir == Movement.Direction.up)
                        move._dir = Movement.Direction.down;
                    else if (move._dir == Movement.Direction.left)
                        move._dir = Movement.Direction.right;
                    else if (move._dir == Movement.Direction.down)
                        move._dir = Movement.Direction.up;

                    justScared = false;
                    prevTile = transform.position;
                    startedFleeing = Time.time;
                }
                else if ((Mathf.Abs(Mathf.Abs(transform.position.x) - Mathf.Abs(prevTile.x)) >= 1.5f ||
                        Mathf.Abs(Mathf.Abs(transform.position.y) - Mathf.Abs(prevTile.y)) >= 1.5f || !move.checkDirectionClear(num2vec((int)move._dir - 1))))
                {                    
                    Vector2[] movements = new Vector2[4];
                    Movement.Direction[] dirs = new Movement.Direction[4];
                    //Figure out where each possible movement would place the ghost
                    movements[0] = new Vector2(transform.position.x + 1.5f, transform.position.y);
                    dirs[0] = Movement.Direction.right;
                    movements[1] = new Vector2(transform.position.x, transform.position.y + 1.5f);
                    dirs[1] = Movement.Direction.up;
                    movements[2] = new Vector2(transform.position.x - 1.5f, transform.position.y);
                    dirs[2] = Movement.Direction.left;
                    movements[3] = new Vector2(transform.position.x, transform.position.y - 1.5f);
                    dirs[3] = Movement.Direction.down;

                    //Sort randomly
                    for (int x = 0; x < 4 - 1; x++)
                    {
                        for (int y = 0; y < 4 - x - 1; y++)
                        {
                            
                            if (UnityEngine.Random.value > UnityEngine.Random.value)
                            {
                                Vector2 temp = movements[y];
                                movements[y] = movements[y + 1];
                                movements[y + 1] = temp;

                                Movement.Direction temp1 = dirs[y];
                                dirs[y] = dirs[y + 1];
                                dirs[y + 1] = temp1;
                            }
                        }
                    }

                    //Starting with the movement first in the randomly sorted array, check if the movements are valid, using the first one that is
                    for (int i = 0; i < 4; i++)
                    {
                        if (move.checkDirectionClear(num2vec((int)dirs[i] - 1)) && !compareDirectionsVectors(num2vec((int)dirs[i] - 1), reverseDir1))
                        {
                            move._dir = dirs[i];
                            //Debug.Log("chosen direction is entry " + i + ", " + dirs[i]);
                            prevTile = transform.position;
                            previousTick = tick;
                            i = 10;

                        }
                    }

                }
                if(!GetComponent<Animator>().GetBool("Running"))
                {
                    //GetComponent<Animator>().SetBool("Running", true);
                    _state = State.active;
                }
                break;
        }

    }

    // Utility routines

    Vector2 num2vec(int n)
    {
        switch (n)
        {
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

    bool compareDirections(bool[] n, bool[] p)
    {
        for (int i = 0; i < n.Length; i++)
        {
            if (n[i] != p[i])
            {
                return false;
            }
        }
        return true;
    }

    bool compareDirectionsVectors(Vector2 n, Vector2 p)
    {
        for (int x = 0; x < 2; x++)
        {
            if (n[x] != p[x])
                return false;
        }
        return true;
    }
}