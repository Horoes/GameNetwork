using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockBlock : MonoBehaviour
{
    public Transform spawnPoint1; // Players1�� �����̵� ��ġ
    public Transform spawnPoint2; // Players2�� �����̵� ��ġ

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Players1 �±׸� ���� ��ü�� �浹���� ��
        if (collision.CompareTag("Player1"))
        {
            // �浹�� ��ü�� spawnPoint1 ��ġ�� �̵�
            collision.transform.position = spawnPoint1.position;

            // TakeDamageRPC ȣ��
            PlayerController playerController = collision.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.photonView.RPC("TakeDamageRPC", RpcTarget.All, 10);
            }
        }

        // Players2 �±׸� ���� ��ü�� �浹���� ��
        if (collision.CompareTag("Player2"))
        {
            // �浹�� ��ü�� spawnPoint2 ��ġ�� �̵�
            collision.transform.position = spawnPoint2.position;

            // TakeDamageRPC ȣ��
            PlayerController playerController = collision.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.photonView.RPC("TakeDamageRPC", RpcTarget.All, 10);
            }
        }
    }
}
