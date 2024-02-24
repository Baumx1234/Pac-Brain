using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PacmanAgent : Agent
{
    private Movement movement;

    public override void Initialize()
    {
        movement = GetComponent<Movement>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Pellet(Clone)")
        {
            AddReward(10);
        }

        if (collision.name == "PowerPellet(Clone)")
        {
            AddReward(50);
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

        // Set the direction of movement
        movement.SetDirection(direction);
    }

    private int currentAction = 3;


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
}