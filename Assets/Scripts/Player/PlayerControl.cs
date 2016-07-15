using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class PlayerControl : MonoBehaviour
{
    [HideInInspector]
    public bool facingRight = true;			// For determining which way the player is currently facing.
    [HideInInspector]
    public bool jump = false;				// Condition for whether the player should jump.


    public float moveForce = 365f;			// Amount of force added to move the player left and right.
    public float maxSpeed = 5f;				// The fastest the player can travel in the x axis.
    public AudioClip[] jumpClips;			// Array of clips for when the player jumps.
    public float jumpForce = 1000f;			// Amount of force added when the player jumps.
    public AudioClip[] taunts;				// Array of clips for when the player taunts.
    public float tauntProbability = 50f;	// Chance of a taunt happening.
    public float tauntDelay = 1f;			// Delay for when the taunt should happen.    

    private int tauntIndex;					// The index of the taunts array indicating the most recent taunt.
    private Transform groundCheck;			// A position marking where to check if the player is grounded.
    private bool grounded = false;			// Whether or not the player is grounded.
    private Animator anim;					// Reference to the player's animator component.

    public float climbSpeed = 5.0f;    
    private bool atLadder = false;

    public Camera MainCamera;

    bool enteringDoorInputActive = false;
    float enterDoorDelay = 0.0f;
    private const float EnterDoorDelayBaseValue = 1.0f;

    private bool ClimbInputUpActive
    {
        get
        {
            return Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
        }
    }

    void Awake()
    {
        // Setting up references.
        groundCheck = transform.Find("groundCheck");
        anim = GetComponent<Animator>();
        StageManager.Player = this;
    }


    void Update()
    {        
        this.enteringDoorInputActive = false;                  		        

        // The player is grounded if a linecast to the groundcheck position hits anything on the ground layer.
        grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));  

        // If the jump button is pressed and the player is grounded then the player should jump.
        //if(Input.GetButtonDown("Jump") && grounded)
        if (Input.GetButtonDown("Jump"))
            jump = true;

        if (Input.GetButton("EnterDoor")) 
        {
            this.enteringDoorInputActive = true;
        }

        if (this.enterDoorDelay > 0.0f)
        {
            this.enterDoorDelay -= Time.deltaTime;
        }

        if (this.atLadder && this.ClimbInputUpActive)
        {
            this.GetComponent<Rigidbody2D>().gravityScale = 0;
            this.GetComponent<Rigidbody2D>().velocity = new Vector2(0, this.climbSpeed);
        }
        else 
        {
            this.GetComponent<Rigidbody2D>().gravityScale = 1;
        }
    }

    void OnTriggerStay2D(Collider2D collision) 
    {
        if (this.enteringDoorInputActive && enterDoorDelay <= 0.0f) 
        {
            this.enterDoorDelay = EnterDoorDelayBaseValue;
            IHasLocation door = collision.transform.GetComponent(typeof(IHasLocation)) as IHasLocation;
            if (door != null) 
            {
                bool transitioned = StageManager.AttemptTransition(door.GetLocation());
                if (!transitioned)
                {
                    FloatyText model = ResourceManager.GetFloatyText();
                    FloatyText text = Instantiate(model) as FloatyText;
                    text.SetText("Locked!");
                    text.transform.parent = this.transform;
                    text.transform.localPosition = new Vector3(0.0f, 1.5f, -1.0f);
                }
            }
        }

        if (collision.tag == "Ladder")
        {
            this.atLadder = true;            
        }
    }

    void FixedUpdate ()
    {        
        this.atLadder = false;

        // Cache the horizontal input.
        float h = Input.GetAxis("Horizontal");

        // The Speed animator parameter is set to the absolute value of the horizontal input.
        anim.SetFloat("Speed", Mathf.Abs(h));

        // If the player is changing direction (h has a different sign to velocity.x) or hasn't reached maxSpeed yet...
        if(h * GetComponent<Rigidbody2D>().velocity.x < maxSpeed)
            // ... add a force to the player.
            GetComponent<Rigidbody2D>().AddForce(Vector2.right * h * moveForce);

        // If the player's horizontal velocity is greater than the maxSpeed...
        if(Mathf.Abs(GetComponent<Rigidbody2D>().velocity.x) > maxSpeed)
            // ... set the player's velocity to the maxSpeed in the x axis.
            GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Sign(GetComponent<Rigidbody2D>().velocity.x) * maxSpeed, GetComponent<Rigidbody2D>().velocity.y);

        // If the input is moving the player right and the player is facing left...
        if(h > 0 && !facingRight)
            // ... flip the player.
            Flip();
        // Otherwise if the input is moving the player left and the player is facing right...
        else if(h < 0 && facingRight)
            // ... flip the player.
            Flip();

        // If the player should jump...
        if(jump)
        {
            // Set the Jump animator trigger parameter.
            anim.SetTrigger("Jump");

            // Play a random jump audio clip.
            int i = Random.Range(0, jumpClips.Length);
            AudioSource.PlayClipAtPoint(jumpClips[i], transform.position);

            // Add a vertical force to the player.
            GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForce));

            // Make sure the player can't jump again until the jump conditions from Update are satisfied.
            jump = false;
        }        
    }    
    
    void Flip ()
    {
        // Switch the way the player is labelled as facing.
        facingRight = !facingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }


    public IEnumerator Taunt()
    {
        // Check the random chance of taunting.
        float tauntChance = Random.Range(0f, 100f);
        if(tauntChance > tauntProbability)
        {
            // Wait for tauntDelay number of seconds.
            yield return new WaitForSeconds(tauntDelay);

            // If there is no clip currently playing.
            if(!GetComponent<AudioSource>().isPlaying)
            {
                // Choose a random, but different taunt.
                tauntIndex = TauntRandom();

                // Play the new taunt.
                GetComponent<AudioSource>().clip = taunts[tauntIndex];
                GetComponent<AudioSource>().Play();
            }
        }
    }


    int TauntRandom()
    {
        // Choose a random index of the taunts array.
        int i = Random.Range(0, taunts.Length);

        // If it's the same as the previous taunt...
        if(i == tauntIndex)
            // ... try another random taunt.
            return TauntRandom();
        else
            // Otherwise return this index.
            return i;
    }

    /// <summary>
    /// Moves character and camera to location
    /// </summary>
    /// <param name="location"></param>
    public void TeleportToLocation(Vector3 location)
    {
        this.transform.position = location;
        this.MainCamera.transform.position = new Vector3(location.x, location.y, this.MainCamera.transform.position.z);
    }
}
