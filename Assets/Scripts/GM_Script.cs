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

    // Scene objects
    GameObject playerCanvas;

    GameObject playerWinObj;
    GameObject moneyText;
    GameObject livesText;
    GameObject levelText;
    GameObject healthBar;
    GameObject expBar;

    public enum GAMEPHASE { LOBBY, PVP, PVE };

    public GAMEPHASE currentPhase;

    public bool gameStarted;
    public bool gameWon;
    public bool pvpDone = false;

    float timer = 30;
    int roundNum = 1;
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

    public void EnablePVP(bool on, int p1 = 0, int p2 = 0)
    {
        if (!on)
        {
            foreach (PlayerController player in FindObjectsOfType<PlayerController>())
            {
                player.pvpEnabled = on;
            }
        }
        else
        {
            foreach (PlayerController player in FindObjectsOfType<PlayerController>())
            {
                if(player.Owner == p1 || player.Owner == p2)
                {
                    player.pvpEnabled = on;
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
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        int firstPlayer = Random.Range(0, players.Length);
        int secondPlayer = Random.Range(0, players.Length);
        while (firstPlayer == secondPlayer)
        {
            secondPlayer = Random.Range(0, players.Length);
        }

        switch (currentPhase)
        {
            case GAMEPHASE.LOBBY:
                if((roundNum + 1) % 4 == 0)
                {
                    pvpDone = false;
                    EnablePVP(true, players[firstPlayer].Owner, players[secondPlayer].Owner);
                    currentPhase = GAMEPHASE.PVP;
                }
                else
                {
                    EnablePVP(false);
                    currentPhase = GAMEPHASE.PVE;
                }

                // Override other cases
                if(roundNum > 10)
                {
                    pvpDone = false;
                    EnablePVP(true, players[firstPlayer].Owner, players[secondPlayer].Owner);
                    currentPhase = GAMEPHASE.PVP;
                }
                break;
            case GAMEPHASE.PVE:
                EnablePVP(false);
                currentPhase = GAMEPHASE.LOBBY;
                break;
            case GAMEPHASE.PVP:
                EnablePVP(false);
                currentPhase = GAMEPHASE.LOBBY;
                break;
        }
        ResetHealth();
        roundNum++;

        SetTimer();

        if (IsServer)
        {

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
                if (timer <= 0 || (pvpDone && currentPhase == GAMEPHASE.PVP))
                {
                    timer = 30;
                    NextPhase();
                }
            }
        }
    }
}
