using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    private Animator[] _doorAnimators;

    [SerializeField]
    private bool DEBUG_TUTORIAL_END = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one GameManager!!!!");
        }
        Instance = this;

        TutorialSign.OnTutorialComplete += OnTutorialComplete;
    }

    private void OnTutorialComplete()
    {
        foreach (Animator animator in _doorAnimators)
        {
            animator.SetTrigger("openTrigger");
        }

        TutorialSign.OnTutorialComplete -= OnTutorialComplete;
    }

    private void OnValidate()
    {
        if (DEBUG_TUTORIAL_END)
        {
            DEBUG_TUTORIAL_END = false;
            OnTutorialComplete();
        }
    }


}
