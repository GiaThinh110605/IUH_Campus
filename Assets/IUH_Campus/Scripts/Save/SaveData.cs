using System;
using System.Collections.Generic;
using UnityEngine;

namespace IUHCampus.Save
{
    [Serializable]
    public class PlayerSaveData
    {
        public float posX;
        public float posY;
        public float posZ;
        public float rotY;
        public string currentSpawnPointId = "SPAWN_MainCampus";

        public Vector3 GetPosition() => new Vector3(posX, posY, posZ);
        public Quaternion GetRotation() => Quaternion.Euler(0, rotY, 0);

        public void SetPose(Vector3 pos, Quaternion rot)
        {
            posX = pos.x;
            posY = pos.y;
            posZ = pos.z;
            rotY = rot.eulerAngles.y;
        }
    }

    [Serializable]
    public class WorldObjectRecord
    {
        public string objectId;
        public string stateValue;

        public WorldObjectRecord() { }
        public WorldObjectRecord(string id, string val)
        {
            objectId = id;
            stateValue = val;
        }
    }

    [Serializable]
    public class SaveContainer
    {
        public string timestamp;
        public string gameVersion = "1.0.0";
        public PlayerSaveData player = new PlayerSaveData();
        public List<WorldObjectRecord> worldObjects = new List<WorldObjectRecord>();
    }
}
