using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class BuildingUpdateMessage_RB_DATA {
    public int buildingIndex;
    public int rigidbodyIndex;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
}

public class BuildingUpdateMessage : MessageBase
{
    static int msgCount = 0;
    public int number;    
    public List<BuildingUpdateMessage_RB_DATA> rigidbodys = new List<BuildingUpdateMessage_RB_DATA>();

    public BuildingUpdateMessage() {
        number = msgCount++;
    }

    public override void Deserialize(NetworkReader reader)
    {  
        number = reader.ReadInt32(); 
        int rbCount = reader.ReadInt32();

        rigidbodys = new List<BuildingUpdateMessage_RB_DATA>(rbCount);

        for (int i = 0; i < rbCount; i++) {
            rigidbodys.Add(new BuildingUpdateMessage_RB_DATA());

            rigidbodys[i].buildingIndex = reader.ReadInt32();
            rigidbodys[i].rigidbodyIndex = reader.ReadInt32();

            rigidbodys[i].position = reader.ReadVector3();
            rigidbodys[i].rotation = reader.ReadQuaternion();
            rigidbodys[i].velocity = reader.ReadVector3();
        }
    }

    public override void Serialize(NetworkWriter writer)
    {
        writer.Write(number);
        writer.Write(rigidbodys.Count);

        foreach (BuildingUpdateMessage_RB_DATA rigidbody in rigidbodys)
        {
            writer.Write(rigidbody.buildingIndex);
            writer.Write(rigidbody.rigidbodyIndex);

            writer.Write(rigidbody.position);
            writer.Write(rigidbody.rotation);
            writer.Write(rigidbody.velocity);                        
        }       
    }
}