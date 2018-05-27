﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace VehicleBehaviour {
    [RequireComponent(typeof(Rigidbody))]
    public class WheelVehicle : NetworkBehaviour {

        [SerializeField] public Sprite preview;
        
        [Header("Inputs")]
        public PlayerNumber playerNumber = PlayerNumber.Player1;
        public string throttleInput = "Throttle";
        public string brakeInput = "Brake";
        public string turnInput = "Horizontal";
        public string jumpInput = "Jump";
        public string driftInput = "Drift";

        [SerializeField] AnimationCurve turnInputCurve;

        [Header("Wheels")]
        public WheelCollider[] driveWheel;
        public WheelCollider[] turnWheel;

        [Header("Behaviour")]
        // Engine
        public AnimationCurve motorTorque;
        public float brakeForce = 1500.0f;
        [Range(0f, 50.0f)]
        public float steerAngle = 30.0f;
        [Range(0.001f, 10.0f)]
        public float steerSpeed = 0.2f;
        [Range(1f, 1.5f)]
        public float jumpVel = 1.3f;
        [Range(0.0f, 2f)]
        public float driftIntensity = 1f;
        //Reset
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;

        public Transform centerOfMass;
        [Range(0.5f, 3f)]
        public float downforce = 1.0f;        

        [Header("External inputs")]
        public float steering = 0.0f;
        public float throttle { get; set; }

        public bool handbreak = false;
        public bool drift = false;

        public float speed = 0.0f;

        [Header("Particles")]
        public ParticleSystem[] gasParticles;

        private Rigidbody _rb;

        WheelCollider[] wheels;

        public override void OnStartClient ()
        {
            _rb = GetComponent<Rigidbody>();
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;

            if (centerOfMass)
            {
                _rb.centerOfMass = centerOfMass.localPosition;
            }

            wheels = GetComponentsInChildren<WheelCollider>();
        }

        void Update()
        {
            foreach (ParticleSystem gasParticle in gasParticles)
            {
                ParticleSystem.EmissionModule em = gasParticle.emission;
                em.rateOverTime = handbreak ? 0 : Mathf.Lerp(em.rateOverTime.constant, Mathf.Clamp(10.0f * throttle, 5.0f, 10.0f), 0.1f);
            }
        }
        
        void FixedUpdate () {
            speed = transform.InverseTransformDirection(_rb.velocity).z * 3.6f;

            if (isLocalPlayer) {
                // Accelerate & brake
                if (throttleInput != "" && throttleInput != null)
                {
                    // throttle = Input.GetAxis(throttleInput) != 0 ? Input.GetAxis(throttleInput) : Mathf.Clamp(throttle, -1, 1);
                    throttle = MultiOSControls.GetValue(throttleInput, playerNumber) - MultiOSControls.GetValue(brakeInput, playerNumber); 
                }
                
                // Turn
                steering = turnInputCurve.Evaluate(MultiOSControls.GetValue(turnInput, playerNumber)) * steerAngle;
                drift = MultiOSControls.GetValue(driftInput, playerNumber) > 0;
            }
            foreach (WheelCollider wheel in turnWheel)
            {
                wheel.steerAngle = Mathf.Lerp(wheel.steerAngle, steering, steerSpeed);
            }

            foreach (WheelCollider wheel in wheels)
            {
                wheel.brakeTorque = 0;
            }

            if (handbreak)
            {
                foreach (WheelCollider wheel in wheels)
                {
                    wheel.motorTorque = 0;
                    wheel.brakeTorque = brakeForce;
                }
            }
            else if (Mathf.Abs(speed) < 4 || Mathf.Sign(speed) == Mathf.Sign(throttle))
            {
                foreach (WheelCollider wheel in driveWheel)
                {
                    wheel.brakeTorque = 0;
                    wheel.motorTorque = throttle * motorTorque.Evaluate(speed) * 4;
                }
            }
            else
            {
                foreach (WheelCollider wheel in wheels)
                {
                    wheel.motorTorque = 0;
                    wheel.brakeTorque = Mathf.Abs(throttle) * brakeForce;
                }
            }

            // Jump
            if (MultiOSControls.GetValue(jumpInput, playerNumber) > 0 && isLocalPlayer) {
                bool isGrounded = true;
                foreach (WheelCollider wheel in wheels)
                {
                    if (!wheel.gameObject.activeSelf || !wheel.isGrounded)
                        isGrounded = false;
                }

                if (!isGrounded)
                    return;
                
                _rb.velocity += transform.up * jumpVel;
            }

            // Drift
            if (drift) {
                Vector3 driftForce = -transform.right;
                driftForce.y = 0.0f;
                driftForce.Normalize();

                driftForce *= _rb.mass * speed/5f * throttle * steering/steerAngle;
                Vector3 driftTorque = transform.up * 0.1f  * steering/steerAngle;


                _rb.AddForce(driftForce * driftIntensity, ForceMode.Force);
                _rb.AddTorque(driftTorque * driftIntensity, ForceMode.VelocityChange);

                Debug.DrawLine(transform.position, transform.position + driftForce, Color.red);              
            }

            // Downforce
            _rb.AddForce(transform.up * speed * downforce);
        }

        public void ResetPos() {
            transform.position = spawnPosition;
            transform.rotation = spawnRotation;

            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        public void toogleHandbrake(bool h)
        {
            handbreak = h;
        }
    }
}
