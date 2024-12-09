using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using ExitGames.Client.Photon;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    private int maxHp = 100;
    private int currentHp;
    private float speed = 10f;        // 이동 속도
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

    }

    private void Update()
    {
        if (pv.IsMine)
        {
            HandleInput();
        }
        if (photonView.IsMine)
        {
            PositionGun();
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

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce); // 점프 처리
        isGrounded = false; // 점프 상태로 전환
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 내가 제어하는 플레이어 데이터 전송
            stream.SendNext(rb.position);
            stream.SendNext(rb.velocity);
        }
        else
        {
            // 다른 플레이어로부터 데이터 수신
            if (stream.Count >= 2) // 데이터가 2개 이상인지 확인
            {
                networkPosition = (Vector2)stream.ReceiveNext();
                rb.velocity = (Vector2)stream.ReceiveNext();
            }
            else
            {
                Debug.LogWarning("Insufficient data received in PhotonStream.");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (pv.IsMine)
        {
            currentHp -= damage;
            if (currentHp < 0)
                currentHp = 0;

            Debug.Log($"HP: {currentHp}");
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
}
