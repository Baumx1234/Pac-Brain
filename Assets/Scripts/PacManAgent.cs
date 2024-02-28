using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Movement))]
public class PacManAgent : Agent
{
    [SerializeField]
    private AnimatedSprite deathSequence;
    private SpriteRenderer spriteRenderer;
    private Movement movement;
    private new Collider2D collider;
    private GameManager gamemanager;
    private int currentAction;
    private float timeSinceLastPellet;

    // Constants
    private const float MaxTimeWithoutPellets = 30f;

    private const float PelletReward = 0.01f;
    private const float PowerPelletReward = 0.05f;
    private const float DeathReward = -1f;
    private const float WinReward = 1f;
    private const float HungerReward = -0.5f;

    private Vector3 worldMin = new(-12.5f, -15.5f, 0f);
    private Vector3 worldMax = new(12.5f, 12.5f, 0f);

    public override void Initialize()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        movement = GetComponent<Movement>();
        collider = GetComponent<Collider2D>();
        gamemanager = FindObjectOfType<GameManager>();
        timeSinceLastPellet = 0f;
        currentAction = 3;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Add player position
        Vector3 normalizedPlayerPosition = NormalizePosition(transform.position);
        sensor.AddObservation(normalizedPlayerPosition.x);
        sensor.AddObservation(normalizedPlayerPosition.y);


        Transform pellets = gamemanager.GetPellets();
        if (!pellets)
            return;
        foreach (Transform pellet in pellets)
        {
            // Add normalized pellet position
            Vector3 normalizedPelletPosition = NormalizePosition(pellet.position);
            sensor.AddObservation(normalizedPelletPosition.x);
            sensor.AddObservation(normalizedPelletPosition.y);
            // Add pellet activation status (1 if active, 0 if inactive)
            sensor.AddObservation(pellet.gameObject.activeSelf ? 1f : 0f);
        }
    }

    // Normalizes position to range [0, 1] based on world bounds
    private Vector3 NormalizePosition(Vector3 position)
    {
        Vector3 normalizedPosition = new Vector3(
            Mathf.InverseLerp(worldMin.x, worldMax.x, position.x),
            Mathf.InverseLerp(worldMin.y, worldMax.y, position.y),
            Mathf.InverseLerp(worldMin.z, worldMax.z, position.z)
        );

        return normalizedPosition;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Pellet(Clone)")
        {
            timeSinceLastPellet = 0f;
            AddReward(PelletReward);
        }

        if (collision.name == "PowerPellet(Clone)")
        {
            timeSinceLastPellet = 0f;
            AddReward(PowerPelletReward);
        }
    }

    private void Update()
    {
        timeSinceLastPellet += Time.deltaTime;
        // Wenn die Zeit seit dem letzten Pellet den Schwellenwert überschreitet, geben Sie einen negativen Reward aus
        if (timeSinceLastPellet >= MaxTimeWithoutPellets)
        {
            // Setze den Timer zurück
            timeSinceLastPellet = 0f;
            // PacMan wurde gegessen (gestorben an hunger)
            AddReward(HungerReward);
            EndEpisode();
        }

        if (!gamemanager.HasRemainingPellets())
        {
            AddReward(WinReward);
        }

        Debug.Log(timeSinceLastPellet);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Convert discrete actions to movement directions
        int movementAction = actions.DiscreteActions[0];
        Vector2 direction = Vector2.zero;

        switch (movementAction)
        {
            case 0:
                direction = Vector2.up;
                break;
            case 1:
                direction = Vector2.down;
                break;
            case 2:
                direction = Vector2.left;
                break;
            case 3:
                direction = Vector2.right;
                break;
        }

        Vector2 relativeDirection = Quaternion.Euler(0, 0, -transform.rotation.eulerAngles.z) * direction;
        movement.SetDirection(direction);

        float angle = Mathf.Atan2(movement.direction.y, movement.direction.x);
        transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Allows manual control for testing purposes
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            currentAction = 0;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            currentAction = 1;
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            currentAction = 2;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            currentAction = 3;
        }

        discreteActions[0] = currentAction;
    }

    public void ResetState()
    {
        enabled = true;
        spriteRenderer.enabled = true;
        collider.enabled = true;
        deathSequence.enabled = false;
        movement.ResetState();
        gameObject.SetActive(true);
    }

    public void DeathSequence()
    {
        AddReward(DeathReward);
        enabled = false;
        spriteRenderer.enabled = false;
        collider.enabled = false;
        movement.enabled = false;
        deathSequence.enabled = true;
        deathSequence.Restart();
    }


}
