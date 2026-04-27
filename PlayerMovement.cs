using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    //Stamina
    public Image staminaBar;
    public float stamina, maxStamina;
    public float staminaDrain;
    public float staminaCharge;
    public bool running = false;
    private Coroutine recharge;
    private float sprintBoost = 2.5f;

    //Tired function
    bool isTired = false;
    public float tiredDuration = 2f;
    public float tiredDebuff = 0.5f;

    //Movement
    public InputAction MoveAction;
    public float walkSpeed = 1.0f;
    public float turnSpeed = 20f;

    //Model
    Animator m_Animator;
    Rigidbody m_Rigidbody;
    Vector3 m_Movement;
    Quaternion m_Rotation = Quaternion.identity;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        MoveAction.Enable();
        m_Animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        var pos = MoveAction.ReadValue<Vector2>();

        float horizontal = pos.x;
        float vertical = pos.y;

        m_Movement.Set(horizontal, 0f, vertical);
        m_Movement.Normalize();

        bool hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);
        bool hasVerticalInput = !Mathf.Approximately(vertical, 0f);
        bool isWalking = hasHorizontalInput || hasVerticalInput;
        m_Animator.SetBool("IsWalking", isWalking);

        bool hasInput = m_Movement.sqrMagnitude > 0.001f;

        // Stop leftover motion (prevents spinning after sprint)
        if (!hasInput)
        {
            m_Rigidbody.linearVelocity = Vector3.zero;
        }
        else
        {
            // Smooth rotation toward movement direction
            Quaternion targetRotation = Quaternion.LookRotation(m_Movement);
            m_Rigidbody.MoveRotation(
                Quaternion.Lerp(m_Rigidbody.rotation, targetRotation, turnSpeed * Time.deltaTime)
            );
        }

        running = Input.GetKey(KeyCode.Space);

        if (isTired)
        {
            m_Rigidbody.linearVelocity = m_Movement * walkSpeed * tiredDebuff;
        }

        else if (running && stamina > 0)
        {
            m_Rigidbody.linearVelocity = m_Movement * walkSpeed * sprintBoost;

            stamina -= staminaDrain * Time.deltaTime;

            if (stamina <= 0)
            {
                stamina = 0;
                StartCoroutine(Tired());
            }
        }

        else
        {
            m_Rigidbody.linearVelocity = m_Movement * walkSpeed;

            if (stamina < maxStamina)
            {
                stamina += staminaCharge * Time.deltaTime;
            }
        }

        stamina = Mathf.Clamp(stamina, 0, maxStamina);
        staminaBar.fillAmount = stamina / maxStamina;

        IEnumerator Tired()
        {
            isTired = true;

            yield return new WaitForSeconds(tiredDuration);

            isTired = false;
        }

        stamina = Mathf.Clamp(stamina, 0, maxStamina);
        staminaBar.fillAmount = stamina / maxStamina;
    }
}