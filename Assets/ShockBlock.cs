using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockBlock : MonoBehaviour
{
    public Transform spawnPoint1; // Players1의 순간이동 위치
    public Transform spawnPoint2; // Players2의 순간이동 위치

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Players1 태그를 가진 객체가 충돌했을 때
        if (collision.CompareTag("Players1"))
        {
            // 충돌한 객체를 spawnPoint1 위치로 이동
            collision.transform.position = spawnPoint1.position;
        }

        // Players2 태그를 가진 객체가 충돌했을 때
        if (collision.CompareTag("Players2"))
        {
            // 충돌한 객체를 spawnPoint2 위치로 이동
            collision.transform.position = spawnPoint2.position;
        }
    }
}
