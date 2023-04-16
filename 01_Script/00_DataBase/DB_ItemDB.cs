using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using Firebase.Storage;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class DB_ItemDB : MonoBehaviour
{
    FirebaseStorage storage;
    public DatabaseReference m_DB;
    private UI_ItemInfo auction_ui;

    [SerializeField] SO_PlayerData playerData;     
    
    public ItemData<Serialization<Cubes>> tempItem; //������ ���̵�� �����ͺ��̽����� ������ ������ �� Ȱ���ϴ� ������ �ӽ� �����
    List<string> items = new List<string>(); //������ ���� ������ ���̵� ����Ʈ

    int ItemCount = 0; //���� ��ü ������ ����
    private string i_id = ""; //������ ���̵� (������ �����ͺ��̽� ����� ���)

    public class ItemData<T>
    {
        public T Item;
        public string Seller_ID = ""; //�Ǹ��� ���̵�
        public string Seller_Name = ""; //�Ǹ��� �г���
        public string Item_ID = ""; //������ ID (8�ڸ�)
        public string Item_Name = ""; //������ �̸� (10����)
        public int Price = 0; //����
        public bool isPublic = false; //���� ���� ����

        public ItemData(T _item, string _sID, string _sName, string _iID, string _iName,
            int _price, bool _isPublic)
        {
            Item = _item;
            Seller_ID = _sID;
            Seller_Name = _sName;
            Item_ID = _iID;
            Item_Name = _iName;
            Price = _price;
            isPublic = _isPublic;
        }
    };

    void Start()
    {
        FirebaseApp.DefaultInstance.Options.DatabaseUrl = new Uri("https://arvr-project-a1e02-default-rtdb.firebaseio.com/");
        m_DB = FirebaseDatabase.DefaultInstance.RootReference;

        storage = FirebaseStorage.DefaultInstance;

        StartCoroutine(LoadData());
    }
    public IEnumerator initData() //�Խ��� �ʱⵥ���� (�ڿ�������)
    {
        int num = 0;
        auction_ui = GameObject.Find("Canvas").GetComponent<UI_ItemInfo>();

        for (int i = 0; i < 10; i++)
            auction_ui.Icon[i].SetActive(false);
        auction_ui.HomeLoad(0);

        RecordsCount(1);

        yield return new WaitForSeconds(1f);

        int _page = auction_ui.Home_Page * 10;

        for (int i = _page; i < _page + 10; i++)
        {
            if (ItemCount - i == 0)
                break;

            SearchData((ItemCount - (i + 1)).ToString().PadLeft(8, '0'));
            yield return new WaitForSeconds(0.33f);

            if (tempItem.isPublic)
            {
                auction_ui.Icon[num].SetActive(true);
                auction_ui.Icon[num].GetComponent<DB_Button>().initIcon(tempItem.Item_ID, tempItem.Item_Name, tempItem.Price);
                num++;
            }            
        }

        yield return new WaitForSeconds(0.33f);
        auction_ui.HomeLoad(0, num);
    }
    public IEnumerator saveData(string _Itemname, Texture2D _texture) //������ ������ �����ͺ��̽� ���� (������ �̸��̶� �ؽ��� �־������)
    {
        yield return new WaitForSeconds(0.33f);
        RecordsCount();
        yield return new WaitForSeconds(0.33f);

        string path = Path.Combine(Application.persistentDataPath, _Itemname + ".json");
        string jsonData = File.ReadAllText(path);
        Serialization<Cubes> itemData = JsonUtility.FromJson<Serialization<Cubes>>(jsonData);

        ItemData<Serialization<Cubes>> data
                    = new ItemData<Serialization<Cubes>>
                    (itemData, playerData.user_id, playerData.user_name, i_id, _Itemname, 0, false);
        string json = JsonUtility.ToJson(data);
        m_DB.Child("Item").Child(i_id).SetRawJsonValueAsync(json);
        StartCoroutine(SaveImage(i_id, _texture));

        items.Add(i_id);
        yield return new WaitForSeconds(0.33f);
        m_DB.Child("users").Child(playerData.user_id).Child("ItemsID").SetValueAsync(items);
    } 
    public IEnumerator insertData(string _Itemname, string _ItemID, int _price) //������ �Է� (���� ���ε�)(�ڿ�������)
    {
        yield return new WaitForSeconds(0.33f);
        RecordsCount();
        SearchData(_ItemID);
        yield return new WaitForSeconds(0.33f);

        ItemData<Serialization<Cubes>> data
                    = new ItemData<Serialization<Cubes>>
                    (tempItem.Item, playerData.user_id, playerData.user_name, _ItemID, _Itemname, _price, true);
        string json = JsonUtility.ToJson(data);

        m_DB.Child("Item").Child(_ItemID).SetRawJsonValueAsync(json);

        StartCoroutine(auction_ui.Toast("���� ������Ʈ�� �Ϸ�Ǿ����ϴ�."));
    }
    public IEnumerator LoadData() //���� ������ ������ ��������
    {
        m_DB.Child("users").Child(playerData.user_id).RunTransaction(multableData =>
        {
            List<object> tempList = multableData.Child("ItemsID").Value as List<object>;

            if (tempList == null)
            {
                Debug.Log("������ ����");
            }
            else
            {
                for(int i = 0; i < tempList.Count; i++)
                {
                    items.Add(tempList[i].ToString());
                }
            }
            return TransactionResult.Success(multableData);
        });

        yield return new WaitForSeconds(0.33f);
    }
    public IEnumerator LoadItem() //(������ ���ε带 ����) ���� ������ ������ ���� (�ڿ�������)
    {
        auction_ui.HomeLoad(1);

        int _page = auction_ui.My_Page * 30;

        int _count = items.Count < 30 ? items.Count : 30;        

        for (int i = 0; i < _count; i++)
        {
            if(items[i + _page] != null)
            {
                SearchData(items[i + _page]);
                auction_ui.MyIcon[i].SetActive(true);
                yield return new WaitForSeconds(0.33f);
                auction_ui.MyIcon[i].GetComponent<DB_Button>().initIcon(items[i + _page], tempItem.Item_Name);
            }
            else
            {
                break;
            }
        }

        yield return new WaitForSeconds(0.33f);
        auction_ui.HomeLoad(1, _count);
    }
    public IEnumerator BuyItem(string _Itemname, int _itemPrice) //������ ���� (�ڿ�������)
    {
        SearchData(_Itemname);
        yield return new WaitForSeconds(0.33f);

        if (items != null)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == _Itemname)
                {
                    StartCoroutine(auction_ui.Toast("�̹� �������ִ� �������Դϴ�."));
                    yield break;
                }
            }
        }

        m_DB.Child("users").Child(playerData.user_id).Child("user_price").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("userDataNull");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                var _sn = snapshot.Value;
                int _UserPrice = int.Parse(_sn.ToString());

                if (_itemPrice <= _UserPrice)
                {
                    m_DB.Child("users").Child(playerData.user_id).Child("user_price").SetValueAsync(_UserPrice - _itemPrice);

                    StartCoroutine(SellItem(_Itemname, _itemPrice));
                    m_DB.Child("users").Child(playerData.user_id).Child("ItemsID").SetValueAsync(_Itemname);

                    StartCoroutine(auction_ui.Toast("���Ű� �Ϸ�Ǿ����ϴ�."));
                }
                else
                {
                    StartCoroutine(auction_ui.Toast("���� �����մϴ�."));
                }                
            }
        });
    }
    public IEnumerator SellItem(string _Itemname, int _itemPrice) //�Ǹ��ڿ��� �� ����
    {
        m_DB.Child("users").Child(tempItem.Seller_ID).Child("user_price").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("SellerDataNull");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                var _sn = snapshot.Value;
                int _UserPrice = int.Parse(_sn.ToString());

                m_DB.Child("users").Child(tempItem.Seller_ID).Child("user_price").SetValueAsync(_UserPrice + _itemPrice);
            }
        });
        yield return new WaitForSeconds(0.33f);
    }
    public IEnumerator SaveImage(string _ItemID, Texture2D _texture)//�׸� ���� (������ ���̵�� �־����)
    {
        SearchData(_ItemID);
        yield return new WaitForSeconds(0.33f);

        StorageReference storage_ref = storage.GetReferenceFromUrl("gs://arvr-project-a1e02.appspot.com");

        byte[] bytes = _texture.EncodeToPNG();
        StorageReference uploadRef = storage_ref.Child("ItemImages/" + _ItemID + ".png");

        uploadRef.PutBytesAsync(bytes).ContinueWithOnMainThread((task)=>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log(task.Exception.ToString());
                Debug.Log("���� ����");
            }
            else
            {
                Debug.Log("File Uploaded Successfully!");
                Debug.Log("���� �Ϸ�");
            }
        });
    }
    void SearchData(string _itemName) //������ ���̵� ������ �˻� �� �ӽ� �����Ϳ� ����
    {
        FirebaseDatabase.DefaultInstance.GetReference("Item").Child(_itemName).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("ItemDataNull");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                string json = task.Result.GetRawJsonValue();
                tempItem = JsonUtility.FromJson<ItemData<Serialization<Cubes>>>(json);
            }
        });
    }
    void RecordsCount(int type = 0) //������ ������ ���� ���� ���̵� ����� (�ƹ��͵� �� ������ ���̵� ����� ���� ���� ������ ���� ���� ��ȯ)
    {
        FirebaseDatabase.DefaultInstance.GetReference("Item").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("error!");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (type == 0)
                {
                    i_id = ((int)snapshot.ChildrenCount).ToString();
                    i_id = i_id.PadLeft(8, '0');
                }
                else
                {
                    ItemCount = (int)snapshot.ChildrenCount;
                }
            }
        });
    }
    public IEnumerator Page(int _type, int _page)
    {
        if(_type == 0)
        {
            RecordsCount(1);
            yield return new WaitForSeconds(0.33f);
            auction_ui.Page(0, _page, ItemCount);

        }
        else
        {
            auction_ui.Page(1, _page, items.Count);
        }
    }
}