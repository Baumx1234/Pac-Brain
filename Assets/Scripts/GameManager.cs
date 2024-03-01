using System.Net;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Ghost[] ghosts;

    [SerializeField] private PacManAgent pacmanagent;

    [SerializeField] private Transform pellets;

    [SerializeField] private Text gameOverText;

    [SerializeField] private Text scoreText;

    [SerializeField] private Text livesText;

    public int totalLives;

    private int ghostMultiplier = 1;
    private int lives;
    private int score = 0;

    public int Lives => lives;
    public int Score => score;

    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        pacmanagent = pacmanagent.GetComponent<PacManAgent>();
        SetLives(totalLives);
        NewGame();
    }

    private void Update()
    {
        if (lives <= 0)
        {
            NewGame();
        }
    }

    private void NewGame()
    {
        SetScore(0);
        SetLives(totalLives);
        NewRound();
    }

    private void NewRound()
    {
        gameOverText.enabled = false;

        foreach (Transform pellet in pellets)
        {
            pellet.gameObject.SetActive(true);
        }

        ResetState();
    }

    private void ResetState()
    {
        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].ResetState();
        }
        // pacmanagent.startTime = Time.time;
        pacmanagent.OnEpisodeBegin();
    }

    private void GameOver()
    {
        gameOverText.enabled = true;

        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].gameObject.SetActive(false);
        }
        pacmanagent.gameObject.SetActive(false);
    }

    private void SetLives(int lives)
    {
        this.lives = lives;
        livesText.text = "x" + lives.ToString();
    }

    private void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString().PadLeft(2, '0');
    }

    public void PacmanEaten()
    {
        pacmanagent.GiveDeathReward();
        pacmanagent.EndEpisode();
        pacmanagent.DeathSequence();

        SetLives(lives - 1);

        if (lives > 0)
        {
            ResetState();
        }
        else
        {
            GameOver();
        }
    }

    public void GhostEaten(Ghost ghost)
    {
        int points = ghost.points * ghostMultiplier;
        SetScore(score + points);

        ghostMultiplier++;
    }

    public void PelletEaten(Pellet pellet)
    {
        pellet.gameObject.SetActive(false);

        SetScore(score + pellet.points);

        if (!HasRemainingPellets())
        {
            pacmanagent.GiveWinReward();
            pacmanagent.EndEpisode();
            pacmanagent.gameObject.SetActive(false);
            NewRound();
        }
    }

    public void PowerPelletEaten(PowerPellet pellet)
    {
        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].frightened.Enable(pellet.duration);
        }

        PelletEaten(pellet);
        CancelInvoke(nameof(ResetGhostMultiplier));
        Invoke(nameof(ResetGhostMultiplier), pellet.duration);
    }

    public bool HasRemainingPellets()
    {
        foreach (Transform pellet in pellets)
        {
            if (pellet.gameObject.activeSelf)
            {
                return true;
            }
        }

        return false;
    }

    public Transform GetPellets()
    {
        return this.pellets;
    }

    private void ResetGhostMultiplier()
    {
        ghostMultiplier = 1;
    }
}