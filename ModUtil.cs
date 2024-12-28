using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace RebalancedMoons
{
    internal class ModUtil
    {
        public static List<T> SearchInLatestScene<T>() where T : UnityEngine.Object
        {
            List<T> returnList = new List<T>();
            foreach (GameObject sceneObject in SceneManager.GetSceneAt(SceneManager.sceneCount - 1).GetRootGameObjects())
                foreach (T component in sceneObject.GetComponentsInChildren<T>())
                    returnList.Add(component);
            return (returnList);
        }
    }
}
