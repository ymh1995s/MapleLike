using Google.Protobuf.Protocol;
using UnityEngine;

public class StartSceneHelper : MonoBehaviour
{
    public GameObject panel;
    void OnEnable()
    {
        PacketHandler.OnActivateStartScenePanel += ActivatePanel;
    }

    void OnDisable()
    {
        PacketHandler.OnActivateStartScenePanel -= ActivatePanel;
    }

    void ActivatePanel()
    {
        panel.SetActive(true);
    }

    public void OnClickClassChoiceBtn(int param=0)
    {
        if (param == 0) return;
        C_ClassChoice pkt = new C_ClassChoice();
        pkt.ClassType = (ClassType)param;
        NetworkManager.Instance.Send(pkt);
        
    }
}
