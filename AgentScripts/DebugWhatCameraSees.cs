using UnityEngine;

public static class DebugWhatCameraSees
{
    public static string Run()
    {
        Camera c = Camera.main;
        if (c == null) return "No camera";

        string res = $"CamPos={c.transform.position} Fwd={c.transform.forward} | ";
        if (Physics.Raycast(c.transform.position, c.transform.forward, out RaycastHit hit, 50f))
        {
            res += $"RayHit={hit.collider.name} (dist={hit.distance:0.2f}, pos={hit.point}, parent={(hit.transform.parent ? hit.transform.parent.name : "root")}) | ";
        }
        else
        {
            res += "RayHit=NONE | ";
        }

        Collider[] near = Physics.OverlapSphere(c.transform.position, 2.5f);
        res += $"NearCount={near.Length}: ";
        for (int i = 0; i < Mathf.Min(near.Length, 6); i++)
        {
            res += near[i].name + ", ";
        }
        return res;
    }
}
