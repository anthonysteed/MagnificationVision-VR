// via https://www.youtube.com/watch?v=-T09oRMDuG8

using System.Collections;
using UnityEngine;
using Valve.VR;

public class Teleporter : MonoBehaviour
{
    public bool IsTeleporting { get; private set; }

    [SerializeField]
    private float _fadeTime = 0.5f;

    private Transform _player;

    private Transform _playSpace;

    private void Awake()
    {
        _player = Camera.main.transform;
        _playSpace = _player.parent;
    }

    public void Teleport(Vector3 target)
    {
        if (IsTeleporting)
        {
            return;
        }
        target.y = _playSpace.position.y;

        Vector3 playerPos = new Vector3(_player.position.x, _playSpace.position.y, _player.position.z);
        StartCoroutine(MovePlaySpace(target - playerPos));
    }

    private IEnumerator MovePlaySpace(Vector3 translation)
    {
        IsTeleporting = true;
        SteamVR_Fade.Start(Color.white, _fadeTime, true);

        yield return new WaitForSeconds(_fadeTime);
        _playSpace.position += translation;

        SteamVR_Fade.Start(Color.clear, _fadeTime, true);

        IsTeleporting = false;
    }

}
