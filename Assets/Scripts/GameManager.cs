﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    private Vector3 _tutorialStartPos;

    [SerializeField]
    private Vector3 _gameStartPos;

    [SerializeField]
    private Animator[] _doorAnimators;

    [SerializeField]
    private bool _skipTutorial = false;

    [SerializeField]
    private bool DEBUG_TUTORIAL_END = false;

    private Teleporter _teleporter;

    private AudioSource _successAudio;

    private int _numItemsFound = 0;

    private List<HiddenItem.Type> _itemsFound = new List<HiddenItem.Type>();

    private BufferedLogger _log = new BufferedLogger("GameManager");

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one GameManager!!!!");
        }
        Instance = this;

        _teleporter = FindObjectOfType<Teleporter>();
        _successAudio = GetComponent<AudioSource>();

        TutorialSign.OnTutorialComplete += OnTutorialComplete;
        HiddenItem.OnItemFound += OnItemFound;
    }

    private void Start()
    {
        Vector3 dest = _skipTutorial ? _gameStartPos : _tutorialStartPos;
        _teleporter.Teleport(dest);
    }

    private void OnTutorialComplete()
    {
        foreach (Animator animator in _doorAnimators)
        {
            animator.SetTrigger("openTrigger");
        }

        TutorialSign.OnTutorialComplete -= OnTutorialComplete;

        _log.Append("tutorialComplete", true);
        _log.CommitLine();
        StartCoroutine(EndTutorial());
    }

    private void OnItemFound(HiddenItem.Type type)
    {
        if (!_itemsFound.Contains(type))
        {
            _itemsFound.Add(type);
            _numItemsFound++;

            if (_numItemsFound == 5)
            {
                _log.Append("gameOver", true);
                _log.CommitLine();
                StartCoroutine(EndGame());
            }
        }
    }

    private IEnumerator EndTutorial()
    {
        yield return new WaitForSeconds(2f);
        _teleporter.Teleport(_gameStartPos);
        yield return new WaitForSeconds(2f);

        foreach (Animator animator in _doorAnimators)
        {
            animator.SetTrigger("closeTrigger");
        }
    }

    private IEnumerator EndGame()
    {
        yield return new WaitForSeconds(1f);
        _successAudio.Play();
        yield return new WaitForSeconds(_successAudio.clip.length);

        SteamVR_Fade.Start(Color.white, 6f, true);
    }

    private void OnValidate()
    {
        if (DEBUG_TUTORIAL_END)
        {
            DEBUG_TUTORIAL_END = false;
            OnTutorialComplete();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_tutorialStartPos, 1f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(_gameStartPos, 1f);
    }


}
