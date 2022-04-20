using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using NETWORK_ENGINE;

public class PlayerController : NetworkComponent
{
    [Header("Stats")]
    // Stats
    public float attackSpeed = 1f;
    public float movementSpeed = 1f;
    public float damage = 1f;
    public float maxHealthMod = 1f;
    public float regen = 0.1f;
    public float critChance = 0.05f;
    public int lives = 3;
    public int money = 0;
    public int level = 1;
    public int exp = 0;
    public bool isAlive = true;

    public float maxHealth = 100f;
    public float health = 100f;

    public Item[] items = new Item[50];
    public GameObject itemDisplayPrefab;
    GameObject[] itemDisplay = new GameObject[20];

    [Header("Core Variables")]
    // Core
    public float movementMod;
    public float sensitivity;
    public bool pvpEnabled = false;
    public bool canJump = true;
    Ability[] abilities = new Ability[4];

    Vector3 LastMove;
    Vector3 LastRotate;
    Rigidbody rb;
    bool isFiringM1;
    bool isFiringM2;
    bool isFiringUtility;
    bool isFiringSpecial;
    bool isFlashing = false;

    // Special Modifiers
    float dashMod = 0;

    public Material baseMat;
    public Material redFlash;

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

        if(flag == "LOOK")
        {
            if (IsServer)
            {
                string[] args = value.Split(',');

                LastRotate = new Vector3(float.Parse(args[0]), 0, 0);
                Transform camPivot = transform.Find("CameraCenter");
                camPivot.localRotation = Quaternion.Euler(new Vector3(float.Parse(args[1]), 0, 0));
            }
        }

        if(flag == "JUMP")
        {
            rb.AddForce(new Vector3(0, 200, 0));
        }

        if(flag == "DAMAGE")
        {
            string[] args = value.Split(',');

            float _damage = float.Parse(args[0]);
            bool _playerOwned = bool.Parse(args[1]);

            if (!pvpEnabled && _playerOwned)
            {
                return;
            }

            health -= _damage;
            if (health < 0)
            {
                health = 0;
            }

            if(health != 0)
            {
                StartCoroutine(FlashRed());
            }
        }

        if(flag == "DEATH")
        {
            string[] args = value.Split(',');

            int _owner = int.Parse(args[0]);
            bool _playerOwned = bool.Parse(args[1]);

            // Die
            lives--;
            if (_playerOwned)
            {
                foreach (PlayerController player in FindObjectsOfType<PlayerController>())
                {
                    if (player.Owner == _owner)
                    {
                        player.money += 20 * player.level; // Arbitrary money gain /////////////////////////////////////
                        break;
                    }
                }
            }
            FindObjectOfType<GM_Script>().pvpDone = true;
            if (lives <= 0)
            {
                // Player is eliminated
                isAlive = false;
            }
            else
            {
                health = maxHealth;
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

    public void TakeDamage(float _damage, int _owner, bool _playerOwned)
    {
        if (!pvpEnabled && _playerOwned)
        {
            return;
        }

        SendUpdate("DAMAGE", _damage + "," + _playerOwned);

        health -= _damage;
        if(health < 0)
        {
            health = 0;
        }

        if(health == 0)
        {
            SendUpdate("DEATH", _owner + "," + _playerOwned);
            // Die
            lives--;
            if (_playerOwned)
            {
                foreach (PlayerController player in FindObjectsOfType<PlayerController>())
                {
                    if (player.Owner == _owner)
                    {
                        money += 20 * player.level; // Arbitrary money gain /////////////////////////////////////
                        break;
                    }
                }
            }
            FindObjectOfType<GM_Script>().pvpDone = true;
            if(lives <= 0)
            {
                // Player is eliminated
                isAlive = false;

                // Check if game has ended
                FindObjectOfType<GM_Script>().CheckWin();
            }
            else
            {
                health = maxHealth;
            }
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (IsLocalPlayer)
        {
            Vector2 movement = context.ReadValue<Vector2>();

            if (context.action.phase == InputActionPhase.Started || context.action.phase == InputActionPhase.Performed)
            {
                LastMove = new Vector3(movement.x, 0, movement.y);
                SendCommand("MOVE", movement.x + "," + movement.y);
            }

            if (context.action.phase == InputActionPhase.Canceled)
            {
                LastMove = new Vector3(movement.x, 0, movement.y);
                SendCommand("MOVE", movement.x + "," + movement.y);
            }
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (IsLocalPlayer)
        {
            Vector2 mouseMovement = context.ReadValue<Vector2>();

            if (context.action.phase == InputActionPhase.Started || context.action.phase == InputActionPhase.Performed)
            {
                LastRotate = new Vector3(mouseMovement.x, mouseMovement.y, 0);

                Transform camPivot = transform.Find("CameraCenter");
                camPivot.localRotation = Quaternion.Euler(camPivot.eulerAngles.x + -LastRotate.y * sensitivity, 0, 0);
                rb.rotation = Quaternion.Euler(new Vector3(0, rb.rotation.eulerAngles.y + LastRotate.x * sensitivity, 0));
                //rb.angularVelocity = new Vector3(0, LastRotate.x * sensitivity * 3, 0);

                SendCommand("LOOK", rb.rotation.eulerAngles.y + "," + camPivot.localRotation.eulerAngles.x);
            }

            if (context.action.phase == InputActionPhase.Canceled)
            {
                LastRotate = new Vector3(mouseMovement.x, mouseMovement.y, 0);
            }
        }

    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (IsLocalPlayer)
        {
            if(context.action.phase == InputActionPhase.Started && canJump)
            {
                SendCommand("JUMP", "");
                canJump = false;
            }
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
                abilities[0].UseAbility(this);
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
                abilities[2].UseAbility(this);
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
                abilities[1].UseAbility(this);
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
                abilities[3].UseAbility(this);
            }
        }
    }

    public IEnumerator StartDash()
    {
        dashMod = 80;
        yield return new WaitForSeconds(0.2f);
        dashMod = 0;
    }

    public void GetPassiveStats()
    {
        attackSpeed = 1f;
        movementSpeed = 1f;
        damage = 1f;
        maxHealthMod = 1f;
        regen = 0.1f;
        critChance = 0.05f;

        for(int i = 0; i < items.Length; i++)
        {
            if(items[i] == null)
            {
                continue;
            }

            items[i].Passive(this);
        }
    }

    public void OnShoot()
    {
        GetPassiveStats();

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                continue;
            }

            items[i].OnShoot(this);
        }
    }

    public void OnTakeDamage()
    {
        GetPassiveStats();

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                continue;
            }

            items[i].OnTakeDamage(this);
        }
    }

    public void OnHit()
    {
        GetPassiveStats();

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                continue;
            }

            items[i].OnHit(this);
        }
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
            rb.rotation = Quaternion.Euler(new Vector3(0, LastRotate.x, 0));

            Vector3 forwardMove = transform.forward * LastMove.z * (movementMod + dashMod) * movementSpeed;
            Vector3 sideStepMove = transform.right * LastMove.x * (movementMod + dashMod) * movementSpeed;
            rb.velocity = new Vector3(forwardMove.x + sideStepMove.x, rb.velocity.y, forwardMove.z + sideStepMove.z);

            //Transform camPivot = transform.Find("CameraCenter");
            //camPivot.localRotation = Quaternion.Euler(camPivot.eulerAngles.x + LastRotate.y * sensitivity, 0, 0);
            //rb.angularVelocity = new Vector3(0, LastRotate.x * sensitivity * 3, 0);
        }

        if (IsClient)
        {
            if (IsLocalPlayer && MyId.IsInit)
            {
                Transform camTransform = transform.Find("CameraCenter");
                Vector3 camPos = camTransform.position - 5 * (camTransform.forward) + camTransform.up;
                Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, camPos, 15);
                Camera.main.transform.LookAt(camTransform);

                Cursor.lockState = CursorLockMode.Locked;

                if (isFiringM1)
                {
                    abilities[0].UseAbility(this);
                }
                if (isFiringM2)
                {
                    abilities[1].UseAbility(this);
                }
                if (isFiringUtility)
                {
                    abilities[2].UseAbility(this);
                }
                if (isFiringSpecial)
                {
                    abilities[3].UseAbility(this);
                }

                FindObjectOfType<GM_Script>().UpdatePlayerUI(money.ToString(), lives.ToString(), level.ToString(), (float)(health / maxHealth), (float)(exp / (level * 100f)));
            }
        }
    }

    public IEnumerator FlashRed()
    {
        if (!isFlashing)
        {
            isFlashing = true;
            SkinnedMeshRenderer mr = transform.Find("Player").GetComponent<SkinnedMeshRenderer>();
            Material[] mats = mr.materials;

            for (int i = 0; i < 10; i++)
            {
                mats[1] = redFlash;
                mr.materials = mats;
                yield return new WaitForSeconds(0.03f);
                mats[1] = baseMat;
                mr.materials = mats;
                yield return new WaitForSeconds(0.03f);
            }

            isFlashing = false;
        }
        yield return null;
    }

    public void UpdateItems()
    {
        GameObject itemObj = GameObject.Find("PlayerCanvas/Items");

        // Delete existing items
        for(int i = 0; i < itemDisplay.Length; i++)
        {
            if(itemDisplay[i] != null)
            {
                Destroy(itemDisplay[i]);
            }
        }

        // Display new items
        for(int i = 0; i < items.Length; i++)
        {
            itemDisplay[i] = Instantiate(itemDisplayPrefab, itemObj.transform);
            //itemDisplay[i].GetComponent<Image>().sprite = items[i].GetSprite();
            itemDisplay[i].transform.Find("Count").GetComponent<Text>().text = "x" + items[i].stacks;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.tag == "GROUND")
        {
            canJump = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "GROUND")
        {
            canJump = false;
        }
    }
}
