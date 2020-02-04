using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSign : MonoBehaviour
{
    public delegate void TutorialEvent();
    public static event TutorialEvent OnTutorialComplete;

    private static readonly int NUMBER_OF_SIGNS = 5;

    private static int s_NextSign = 1;

    [SerializeField]
    [Range(1, 5)]
    private int _number;

    private Transform _player;

    private LerpAlpha _fadeEffect;

    private void Awake()
    {
        _player = Camera.main.transform;
        _fadeEffect = GetComponent<LerpAlpha>();
    }

    private void Update()
    {
        if (s_NextSign == _number)
        {
            float distToPlayer = Vector3.Distance(_player.position, transform.position);
            if (distToPlayer < 2f)
            {
                Complete();
            }
        }
    }

    private void Complete()
    {
        if (s_NextSign == NUMBER_OF_SIGNS)
        {
            OnTutorialComplete();
        }
        s_NextSign++;
        _fadeEffect.FadeWithEmission();
        enabled = false;
        Destroy(gameObject, 2f);
    }


}
