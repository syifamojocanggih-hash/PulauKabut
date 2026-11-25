using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

public static class DebugCameraPlayMode
{
    public static string Run()
    {
        GameObject p = GameObject.Find("PF_Player");
        Camera c = Camera.main;
        CinemachineBrain b = c != null ? c.GetComponent<CinemachineBrain>() : null;
        string activeCam = (b != null && b.ActiveVirtualCamera != null) ? b.ActiveVirtualCamera.Name : "none";

        string camTargetInfo = "none";
        if (p != null)
        {
            Transform ct = p.transform.Find("CameraTarget");
            if (ct != null) camTargetInfo = $"pos={ct.position} rot={ct.eulerAngles}";
        }

        string tpInfo = "none";
        GameObject tp = GameObject.Find("CM_POV1_ThirdPerson");
        if (tp != null)
        {
            var follow = tp.GetComponent<CinemachineThirdPersonFollow>();
            var deoc = tp.GetComponent<CinemachineDeoccluder>();
            tpInfo = $"pos={tp.transform.position} dist={(follow ? follow.CameraDistance : -1)} hasDeoc={(deoc != null)}";
        }

        return $"Playing={EditorApplication.isPlaying} | Player={(p ? p.transform.position.ToString() : "null")} | CamTarget=[{camTargetInfo}] | MainCam={(c ? c.transform.position.ToString() : "null")} rot={(c ? c.transform.eulerAngles.ToString() : "null")} | ActiveCM={activeCam} | TP=[{tpInfo}]";
    }
}
