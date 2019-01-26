using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField] private float m_JumpForce = 400f;							// Amount of force added when the player jumps.
    [SerializeField] private float m_LowJumpMul = 2f;                           // Gravity multiplier if jumpkey is released
    [SerializeField] private float m_JumpEndMul = 2.5f;                         // Gravity multiplier for the downward part of the jump
    [Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;          // Amount of maxSpeed applied to crouching movement. 1 = 100%
    [Range(0, .3f)] [SerializeField] private float m_GroundSmoothing = .05f;	// How much to smooth out the movement
    [Range(0, .3f)] [SerializeField] private float m_AirSmoothing = .1f;        // How much to smooth out the movement
    [SerializeField] private bool m_AirControl = false;							// Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
    [SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
    [SerializeField] private Transform m_PreciseGroundCheck;                    // A position marking where to check if the player is grounded more precise.
    [SerializeField] private Transform m_CeilingCheck;                          // A position marking where to check for ceilings
    [SerializeField] private Collider2D m_CrouchDisableCollider;                // A collider that will be disabled when crouching
    [SerializeField] private float m_MinRagdolVel = 1f;                         // velocity at which ragdolling ends
    [Range(-1000, 1000)] [SerializeField] private float m_RagdollTorque = 500f;
    [Range(-10, 10)] [SerializeField] private float m_JumpTorque = 1f;
    [Range(0, 1)] [SerializeField] private float m_TorqueTime = 0.5f;
    [SerializeField] private PhysicsMaterial2D m_RagdollMaterial;                // The material used for ragdolling
    private float m_TorqueThresh = 2.5f;
    public GameObject m_Corpse;                                                 //This character's corpse

    [Header("Events")]
    [Space]

    public static UnityEvent OnLandEvent;
    public static UnityEvent OnDeathEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public static BoolEvent OnCrouchEvent;
    [HideInInspector] public bool IsTouchingSpikes = false;


    const float m_GroundedRadius = .2f;                 // Radius of the overlap circle to determine if grounded
    private bool m_Grounded;                            // Whether or not the player is grounded.
    const float k_CeilingRadius = .2f;                  // Radius of the overlap circle to determine if the player can stand up
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true;                  // For determining which way the player is currently facing.
    private Vector3 m_Velocity = Vector3.zero;
    private float playerGravity = 3f;                   // for storing the original player gravity
    private bool canJump = true;                        // can the character jump again
    private bool addTorque = false;                     // Do we want to spin
    private float torqueTime;
    private Vector2 m_LastFrameVelocity;                // the velocity during the last frame.
    private Transform m_SpriteFollowerTrans;            // the sprite follower transform following the player
    private LevelManager m_Level;                       // the level manager
    private float m_RagdollStartTime;                   // the time when the ragdoll has started
    private float m_RagdollTimeout = 5f;                // The timeout for the ragdoll
    private bool m_IsRagdollOver = true;

    private bool m_wasCrouching = false;

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();

        playerGravity = m_Rigidbody2D.gravityScale;

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();

        if (OnDeathEvent == null)
            OnDeathEvent = new UnityEvent();

        // get follower
        m_SpriteFollowerTrans = gameObject.transform.Find("SpriteFollower");

        // get level manager
        m_Level = GameObject.Find("Level").GetComponent<LevelManager>();
    }

    private void FixedUpdate()
    {
        // store last frame veolicty for later
        m_LastFrameVelocity = m_Rigidbody2D.velocity;

        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, m_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;
                if (!wasGrounded)
                {
                    OnLandEvent.Invoke();
                }
            }
        }

        if (addTorque && Time.timeSinceLevelLoad < torqueTime)
        {
            
            //Adds Torque To The SpriteFollower
            if (Mathf.Abs(m_Rigidbody2D.velocity.x) > m_TorqueThresh)
            {
                int Multi = 1;
                if (m_Rigidbody2D.velocity.x > 0)
                {
                    Multi = -1;
                }
                gameObject.transform.Find("SpriteFollower").GetComponent<Rigidbody2D>().AddTorque(m_JumpTorque * Multi);
            }
        }

        if (!m_IsRagdollOver)
        {
            // check if ragdoll has ended
            bool slowed = m_Rigidbody2D.velocity.magnitude < m_MinRagdolVel;
            if ((slowed || Time.timeSinceLevelLoad > m_RagdollStartTime + m_RagdollTimeout) && (IsTouchingSpikes|| PreciseGroundCheck()))
            {
                m_IsRagdollOver = true;
            }
        }
    }

    // a ground check that is a lot more precise
    private bool PreciseGroundCheck()
    {
        return Physics2D.OverlapPoint(m_PreciseGroundCheck.position, m_WhatIsGround) != null;
    }

    public void Move(float move, bool crouch, bool jump)
    {
        // If crouching, check to see if the character can stand up
        if (!crouch)
        {
            // If the character has a ceiling preventing them from standing up, keep them crouching
            if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
            {
                crouch = true;
            }
        }

        //only control the player if grounded or airControl is turned on
        if (m_Grounded || m_AirControl)
        {

            // If crouching
            if (crouch)
            {
                if (!m_wasCrouching)
                {
                    m_wasCrouching = true;
                    OnCrouchEvent.Invoke(true);
                }

                // Reduce the speed by the crouchSpeed multiplier
                move *= m_CrouchSpeed;

                // Disable one of the colliders when crouching
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = false;
            }
            else
            {
                // Enable the collider when not crouching
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = true;

                if (m_wasCrouching)
                {
                    m_wasCrouching = false;
                    OnCrouchEvent.Invoke(false);
                }
            }

            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
            float movementSmoothing;

            if (m_Grounded)
            {
                movementSmoothing = m_GroundSmoothing;
            }
            else
            {
                movementSmoothing = m_AirSmoothing;
            }

            // And then smoothing it out and applying it to the character
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, movementSmoothing);

            // If the input is moving the player right and the player is facing left...
            if (move > 0 && !m_FacingRight)
            {
                // ... flip the player.
                // Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0 && m_FacingRight)
            {
                // ... flip the player.
                // Flip();
            }
        }

        // If the player should jump...
        if (m_Grounded && jump && canJump)
        {
            Jump();   
        }

        // If not grounded and not jumping increase Gravity
        m_Rigidbody2D.gravityScale = playerGravity;
        if (!m_Grounded)
        if (!m_Grounded)
        if (!m_Grounded)
        {
            if (m_Rigidbody2D.velocity.y < 0)  // going downwards
            {
                m_Rigidbody2D.gravityScale = playerGravity * m_JumpEndMul;
            }
            else if (!jump)  // low jump
            {
                m_Rigidbody2D.gravityScale = playerGravity * m_LowJumpMul;
            }
        }


        // Reset jump ability if you release space bar
        if (!jump)
        {
            canJump = true;
        }
    }

    private void Jump()
    {
        Debug.Log("Jump");
        // Add a vertical force to the player.
        m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
        canJump = false;
        addTorque = true;
        torqueTime = Time.timeSinceLevelLoad + m_TorqueTime;
    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public bool IsGrouned()
    {
        return m_Grounded;
    }

    public void StartRagdoll()
    {
        gameObject.GetComponent<Collider2D>().sharedMaterial = m_RagdollMaterial;
        gameObject.transform.Find("SpriteFollower").GetComponent<Rigidbody2D>().AddTorque(m_RagdollTorque);
        m_IsRagdollOver = false;
        m_RagdollStartTime = Time.timeSinceLevelLoad;
    }

    public void OnRagdollEnd()
    {
        LeaveCorpse();
    }

    // leaves corpse at current position
    public void LeaveCorpse()
    {
        float zRot = m_SpriteFollowerTrans.localEulerAngles.z;

        // leave corpse and round rotation to nearest 90 degrees
        GameObject corpse = Instantiate(m_Corpse, transform.position, Quaternion.Euler(0f, 0f, Mathf.Round(zRot / 90) * 90), m_Level.m_Corpses.transform);
    }

    public bool HasRagdollEnded()
    {
        return m_IsRagdollOver;
    }

    public Vector2 GetLastFrameVelocity()
    {
        return m_LastFrameVelocity;
    }

    // kill the player
    public void Kill()
    {
        OnDeathEvent.Invoke();
    }
}
