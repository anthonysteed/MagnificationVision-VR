using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    private Animator[] _doorAnimators;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one GameManager!!!!");
        }
        Instance = this;

        TutorialSign.OnTutorialComplete += OnTutorialComplete;
        Debug.Log("GameManager awake called");
    }

    private void OnTutorialComplete()
    {
        Debug.Log("GameManager registered tutorial complete");
        foreach (Animator animator in _doorAnimators)
        {
            animator.SetTrigger("openTrigger");
        }

        TutorialSign.OnTutorialComplete -= OnTutorialComplete;
    }


}
