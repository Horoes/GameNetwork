using UnityEngine;

public class CreateRoom : MonoBehaviour
{
    public GameObject MakeRoomPanel;

    public GameObject Image;


    public void CreateRoomBtnOnClick()
    {
        MakeRoomPanel.SetActive(true);
    }

    public void OKBtnOnClick()
    {
        MakeRoomPanel.SetActive(false);
    }

 
}
