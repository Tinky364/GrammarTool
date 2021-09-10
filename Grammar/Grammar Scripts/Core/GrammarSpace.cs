using System.Text;
using UnityEngine;

namespace Grammar.Core
{
    /// <summary>
    /// It helps to the Grammar Executor component to spawn objWords inside the rectangular prism.
    /// </summary>
    [RequireComponent(typeof(GrammarExecutor))]
    [AddComponentMenu("Grammar Space")]
    public class GrammarSpace : MonoBehaviour
    {
        #region INSPECTOR VARIABLES

        [SerializeField, Min(0), Tooltip("A position inside the rectangular prism space. The first GameObject will spawn at this position. The position cannot be at outside the rectangular prism space.")]
        private Vector3 spawnStartLocalPosition;

        [SerializeField, Tooltip("The position of the rectangular prism space. Position: -X, -Y, -Z")]
        private Vector3 positionOfSpace;

        [SerializeField, Min(0), Tooltip("The size of the rectangular prism space.")]
        private Vector3 sizeOfSpace;

        [SerializeField, Tooltip("It reduces execution time of a complex structures by created with same objects.")]
        private bool doNotCheckSameErrorForOtherObjects;

        #endregion

        public bool DoNotCheckSameErrorForOtherObjects => doNotCheckSameErrorForOtherObjects;

        // world position of the spawnStartPosition
        public Vector3 SpawnStartPosition => positionOfSpace + spawnStartLocalPosition;

        // center position of the rectangular prism space. 
        private Vector3 CenterPositionOfSpace => new Vector3(
            positionOfSpace.x + sizeOfSpace.x / 2f, 
            positionOfSpace.y + sizeOfSpace.y / 2f,
            positionOfSpace.z + sizeOfSpace.z / 2f);

        private readonly StringBuilder errorString = new StringBuilder();

        /// <summary>
        /// It checks whether the objTile is inside the rectangular prism space or not.
        /// </summary>
        /// <param name="objTile">Check an error for this objTile.</param>
        /// <returns>If an error occurs, returns true.</returns>
        public bool CheckSpaceError(ObjTile objTile)
        {
            // check whether the first position is inside of space or not.
            Vector3 position = objTile.transform.position;
            if (!IsPositionInsideCube(position, CenterPositionOfSpace, sizeOfSpace))
            {
                errorString.Clear();
                errorString.Append($"GameObject: {objTile.name} => position:{position} is not within space!");
                return true;
            }

            // if the first position is inside of space, check the second position.
            position = objTile.transform.TransformPoint(objTile.Size.x, objTile.Size.y, objTile.Size.z);
            if (!IsPositionInsideCube(position, CenterPositionOfSpace, sizeOfSpace))
            {
                errorString.Clear();
                errorString.Append($"GameObject: {objTile.name} => position:{position} is not within space!");
                return true;
            }

            return false;
        }

        /// <summary>
        /// It checks whether the position is inside the rectangular prism space or not.
        /// </summary>
        /// <param name="position">Position to be checked.</param>
        /// <param name="centerOfSpace">Center position of rectangular prism space.</param>
        /// <param name="sizeOfSpace">Size of rectangular prism space.</param>
        /// <returns>If position is inside the rectangular prism space, returns true.</returns>
        private static bool IsPositionInsideCube(Vector3 position, Vector3 centerOfSpace, Vector3 sizeOfSpace)
        {
            Vector3 centerToPointVector = centerOfSpace - position; // vector from the center of space to the position.
            float proXMag = Vector3.Project(centerToPointVector, Vector3.right).magnitude; // projection vector magnitude on X surface of rectangular prism
            float proYMag = Vector3.Project(centerToPointVector, Vector3.up).magnitude; // projection vector magnitude on Y surface of rectangular prism
            float proZMag = Vector3.Project(centerToPointVector, Vector3.forward).magnitude; // projection vector magnitude on Z surface of rectangular prism

            // check whether the magnitude of the vector`s projections are greater than their corresponding surface`s length or not.
            // if all of them are smaller than their surface magnitude, the position is inside of rectangular prism space.
            if (2 * proXMag <= sizeOfSpace.x + 0.2f && 2 * proYMag <= sizeOfSpace.y + 0.2f && 2 * proZMag <= sizeOfSpace.z + 0.2f)
                return true;

            return false;
        }

        /// <summary>
        /// It prints the space error to the console.
        /// </summary>
        /// <param name="objTypeWord">The word causing a space error.</param>
        public void PrintError(Word objTypeWord)
        {
            Debug.LogWarning($" ^^ Space Error Occurred: {errorString} ^^", objTypeWord.objWord.gameObject);
        }
    }
}

