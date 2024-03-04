using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

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
    private Vector3 previousPosition;


    // Constants
    private const float PelletReward = 0.02f;
    private const float PowerPelletReward = 0.05f;
    private const float DeathReward = -1f;
    private const float WinReward = 1f;
    private const float NearPelletReward = 0.01f;
    private const float AwayPelletReward = -0.001f;

    public override void Initialize()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        movement = GetComponent<Movement>();
        collider = GetComponent<Collider2D>();
        gamemanager = FindObjectOfType<GameManager>();
        currentAction = 3;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Pellet"))
        {
            // timeSinceLastPellet = 0f;
            AddReward(PelletReward);
        }

        if (collision.CompareTag("PowerPellet"))
        {
            // timeSinceLastPellet = 0f;
            AddReward(PowerPelletReward);
        }
    }

    // For the gamemanager
    public void GiveWinReward()
    {
        AddReward(WinReward);
    }

    // For the gamemanager
    public void GiveDeathReward()
    {
        AddReward(DeathReward);
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



        Transform closestPellet = null;
        float closestDistance = float.MaxValue;

        foreach (Transform pellet in gamemanager.pellets)
        {
            if (pellet.gameObject.activeSelf)
            {
                float distance = Vector3.Distance(transform.position, pellet.position);
                if (distance < closestDistance)
                {
                    closestPellet = pellet;
                    closestDistance = distance;
                }
            }
        }

        // If there is an active pellet, compare the distance to the previous position
        if (closestPellet != null)
        {
            float previousDistance = Vector3.Distance(previousPosition, closestPellet.position);
            float distanceChange = closestDistance - previousDistance;

            // Introduce a tolerance for the distance change
            float tolerance = 0.1f;

            if (distanceChange < -tolerance)
            {
                // If the agent is closer to the pellet than before, give a reward
                AddReward(NearPelletReward);
            }
            else if (distanceChange > tolerance)
            {
                // If the agent is farther from the pellet than before, give a penalty
                AddReward(AwayPelletReward);
            }
        }

        // Remember the current position for the next action
        previousPosition = transform.position;


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

    public override void OnEpisodeBegin()
    {
        enabled = true;
        spriteRenderer.enabled = true;
        collider.enabled = true;
        deathSequence.enabled = false;
        movement.ResetState();
        gamemanager.GameIsWon = false;
        gameObject.SetActive(true);
    }

    public void DeathSequence()
    {
        enabled = false;
        spriteRenderer.enabled = false;
        collider.enabled = false;
        movement.enabled = false;
        deathSequence.enabled = true;
        deathSequence.Restart();
    }
}
