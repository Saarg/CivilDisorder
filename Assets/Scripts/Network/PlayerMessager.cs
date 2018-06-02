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

    public override void OnStartServer() {
        NetworkServer.RegisterHandler(NetworkMessages.PlayerUpdatePos, OnServerVehiclePosMsg);
    }

    public override void OnStartClient()
    {
        messagers.Add(this);

        lastSync = Time.realtimeSinceStartup;

        player = GetComponent<Player>();
        vehicle = GetComponent<WheelVehicle>();
        rigidbody = GetComponent<Rigidbody>();

        foreach (NetworkClient client in NetworkClient.allClients)
        {
            client.RegisterHandler(NetworkMessages.PlayerUpdatePos, OnClientVehiclePosMsg);
        }
    }

    void OnDestroy()
    {
        messagers.Remove(this);
    }

    static void OnServerVehiclePosMsg(NetworkMessage message) {
        VehiclePosMessage msg = message.ReadMessage<VehiclePosMessage>();
        if (msg == null)
            return;
        
        NetworkServer.SendUnreliableToAll(NetworkMessages.PlayerUpdatePos, msg);
    }

    static void OnClientVehiclePosMsg(NetworkMessage message) {
        VehiclePosMessage msg = message.ReadMessage<VehiclePosMessage>();
        if (msg == null)
            return;

        PlayerMessager messager = messagers.Find((m) => { return m.netId == msg.netId; });

        if (messager != null && !messager.isLocalPlayer && msg.number >  messager.lastNumber) { 
            messager.lastNumber = msg.number;

            if (!msg.collision) {
                messager.transform.position = Vector3.Lerp(messager.transform.position, msg.pos, messager.posLerp);
                messager.transform.rotation = Quaternion.Lerp(messager.transform.rotation, msg.rot, messager.rotLerp);
            } else {
                messager.transform.position = msg.pos;
                messager.transform.rotation = msg.rot;                
            }

            messager.rigidbody.velocity = msg.velocity;
            messager.rigidbody.angularVelocity = msg.angularVelocity;
            messager.vehicle.steering = msg.steering;
            messager.vehicle.throttle = msg.throttle;
            messager.vehicle.boosting = msg.boosting;
            messager.vehicle.drift = msg.drifting;
        } else if (messager == null) {
            Debug.LogWarning("Could not find target");
        }
    }

    void Update()
    {
        if (isLocalPlayer && (Time.realtimeSinceStartup - lastSync >= 1 / updateRate || player.collisionDetected)) { 
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
            msg.boosting = vehicle.boosting;
            msg.drifting = vehicle.drift;
            msg.collision = player.collisionDetected;

            connectionToServer.SendByChannel(NetworkMessages.PlayerUpdatePos, msg, 1);

            player.collisionDetected = false;            
        }
    }
}