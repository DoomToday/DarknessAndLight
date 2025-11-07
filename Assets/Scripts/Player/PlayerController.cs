using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputController inputController;
    private int maxHealth = 5;
    private int currentHealth = 5;
    public enum PlayerState
    {
        Idle,
        Walking,
        Running,
        Interacting,
        GettingHit,
        Dying
    }
    public PlayerState currentState = PlayerState.Idle;

    private void Update()
    {
        // to do: implement ability to enter other states than implemented already
        if (inputController.isWalking)
        {
            currentState = (inputController.tryToRun) ? PlayerState.Running : PlayerState.Walking;
        }
        else
        {
            currentState = PlayerState.Idle;
        }
        if (currentState == PlayerState.Running)
        {
            SoundManager.MakeSound(transform.position);
        }
    }

}
