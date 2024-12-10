using Photon.Pun;
using UnityEngine;

public class TrapBlock : MonoBehaviour
{
    public float knockbackForce = 5f; // �з����� ��

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Player�� �浹���� ��
        if (collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Player2"))
        {
            // Rigidbody2D ��������
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // �浹 ���� ��� (TrapBlock �߽� ���� �ݴ� ����)
                Vector2 collisionNormal = collision.contacts[0].normal;
                Vector2 knockbackDirection = -collisionNormal.normalized;

                // �� ���ϱ� (�ڽ��� PhotonView�� ����)
                PhotonView photonView = collision.gameObject.GetComponent<PhotonView>();
                if (photonView != null && photonView.IsMine)
                {
                    rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
                }

                // ������ ����
                if (photonView != null)
                {
                    photonView.RPC("TakeDamageRPC", RpcTarget.All, 10); // ������ ���� 10
                }
            }
        }
    }
}
