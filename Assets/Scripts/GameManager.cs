using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    private class Sounds
    {
        public AudioClip GameStart;
        public AudioClip DoorOpen;
        public AudioClip DoorShut;
        public AudioClip TutorialEnd;
        public AudioClip ItemFound;
        public AudioClip GameOver;
    }

    public static GameManager Instance { get; private set; }

    [SerializeField]
    private Vector3 _tutorialStartPos;

    [SerializeField]
    private Vector3 _gameStartPos;

    [SerializeField]
    private Animator[] _doorAnimators;
    
    [SerializeField]
    private Sounds _sounds;

    [SerializeField]
    private bool _skipTutorial = false;

    [SerializeField]
    private bool DEBUG_TUTORIAL_END = false;

    private Teleporter _teleporter;

    private AudioSource _audioSource;

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
        _audioSource = GetComponent<AudioSource>();

        TutorialSign.OnTutorialComplete += OnTutorialComplete;
        HiddenItem.OnItemFound += OnItemFound;
    }

    private void Start()
    {
        Vector3 dest = _skipTutorial ? _gameStartPos : _tutorialStartPos;
        _teleporter.Teleport(dest);

        _audioSource.clip = _sounds.GameStart;
        _audioSource.Play();
    }

    private void OnTutorialComplete()
    {
        foreach (Animator animator in _doorAnimators)
        {
            animator.SetTrigger("openTrigger");
        }
        _audioSource.clip = _sounds.DoorOpen;
        _audioSource.Play();

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
            _audioSource.clip = _sounds.ItemFound;
            _audioSource.Play();

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
        _audioSource.clip = _sounds.TutorialEnd;
        _audioSource.Play();
        yield return new WaitForSeconds(2f);

        foreach (Animator animator in _doorAnimators)
        {
            animator.SetTrigger("closeTrigger");
        }
        _audioSource.clip = _sounds.DoorShut;
        _audioSource.Play();
    }

    private IEnumerator EndGame()
    {
        yield return new WaitForSeconds(2f);
        _audioSource.clip = _sounds.GameOver;
        _audioSource.Play();
        yield return new WaitForSeconds(2f);

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
