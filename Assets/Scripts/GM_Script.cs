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

    GameObject[] aliveEnemies = new GameObject[10];

    // Scene objects
    GameObject playerCanvas;

    GameObject playerWinObj;
    GameObject moneyText;
    GameObject livesText;
    GameObject levelText;
    GameObject healthBar;
    GameObject expBar;

    GameObject[] lobbyLives = new GameObject[4];

    AudioSource aSource;
    public AudioClip fanfareSound;
    public AudioClip[] songs;

    public enum GAMEPHASE { LOBBY, PVP, PVE };

    public GAMEPHASE currentPhase;

    public bool gameStarted;
    public bool gameWon;
    public bool pvpDone = false;

    float timer = 40;
    public int roundNum = 1;
    int gameWinner = 0;

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "GAMESTART")
        {
            gameStarted = true;
            playerCanvas.SetActive(true);

            foreach (NetPM npm in FindObjectsOfType<NetPM>())
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
                if (IsLocalPlayer && currentPhase != GAMEPHASE.LOBBY)
                {
                    aSource.PlayOneShot(fanfareSound);
                }
                NextPhase();
            }
        }
        if(flag == "SHOW_WINNER")
        {
            playerWinObj.SetActive(true);
            string winnerName = "<NAME>";
            foreach (NetPM npm in FindObjectsOfType<NetPM>())
            {
                if (npm.Owner == int.Parse(value))
                {
                    winnerName = npm.pName;
                }
            }
            playerWinObj.GetComponent<Text>().text = winnerName + " Wins!";
        }
        if(flag == "PVP")
        {
            string[] args = value.Split(',');

            bool on = bool.Parse(args[0]);
            int p1 = int.Parse(args[1]);
            int p2 = int.Parse(args[2]);

            EnablePVP(on, p1, p2);
        }
        if(flag == "RESET_HEALTH")
        {
            foreach(PlayerController player in FindObjectsOfType<PlayerController>())
            {
                player.health = player.maxHealth;
            }
        }
    }

    public override void NetworkedStart()
    {
        playerCanvas = GameObject.Find("PlayerCanvas");
        playerCanvas.SetActive(false);
        aSource = GetComponent<AudioSource>();
        lobbyLives[0] = GameObject.Find("Lobby/LobbyCanvas/Player1Info");
        lobbyLives[1] = GameObject.Find("Lobby/LobbyCanvas/Player2Info");
        lobbyLives[2] = GameObject.Find("Lobby/LobbyCanvas/Player3Info");
        lobbyLives[3] = GameObject.Find("Lobby/LobbyCanvas/Player4Info");
        aSource.clip = songs[0];
        aSource.Play();
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
            if (count < 2)
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
                temp.GetComponent<PlayerController>().playerName = lp.pName;
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
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        for(int i = 0; i < 4; i++)
        {
            lobbyLives[i].transform.Find("Text").GetComponent<Text>().text = "";
            lobbyLives[i].transform.Find("LivesText").GetComponent<Text>().text = "";
        }
        for(int i = 0; i < players.Length; i++)
        {
            lobbyLives[i].transform.Find("Text").GetComponent<Text>().text = players[i].playerName + "\nLives:";
            lobbyLives[i].transform.Find("LivesText").GetComponent<Text>().text = players[i].lives.ToString();
        }
    }

    public void EnablePVP(bool on, int p1 = 0, int p2 = 0)
    {
        if (!on)
        {
            foreach (PlayerController player in FindObjectsOfType<PlayerController>())
            {
                player.pvpEnabled = on;
                Debug.Log("Player " + player.Owner + " pvp set to " + player.pvpEnabled);
            }
        }
        else
        {
            foreach (PlayerController player in FindObjectsOfType<PlayerController>())
            {
                if(player.Owner == p1 || player.Owner == p2)
                {
                    player.pvpEnabled = on;
                    Debug.Log("Player " + player.Owner + " pvp set to " + player.pvpEnabled);
                }
            }
        }
        
        if (IsServer)
        {
            SendUpdate("PVP", on + "," + p1 + "," + p2);
        }
    }

    public void ResetHealth()
    {
        foreach (PlayerController player in FindObjectsOfType<PlayerController>())
        {
            player.health = player.maxHealth;
        }
        SendUpdate("RESET_HEALTH", "");
    }

    public void NextPhase()
    {
        if (IsServer)
        {
            CheckWin();
            if (gameWon)
            {
                return;
            }

            if(currentPhase == GAMEPHASE.PVE)
            {
                DeleteEnemies();
            }
        }

        PlayerController[] players = FindObjectsOfType<PlayerController>();

        int firstPlayer = Random.Range(0, players.Length);
        int secondPlayer = Random.Range(0, players.Length);
        while (!players[firstPlayer].isAlive)
        {
            firstPlayer = Random.Range(0, players.Length);
        }
        while (firstPlayer == secondPlayer || !players[secondPlayer].isAlive)
        {
            secondPlayer = Random.Range(0, players.Length);
        }

        switch (currentPhase)
        {
            case GAMEPHASE.LOBBY:
                if((roundNum + 1) % 4 == 0)
                {
                    pvpDone = false;
                    if (IsServer)
                    {
                        EnablePVP(true, players[firstPlayer].Owner, players[secondPlayer].Owner);
                    }
                    currentPhase = GAMEPHASE.PVP;
                }
                else
                {
                    if (IsServer)
                    {
                        EnablePVP(false);
                    }
                    currentPhase = GAMEPHASE.PVE;
                }

                // Override other cases
                if(roundNum > 10)
                {
                    pvpDone = false;
                    if (IsServer)
                    {
                        EnablePVP(true, players[firstPlayer].Owner, players[secondPlayer].Owner);
                    }
                    currentPhase = GAMEPHASE.PVP;
                }
                break;
            case GAMEPHASE.PVE:
                if (IsServer)
                {
                    EnablePVP(false);
                }
                currentPhase = GAMEPHASE.LOBBY;
                break;
            case GAMEPHASE.PVP:
                if (IsServer)
                {
                    EnablePVP(false);
                }
                currentPhase = GAMEPHASE.LOBBY;
                break;
        }

        switch (currentPhase)
        {
            case GAMEPHASE.LOBBY:
                aSource.clip = songs[0];
                aSource.Play();
                break;
            case GAMEPHASE.PVE:
                aSource.clip = songs[1];
                aSource.Play();
                break;
            case GAMEPHASE.PVP:
                aSource.clip = songs[2];
                aSource.Play();
                break;
        }

        ResetHealth();
        roundNum++;

        SetTimer();

        if (IsServer)
        {
            foreach(PlayerController player in FindObjectsOfType<PlayerController>())
            {
                player.Heal(99999);
            }

            switch (currentPhase)
            {
                case GAMEPHASE.LOBBY:
                    for (int i = 0; i < players.Length; i++)
                    {
                        if (players[i].isAlive)
                        {
                            players[i].transform.position = lobbyPosList[i].position;
                        }
                    }
                    break;

                case GAMEPHASE.PVE:
                    for (int i = 0; i < players.Length; i++)
                    {
                        if (players[i].isAlive)
                        {
                            players[i].transform.position = PVEPosList[i].position;
                        }
                    }
                    SpawnEnemies();
                    break;

                case GAMEPHASE.PVP:
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
                timer = 26;
                roundNumText.text = "Round " + roundNum + ": Lobby";
                break;

            case GAMEPHASE.PVE:
                timer = 60;
                roundNumText.text = "Round " + roundNum + ": PVE";
                break;

            case GAMEPHASE.PVP:
                timer = 60;
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
        Transform enemyPos = GameObject.Find("Spawns/EnemySpawnLoc").transform;
        enemyPosList = new Transform[10];
        enemyPosList[0] = enemyPos.Find("1").transform;
        enemyPosList[1] = enemyPos.Find("2").transform;
        enemyPosList[2] = enemyPos.Find("3").transform;
        enemyPosList[3] = enemyPos.Find("4").transform;
        enemyPosList[4] = enemyPos.Find("5").transform;
        enemyPosList[5] = enemyPos.Find("6").transform;
        enemyPosList[6] = enemyPos.Find("7").transform;
        enemyPosList[7] = enemyPos.Find("8").transform;
        enemyPosList[8] = enemyPos.Find("9").transform;
        enemyPosList[9] = enemyPos.Find("10").transform;
    }

    public void SpawnEnemies()
    {
        if (IsServer)
        {
            for(int i = 0; i < enemyPosList.Length; i++)
            {
                int isRed = Random.Range(0, 2);
                aliveEnemies[i] = MyCore.NetCreateObject(7 + isRed, Owner, enemyPosList[i].position, Quaternion.identity);
            }
        }
    }

    public void DeleteEnemies()
    {
        if (IsServer)
        {
            for (int i = 0; i < enemyPosList.Length; i++)
            {
                if(aliveEnemies[i] != null)
                {
                    MyCore.NetDestroyObject(aliveEnemies[i].GetComponent<NetworkID>().NetId);
                }
                int isRed = Random.Range(0, 2);
                aliveEnemies[i] = MyCore.NetCreateObject(7 + isRed, Owner, enemyPosList[i].position, Quaternion.identity);
            }
        }
    }

    public void CheckWin()
    {
        int alivePlayers = 0;
        int winnerOwner = 0;
        foreach(PlayerController player in FindObjectsOfType<PlayerController>())
        {
            if (player.isAlive)
            {
                alivePlayers++;
                winnerOwner = player.Owner;
            }
        }

        if(alivePlayers == 1)
        {
            // winnerOwner player wins
            gameWinner = winnerOwner;
            playerWinObj.SetActive(true);
            string winnerName = "<NAME>";
            foreach(NetPM npm in FindObjectsOfType<NetPM>())
            {
                if(npm.Owner == gameWinner)
                {
                    winnerName = npm.pName;
                }
            }
            playerWinObj.GetComponent<Text>().text = winnerName + " Wins!";
            SendUpdate("SHOW_WINNER", gameWinner.ToString());
            WinGame();
        }
    }

    public void UpdatePlayerUI(string money, string lives, string level, float healthPercent, float expPercent)
    {
        moneyText.GetComponent<Text>().text = money;
        livesText.GetComponent<Text>().text = lives;
        levelText.GetComponent<Text>().text = level;
        healthBar.GetComponent<Image>().fillAmount = healthPercent;
        expBar.GetComponent<Image>().fillAmount = expPercent;
    }

    public int GetNumEnemies()
    {
        int num = 0;
        for(int i = 0; i < aliveEnemies.Length; i++)
        {
            if(aliveEnemies[i] != null)
            {
                num++;
            }
        }
        return num;
    }

    // Start is called before the first frame update
    void Start()
    {
        timerText = GameObject.Find("Scoreboard/Canvas/Timer").GetComponent<Text>();
        roundNumText = GameObject.Find("Scoreboard/Canvas/RoundNum").GetComponent<Text>();
        playerWinObj = GameObject.Find("PlayerCanvas/PlayerWin/Text");
        moneyText = GameObject.Find("PlayerCanvas/Stats/Money/Text");
        livesText = GameObject.Find("PlayerCanvas/Stats/Lives/Text");
        levelText = GameObject.Find("PlayerCanvas/Bars/Level/Text");
        healthBar = GameObject.Find("PlayerCanvas/Bars/HealthBar/Fill");
        expBar = GameObject.Find("PlayerCanvas/Bars/EXPBar/Fill");
        playerWinObj.SetActive(false);

        gameStarted = false;
        gameWon = false;
        currentPhase = GAMEPHASE.LOBBY;
        GetSpawnLocations();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStarted && !gameWon)
        {
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                timer = 0;
            }
            UpdateUI();
            if (IsServer)
            {
                if (timer <= 0 || (pvpDone && currentPhase == GAMEPHASE.PVP) || (GetNumEnemies() == 0 && currentPhase == GAMEPHASE.PVE))
                {
                    timer = 40;
                    NextPhase();
                }
            }
        }
    }
}
