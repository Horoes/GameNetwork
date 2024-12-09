using Photon.Pun;
using UnityEngine;

public class GunController : MonoBehaviourPun, IPunObservable
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;

    void Update()
    {
        if (photonView.IsMine && Input.GetMouseButtonDown(0))
        {
            Vector3 velocity = firePoint.up * bulletSpeed;
            photonView.RPC("Fire", RpcTarget.All, firePoint.position, firePoint.rotation, velocity);
        }
    }

    [PunRPC]
    void Fire(Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        if (photonView.IsMine)
        {
            GameObject bullet = PhotonNetwork.Instantiate(bulletPrefab.name, position, rotation);
            bullet.GetComponent<PhotonView>().RPC("InitializeBullet", RpcTarget.All, velocity);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 데이터 전송: 총의 현재 위치와 회전을 전송
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            // 데이터 수신: 전송받은 위치와 회전으로 총의 상태를 업데이트
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
