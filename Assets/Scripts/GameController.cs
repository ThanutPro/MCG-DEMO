using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private Sprite bgImage;

    public Sprite[] puzzles;
    public List<Sprite> gamePuzzles = new List<Sprite>();
    public List<Button> btns = new List<Button>();

    [Header("Sound Effects")]
    public AudioSource buttonPressSFX;
    public AudioSource correctAnswerSFX;
    public AudioSource wrongAnswerSFX;
    public AudioSource gameEndSFX;

    private bool firstGuess, secondGuess;
    private int countGuesses;
    private int countCorrectGuesses;
    private int gameGuesses;
    private int firstGuessIndex, secondGuessIndex;
    private string firstGuessPuzzle, secondGuessPuzzle;

    void Awake()
    {
        puzzles = Resources.LoadAll<Sprite>("Sprites/Animals");
    }

    void Start()
    {
        GetButtons();
        AddListeners();
        AddGamePuzzles();
        Shuffle(gamePuzzles);
        gameGuesses = gamePuzzles.Count / 2;
    }

    void GetButtons()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Puzzle Button");

        for (int i = 0; i < objects.Length; i++)
        {
            btns.Add(objects[i].GetComponent<Button>());
            btns[i].GetComponent<Image>().sprite = bgImage;
        }
    }

    void AddGamePuzzles()
    {
        int looper = btns.Count;
        int index = 0;

        for (int i = 0; i < looper; i++)
        {
            if (index == looper / 2)
            {
                index = 0;
            }

            gamePuzzles.Add(puzzles[index]);

            index++;
        }
    }

    void AddListeners()
    {
        foreach (Button btn in btns)
        {
            btn.onClick.AddListener(() => PickAPuzzle());
        }
    }

    public void PickAPuzzle()
    {
        // Play button press sound
        buttonPressSFX.Play();

        int selectedButtonIndex = int.Parse(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name);

        if (!firstGuess)
        {
            firstGuess = true;

            firstGuessIndex = selectedButtonIndex;
            firstGuessPuzzle = gamePuzzles[firstGuessIndex].name;

            StartCoroutine(FlipAnimation(btns[firstGuessIndex].transform, gamePuzzles[firstGuessIndex]));
        }
        else if (!secondGuess)
        {
            // Ensure the second guess is not the same button as the first guess
            if (selectedButtonIndex == firstGuessIndex)
            {
                return; // Do nothing if the player clicks the same button
            }

            secondGuess = true;

            secondGuessIndex = selectedButtonIndex;
            secondGuessPuzzle = gamePuzzles[secondGuessIndex].name;

            StartCoroutine(FlipAnimation(btns[secondGuessIndex].transform, gamePuzzles[secondGuessIndex], () =>
            {
                // Increment the guess counter
                countGuesses++;
                StartCoroutine(CheckIfThePuzzlesMatch());
            }));
        }
    }

    IEnumerator FlipAnimation(Transform cardTransform, Sprite newSprite, System.Action onComplete = null)
    {
        RectTransform rectTransform = cardTransform.GetComponent<RectTransform>();
        Vector3 originalRotation = rectTransform.localEulerAngles;
        Vector3 flippedRotation = new Vector3(0, 90, 0); // Flip along Y axis

        float duration = 0.2f;
        float elapsedTime = 0f;

        // Animate flip
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            rectTransform.localEulerAngles = Vector3.Lerp(originalRotation, flippedRotation, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.localEulerAngles = flippedRotation;

        cardTransform.GetComponentInChildren<Image>().sprite = newSprite;

        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            rectTransform.localEulerAngles = Vector3.Lerp(flippedRotation, originalRotation, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.localEulerAngles = originalRotation;

        // Execute the callback action
        onComplete?.Invoke();
    }

    IEnumerator CheckIfThePuzzlesMatch()
    {
        if (firstGuessPuzzle == secondGuessPuzzle)
        {
            yield return new WaitForSeconds(.5f);

            // Play correct answer sound
            correctAnswerSFX.Play();

            btns[firstGuessIndex].interactable = false;
            btns[secondGuessIndex].interactable = false;

            btns[firstGuessIndex].image.color = new Color(0, 0, 0, 0);
            btns[secondGuessIndex].image.color = new Color(0, 0, 0, 0);

            CheckIfTheGameIsFinished();
        }
        else
        {
            yield return new WaitForSeconds(.5f);

            // Play wrong answer sound
            wrongAnswerSFX.Play();

            btns[firstGuessIndex].image.sprite = bgImage;
            btns[secondGuessIndex].image.sprite = bgImage;
        }

        yield return new WaitForSeconds(.5f);

        firstGuess = secondGuess = false;
    }

    void CheckIfTheGameIsFinished()
    {
        countCorrectGuesses++;

        if (countCorrectGuesses == gameGuesses)
        {
            // Play game end sound
            gameEndSFX.Play();

            Debug.Log("Game Finished");
            Debug.Log("It took you " + countGuesses + " guesses to finish the game");
        }
    }

    void Shuffle(List<Sprite> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Sprite temp = list[i];
            int randomIndex = Random.Range(0, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
