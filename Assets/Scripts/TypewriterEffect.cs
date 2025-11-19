using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

public class TypewriterEffect : MonoBehaviour
{
    private TMP_Text _textBox;

    private int _currentVisibleCharacterIndex;
    private Coroutine _typewriterCoroutine;
    private bool _readyForNewText = true;

    private WaitForSeconds _simpleDelay;
    private WaitForSeconds _interpunctuationDelay;

    [Header("Typewriter Settings")]
    [SerializeField] private float charactersPerSecond = 20;
    [SerializeField] private float interpunctuationDelay = 0.5f;

    public bool CurrentlySkipping { get; private set; }
    private WaitForSeconds _skipDelay;

    [Header("Skip options")]
    [SerializeField] private bool quickSkip;
    [SerializeField][Min(1)] private int skipSpeedup = 5;

    private WaitForSeconds _textboxFullEventDelay;
    [SerializeField][Range(0.1f, 0.5f)] private float sendDoneDelay = 0.25f;

    public static event Action CompleteTextRevealed;
    public static event Action<char> CharacterRevealed;

    private void Awake()
    {
        _textBox = GetComponent<TMP_Text>();
        _simpleDelay = new WaitForSeconds(1 / charactersPerSecond);
        _interpunctuationDelay = new WaitForSeconds(interpunctuationDelay);
        _skipDelay = new WaitForSeconds(1 / (charactersPerSecond * skipSpeedup));
        _textboxFullEventDelay = new WaitForSeconds(sendDoneDelay);
    }

    private void OnEnable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(PrepareForNewText);
    }

    private void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(PrepareForNewText);
    }

    private void PrepareForNewText(Object obj)
    {
        if (obj != _textBox || !_readyForNewText)
            return;

        CurrentlySkipping = false;
        _readyForNewText = false;

        if (_typewriterCoroutine != null)
            StopCoroutine(_typewriterCoroutine);

        // Instantly hide text and prevent TMP from rendering
        _textBox.alpha = 0;
        _textBox.maxVisibleCharacters = 0;
        _currentVisibleCharacterIndex = 0;

        StartCoroutine(StartAfterTMPReady());
    }

    private IEnumerator StartAfterTMPReady()
    {
        // Wait until TMP has valid geometry (character count > 0)
        int safetyFrames = 0;
        while (_textBox.textInfo.characterCount == 0 && safetyFrames < 10)
        {
            yield return null;
            _textBox.ForceMeshUpdate();
            safetyFrames++;
        }

        // If still no text, stop silently
        if (_textBox.textInfo.characterCount == 0)
        {
            Debug.LogWarning($"[TypewriterEffect] TMP text not ready on {name} after waiting {safetyFrames} frames.");
            _readyForNewText = true;
            yield break;
        }

        // TMP is now ready — show text again
        _textBox.alpha = 1;

        // Start the typing animation
        _typewriterCoroutine = StartCoroutine(Typewriter());
    }


    private IEnumerator Typewriter()
    {
        TMP_TextInfo textInfo = _textBox.textInfo;
        int totalCharacters = textInfo.characterCount;

        if (totalCharacters == 0)
        {
            _readyForNewText = true;
            yield break;
        }

        while (_currentVisibleCharacterIndex < totalCharacters)
        {
            char character = textInfo.characterInfo[_currentVisibleCharacterIndex].character;

            // Reveal the next character
            _textBox.maxVisibleCharacters = _currentVisibleCharacterIndex + 1;

            // Apply punctuation delay if not skipping
            if (!CurrentlySkipping && 
                (character == '?' || character == '.' || character == ',' ||
                 character == ':' || character == ';' || character == '!' || character == '-'))
            {
                yield return _interpunctuationDelay;
            }
            else
            {
                yield return CurrentlySkipping ? _skipDelay : _simpleDelay;
            }

            CharacterRevealed?.Invoke(character);
            _currentVisibleCharacterIndex++;
        }

        // Delay slightly, then signal completion
        yield return _textboxFullEventDelay;
        CompleteTextRevealed?.Invoke();
        _readyForNewText = true;
    }

    public void ForceSkip()
    {
        CurrentlySkipping = true;
    }

    public void ForceComplete()
    {
        CurrentlySkipping = true;

        if (_textBox != null)
        {
            // Set index to the end
            _currentVisibleCharacterIndex = _textBox.textInfo.characterCount;
            _textBox.maxVisibleCharacters = _currentVisibleCharacterIndex;
        }

        // Immediately notify listeners
        CompleteTextRevealed?.Invoke();
    }


}
