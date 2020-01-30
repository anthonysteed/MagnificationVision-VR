using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PlayerLogger : MonoBehaviour
{
    private BufferedLogger _log = new BufferedLogger("Player");

    private void Update()
    {
        _log.Append("LocalHeadPos", transform.localPosition);
        _log.Append("GlobalHeadPos", transform.position);
        _log.Append("HeadRot", transform.rotation.eulerAngles);
        _log.Append("PlayspacePos", transform.parent.position);


        _log.CommitLine();
    }




}
