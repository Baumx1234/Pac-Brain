using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

[RequireComponent(typeof(Movement))]
public class PacManAgent : Agent
{
    [SerializeField] private AnimatedSprite deathSequence;
    private SpriteRenderer spriteRenderer;
    private Movement movement;
    private new Collider2D collider;
    private int currentAction = 3;

    public override void Initialize()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        movement = GetComponent<Movement>();
        collider = GetComponent<Collider2D>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.name == "Pellet(Clone)")
        {
            AddReward(10f);
        }

        if (collision.name == "PowerPellet(Clone)")
        {
            AddReward(50f);
        }

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
        EndEpisode();
    }

    public void DeathSequence()
    {
        // negative reward if pacman dies
        AddReward(-100f);

        enabled = false;
        spriteRenderer.enabled = false;
        collider.enabled = false;
        movement.enabled = false;
        deathSequence.enabled = true;
        deathSequence.Restart();
        EndEpisode();
    }
}