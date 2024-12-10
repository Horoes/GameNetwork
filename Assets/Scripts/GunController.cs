using Photon.Pun;
using UnityEngine;

public class GunController : MonoBehaviourPun, IPunObservable
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float bulletSize = 0.1f; // 초기 크기
    private bool triple = false;

    void Update()
    {
        if (photonView.IsMine)
        {
            // K 키: 총알 크기 증가
            if (Input.GetKeyDown(KeyCode.K))
            {
                bulletSize *= 1.3f; // 30% 증가
                photonView.RPC("UpdateBulletSize", RpcTarget.All, bulletSize);
            }

            // L 키: 총알 속도 증가
            if (Input.GetKeyDown(KeyCode.L))
            {
                bulletSpeed *= 2f; // 속도 2배 증가
                photonView.RPC("UpdateBulletSpeed", RpcTarget.All, bulletSpeed);
            }

            // 마우스 클릭으로 총 발사
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 velocity = firePoint.up * bulletSpeed;
                if (triple)
                {
                    Vector3 velocity1 = firePoint.up * bulletSpeed; // 기본 방향 총알
                    Vector3 velocity2 = Quaternion.Euler(0, 0, 30) * firePoint.up * bulletSpeed; // 30도 낮은 각도
                    Vector3 velocity3 = Quaternion.Euler(0, 0, 15) * firePoint.up * bulletSpeed; // 30도 낮은 각도
                    photonView.RPC("Fire", RpcTarget.All, firePoint.position, firePoint.rotation, velocity1, bulletSize); // 기본 방향 발사
                    photonView.RPC("Fire", RpcTarget.All, firePoint.position, Quaternion.Euler(0, 0, -30) * firePoint.rotation, velocity2, bulletSize); // 30도 낮은 방향 발사
                    photonView.RPC("Fire", RpcTarget.All, firePoint.position, Quaternion.Euler(0, 0, -30) * firePoint.rotation, velocity3, bulletSize);
                }
                else
                {
                    photonView.RPC("Fire", RpcTarget.All, firePoint.position, firePoint.rotation, velocity, bulletSize);
                }
            }

            // J 키: 30도 낮은 각도로 추가 발사
            if (Input.GetKeyDown(KeyCode.J))
            {
                Debug.Log("a");
                Vector3 velocity1 = firePoint.up * bulletSpeed; // 기본 방향 총알
                triple = true;

            }
        }
    }

    [PunRPC]
    void Fire(Vector3 position, Quaternion rotation, Vector3 velocity, float size)
    {
        if (photonView.IsMine)
        {
            GameObject bullet = PhotonNetwork.Instantiate(bulletPrefab.name, position, rotation);
            bullet.GetComponent<PhotonView>().RPC("InitializeBullet", RpcTarget.All, velocity, size);
        }
    }

    [PunRPC]
    void UpdateBulletSize(float size)
    {
        bulletSize = size;
    }

    [PunRPC]
    void UpdateBulletSpeed(float speed)
    {
        bulletSpeed = speed;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}