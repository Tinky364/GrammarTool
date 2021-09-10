using UnityEngine;
using UnityEngine.Serialization;

namespace Grammar.Core
{
    /// <summary>
    /// It helps to the Grammar Executor component to execute the starting sentence more than once.
    /// </summary>
    [RequireComponent(typeof(GrammarExecutor))]
    [AddComponentMenu("Grammar Iterator")]
    public class GrammarIterator : MonoBehaviour
    {
        #region INSPECTOR VARIABLES

        [Min(1), Tooltip("The variable indicates how many times the Grammar Executor component will execute the starting sentence.")]
        public int executeCount = 1;

        [SerializeField, Tooltip("The position of the rectangular prism space. Position: -X, -Y, -Z")]
        private Vector3 positionOfSpace;

        [SerializeField, Tooltip("The size of the rectangular prism space.")]
        private Vector3 sizeOfSpace;

        #endregion

        /// <summary>
        /// It returns a random position from within the area. 
        /// </summary>
        /// <returns>A random position from within the area.</returns>
        public Vector3 ReturnPositionFromArea()
        {
            float x = Random.Range(positionOfSpace.x, positionOfSpace.x + sizeOfSpace.x);
            float y = Random.Range(positionOfSpace.y, positionOfSpace.y + sizeOfSpace.y);
            float z = Random.Range(positionOfSpace.z, positionOfSpace.z + sizeOfSpace.z);
            return new Vector3(x, y, z);
        }
    }
}
