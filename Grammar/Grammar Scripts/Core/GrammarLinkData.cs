using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Grammar.Core
{
    [Serializable]
    public struct ObjWordLink
    {
        public string wordName; // word name
        public ObjWord[] objWord; // prefabs
    }

    /// <summary>
    /// To create = Right click on Project window -> Create/Grammar/Grammar Link Data.
    /// It holds words and their linked objWord prefabs data of one grammar rule set.
    /// </summary>
    [CreateAssetMenu(fileName = "Grammar Link Data", menuName = "Grammar/Grammar Link Data", order = 2)]
    public class GrammarLinkData : ScriptableObject
    {
        [SerializeField, Tooltip("List of all links of words with their ObjWord prefabs.")]
        private List<ObjWordLink> links = new List<ObjWordLink>();

        /// <summary>
        /// It searches the links list to find the word.
        /// If it finds returns the corresponding prefab with the index number.
        /// If the index number is equal -1, it returns a random prefab.
        /// </summary>
        /// <param name="word">Searching grammar link data for finding this word.</param>
        /// <param name="prefabIndex">Prefab index of the word. Type -1 for a random index</param>
        /// <returns>Corresponding ObjWord prefab with the word and the index</returns>
        public ObjWord ReturnPrefab(Word word, int prefabIndex)
        {
            // compare every wordName in the links list.
            foreach (ObjWordLink link in links)
            {
                if (!word.Name.Equals(link.wordName)) continue; // if not equal, continue comparing.

                // if the index number is equal -1, return a random prefab.
                // else return the prefab with the index number.
                return prefabIndex == -1 ? link.objWord[Random.Range(0, link.objWord.Length)] : link.objWord[prefabIndex];
            }
            return null;
        }
    }
}
