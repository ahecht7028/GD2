using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NETWORK_ENGINE;

public class NetPM : NetworkComponent
{
    public InputField nameField;
    public Toggle readyToggle;

    public string pName = "";
    public bool isReady = false;

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "NAME")
        {
            pName = value;
            if (IsServer)
            {
                SendUpdate("NAME", value);
            }
            if(!IsLocalPlayer && IsClient)
            {
                nameField.text = pName;
            }
        }

        if(flag == "READY")
        {
            isReady = bool.Parse(value);
            if (IsServer)
            {
                SendUpdate("READY", value);
            }
            if(!IsLocalPlayer && IsClient)
            {
                readyToggle.isOn = isReady;
            }
        }
    }

    public override void NetworkedStart()
    {
        if (!IsLocalPlayer && !IsServer)
        {
            transform.Find("Canvas/Back").gameObject.SetActive(false);
            nameField.interactable = false;
            readyToggle.interactable = false;
        }
        transform.Find("Canvas/PlayerConnect").GetComponent<RectTransform>().anchoredPosition = new Vector3(-200, 150 - (100 * Owner), 0);
    }

    public override IEnumerator SlowUpdate()
    {

        while (IsConnected)
        {



            if (IsServer)
            {


                if (IsDirty)
                {
                    SendUpdate("NAME", pName);
                    SendUpdate("READY", isReady.ToString());
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void ChangeName(string n)
    {
        if (IsLocalPlayer && MyId.IsInit)
        {
            SendCommand("NAME", n);
        }
    }

    public void ToggleReady(bool t)
    {
        if(IsLocalPlayer && MyId.IsInit)
        {
            SendCommand("READY", t.ToString());
        }
    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
