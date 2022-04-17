using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NETWORK_ENGINE;


public class GM_Script : NetworkComponent
{
    Text timerText;
    Text roundNumText;
    Transform[] lobbyPosList;
    Transform[] PVEPosList;
    Transform[] PVPPosList;
    Transform[] enemyPosList;

    public enum GAMEPHASE { LOBBY, PVP, PVE };

    public GAMEPHASE currentPhase;

    public bool gameStarted;
    public bool gameWon;

    float timer = 30;
    int roundNum = 1;

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
        if(flag == "SET_TIMER")
        {
            if (IsClient)
            {
                NextPhase();
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

        if (IsServer)
        {
            PlayerController[] players = FindObjectsOfType<PlayerController>();
            for (int i = 0; i < players.Length; i++)
            {
                players[i].transform.position = lobbyPosList[i].position;
            }
        }

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

    public void UpdateUI()
    {
        timerText.text = ((int)timer).ToString();
    }

    public void NextPhase()
    {
        switch (currentPhase)
        {
            case GAMEPHASE.LOBBY:
                if((roundNum + 1) % 4 == 0)
                {
                    currentPhase = GAMEPHASE.PVP;
                }
                else
                {
                    currentPhase = GAMEPHASE.PVE;
                }

                // Override other cases
                if(roundNum > 10)
                {
                    currentPhase = GAMEPHASE.PVP;
                }
                break;
            case GAMEPHASE.PVE:
                currentPhase = GAMEPHASE.LOBBY;
                break;
            case GAMEPHASE.PVP:
                currentPhase = GAMEPHASE.LOBBY;
                break;
        }
        roundNum++;

        SetTimer();

        if (IsServer)
        {
            PlayerController[] players = FindObjectsOfType<PlayerController>();
            switch (currentPhase)
            {
                case GAMEPHASE.LOBBY:
                    for (int i = 0; i < players.Length; i++)
                    {
                        players[i].transform.position = lobbyPosList[i].position;
                    }
                    break;

                case GAMEPHASE.PVE:
                    for (int i = 0; i < players.Length; i++)
                    {
                        players[i].transform.position = PVEPosList[i].position;
                    }
                    break;

                case GAMEPHASE.PVP:
                    int firstPlayer = Random.Range(0, players.Length);
                    int secondPlayer = Random.Range(0, players.Length);
                    while(firstPlayer == secondPlayer)
                    {
                        secondPlayer = Random.Range(0, players.Length);
                    }
                    players[firstPlayer].transform.position = PVPPosList[0].position;
                    players[secondPlayer].transform.position = PVPPosList[1].position;
                    break;
            }
            SendUpdate("SET_TIMER", "");
        }
    }

    public void SetTimer()
    {
        switch (currentPhase)
        {
            case GAMEPHASE.LOBBY:
                timer = 10;
                roundNumText.text = "Round " + roundNum + ": Lobby";
                break;

            case GAMEPHASE.PVE:
                timer = 15;
                roundNumText.text = "Round " + roundNum + ": PVE";
                break;

            case GAMEPHASE.PVP:
                timer = 20;
                roundNumText.text = "Round " + roundNum + ": PVP";
                break;
        }
    }

    public void GetSpawnLocations()
    {
        // Lobby
        Transform lobbyPos = GameObject.Find("Spawns/PlayerSpawnLoc/Lobby").transform;
        lobbyPosList = new Transform[4];
        lobbyPosList[0] = lobbyPos.Find("Pos1");
        lobbyPosList[1] = lobbyPos.Find("Pos2");
        lobbyPosList[2] = lobbyPos.Find("Pos3");
        lobbyPosList[3] = lobbyPos.Find("Pos4");

        // PVE
        Transform PVEPos = GameObject.Find("Spawns/PlayerSpawnLoc/PVE").transform;
        PVEPosList = new Transform[4];
        PVEPosList[0] = PVEPos.Find("Pos1");
        PVEPosList[1] = PVEPos.Find("Pos2");
        PVEPosList[2] = PVEPos.Find("Pos3");
        PVEPosList[3] = PVEPos.Find("Pos4");

        // PVP
        Transform PVPPos = GameObject.Find("Spawns/PlayerSpawnLoc/PVP").transform;
        PVPPosList = new Transform[2];
        PVPPosList[0] = PVPPos.Find("Pos1");
        PVPPosList[1] = PVPPos.Find("Pos2");

        // Enemy

    }

    // Start is called before the first frame update
    void Start()
    {
        timerText = GameObject.Find("Scoreboard/Canvas/Timer").GetComponent<Text>();
        roundNumText = GameObject.Find("Scoreboard/Canvas/RoundNum").GetComponent<Text>();
        gameStarted = false;
        gameWon = false;
        currentPhase = GAMEPHASE.LOBBY;
        GetSpawnLocations();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStarted)
        {
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                timer = 0;
            }
            UpdateUI();
            if (IsServer)
            {
                if (timer <= 0)
                {
                    timer = 30;
                    NextPhase();
                }
            }
        }
    }
}
