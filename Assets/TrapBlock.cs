using Photon.Pun;
using UnityEngine;

public class TrapBlock : MonoBehaviour
{
    public float knockbackForce = 5f; // 밀려나는 힘

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Player와 충돌했을 때
        if (collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Player2"))
        {
            // Rigidbody2D 가져오기
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // 충돌 방향 계산 (TrapBlock 중심 기준 반대 방향)
                Vector2 collisionNormal = collision.contacts[0].normal;
                Vector2 knockbackDirection = -collisionNormal.normalized;

                // 힘 가하기 (자신의 PhotonView만 조작)
                PhotonView photonView = collision.gameObject.GetComponent<PhotonView>();
                if (photonView != null && photonView.IsMine)
                {
                    rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
                }

                // 데미지 적용
                if (photonView != null)
                {
                    photonView.RPC("TakeDamageRPC", RpcTarget.All, 10); // 데미지 값은 10
                }
            }
        }
    }
}
