using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.classComponents
{
    [Serializable]
    public class GenericComponent : MonoBehaviour
    {
        public GenericComponent()
        {

        }


        public void ThrowError(String message)
        {
            GUIManager gm = GameObject.FindObjectOfType<GUIManager>();
            gm.DisplayErrorMessage(message);
        }
    }
}
