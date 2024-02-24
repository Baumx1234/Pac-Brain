using UnityEngine;
using UnityEngine.SceneManagement;

public class PowerPellet : Pellet
{
    public float duration = 8f;

    protected override void Eat()
    {
        // Deactivate eating ghost for training
        if(SceneManager.GetActiveScene().name != "All") {
            this.duration = 0f;
        }

        GameManager.Instance.PowerPelletEaten(this);
    }

}
