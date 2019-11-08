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

public class GhostAI : MonoBehaviour {

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
	private int[] prevChoices = new int[4]{0,0,0,0};

    // This will be PacMan when chasing, or Gate, when leaving the Pit
	public GameObject target;
	GameObject gate;
	GameObject pacMan;

	public bool chooseDirection = true;
	public int[] choices ;
	public float choice;

    public int frameLockout;
    int previousTick;
    int tick = 0;

    Vector2 test = new Vector2(0, 0);

    public void OnDrawGizmos() {
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(test, 1);
    }

    public enum State{
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

    void Start () {
		move = GetComponent<Movement> ();
		gate = GameObject.Find("Gate(Clone)");
		pacMan = GameObject.Find("PacMan(Clone)") ? GameObject.Find("PacMan(Clone)") : GameObject.Find("PacMan 1(Clone)");
		releaseTimeReset = releaseTime;
	}

	public void restart(){
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
	void Update () {
        //Debug.Log("got here 1");
        tick++;
        switch (_state) {
		case(State.waiting):
            //Debug.Log("got here 2");
            // below is some sample code showing how you deal with animations, etc.
			move._dir = Movement.Direction.still;
			if (releaseTime <= 0f) {
				chooseDirection = true;
				gameObject.GetComponent<Animator>().SetBool("Dead", false);
				gameObject.GetComponent<Animator>().SetBool("Running", false);
				gameObject.GetComponent<Animator>().SetInteger ("Direction", 0);
				gameObject.GetComponent<Movement> ().MSpeed = 5f;

				_state = State.leaving;

                // etc.
			}
			gameObject.GetComponent<Animator>().SetBool("Dead", false);
			gameObject.GetComponent<Animator>().SetBool("Running", false);
			gameObject.GetComponent<Animator>().SetInteger ("Direction", 0);
			gameObject.GetComponent<Movement> ().MSpeed = 5f;
			releaseTime -= Time.deltaTime;
            // etc.
			break;


		case(State.leaving):
                move._dir = Movement.Direction.still;
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
                if(transform.position.y < -10.8 && transform.position.y > -11.2)
                {
                    fleeing = false;
                    dead = false;
                    gameObject.GetComponent<Animator>().SetBool("Running", false);
                    _state = State.active;
                }
                break;

            case (State.active):
                //Debug.Log("got here 4");
                if (!dead)
                {
                    // etc.
                    // most of your AI code will be placed here!
                    //remember what direction reverse is
                    Vector2 reverseDir = new Vector2(0,0);
                    if (tick - previousTick >= frameLockout)
                    {
                        reverseDir = move.direc * -1;
                        //Debug.Log("reverseDir is " + reverseDir);
                    }
                    if (ghostID == BLINKY && tick - previousTick >= frameLockout)
                    {
                        if (ghostMode == ORIGINAL) {
                            target = pacMan;
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
                            /*for (int x = 0; x < 4 - 1; x++) {
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
                            } */

                            // Debug stuff
                            /*Debug.Log("movements are: ");
                            for (int z = 0; z < 4; z++)
                            {
                                Debug.Log(movements[z] + " with direction " + dirs[z] + " with distance " + Vector2.Distance(movements[z], new Vector2(target.transform.position.x, target.transform.position.y)));
                            } */
                            // Starting with the movement that brings the ghost the closest, check if the movements are valid, using the first one that is

                            int[] validDirs = new int[4]{1, 1, 1, 1};

                            // Check to see if the vector is initialized (if the ghost has moved yet)
                            if (!checkPrevInit()) {
                                Debug.Log("Initializing");
                                for (int i = 0; i < 4; i++) {
                                    prevChoices[i] = 1;
                                }
                            }

                            for (int i = 0; i < 4; i++) {
                                validDirs[i] = 0;
                            }

                            // Checks to see if there is any obstructions
                            for (int i = 0; i < 4; i++) {
                                if (move.checkDirectionClear(num2vec((int)dirs[i] - 1))) {
                                    validDirs[i] = 1;
                                }
                                else {
                                    validDirs[i] = 0;
                                }
                            }

                            Debug.Log("startcap");
                            for (int i = 0; i < 4; i++) {
                                Debug.Log(validDirs[i] + " ");
                            }
                            Debug.Log("endcap");

                            // Ands the two arrays together, to invalidate the reverse direction
                            // C# can't use ints as bools for some reason
                            for (int i = 0; i < 4; i++) {
                                if (validDirs[i] == 1 && prevChoices[i] == 1) {
                                    validDirs[i] = 1;
                                } else {
                                    validDirs[i] = 0;
                                }
                            }
                            
                            Debug.Log("startcap1");
                            for (int i = 0; i < 4; i++) {
                                Debug.Log(validDirs[i] + " ");
                            }
                            Debug.Log("endcap1");

                            int decision = 4;
                            float[] dists = new float[4];
                            float minDist = float.MaxValue;
                            Vector2 targPos = target.transform.position;

                            for (int i = 0; i < 4; i++) {
                                if (validDirs[i] == 1) {
                                    // check distance to target
                                    dists[i] = Vector2.Distance(movements[i], targPos);
                                }
                            }

                            // chose based on minimum distance
                            minDist = dists[0];
                            decision = 0;
                            for (int i = 0; i < 4; i++) {
                                if (dists[i] < minDist && validDirs[i] == 1) {
                                    minDist = dists[i];
                                    decision = i;
                                }
                            }

                            if (decision < 0 || decision > 3) {
                                Debug.Log("Decision is invalid");
                            }

                            //Debug.Log("test " + dirs[decision]);
                            move._dir = dirs[decision];

                            for (int i = 0; i < 4; i++) {
                                prevChoices[i] = 1;
                            }

                            switch (decision) {
                                case 0:
                                    prevChoices[2] = 0;
                                    break;
                                case 1:
                                    prevChoices[3] = 0;
                                    break;
                                case 2:
                                    prevChoices[0] = 0;
                                    break;
                                case 3:
                                    prevChoices[1] = 0;
                                    break;
                                default:
                                    Debug.Log("DirSwitch is defaulting with decision = " + decision);
                                    break;
                            }

                        }
                    }

                    if (ghostID == PINKY && tick - previousTick >= frameLockout) {
                        target = pacMan;
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
                        for (int x = 0; x < 4 - 1; x++) {
                            for (int y = 0; y < 4 - x - 1; y++) {
                                float newx = target.transform.position.x;
                                float newy = target.transform.position.y;
                                //Debug.Log(num2vec((int)target.GetComponent<PlayerMovement>().getDirection() - 1));
                                newx += num2vec((int)target.GetComponent<PlayerMovement>().getDirection() - 1).x * 4;
                                newy += num2vec((int)target.GetComponent<PlayerMovement>().getDirection() - 1).y * 4;
                                if (Vector2.Distance(movements[y], new Vector2(newx, newy)) >
                                   Vector2.Distance(movements[y + 1], new Vector2(newx, newy))) {
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
                            if (move.checkDirectionClear(num2vec((int)dirs[i] - 1)) && !compareDirectionsButUseful(num2vec((int)dirs[i] - 1), reverseDir)) {
                                move._dir = dirs[i];
                                //Debug.Log("chosen direction is entry " + i + ", " + dirs[i]);
                                previousTick = tick;
                                i = 10;

                            }
                        }
                    }

                    if (ghostID == INKY && tick - previousTick >= frameLockout) {
                        target = pacMan;
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
                        for (int x = 0; x < 4 - 1; x++) {
                            for (int y = 0; y < 4 - x - 1; y++) {
                                float newx = target.transform.position.x;
                                float newy = target.transform.position.y;
                                newx += num2vec((int)target.GetComponent<PlayerMovement>().getDirection() - 1).x * 2;
                                newy += num2vec((int)target.GetComponent<PlayerMovement>().getDirection() - 1).y * 2;
                                Vector2 tempTarget = new Vector2(newx, newy);
                                GameObject B = GameObject.Find("Blinky(Clone)");
                                Vector2 Bpos = new Vector2(B.GetComponent<Transform>().position.x, B.GetComponent<Transform>().position.y);
                                Vector2 tempVec = Bpos - tempTarget;
                                float mag = tempVec.magnitude;
                                tempVec.Normalize();
                                tempVec *= (-1 * mag);
                                newx = tempVec.x;
                                newy = tempVec.y;
                                //Debug.Log("Testing: " + newx + " " + newy);
                                if (Vector2.Distance(movements[y], new Vector2(newx, newy)) >
                                   Vector2.Distance(movements[y + 1], new Vector2(newx, newy))) {
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
                            if (move.checkDirectionClear(num2vec((int)dirs[i] - 1)) && !compareDirectionsButUseful(num2vec((int)dirs[i] - 1), reverseDir)) {
                                move._dir = dirs[i];
                                //Debug.Log("chosen direction is entry " + i + ", " + dirs[i]);
                                previousTick = tick;
                                i = 10;

                            }
                        }
                    }

                }
                // etc.

                break;

		case State.entering:

            // Leaving this code in here for you.
			move._dir = Movement.Direction.still;
            //Debug.Log("got here 5");
            if (transform.position.x < 13.48f || transform.position.x > 13.52) {
			    //print ("GOING LEFT OR RIGHT");
			    transform.position = Vector3.Lerp (transform.position, new Vector3 (13.5f, transform.position.y, transform.position.z), 3f * Time.deltaTime);
			} else if (transform.position.y > -13.99f || transform.position.y < -14.01f) {
				gameObject.GetComponent<Animator>().SetInteger ("Direction", 2);
				transform.position = Vector3.Lerp (transform.position, new Vector3 (transform.position.x, -14f, transform.position.z), 3f * Time.deltaTime);
			} else {
				fleeing = false;
				dead = false;
				gameObject.GetComponent<Animator>().SetBool("Running", true);
				_state = State.waiting;
			}

            break;
		}
	}

    // Utility routines

	Vector2 num2vec(int n){
        switch (n)
        {
            case 0:
                return new Vector2(1, 0);
            case 1:
    			return new Vector2(0, 1);
		    case 2:
			    return new Vector2(-1, 0);
            case 3:
			    return new Vector2(0, -1);
            default:    // should never happen
                if (ghostID != PINKY) {
                    Debug.Log("num2vec defaulting at " + n);
                }
                return new Vector2(0, 0);
        }
	}

	bool compareDirections(bool[] n, bool[] p){
		for(int i = 0; i < n.Length; i++){
			if (n [i] != p [i]) {
				return false;
			}
		}
		return true;
	}

    bool compareDirectionsButUseful(Vector2 n, Vector2 p)
    {
        for(int x = 0; x < 2; x++)
        {
            if (n[x] != p[x])
                return false;
        }
        return true;
    }

    bool checkPrevInit() {
        for (int i = 0; i < 4; i++) {
            if (prevChoices[i] != 0) {
                return true;
            }
        }
        return false;
    }

}