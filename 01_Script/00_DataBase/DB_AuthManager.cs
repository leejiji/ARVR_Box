using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Unity;
using UnityEngine.SceneManagement;

public class DB_AuthManager : MonoBehaviour
{
    [SerializeField] InputField Login_ID_Field;
    [SerializeField] InputField Login_Pass_Field;
    [SerializeField] InputField emailField;
    [SerializeField] InputField passField;
    [SerializeField] InputField passCheckField;
    [SerializeField] InputField nameField;

    [SerializeField] SO_PlayerData playerData;
    [SerializeField] GameObject toast;
    [SerializeField] Text toastMessage;

    // ������ ������ ��ü
    FirebaseAuth auth;
    DatabaseReference reference; // �����͸� ���� ���� reference ����

    public class User // ����� Ŭ���� ����
    {
        public string user_id;
        public string user_name;
        public int user_price;
        public List<string> ItemsID;
        public User(string _id, string _name)
        {
            this.user_id = _id;
            this.user_name = _name;
            this.user_price = 0;
            this.ItemsID = null;
        }
    }

    void Awake()
    {
        // ��ü �ʱ�ȭ
        auth = FirebaseAuth.DefaultInstance;
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        
        if (PlayerPrefs.HasKey("UserID") == false || PlayerPrefs.HasKey("UserName") == false)
        {
            StartCoroutine(Toast("�ڵ��α��� ����"));
        }
        else
        {
            StartCoroutine(Toast("�ڵ��α��� ����!"));
            //�ھ� �̵�
        }
    }

    public void login_btn() //�α��� ��ư
    {
        StartCoroutine(login());
    }
    public IEnumerator login() //�α���
    {
        string uid = null;
        string uName = null;
        bool isSuccess = false;
        auth.SignInWithEmailAndPasswordAsync(Login_ID_Field.text, Login_Pass_Field.text).ContinueWith(
            task =>
            {
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                {
                    uid = task.Result.UserId;                  

                    FirebaseDatabase.DefaultInstance.GetReference("users")
                    .Child(uid).GetValueAsync().ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            StartCoroutine(Toast("�α��ο� �����ϼ̽��ϴ�"));
                        }
                        else if (task.IsCompleted)
                        {
                            DataSnapshot snapshot = task.Result;
                            uName = snapshot.Child("user_name").Value.ToString();
                            isSuccess = true;

                            StartCoroutine(Toast(Login_ID_Field.text + "��\n�α����ϼ̽��ϴ�."));
                        }
                    });
                }
                else
                {
                    StartCoroutine(Toast("�α��ο� �����ϼ̽��ϴ�"));
                }
            }
        );
        
        yield return new WaitForSeconds(1f);

        if (isSuccess)
        {
            print(uid);
            print(uName);
            SaveData(uid, uName);
        }        
    }
    public void register() //ȸ������
    {
        auth.CreateUserWithEmailAndPasswordAsync(emailField.text, passField.text).ContinueWith(
            task =>
            {
                if (!task.IsCanceled && !task.IsFaulted
                && passField.text == passCheckField.text && nameField.text.Length <= 8)
                {
                    FirebaseUser newUser = task.Result;
                    writeNewUser(newUser.UserId, nameField.text);

                    StartCoroutine(Toast(emailField.text + "�� ȸ��������\n�Ϸ�Ǿ����ϴ�."));
                }
                else
                {
                    StartCoroutine(Toast("ȸ�����Կ� �����ϼ̽��ϴ�"));
                }
            });
    }
    void writeNewUser(string _id, string _name) // ������ ȸ�� ���� ��ȣ�� ���� ����� �⺻�� ����
    {
        User user = new User(_id, _name);
        string json = JsonUtility.ToJson(user); // ������ ����ڿ� ���� ���� json �������� ����
        reference.Child("users").Child(_id).SetRawJsonValueAsync(json); // �����ͺ��̽��� json ���� ���ε�
    }
    void SaveData(string _id, string _name) //���� ���̵� ����
    {    
        PlayerPrefs.SetString("UserID", _id);        
        PlayerPrefs.SetString("UserName", _name);

        playerData.user_id = PlayerPrefs.GetString("UserID");
        playerData.user_name = PlayerPrefs.GetString("UserName");

        PlayerPrefs.Save();

        print("���� �Ϸ�");

        //�ھ� �̵�
    }
    public IEnumerator Toast(string _text, float _time = 2f) //�佺Ʈ �޼���, �ð� �Է��Ѹ�ŭ ǥ��...
    {
        toastMessage.text = _text;
        toast.SetActive(true);

        yield return new WaitForSeconds(_time);

        toast.SetActive(false);
    }
}