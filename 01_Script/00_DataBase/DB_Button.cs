using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Storage;
using UnityEngine.UI;
using System;

using Firebase;
using Firebase.Database;
using Firebase.Unity;
using UnityEngine.Networking;
using System.IO;
using Firebase.Extensions;

public class DB_Button : MonoBehaviour
{
    public string ItemID;
    public string ItemName;
    public int Price;
    public bool Success = false;

    public Image IconImage;
    public Text NameText;
    public Text PriceText;

    FirebaseStorage storage;
    StorageReference storageRef;

    public void initIcon(string _id, string _name, int _price = -1)
    {
        ItemID = _id;
        ItemName = _name;
        Price = _price;
        Success = false;

        NameText.text = ItemName;

        if(Price != -1)
            PriceText.text = Price.ToString();
        loadImage(ItemID);
    }
    public void loadImage(string _itemName)
    {
        storage = FirebaseStorage.DefaultInstance;
        storageRef = storage.GetReferenceFromUrl("gs://arvr-project-a1e02.appspot.com/ItemImages/" + _itemName + ".png");

        storageRef.GetDownloadUrlAsync().ContinueWithOnMainThread(task => {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                StartCoroutine(DownloadImage(task.Result.ToString()));
            }
            else
            {
                loadImage("NullImage");
            }
        });
    }
    private IEnumerator DownloadImage(string url)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                var texture = DownloadHandlerTexture.GetContent(uwr);
                Rect rect = new Rect(0, 0, texture.width, texture.height);
                Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));

                IconImage.sprite = sprite;
                Success = true;
            }
        }
    }
    public void PageBtn(string _type)
    {
        var DB = GameObject.Find("ItemManager").GetComponent<DB_ItemDB>();

        string[] _page = _type.Split(',');

        StartCoroutine(DB.Page(int.Parse(_page[0]), int.Parse(_page[1])));
    }
    public void BuyBtn()
    {
        var DB = GameObject.Find("ItemManager").GetComponent<DB_ItemDB>();

        StartCoroutine(DB.BuyItem(ItemID, Price));
    }
    public void Upload()
    {
        var Infopanel = GameObject.Find("Canvas").GetComponent<UI_ItemInfo>();
        
        Infopanel.u_Image.sprite = IconImage.sprite;
        Infopanel.u_nameText.text = ItemName;
        Infopanel.u_ItemID = ItemID;
        Infopanel.fadeIn(1);
    }    
}

