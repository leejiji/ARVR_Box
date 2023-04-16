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

    // 인증을 관리할 객체
    FirebaseAuth auth;
    DatabaseReference reference; // 데이터를 쓰기 위한 reference 변수

    public class User // 사용자 클래스 생성
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
        // 객체 초기화
        auth = FirebaseAuth.DefaultInstance;
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        
        if (PlayerPrefs.HasKey("UserID") == false || PlayerPrefs.HasKey("UserName") == false)
        {
            StartCoroutine(Toast("자동로그인 실패"));
        }
        else
        {
            StartCoroutine(Toast("자동로그인 성공!"));
            //★씬 이동
        }
    }

    public void login_btn() //로그인 버튼
    {
        StartCoroutine(login());
    }
    public IEnumerator login() //로그인
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
                            StartCoroutine(Toast("로그인에 실패하셨습니다"));
                        }
                        else if (task.IsCompleted)
                        {
                            DataSnapshot snapshot = task.Result;
                            uName = snapshot.Child("user_name").Value.ToString();
                            isSuccess = true;

                            StartCoroutine(Toast(Login_ID_Field.text + "로\n로그인하셨습니다."));
                        }
                    });
                }
                else
                {
                    StartCoroutine(Toast("로그인에 실패하셨습니다"));
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
    public void register() //회원가입
    {
        auth.CreateUserWithEmailAndPasswordAsync(emailField.text, passField.text).ContinueWith(
            task =>
            {
                if (!task.IsCanceled && !task.IsFaulted
                && passField.text == passCheckField.text && nameField.text.Length <= 8)
                {
                    FirebaseUser newUser = task.Result;
                    writeNewUser(newUser.UserId, nameField.text);

                    StartCoroutine(Toast(emailField.text + "로 회원가입이\n완료되었습니다."));
                }
                else
                {
                    StartCoroutine(Toast("회원가입에 실패하셨습니다"));
                }
            });
    }
    void writeNewUser(string _id, string _name) // 가입한 회원 고유 번호에 대한 사용자 기본값 설정
    {
        User user = new User(_id, _name);
        string json = JsonUtility.ToJson(user); // 생성한 사용자에 대한 정보 json 형식으로 저장
        reference.Child("users").Child(_id).SetRawJsonValueAsync(json); // 데이터베이스에 json 파일 업로드
    }
    void SaveData(string _id, string _name) //유저 아이디 저장
    {    
        PlayerPrefs.SetString("UserID", _id);        
        PlayerPrefs.SetString("UserName", _name);

        playerData.user_id = PlayerPrefs.GetString("UserID");
        playerData.user_name = PlayerPrefs.GetString("UserName");

        PlayerPrefs.Save();

        print("저장 완료");

        //★씬 이동
    }
    public IEnumerator Toast(string _text, float _time = 2f) //토스트 메세지, 시간 입력한만큼 표시...
    {
        toastMessage.text = _text;
        toast.SetActive(true);

        yield return new WaitForSeconds(_time);

        toast.SetActive(false);
    }
}