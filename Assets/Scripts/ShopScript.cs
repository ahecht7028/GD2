using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopScript : MonoBehaviour
{
    public GameObject[] optionIcons;
    public Text nameTextField;
    public Text descTextField;
    public GameObject shopIcons;
    public GameObject buyButton;

    Item[] generatedItems = new Item[3];

    public bool shopEnabled = false;

    public void HoverOption(int op)
    {
        if (!shopEnabled)
        {
            return;
        }
        nameTextField.text = generatedItems[op].itemName;
        descTextField.text = generatedItems[op].desc;
    }

    public void ReleaseHover()
    {
        if (!shopEnabled)
        {
            return;
        }
        nameTextField.text = "";
        descTextField.text = "";
    }

    public void ShopOpened()
    {
        shopIcons.SetActive(false);
        buyButton.SetActive(true);

        shopEnabled = true;
        ReleaseHover();
    }

    public void BuyItem()
    {
        foreach (PlayerController player in FindObjectsOfType<PlayerController>())
        {
            if (player.IsLocalPlayer)
            {
                if(player.money < 120)
                {
                    // Player can't buy the item
                    return;
                }
                else
                {
                    player.money -= 120;
                }
                break;
            }
        }
        shopIcons.SetActive(true);
        buyButton.SetActive(false);
        for(int i = 0; i < generatedItems.Length; i++)
        {
            generatedItems[i] = GenerateItem();
            optionIcons[i].GetComponent<Image>().sprite = generatedItems[i].GetSprite();
        }
    }

    public void SelectItem(int op)
    {
        foreach(PlayerController player in FindObjectsOfType<PlayerController>())
        {
            if (player.IsLocalPlayer)
            {
                player.PickupItem(generatedItems[op].id);
                player.OpenShopMenu();
                break;
            }
        }
    }

    public Item GenerateItem()
    {
        int itemId = Random.Range(0, 6);
        switch (itemId)
        {
            case 0:
                return new Boots();
            case 1:
                return new Heart();
            case 2:
                return new Ammo();
            case 3:
                return new AttackSpeed();
            case 4:
                return new Scope();
            case 5:
                return new Medkit();
        }
        Debug.LogError("No item id found");
        return new Boots();
    }

    void Start()
    {
        
    }



    void Update()
    {
        
    }
}
