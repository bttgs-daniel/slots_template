using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    27.08.2020 - first
    20.09.2021 - InstantiatePrefabAtPosition()
 */
namespace Mkey
{
    public class PrefabInstantiator : MonoBehaviour
    {
        [SerializeField]
        private GameObject prefab;

        public void InstantiatePrefab()
        {
            if (prefab)
            {
                Instantiate(prefab, transform);
            }
        }

        public void InstantiatePrefabAtPosition()
        {
            if (prefab)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }
    }
}
