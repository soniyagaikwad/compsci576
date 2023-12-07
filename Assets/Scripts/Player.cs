using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]



public class FPSController : MonoBehaviour
{
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    public GameObject bulletPrefab0;
    public GameObject bulletPrefab1;
    public GameObject bulletPrefab2;
    public GameObject bulletPrefab3;
    public GameObject bulletPrefab4;
    public GameObject bulletPrefab5;
    public GameObject bulletPrefab6;
    public GameObject bulletPrefab7;
    public GameObject bulletPrefab8;
    public GameObject bulletPrefab9;

    public GameObject equippedBullet;

    public TextMesh currentNumber;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    [HideInInspector]
    public bool canMove = true;

    void Start()
    {
        currentNumber.text = "Number 1";
        equippedBullet = bulletPrefab1;
        characterController = GetComponent<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }


        //Switching bullets
        if (Input.GetKeyDown(KeyCode.Alpha0)){
             currentNumber.text = "Number 0";
            equippedBullet = bulletPrefab0;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)){
             currentNumber.text = "Number 1";
            equippedBullet = bulletPrefab1;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)){
             currentNumber.text = "Number 2";
            equippedBullet = bulletPrefab2;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3)){
             currentNumber.text = "Number 3";
            equippedBullet = bulletPrefab3;
        }

        if (Input.GetKeyDown(KeyCode.Alpha4)){
             currentNumber.text = "Number 4";
            equippedBullet = bulletPrefab4;
        }

        if (Input.GetKeyDown(KeyCode.Alpha5)){
             currentNumber.text = "Number 5";
            equippedBullet = bulletPrefab5;
        }

        if (Input.GetKeyDown(KeyCode.Alpha6)){
             currentNumber.text = "Number 6";
            equippedBullet = bulletPrefab6;
        }

        if (Input.GetKeyDown(KeyCode.Alpha7)){
             currentNumber.text = "Number 7";
            equippedBullet = bulletPrefab7;
        }

        if (Input.GetKeyDown(KeyCode.Alpha8)){
             currentNumber.text = "Number 8";
            equippedBullet = bulletPrefab8;
        }

        if (Input.GetKeyDown(KeyCode.Alpha9)){
             currentNumber.text = "Number 9";
            equippedBullet = bulletPrefab9;
        }

        //Shooting
        if (Input.GetMouseButtonDown(0)) {
                GameObject bulletObject = Instantiate (equippedBullet);
                bulletObject.transform.position = playerCamera.transform.position + playerCamera.transform.forward;
                bulletObject.transform.forward = playerCamera.transform.forward;
        }

    }
}