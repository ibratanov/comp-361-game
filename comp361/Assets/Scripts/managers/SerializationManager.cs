using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.managers
{
    public class SerializationManager : MonoBehaviour
    {

        public bool SaveGame(GameComponent game)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/gameInfo.dat",
                             FileMode.Open);
            return true;
        }

        public bool LoadGame()
        {
            return true;
        }

    }
}
