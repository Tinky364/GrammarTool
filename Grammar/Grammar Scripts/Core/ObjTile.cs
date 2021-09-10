using Grammar.Editor;
using Grammar.Helper;
using UnityEngine;

namespace Grammar.Core
{
    /// <summary>
    /// Extended version of objWord. If a spawned object will be placed on surfaces of other spawned objects,
    /// both spawned objects must contain ObjTile component. 
    /// </summary>
    [AddComponentMenu("ObjTile")]
    public class ObjTile : ObjWord
    {
        [SerializeField, Min(0), Tooltip("The size of the tile object.")]
        private Vector3 size;

        [Header("Surface Offsets")]

        [SerializeField, Min(0), Tooltip("X: Starter local position of X positive surface. Y: End local position of X positive surface.")]
        private Vector2 xPosSurfaceOffset;

        [SerializeField, Min(0), Tooltip("X: Starter local position of Z negative surface. Y: End local position of Z negative surface.")]
        private Vector2 zNegSurfaceOffset;

        [SerializeField, Min(0), Tooltip("X: Starter local position of X negative surface. Y: End local position of X negative surface.")]
        private Vector2 xNegSurfaceOffset;

        [SerializeField, Min(0), Tooltip("X: Starter local position of Z positive surface. Y: End local position of Z positive surface.")]
        private Vector2 zPosSurfaceOffset;

        public Vector3 Size => size;

        private Vector3 XPosPivot => transform.TransformPoint(new Vector3(size.x, 0, xPosSurfaceOffset.x));
        private Vector3 ZNegPivot => transform.TransformPoint(new Vector3(zNegSurfaceOffset.x, 0, 0));
        private Vector3 XNegPivot => transform.TransformPoint(new Vector3(0, 0, xNegSurfaceOffset.x));
        private Vector3 ZPosPivot => transform.TransformPoint(new Vector3(zPosSurfaceOffset.x, 0, size.z));
        private Vector3 YPosPivot => transform.TransformPoint(new Vector3(0, size.y, 0));
        private Vector3 YNegPivot => transform.TransformPoint(new Vector3(0, 0, 0));

        private Vector3 XPosRevertedPivot => transform.TransformPoint(new Vector3(size.x, 0, xPosSurfaceOffset.y));
        private Vector3 ZNegRevertedPivot => transform.TransformPoint(new Vector3(zNegSurfaceOffset.y, 0, 0));
        private Vector3 XNegRevertedPivot => transform.TransformPoint(new Vector3(0, 0, xNegSurfaceOffset.y));
        private Vector3 ZPosRevertedPivot => transform.TransformPoint(new Vector3(zPosSurfaceOffset.y, 0, size.z));

        // it is for ObjGenerator. remove HideInInspector if you wanna use it.
        [Expandable, HideInInspector] public SurfaceIdData surfaceIdData;
        [HideInInspector] public bool[] surfaceCheck = new bool[6];
        //

        /// <summary>
        /// It is called when ObjTile spawns.
        /// </summary>
        public override void OnSpawn()
        {
            
        }

        #region METHOD WORDS

        /// <summary>
        /// It calls corresponding methods with methodWord.
        /// </summary>
        /// <param name="methodWord">Method type word that will call corresponding method.</param>
        /// <returns>If it finds the corresponding method, returns true.</returns>
        public override bool CallCorrespondingMethod(Word methodWord)
        {
            bool called = false;
            switch (methodWord.Name)
            {
                case "Place":
                    Place(methodWord);
                    called = true;
                    break;
                default: // if it cannot find the method in here, search in the parent class' methods
                    called = base.CallCorrespondingMethod(methodWord);
                    break;
            }
            return called;
        }

        /// <summary>
        /// It places objTile's surface(second parameter in the methodWord) to the parent objTile's surface(first parameter in the methodWord).
        /// </summary>
        /// <param name="methodWord">The first parameter is the surface of the parent objTile.
        /// The second parameter is the surface of the current objTile.</param>
        protected void Place(Word methodWord)
        {
            if (!methodWord.Parent.objWord.TryGetComponent(out ObjTile parentObjTile)) return;

            // get parameters
            string args = methodWord.Args;
            string[] parameters = args.Split(',');
            bool isY = args.Contains("Y");
            SurfaceDirection placeNextToSD = parameters[0].StringToSurfaceDirection();
            SurfaceDirection thisObjSD = parameters[1].StringToSurfaceDirection();
            //

            SurfaceChose surfaceChose = isY ? SurfaceChose.Y : SurfaceChose.XZ;
            Rotate(parentObjTile, placeNextToSD, thisObjSD, surfaceChose);

            Vector3 targetPos = CalculateTargetPosition(parentObjTile, placeNextToSD, thisObjSD);
            Vector3 surfacePos = FindRelatedSurfacePosition(thisObjSD);
            Move(targetPos, surfacePos);
        }

        #endregion

        /// <summary>
        /// It places objTile's surface to the placeNextTo objTile's surface.
        /// </summary>
        /// <param name="placeNextTo">The object will spawn next to this placeNextTo object.</param>
        /// <param name="placeNextToSD">The surface of the placeNextTo object.</param>
        /// <param name="thisObjSD">This object surface.</param>
        /// <param name="surfaceChose">Surface chose type.</param>
        public void Place(ObjTile placeNextTo, SurfaceDirection placeNextToSD, SurfaceDirection thisObjSD, SurfaceChose surfaceChose)
        {
            Rotate(placeNextTo, placeNextToSD, thisObjSD, surfaceChose);

            Vector3 targetPos = CalculateTargetPosition(placeNextTo, placeNextToSD, thisObjSD);
            Vector3 relatedSurfacePos = FindRelatedSurfacePosition(thisObjSD);

            Move(targetPos, relatedSurfacePos);
        }

        /// <summary>
        /// It rotates the object accordingly to the placeNextTo object rotation and the target surface.
        /// </summary>
        /// <param name="placeNextTo">The object will spawn next to this placeNextTo object.</param>
        /// <param name="placeNextToSD">The surface of the placeNextTo object.</param>
        /// <param name="thisObjSD">This object surface.</param>
        /// <param name="surfaceChose">Surface chose type.</param>
        private void Rotate(ObjTile placeNextTo, SurfaceDirection placeNextToSD, SurfaceDirection thisObjSD, SurfaceChose surfaceChose)
        {
            SurfaceDirection targetSurface = SurfaceDirectionHelper.OppositeOfSD(placeNextToSD);
            float rotNeeded = SurfaceDirectionHelper.RotationForSDToSD(thisObjSD, targetSurface, surfaceChose);
            float placeNextToRotY = (float)placeNextTo.CurrentRotation(Axis.Y);
            transform.Rotate(0, placeNextToRotY + rotNeeded, 0);
        }

        /// <summary>
        /// It moves the object's relatedPosition to the targetWorldPosition.
        /// </summary>
        /// <param name="targetWorldPosition"></param>
        /// <param name="relatedPosition"></param>
        private void Move(Vector3 targetWorldPosition, Vector3 relatedPosition)
        {
            transform.position += (targetWorldPosition - relatedPosition);
        }

        /// <summary>
        /// It calculates the target position to place the object to the next of the placeNextTo object surface.
        /// </summary>
        /// <param name="placeNextTo">The object will spawn next to this placeNextTo object.</param>
        /// <param name="placeNextToSD">The surface of the placeNextTo object.</param>
        /// <param name="thisObjSD">This object surface.</param>
        /// <returns></returns>
        private static Vector3 CalculateTargetPosition(ObjTile placeNextTo, SurfaceDirection placeNextToSD, SurfaceDirection thisObjSD)
        {
            Vector3 pos = Vector3.zero;
            switch (placeNextToSD)
            {
                case SurfaceDirection.XPos:
                    if (thisObjSD == SurfaceDirection.XPos)
                        pos = placeNextTo.XPosRevertedPivot;
                    else
                        pos = placeNextTo.XPosPivot;
                    break;
                case SurfaceDirection.ZNeg:
                    if (thisObjSD == SurfaceDirection.ZNeg)
                        pos = placeNextTo.ZNegRevertedPivot;
                    else
                        pos = placeNextTo.ZNegPivot;
                    break;
                case SurfaceDirection.XNeg:
                    if (thisObjSD == SurfaceDirection.XNeg || thisObjSD == SurfaceDirection.ZPos)
                        pos = placeNextTo.XNegRevertedPivot;
                    else
                        pos = placeNextTo.XNegPivot;
                    break;
                case SurfaceDirection.ZPos:
                    if (thisObjSD == SurfaceDirection.ZPos || thisObjSD == SurfaceDirection.XNeg)
                        pos = placeNextTo.ZPosRevertedPivot;
                    else
                        pos = placeNextTo.ZPosPivot;
                    break;
                case SurfaceDirection.YPos:
                    pos = placeNextTo.YPosPivot;
                    break;
                case SurfaceDirection.YNeg:
                    pos = placeNextTo.YNegPivot;
                    break;
            }

            return pos;
        }

        private Vector3 FindRelatedSurfacePosition(SurfaceDirection sd)
        {
            Vector3 relatedSurfacePosition = Vector3.zero;
            switch (sd)
            {
                case SurfaceDirection.XPos:
                    relatedSurfacePosition = XPosPivot;
                    break;
                case SurfaceDirection.ZPos:
                    relatedSurfacePosition = ZPosPivot;
                    break;
                case SurfaceDirection.XNeg:
                    relatedSurfacePosition = XNegPivot;
                    break;
                case SurfaceDirection.ZNeg:
                    relatedSurfacePosition = ZNegPivot;
                    break;
                case SurfaceDirection.YPos:
                    relatedSurfacePosition = transform.position + new Vector3(0, size.y, 0);
                    break;
                case SurfaceDirection.YNeg:
                    relatedSurfacePosition = transform.position;
                    break;
            }
            return relatedSurfacePosition;
        }

        private enum Axis
        {
            X, Y, Z
        }
        private enum Rotation
        {
            rot0 = 0,
            rot90 = 90,
            rot180 = 180,
            rot270 = 270
        }
        private Rotation CurrentRotation(Axis axis)
        {
            float rot = 0;
            switch (axis)
            {
                case Axis.X:
                    rot = transform.localEulerAngles.x;
                    break;
                case Axis.Y:
                    rot = transform.localEulerAngles.y;
                    break;
                case Axis.Z:
                    rot = transform.localEulerAngles.z;
                    break;
            }
            rot %= 360;
            if (rot < 0f)
                rot += 360f;

            Rotation rotation = Rotation.rot0;

            if (Mathf.Approximately(rot, (int)Rotation.rot0))
                rotation = Rotation.rot0;
            else if (Mathf.Approximately(rot, (int)Rotation.rot90))
                rotation = Rotation.rot90;
            else if (Mathf.Approximately(rot, (int)Rotation.rot180))
                rotation = Rotation.rot180;
            else if (Mathf.Approximately(rot, (int)Rotation.rot270))
                rotation = Rotation.rot270;

            return rotation;
        }
    }
}
