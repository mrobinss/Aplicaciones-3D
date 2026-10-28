using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Player : MonoBehaviour
{
    // Serialized
    [SerializeField] private float speed = 4;
    [SerializeField] private float gravity = 20f;

    // Private
    private CharacterController _characterController;
    private Animator _anim;
    private Vector3 _currentMovement;
    private float _verticalSpeed;
    private bool _isMoving;

    // MonoBehaviour Callbacks
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _anim = GetComponent<Animator>();

        // El Rigidbody solo sirve para detectar las estaciones (triggers). Si fuera dinamico, la fisica
        // empujaria al personaje al rozar los mesones y lo haria deslizarse o flotar.
        if (TryGetComponent(out Rigidbody body)) body.isKinematic = true;
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Gravedad para que el personaje siempre quede apoyado en el piso
        if (_characterController.isGrounded) _verticalSpeed = -1f;
        else _verticalSpeed -= gravity * Time.deltaTime;
        Vector3 fall = Vector3.up * (_verticalSpeed * Time.deltaTime);

        // Mientras muele o amasa (animacion "Cutting") el jugador no se mueve ni gira
        if (_anim.GetBool("Cutting"))
        {
            _isMoving = false;
            _anim.SetBool("Running", false);
            _characterController.Move(fall);
            return;
        }

        Vector3 move = new Vector3(-Input.GetAxis("Horizontal"), 0, -Input.GetAxis("Vertical"));
        _currentMovement = move.normalized * (Time.deltaTime * speed);
        _characterController.Move(_currentMovement + fall);
        
        // Rotation
        if (move != Vector3.zero)
        {
            _isMoving = true;
            _anim.SetBool("Running", true);
            Quaternion toRotation = Quaternion.LookRotation(_currentMovement);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 2000 * Time.deltaTime);
        }
        else
        {
            _isMoving = true;
            _anim.SetBool("Running", false);
        }
    }
    
    // Public *****
    public bool IsMoving() => _isMoving;
}
