using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NETWORK_ENGINE;

public class PlayerController : NetworkComponent
{
    public float movementSpeed;
    Ability[] abilities = new Ability[4];

    Vector3 LastMove;
    Rigidbody rb;
    bool isFiringM1;
    bool isFiringM2;
    bool isFiringUtility;
    bool isFiringSpecial;

    // Special Modifiers
    float dashMod = 0;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "MOVE")
        {
            if (IsServer)
            {
                string[] args = value.Split(',');
                Vector2 movement = new Vector2(float.Parse(args[0]), float.Parse(args[1]));
                LastMove = new Vector3(movement.x, 0, movement.y);
            }
        }
    }

    public override void NetworkedStart()
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(0.1f);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 movement = context.ReadValue<Vector2>();

        if(context.action.phase == InputActionPhase.Started || context.action.phase == InputActionPhase.Performed)
        {
            LastMove = new Vector3(movement.x, 0, movement.y);
            SendCommand("MOVE", movement.x + "," + movement.y);
        }

        if(context.action.phase == InputActionPhase.Canceled)
        {
            LastMove = new Vector3(movement.x, 0, movement.y);
            SendCommand("MOVE", movement.x + "," + movement.y);
        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (abilities[0].autoUse)
        {
            if(context.action.phase == InputActionPhase.Started)
            {
                isFiringM1 = true;
            }
            if(context.action.phase == InputActionPhase.Canceled)
            {
                isFiringM1 = false;
            }
        }
        else
        {
            if (context.action.phase == InputActionPhase.Started)
            {
                abilities[0].UseAbility();
            }
        }
    }

    public void OnUtility(InputAction.CallbackContext context)
    {
        if (abilities[2].autoUse)
        {
            if (context.action.phase == InputActionPhase.Started)
            {
                isFiringUtility = true;
            }
            if (context.action.phase == InputActionPhase.Canceled)
            {
                isFiringUtility = false;
            }
        }
        else
        {
            if (context.action.phase == InputActionPhase.Started)
            {
                abilities[2].UseAbility();
            }
        }
    }

    public void OnSecondary(InputAction.CallbackContext context)
    {
        if (abilities[1].autoUse)
        {
            if (context.action.phase == InputActionPhase.Started)
            {
                isFiringM2 = true;
            }
            if (context.action.phase == InputActionPhase.Canceled)
            {
                isFiringM2 = false;
            }
        }
        else
        {
            if (context.action.phase == InputActionPhase.Started)
            {
                abilities[1].UseAbility();
            }
        }
    }

    public void OnSpecial(InputAction.CallbackContext context)
    {
        if (abilities[3].autoUse)
        {
            if (context.action.phase == InputActionPhase.Started)
            {
                isFiringSpecial = true;
            }
            if (context.action.phase == InputActionPhase.Canceled)
            {
                isFiringSpecial = false;
            }
        }
        else
        {
            if (context.action.phase == InputActionPhase.Started)
            {
                abilities[3].UseAbility();
            }
        }
    }

    public IEnumerator StartDash()
    {
        dashMod = 80;
        yield return new WaitForSeconds(0.2f);
        dashMod = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        LastMove = Vector3.zero;
        isFiringM1 = false;
        isFiringM2 = false;
        isFiringUtility = false;
        isFiringSpecial = false;

        gameObject.AddComponent<M1RapidShoot>();
        gameObject.AddComponent<M2Slash>();
        gameObject.AddComponent<UtilityDash>();
        gameObject.AddComponent<SpecialGrenade>();

        abilities[0] = GetComponent<M1RapidShoot>();
        abilities[1] = GetComponent<M2Slash>();
        abilities[2] = GetComponent<UtilityDash>();
        abilities[3] = GetComponent<SpecialGrenade>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            Vector3 forwardMove = transform.forward * LastMove.z * (movementSpeed + dashMod);
            Vector3 sideStepMove = transform.right * LastMove.x * (movementSpeed + dashMod);
            // rb.velocity = new Vector3((transform.forward * LastMove.x * movementSpeed * Time.deltaTime).x, rb.velocity.y, (transform.forward * LastMove.y * movementSpeed * Time.deltaTime).z);
            rb.velocity = new Vector3(forwardMove.x + sideStepMove.x, rb.velocity.y, forwardMove.z + sideStepMove.z);
        }

        if (IsClient)
        {
            if (IsLocalPlayer)
            {
                Transform camTransform = transform.Find("CameraCenter");
                Vector3 camPos = camTransform.position - 5 * (camTransform.forward) + 2 * camTransform.up;
                Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, camPos, 1);
                Camera.main.transform.LookAt(camTransform);

                if (isFiringM1)
                {
                    abilities[0].UseAbility();
                }
                if (isFiringM2)
                {
                    abilities[1].UseAbility();
                }
                if (isFiringUtility)
                {
                    abilities[2].UseAbility();
                }
                if (isFiringSpecial)
                {
                    abilities[3].UseAbility();
                }
            }
        }
    }
}
