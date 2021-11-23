using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace wovr
{
    public class FindeDenFehlerManager : MonoBehaviour
    {
        public int fehlerCount = 1;
        private static List<FindeDenFehler> fehler = new List<FindeDenFehler>();
        [Tooltip("Only needed if these should be included in the E-Mail.")]

        public static int foundObjects;
        /// <summary>
        /// Start the mini game.
        /// </summary>
        private void Start()
        {

            var temp = fehler;
            for (int i = 0; i < fehlerCount; i++)
            {
                var number = Random.Range(0, temp.Count);
                temp[number].StartFehler();
                temp.RemoveAt(number);
            }
        }

        /// <summary>
        /// Remove fehler.
        /// </summary>
        /// <param name="findeDenFehler"></param>
        internal static void Remove(FindeDenFehler findeDenFehler)
        {
            foundObjects++;
            fehler.Remove(findeDenFehler);
        }

        /// <summary>
        /// Add fehler.
        /// </summary>
        /// <param name="findeDenFehler"></param>
        internal static void Register(FindeDenFehler findeDenFehler)
        {
            fehler.Add(findeDenFehler);
        }
    }
}
