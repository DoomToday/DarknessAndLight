using UnityEngine;
using UnityEngine.EventSystems;

public class MovePlayer : MonoBehaviour
{
    [SerializeField] private InputController inputController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform cameraTransform;
    private Rigidbody playerRigidbody;
    private Vector3 moveInput;
    private Vector3 moveDir;
    private float maxSpeed = 5f;
    [SerializeField] private float maxWalkingSpeed = 5f;
    [SerializeField] private float maxSprintSpeed = 8f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float rotationSpeed = 10f;
    private Vector3 finalAcceleration;
    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        if (playerController.currentState != PlayerController.PlayerState.Walking &&
            playerController.currentState != PlayerController.PlayerState.Running)
            return;
        maxSpeed = playerController.currentState == PlayerController.PlayerState.Running ? maxSprintSpeed : maxWalkingSpeed;
        Vector3 targetVelocity = moveDir * maxSpeed;
        Vector3 velocityChange = targetVelocity - playerRigidbody.linearVelocity;
        finalAcceleration = velocityChange / Time.fixedDeltaTime;
        finalAcceleration = Vector3.ClampMagnitude(finalAcceleration, acceleration);
        playerRigidbody.AddForce(finalAcceleration, ForceMode.Acceleration);
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            playerRigidbody.MoveRotation(Quaternion.Slerp(playerRigidbody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
    }
    private void Update()
    {
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        moveDir = (camForward * moveInput.y + camRight * moveInput.x).normalized;
    }
    private void ReadDirection(Vector2 input)
    {
        moveInput = new Vector3(input.x, input.y, 0);
    }
    private void OnEnable()
    {
        inputController.OnMoveInput += ReadDirection;
    }
    private void OnDisable()
    {
        inputController.OnMoveInput -= ReadDirection;
    }
}
