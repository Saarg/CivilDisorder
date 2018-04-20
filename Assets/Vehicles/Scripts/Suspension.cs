﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VehicleBehaviour {
    [RequireComponent(typeof(WheelCollider))]

    public class Suspension : MonoBehaviour {

        public GameObject _wheelModel;
        private WheelCollider _wheelCollider;

        private float lastUpdate;

        void Start()
        {
            lastUpdate = Time.realtimeSinceStartup;

            _wheelCollider = GetComponent<WheelCollider>();

            /*Vector3 scale = new Vector3(0, 0, 0);
            scale.x = _wheelCollider.radius * 2;
            scale.y = _wheelModel.transform.localScale.y;
            scale.z = _wheelCollider.radius * 2;
            _wheelModel.transform.localScale = scale;

            _wheelModel.transform.Rotate(Vector3.forward * 90);*/
        }
        
        void FixedUpdate()
        {
            if (Time.realtimeSinceStartup - lastUpdate < 1f/60f)
            {
                return;
            }
            lastUpdate = Time.realtimeSinceStartup;

            if (_wheelModel && _wheelCollider)
            {
                Vector3 pos = new Vector3(0, 0, 0);
                Quaternion quat = new Quaternion();
                _wheelCollider.GetWorldPose(out pos, out quat);

                _wheelModel.transform.rotation = quat;
            // _wheelModel.transform.Rotate(Vector3.up * -90);
                _wheelModel.transform.position = pos;

                WheelHit wheelHit;
                _wheelCollider.GetGroundHit(out wheelHit);
            }
        }
    }
}
