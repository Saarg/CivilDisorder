using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using VehicleBehaviour;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(WheelVehicle))]
[RequireComponent(typeof(Rigidbody))]

public class PlayerMessager : NetworkBehaviour {
    static List<PlayerMessager> messagers = new List<PlayerMessager>();

    float lastSync;
    [Range(0, 30)]
    [SerializeField] int updateRate;
    int messageCount = 0;
    int lastNumber = -1;

    [Range(0f, 0.1f)]
    [SerializeField] float posLerp = 0.005f;
    [Range(0f, 0.1f)]
    [SerializeField] float rotLerp = 0.005f;

    Player player;
    WheelVehicle vehicle;
	new Rigidbody rigidbody;

    public override void OnStartClient()
    {
        messagers.Add(this);

        lastSync = Time.realtimeSinceStartup;

        player = GetComponent<Player>();
        vehicle = GetComponent<WheelVehicle>();
        rigidbody = GetComponent<Rigidbody>();

        if (isServer) {
            NetworkServer.RegisterHandler(NetworkMessages.PlayerUpdatePos, OnVehiclePosMsg);
        } else {
            foreach (NetworkClient client in NetworkClient.allClients)
            {
                client.RegisterHandler(NetworkMessages.PlayerUpdatePos, OnVehiclePosMsg);
            }
        }
    }

    void OnDestroy()
    {
        messagers.Remove(this);
    }

    static void OnVehiclePosMsg(NetworkMessage message) {
        VehiclePosMessage msg = message.ReadMessage<VehiclePosMessage>();
        if (msg == null)
            return;

        PlayerMessager messager = messagers.Find((m) => { return m.netId == msg.netId; });

        if (messager.isServer) {
            NetworkServer.SendUnreliableToAll(NetworkMessages.PlayerUpdatePos, msg);
        }

        if (messager != null && !messager.isLocalPlayer && msg.number >  messager.lastNumber) { 
            messager.lastNumber = msg.number;           
            messager.transform.position = Vector3.Lerp(messager.transform.position, msg.pos, messager.posLerp);
            messager.transform.rotation = Quaternion.Lerp(messager.transform.rotation, msg.rot, messager.rotLerp);

            messager.rigidbody.velocity = msg.velocity;
            messager.rigidbody.angularVelocity = msg.angularVelocity;
            messager.vehicle.steering = msg.steering;
            messager.vehicle.throttle = msg.throttle;
        } else if (messager == null) {
            Debug.LogWarning("Could not find target");
        }
    }

    void LateUpdate()
    {
        if (isLocalPlayer && Time.realtimeSinceStartup - lastSync >= 1 / updateRate) {         
            lastSync = Time.realtimeSinceStartup;

            VehiclePosMessage msg = new VehiclePosMessage();
            msg.number = messageCount++;
            msg.netId = netId;
            msg.pos = transform.position;
            msg.rot = transform.rotation;
            msg.velocity = rigidbody.velocity;
            msg.angularVelocity = rigidbody.angularVelocity;
            msg.steering = vehicle.steering;
            msg.throttle = vehicle.throttle;

            connectionToServer.SendByChannel(NetworkMessages.PlayerUpdatePos, msg, 1);
        }
    }
}