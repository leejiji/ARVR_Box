using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemInfo : MonoBehaviour
{
    [SerializeField] private GameObject LoadPanel;

    [SerializeField] private GameObject[] ItemInfo;
    [SerializeField] private GameObject ToastMessage;
    [SerializeField] private Text ToastText;

    public GameObject[] Icon;
    public GameObject[] MyIcon;

    [SerializeField] private DB_ItemDB db_Item;

    public Image u_Image;
    public Text u_nameText;
    public string u_ItemID;

    public InputField priceField;

    public int Home_Page = 0;
    public int My_Page = 0;

    private void Start()
    {
        StartCoroutine(db_Item.initData());
    }
    public void fadeOut(int _num)
    {
        ItemInfo[_num].SetActive(false);
    }
    public void fadeIn(int _num)
    {
        ItemInfo[_num].SetActive(true);

        if(_num == 0)
            StartCoroutine(db_Item.LoadItem());
    }
    public void ItemLoad() //마이페이지 클릭
    {
        ItemInfo[1].SetActive(true);
        StartCoroutine(db_Item.LoadItem());
    }
    public void UpdateBtn() //업로드 버튼 클릭
    {
        fadeOut(1);
        StartCoroutine(db_Item.insertData(u_nameText.text, u_ItemID, int.Parse(priceField.text)));
    }
    public void HomeBtn() //홈 버튼 클릭
    {
        fadeOut(0);
        fadeOut(1);

        StartCoroutine(db_Item.initData());
    }
    public void Page(int _type, int _page, int _count)//페이지 이동
    {
        switch (_type)
        {
            case 0:
                if(Home_Page + _page > -1)
                {
                    if (_count - ((Home_Page + _page) * 10) > 0)
                    {
                        Home_Page += _page;
                        StartCoroutine(db_Item.initData());
                    }
                    else
                        StartCoroutine(Toast("마지막 페이지입니다."));
                }
                else
                {
                    StartCoroutine(Toast("첫번째 페이지입니다."));
                }
                break;

            case 1:
                if (My_Page + _page > -1)
                {
                    if (_count - ((My_Page + _page) * 10) > 0)
                    {
                        My_Page += _page;
                        StartCoroutine(db_Item.LoadItem());
                    }
                    else
                        StartCoroutine(Toast("마지막 페이지입니다."));
                }
                else
                {
                    StartCoroutine(Toast("첫번째 페이지입니다."));
                }
                break;
        }
    }
    public void HomeLoad(int _type, int _count = -1) //홈 로드 (타입에 0 입력하면 홈 로드, 1 입력하면 마이페이지 로드)
    {    
        if(_count == -1)
        {
            LoadPanel.SetActive(true);
        }
        else
        {
            StartCoroutine(LoadCheck(_type, _count));
        }
    }
    IEnumerator LoadCheck(int _type, int _count)
    {
        int LoadCount = 0;

        while (LoadCount < _count)
        {
            yield return new WaitForSeconds(0.33f);

            if(_type == 0)
            {
                if (Icon[LoadCount].GetComponent<DB_Button>().Success == true)
                    LoadCount++;
            }
            else
            {
                if (MyIcon[LoadCount].GetComponent<DB_Button>().Success == true)
                    LoadCount++;
            }
        }

        print("로딩완료");
        LoadPanel.SetActive(false);
    }
    public IEnumerator Toast(string _text, float _time = 2f) //토스트 메세지, 시간 입력한만큼 표시...
    {
        ToastText.text = _text;
        ToastMessage.SetActive(true);

        yield return new WaitForSeconds(_time);

        ToastMessage.SetActive(false);
    }
}
