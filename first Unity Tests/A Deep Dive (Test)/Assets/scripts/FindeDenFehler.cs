using System.Collections.Generic;
using UnityEngine;

namespace wovr
{
    public class FindeDenFehler : MonoBehaviour
    {
        public List<GameObject> fehler;
        private GameObject currentFehler;

        /// <summary>
        /// Start the error.
        /// </summary>
        internal void StartFehler()
        {
            var number = Random.Range(0, fehler.Count);
            currentFehler = Instantiate(fehler[number], transform);

            currentFehler.transform.localPosition = Vector3.zero;
            //currentFehler.transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Add listener.
        /// </summary>
        private void OnEnable()
        {
            FindeDenFehlerManager.Register(this);
        }

        /// <summary>
        /// Remove listener
        /// </summary>
        private void OnDisable()
        {
            FindeDenFehlerManager.Remove(this);
            if (currentFehler != null)
                Destroy(currentFehler);
        }
    }
}
