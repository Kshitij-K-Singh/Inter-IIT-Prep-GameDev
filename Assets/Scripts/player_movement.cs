using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float runSpeed = 6f;

    [Header("Interaction Settings")]
    public float interactRange = 3f;
    public LayerMask interactableMask;
    public Transform tr;

    private CharacterController controller;
    private Vector3 playerVelocity;
    public bool isGrounded;

    public Animator anim;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
        HandleInteraction();
    }

    // --- Movement ---
    void HandleMovement()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -1f;
        }

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * verticalInput + right * horizontalInput).normalized;

        float currentSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = runSpeed;
        }

        if (moveDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);

            controller.Move(moveDirection * currentSpeed * Time.deltaTime);
        }

        controller.Move(playerVelocity * Time.deltaTime);

        UpdateAnimator(moveDirection);
    }

    void UpdateAnimator(Vector3 moveDirection)
    {
        bool isMoving = moveDirection.magnitude > 0.1f;

        if (!isMoving)
        {
            anim.SetBool("Walk", false);
            anim.SetBool("Run", false);
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                anim.SetBool("Walk", false);
                anim.SetBool("Run", true);
            }
            else
            {
                anim.SetBool("Walk", true);
                anim.SetBool("Run", false);
            }
        }

        if (Input.GetButtonDown("Jump"))
            anim.SetTrigger("Jump");
    }

    // --- Interaction ---
    void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(tr.position + Vector3.up * 1.0f, tr.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactRange, interactableMask))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact();
                }
                else
                {
                    Debug.Log("Interacted with: " + hit.collider.name);
                }
            }
        }
    }

    // Helper for recorder fallback (if needed)
    public int TryGetInteractableID()
    {
        Ray ray = new Ray(tr.position + Vector3.up * 1.0f, tr.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableMask))
        {
            return hit.collider.gameObject.GetInstanceID();
        }
        return -1;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(tr.position + Vector3.up * 1.0f, tr.forward * interactRange);
    }
}

// --- Interface for interactables ---
public interface IInteractable
{
    void Interact();
}
