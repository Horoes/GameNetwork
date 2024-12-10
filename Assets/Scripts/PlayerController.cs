using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using ExitGames.Client.Photon;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    private int maxHp = 100;
    public int currentHp;
    private Transform healthBarTransform;

    private Slider healthBarSlider;


    private float speed = 5f;        // 이동 속도
    private float jumpForce = 13f;    // 점프 힘
    private Rigidbody2D rb;
    private Vector2 input;
    private Vector2 networkPosition; // 네트워크에서 동기화된 위치
    private Transform tr;
    private PhotonView pv;
    private bool isGrounded = false; // 바닥 여부
    public GameObject gun; // Inspector에서 설정
    public float gunDistance = 1.5f; // 총과 플레이어 사이의 거리

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        tr = GetComponent<Transform>();
        pv = GetComponent<PhotonView>();

        // PhotonView의 ObservedComponents 설정
        if (!pv.ObservedComponents.Contains(this))
        {
            pv.ObservedComponents.Add(this);
        }

        currentHp = maxHp;
    }

    private void Start()
    {
        // 초기 속도 설정
        if (pv.IsMine)
        {
            rb.velocity = Vector2.zero;
        }

        InitializeHealthBar();

    }

    private void Update()
    {
        UpdateHealthBarPosition();
        if (pv.IsMine)
        {
            HandleInput();

        }
        if (photonView.IsMine)
        {
            PositionGun();
            if (Input.GetKeyDown(KeyCode.P))
            {
                float newSpeed = speed * 2f; // 이동 속도 2배 증가
                photonView.RPC("UpdateSpeed", RpcTarget.All, newSpeed);
            }
            if (Input.GetKeyDown(KeyCode.M)) // M 키로 테스트
            {
                UpdateMaxHealth(200); // 자신만 최대 체력을 200으로 변경
            }
        }
    }
    private void InitializeHealthBar()
    {
        // Canvas에 체력바 생성
        GameObject canvas = GameObject.Find("Canvas"); // Canvas를 찾음
        GameObject healthBarPrefab = Resources.Load<GameObject>("HealthBarPrefab");
        GameObject healthBarInstance = Instantiate(healthBarPrefab, canvas.transform);

        // Slider 컴포넌트 연결
        healthBarSlider = healthBarInstance.GetComponent<Slider>();
        healthBarTransform = healthBarInstance.transform;

        // 초기 체력바 설정
        healthBarSlider.maxValue = maxHp;
        healthBarSlider.value = currentHp;

        // Fill 색상 설정 (빨간색)
        Image fillImage = healthBarInstance.transform.Find("Fill Area/Fill").GetComponent<Image>();
        fillImage.color = Color.red;

        // Background 색상 설정 (회색)
        Image backgroundImage = healthBarInstance.transform.Find("Background").GetComponent<Image>();
        backgroundImage.color = Color.gray;
    }

    private void UpdateHealthBarPosition()
    {
        if (healthBarTransform != null)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 1.0f, 0));
            healthBarTransform.position = screenPosition;
        }
    }

    private void HandleInput()
    {
        // 수평 이동
        input.x = Input.GetAxis("Horizontal");

        // 점프 입력
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        // HP가 0 이하일 때 라운드 종료
        if (currentHp <= 0)
        {
            PhotonInit.instance.EndRound(GetPlayerIndex());
        }
    }

    private void FixedUpdate()
    {
        if (pv.IsMine)
        {
            // 로컬 플레이어의 움직임 처리
            rb.velocity = new Vector2(input.x * speed, rb.velocity.y);
        }
        else
        {
            // 네트워크로 동기화된 위치로 부드럽게 이동
            if (networkPosition != Vector2.zero)
            {
                rb.position = Vector2.Lerp(rb.position, networkPosition, Time.fixedDeltaTime * 10f);
            }
        }
    }
    [PunRPC]
    public void UpdateSpeed(float newSpeed)
    {
        speed = newSpeed;

        Debug.Log($"[UpdateSpeed] Player speed updated to: {speed}");
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce); // 점프 처리
        isGrounded = false; // 점프 상태로 전환
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 데이터 전송
            stream.SendNext(rb.position);  // Vector2
            stream.SendNext(rb.velocity); // Vector2
            stream.SendNext(currentHp);   // int
            stream.SendNext(speed);       // float
        }
        else
        {
            // 데이터 수신
            networkPosition = (Vector2)stream.ReceiveNext();
            rb.velocity = (Vector2)stream.ReceiveNext();

            // 체력 값과 속도는 순서에 따라 수신
            if (!photonView.IsMine)
            {
                currentHp = (int)stream.ReceiveNext(); // int로 체력 수신
            }
            else
            {
                stream.ReceiveNext(); // currentHp를 스킵 (자신의 체력은 업데이트하지 않음)
            }

            speed = (float)stream.ReceiveNext(); // float로 속도 수신

            // 체력 UI 업데이트
            if (!photonView.IsMine && healthBarSlider != null)
            {
                healthBarSlider.value = currentHp;
                Debug.Log($"[OnPhotonSerializeView] Health bar slider updated to: {healthBarSlider.value}");
            }
        }
    }



    [PunRPC]
    public void TakeDamageRPC(int damage)
    {
        if (photonView.IsMine) // 로컬 소유자인 경우
        {
            currentHp -= damage;
            if (currentHp < 0) currentHp = 0;

            Debug.Log($"[TakeDamageRPC] Local player HP updated: {currentHp}");

            // UI 업데이트
            if (healthBarSlider != null)
            {
                healthBarSlider.value = currentHp;
            }

            // 체력 동기화
            photonView.RPC("UpdateHealth", RpcTarget.Others, currentHp);
        }
    }
    [PunRPC]
    public void SetMaxHealth(int newMaxHp)
    {
        // 현재 체력을 비율로 계산
        float healthPercentage = (float)currentHp / maxHp;

        // 새로운 최대 체력 설정
        maxHp = newMaxHp;

        // 현재 체력을 새로운 최대 체력 비율에 맞게 조정
        currentHp = Mathf.RoundToInt(healthPercentage * maxHp);

        // Slider 값 업데이트
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHp;
            healthBarSlider.value = currentHp;
        }

        Debug.Log($"[SetMaxHealth] Max HP updated to: {maxHp}, Current HP updated to: {currentHp}");
    }

    [PunRPC]
    void UpdateHealth(int newHp)
    {
        currentHp = newHp;

        Debug.Log($"[UpdateHealth] HP updated to: {currentHp}");

        // UI 업데이트
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHp;
            Debug.Log($"[UpdateHealth] Health bar slider updated to: {healthBarSlider.value}");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Walls"))
        {
            isGrounded = true;
        }
    }

    private int GetPlayerIndex()
    {
        return PhotonNetwork.IsMasterClient ? 0 : 1; // 마스터 클라이언트는 0, 다른 클라이언트는 1
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // 현재 상태를 새로 들어온 플레이어에게 전송
            object[] data = { rb.position, rb.velocity, currentHp };
            PhotonNetwork.RaiseEvent(0, data, new RaiseEventOptions { TargetActors = new int[] { newPlayer.ActorNumber } }, SendOptions.SendReliable);
        }
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEventReceived;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEventReceived;
    }

    private void OnEventReceived(ExitGames.Client.Photon.EventData photonEvent)
    {
        if (photonEvent.Code == 0) // 초기화 이벤트 코드
        {
            object[] data = (object[])photonEvent.CustomData;
            networkPosition = (Vector2)data[0];
            rb.velocity = (Vector2)data[1];
            currentHp = (int)data[2];
        }
    }
    void PositionGun()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.transform.position.z - transform.position.z));

        Vector3 direction = mousePosition - transform.position;
        direction.z = 0; // 2D 게임의 경우 z축 방향을 0으로 설정

        gun.transform.position = transform.position + direction.normalized * gunDistance;
        gun.transform.up = direction; // 총이 마우스 위치를 바라보게 함
    }

    private void OnDestroy()
    {
        if (healthBarTransform != null)
        {
            Destroy(healthBarTransform.gameObject); // 체력바 제거
        }
    }
    public void UpdateMaxHealth(int newMaxHp)
    {
        // 자신의 클라이언트에서만 최대 체력 변경
        if (photonView.IsMine)
        {
            photonView.RPC("SetMaxHealth", photonView.Owner, newMaxHp);
        }
    }

}
