using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPerson : MonoBehaviour
{
    public void Teleport(Vector3 worldDest)
    {
        Vector3 rigPos = transform.parent.position;
        Vector3 offsetInRig = rigPos - transform.position;

        transform.parent.position = worldDest + offsetInRig;
    }
}
