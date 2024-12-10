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
        if (collision.CompareTag("Players1"))
        {
            // �浹�� ��ü�� spawnPoint1 ��ġ�� �̵�
            collision.transform.position = spawnPoint1.position;
        }

        // Players2 �±׸� ���� ��ü�� �浹���� ��
        if (collision.CompareTag("Players2"))
        {
            // �浹�� ��ü�� spawnPoint2 ��ġ�� �̵�
            collision.transform.position = spawnPoint2.position;
        }
    }
}
