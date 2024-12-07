using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCtrl1 : MonoBehaviourPunCallbacks, IPunObservable
{

    private int maxHp = 100;
    private int currentHp;
    private float speed = 10f;         // 이동 속도
    private float jumpforce = 13f;   // 점프 힘
    private Rigidbody2D rb;
    private Vector2 input;
    private Vector2 networkPosition;
    private bool isGrounded = false; // 바닥 여부


    private void Awake()
    {

    }
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        currentHp = maxHp;
    }

    private void Update()
    {
        if (photonView.IsMine) // 로컬 플레이어만 입력 처리
        {
            // 수평 이동
            input.x = Input.GetAxis("Horizontal");

            // 점프 입력
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                Jump();
            }

            // HP가 0이 되면 게임 종료
            if (currentHp <= 0)
            {
                PhotonInit.instance.EndRound(GetPlayerIndex());
            }
        }
    }
    private int GetPlayerIndex()        // 현재 반환하려는 플레이어의 인덱스
    {
        return PhotonNetwork.IsMasterClient ? 0 : 1; // 마스터 클라이언트는 0, 다른 클라이언트는 1
    }

    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            rb.velocity = new Vector2(input.x * speed, rb.velocity.y);

        }
        else
        {
            rb.position = Vector2.Lerp(rb.position, networkPosition, Time.fixedDeltaTime * 10f);
        }

    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpforce); // Y축 속도 설정
        isGrounded = false; // 점프 상태

    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)      // 데이터 동기화, PhotonView로 관리되는 객체 데이터 송수신
    {
        if (stream.IsWriting) // 내가 제어하는 플레이어의 데이터 전송
        {
            stream.SendNext(rb.position);
            stream.SendNext(rb.velocity);
        }
        else // 다른 플레이어 데이터 받음
        {
            networkPosition = (Vector2)stream.ReceiveNext();
            rb.velocity = (Vector2)stream.ReceiveNext();
        }
    }

    public void TakeDamage(int damage)
    {
        if (photonView.IsMine)
        {
            currentHp -= damage;
            if (currentHp < 0)
                currentHp = 0;
            Debug.Log($"HP: {currentHp}");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Walls")
        {
            isGrounded = true;

        }
    }


}
