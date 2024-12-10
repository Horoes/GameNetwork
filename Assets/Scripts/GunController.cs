using Photon.Pun;
using UnityEngine;

public class GunController : MonoBehaviourPun, IPunObservable
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float bulletSize = 0.1f; // �ʱ� ũ��
    private bool triple = false;

    void Update()
    {
        if (photonView.IsMine)
        {
            // K Ű: �Ѿ� ũ�� ����
            if (Input.GetKeyDown(KeyCode.K))
            {
                bulletSize *= 1.3f; // 30% ����
                photonView.RPC("UpdateBulletSize", RpcTarget.All, bulletSize);
            }

            // L Ű: �Ѿ� �ӵ� ����
            if (Input.GetKeyDown(KeyCode.L))
            {
                bulletSpeed *= 2f; // �ӵ� 2�� ����
                photonView.RPC("UpdateBulletSpeed", RpcTarget.All, bulletSpeed);
            }

            // ���콺 Ŭ������ �� �߻�
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 velocity = firePoint.up * bulletSpeed;
                if (triple)
                {
                    Vector3 velocity1 = firePoint.up * bulletSpeed; // �⺻ ���� �Ѿ�
                    Vector3 velocity2 = Quaternion.Euler(0, 0, 30) * firePoint.up * bulletSpeed; // 30�� ���� ����
                    Vector3 velocity3 = Quaternion.Euler(0, 0, 15) * firePoint.up * bulletSpeed; // 30�� ���� ����
                    photonView.RPC("Fire", RpcTarget.All, firePoint.position, firePoint.rotation, velocity1, bulletSize); // �⺻ ���� �߻�
                    photonView.RPC("Fire", RpcTarget.All, firePoint.position, Quaternion.Euler(0, 0, -30) * firePoint.rotation, velocity2, bulletSize); // 30�� ���� ���� �߻�
                    photonView.RPC("Fire", RpcTarget.All, firePoint.position, Quaternion.Euler(0, 0, -30) * firePoint.rotation, velocity3, bulletSize);
                }
                else
                {
                    photonView.RPC("Fire", RpcTarget.All, firePoint.position, firePoint.rotation, velocity, bulletSize);
                }
            }

            // J Ű: 30�� ���� ������ �߰� �߻�
            if (Input.GetKeyDown(KeyCode.J))
            {
                Debug.Log("a");
                Vector3 velocity1 = firePoint.up * bulletSpeed; // �⺻ ���� �Ѿ�
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