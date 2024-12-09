using UnityEngine;
using Photon.Pun;

public class OrbitAroundPlayer : MonoBehaviourPun, IPunObservable
{
    public Transform orbitingObject;
    public float distanceFromPlayer = 5.0f;

    void Update()
    {
        if (photonView.IsMine)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
            mousePosition.z = 0;

            Vector3 direction = (mousePosition - transform.position).normalized;
            orbitingObject.position = transform.position + direction * distanceFromPlayer;
            orbitingObject.rotation = Quaternion.LookRotation(Vector3.forward, mousePosition - orbitingObject.position);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 데이터 전송
            stream.SendNext(orbitingObject.position);
            stream.SendNext(orbitingObject.rotation);
        }
        else
        {
            // 데이터 수신
            orbitingObject.position = (Vector3)stream.ReceiveNext();
            orbitingObject.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
