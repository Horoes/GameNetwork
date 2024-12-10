using Photon.Pun;
using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviourPun
{
    public float lifetime = 5f;

    [PunRPC]
    public void InitializeBullet(Vector3 velocity, float size)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.velocity = velocity;

        // 크기 초기화
        transform.localScale = Vector3.one * size;
    }

    private void Start()
    {
        StartCoroutine(DestroyAfterTime());
    }

    private IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(0.1f);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb.velocity == Vector2.zero)
        {
            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            yield break;
        }

        yield return new WaitForSeconds(lifetime - 0.1f);
        if (photonView.IsMine || PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.IsMine) return;

        if (collision.CompareTag("Player1")|| collision.CompareTag("Player2"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.photonView.RPC("TakeDamageRPC", player.photonView.Owner, 10);
            }

            if (photonView.IsMine || PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
        else if (collision.CompareTag("Walls"))
        {
            if (photonView.IsMine || PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}
