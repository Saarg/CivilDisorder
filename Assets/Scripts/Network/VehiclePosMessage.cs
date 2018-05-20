using UnityEngine;
using UnityEngine.Networking;

 public class VehiclePosMessage : MessageBase
{
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 velocity;
    public Vector3 angularVelocity;
    public float steering;
    public float throttle;
    public int number;
    public NetworkInstanceId netId; 
}