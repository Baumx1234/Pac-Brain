using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

[RequireComponent(typeof(Movement))]
public class PacManAgent : Agent
{
    [SerializeField] private AnimatedSprite deathSequence;
    private SpriteRenderer spriteRenderer;
    private Movement movement;
    private float timeSinceLastPellet;
    private new Collider2D collider;

    private GameManager gamemanager;
    private int currentAction;
    private Vector3 previousPosition;
    public float startTime;

    // Constants
    private const float MaxTimeWithoutPellets = 30f;
    private const float PelletReward = 2f;
    private const float PowerPelletReward = 5f;
    private const float DeathReward = -2f;
    private const float WinReward = 1f;
    private const float NearPelletReward = 0.002f;
    private const float AwayPelletReward = -0.001f;

    private const int Up = 0;
    private const int Down = 1;
    private const int Left = 2;
    private const int Right = 3;


    private RayPerceptionSensorComponent2D rayPerceptionSensor;

    public override void Initialize()
    {
        rayPerceptionSensor = GetComponent<RayPerceptionSensorComponent2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        movement = GetComponent<Movement>();
        collider = GetComponent<Collider2D>();
        gamemanager = FindObjectOfType<GameManager>();
        currentAction = -1;
        timeSinceLastPellet = 0f;
        startTime = Time.time;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(timeSinceLastPellet);
    }

    private void FixedUpdate()
    {
        timeSinceLastPellet += Time.fixedDeltaTime;
        if (timeSinceLastPellet >= MaxTimeWithoutPellets)
        {
            // Setze den Timer zurück
            timeSinceLastPellet = 0f;
            GiveDeathReward();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Pellet"))
        {
            timeSinceLastPellet = 0f;
            AddReward(PelletReward);
        }

        if (collision.CompareTag("PowerPellet"))
        {
            timeSinceLastPellet = 0f;
            AddReward(PowerPelletReward);
        }
    }

    // For the gamemanager
    public void GiveWinReward()
    {
        AddReward(WinReward);
        EndEpisode();
    }

    // For the gamemanager
    public void GiveDeathReward()
    {
        AddReward(DeathReward);
        EndEpisode();
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        // Convert discrete actions to movement directions
        int movementAction = actions.DiscreteActions[0];
        Vector2 direction = Vector2.zero;

        switch (movementAction)
        {
            case Up:
                direction = Vector2.up;
                break;
            case Down:
                direction = Vector2.down;
                break;
            case Left:
                direction = Vector2.left;
                break;
            case Right:
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


    private float GetRayHitDistance(int index)
    {
        RayPerceptionOutput.RayOutput[] rayOutputs =
            RayPerceptionSensor.Perceive(rayPerceptionSensor.GetRayPerceptionInput(), false).RayOutputs;

        var rayDirection = rayOutputs[index].EndPositionWorld - rayOutputs[index].StartPositionWorld;
        var scaledRayLength = rayDirection.magnitude;
        return rayOutputs[index].HitFraction * scaledRayLength;
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        Debug.Log(GetRayHitDistance(0));
        if (GetRayHitDistance(0) >= 0.01f)
        {
            actionMask.SetActionEnabled(0, Up, true);
            Debug.Log("Nach Oben an");
        }
        else
        {
            actionMask.SetActionEnabled(0, Up, false);
            Debug.Log("Nach Oben aus");
        }

        if (GetRayHitDistance(1) >= 0.01f)
        {
            actionMask.SetActionEnabled(0, Right, true);
        }
        else
        {
            actionMask.SetActionEnabled(0, Right, false);
        }

        if (GetRayHitDistance(2) >= 0.01f)
        {
            actionMask.SetActionEnabled(0, Left, true);
        }
        else
        {
            actionMask.SetActionEnabled(0, Left, false);
        }

        if (GetRayHitDistance(3) >= 0.01f)
        {
            actionMask.SetActionEnabled(0, Down, true);
        }
        else
        {
            actionMask.SetActionEnabled(0, Down, false);
        }
        /*
        RayPerceptionOutput.RayOutput[] rayOutputs = RayPerceptionSensor.Perceive(rayPerceptionSensor.GetRayPerceptionInput(), false).RayOutputs;
        GameObject UpRay = rayOutputs[0].HitGameObject;
        GameObject RightRay = rayOutputs[1].HitGameObject;
        GameObject LeftRay = rayOutputs[2].HitGameObject;
        GameObject DownRay = rayOutputs[3].HitGameObject;

        if (UpRay == null)
        {
            actionMask.SetActionEnabled(0, Up, true);
            //Debug.Log("Nach Oben an");
        }
        else
        {
            actionMask.SetActionEnabled(0, Up, false);
            //Debug.Log("Nach Oben aus");
        }

        if (RightRay == null)
        {
            actionMask.SetActionEnabled(0, Right, true);
            Debug.Log("Nach Rechts an");
        }
        else
        {
            actionMask.SetActionEnabled(0, Right, false);
            Debug.Log("Nach Rechts aus");
        }

        if (LeftRay == null)
        {
            actionMask.SetActionEnabled(0, Left, true);
            Debug.Log("Nach Links an");
        }
        else
        {
            actionMask.SetActionEnabled(0, Left, false);
            Debug.Log("Nach Links aus");
        }

        if (DownRay == null)
        {
            actionMask.SetActionEnabled(0, Down, true);
        }
        else
        {
            actionMask.SetActionEnabled(0, Down, false);
        }

*/
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Allows manual control for testing purposes
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            currentAction = Up;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            currentAction = Down;
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            currentAction = Left;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            currentAction = Right;
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