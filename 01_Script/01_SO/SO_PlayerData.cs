using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_PlayerData", menuName = "SO/SO_PlayerData", order = 0)]
public class SO_PlayerData : ScriptableObject
{
    public string user_id;
    public string user_name;

    void Awake()
    {
        if (PlayerPrefs.HasKey("UserID") && PlayerPrefs.HasKey("UserName"))
        {
            user_id = PlayerPrefs.GetString("UserID");
            user_name = PlayerPrefs.GetString("UserName");
        }
    }

}
