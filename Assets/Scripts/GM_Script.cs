using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NETWORK_ENGINE;


public class GM_Script : NetworkComponent
{
    public bool gameStarted;
    public bool gameWon;

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "GAMESTART")
        {
            gameStarted = true;

            foreach(NetPM npm in FindObjectsOfType<NetPM>())
            {
                npm.transform.Find("Canvas").gameObject.SetActive(false);
            }
        }
        if(flag == "GAMEWON")
        {
            gameWon = true;
            if (IsServer)
            {
                SendUpdate("GAMEWON", value);
            }
        }
    }

    public override void NetworkedStart()
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        while (!gameStarted && IsServer)
        {
            bool readyGo = true;
            int count = 0;
            foreach (NetPM lp in FindObjectsOfType<NetPM>())
            {
                if (!lp.isReady)
                {
                    readyGo = false;
                    break;
                }
                count++;
            }
            if (count < 1)
            {
                readyGo = false;
            }
            gameStarted = readyGo;
            yield return new WaitForSeconds(1);
        }
        if (IsServer)
        {
            SendUpdate("GAMESTART", gameStarted.ToString());

            foreach (NetPM lp in FindObjectsOfType<NetPM>())
            {
                // Object 0: Player
                GameObject temp = MyCore.NetCreateObject(0, lp.Owner, new Vector3(-5 + (lp.Owner * 2), 0, 0), Quaternion.identity);

            }
        }

        yield return new WaitForSeconds(0.2f);

        while (IsConnected)
        {
            if (IsServer)
            {
                if (!gameWon)
                {

                }
                else
                {
                    yield return new WaitForSeconds(15);
                    // Disconnect Server
                    StartCoroutine(MyCore.DisconnectServer());
                }

                if (IsDirty)
                {
                    SendUpdate("GAMESTART", gameStarted.ToString());
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(1);
        }
    }

    public void WinGame()
    {
        if (IsServer)
        {
            gameWon = true;
        }
        if (IsClient)
        {
            SendCommand("GAMEWON", "");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        gameStarted = false;
        gameWon = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
