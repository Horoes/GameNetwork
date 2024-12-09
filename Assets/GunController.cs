using Photon.Pun;
using UnityEngine;

public class GunController : MonoBehaviourPun, IPunObservable
{
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // ������ ����: ���� ���� ��ġ�� ȸ���� ����
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            // ������ ����: ���۹��� ��ġ�� ȸ������ ���� ���¸� ������Ʈ
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
