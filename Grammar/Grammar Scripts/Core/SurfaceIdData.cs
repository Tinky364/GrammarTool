using System.Collections.Generic;
using Grammar.Helper;
using UnityEngine;

namespace Grammar.Core
{
    [CreateAssetMenu(fileName = "Surface Id Data", menuName = "Grammar/Surface Id Data", order = 3)]
    public class SurfaceIdData : ScriptableObject, ISerializationCallbackReceiver
    {
        public int xPosId;
        public int zNegId;
        public int xNegId;
        public int zPosId;
        public int yPosId;
        public int yNegId;

        public Dictionary<SurfaceDirection, int> faceIdDict;

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            faceIdDict = new Dictionary<SurfaceDirection, int>()
            {
                {SurfaceDirection.XPos, xPosId},
                {SurfaceDirection.ZNeg, zNegId},
                {SurfaceDirection.XNeg, xNegId},
                {SurfaceDirection.ZPos, zPosId},
                {SurfaceDirection.YPos, yPosId},
                {SurfaceDirection.YNeg, yNegId}
            };
        }
    }
}
