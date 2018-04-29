using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VehicleBehaviour {
    [RequireComponent(typeof(Rigidbody))]
    public class WheelVehicle : MonoBehaviour {

        [SerializeField] public Sprite preview;
        
        [Header("Inputs")]
        public PlayerNumber playerNumber = PlayerNumber.Player1;
        public string throttleInput = "Throttle";
        public string brakeInput = "Brake";
        public string turnInput = "Horizontal";
        public string resetInput = "Reset";

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
        //Reset
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;

        public Transform centerOfMass;

        [Header("External inputs")]
        public float steering = 0.0f;
        public float throttle { get; set; }

        public bool handbreak = false;

        public float speed = 0.0f;

        [Header("Particles")]
        public ParticleSystem gasParticle;

        private Rigidbody _rb;


        void Start ()
        {
            _rb = GetComponent<Rigidbody>();
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;

            if (centerOfMass)
            {
                _rb.centerOfMass = centerOfMass.localPosition;
            }
        }
        
        void FixedUpdate () {
            speed = transform.InverseTransformDirection(_rb.velocity).z * 3.6f;

            // Accelerate & brake
            if (throttleInput != "" && throttleInput != null)
            {
                // throttle = Input.GetAxis(throttleInput) != 0 ? Input.GetAxis(throttleInput) : Mathf.Clamp(throttle, -1, 1);
                throttle = MultiOSControls.GetValue(throttleInput, playerNumber) - MultiOSControls.GetValue(brakeInput, playerNumber); 
            }
            
            // Turn
            foreach (WheelCollider wheel in turnWheel)
            {
                wheel.steerAngle = Mathf.Lerp(wheel.steerAngle, turnInputCurve.Evaluate(MultiOSControls.GetValue(turnInput, playerNumber)) * steerAngle, steerSpeed);
            }

            // Reset
            if (MultiOSControls.GetValue(resetInput, playerNumber) > .5f)
            {
                transform.position = spawnPosition;
                transform.rotation = spawnRotation;

                _rb.velocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }


            foreach (WheelCollider wheel in GetComponentsInChildren<WheelCollider>())
            {
                wheel.brakeTorque = 0;
            }

            if (handbreak)
            {
                foreach (WheelCollider wheel in GetComponentsInChildren<WheelCollider>())
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
                foreach (WheelCollider wheel in GetComponentsInChildren<WheelCollider>())
                {
                    wheel.motorTorque = 0;
                    wheel.brakeTorque = Mathf.Abs(throttle) * brakeForce;
                }
            }

            if(gasParticle)
            {
                ParticleSystem.EmissionModule em = gasParticle.emission;
                em.rateOverTime = handbreak ? 0 : Mathf.Lerp(em.rateOverTime.constant, Mathf.Clamp(10.0f * throttle, 5.0f, 10.0f), 0.1f);
            }
        }

        public void toogleHandbrake(bool h)
        {
            handbreak = h;
        }
    }
}
