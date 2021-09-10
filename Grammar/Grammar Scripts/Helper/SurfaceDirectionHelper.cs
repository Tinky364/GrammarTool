using System.Collections.Generic;
using UnityEngine;

namespace Grammar.Helper
{
    // DO NOT change order and values
    public enum SurfaceDirection
    {
        Null = -1,
        XPos,
        ZNeg,
        XNeg,
        ZPos,
        YPos,
        YNeg
    }

    public enum SurfaceChose
    {
        XZ,
        X,
        Y,
        Z
    }

    public static class SurfaceDirectionHelper
    {
        public static List<SurfaceDirection> AvailableSurfaceDirections(SurfaceChose chose)
        {
            switch (chose)
            {
                case SurfaceChose.XZ:
                    return new List<SurfaceDirection>()
                    {SurfaceDirection.XPos, SurfaceDirection.ZNeg,
                    SurfaceDirection.XNeg, SurfaceDirection.ZPos};
                case SurfaceChose.X:
                    return new List<SurfaceDirection>() { SurfaceDirection.XPos, SurfaceDirection.XNeg };
                case SurfaceChose.Z:
                    return new List<SurfaceDirection>() { SurfaceDirection.ZPos, SurfaceDirection.ZNeg };
                case SurfaceChose.Y:
                    return new List<SurfaceDirection>() { SurfaceDirection.YPos, SurfaceDirection.YNeg };
                default:
                    return new List<SurfaceDirection>()
                    {SurfaceDirection.XPos, SurfaceDirection.ZNeg,
                    SurfaceDirection.XNeg, SurfaceDirection.ZPos};
            }
        }

        public static int RotationForSDToSD(SurfaceDirection current, SurfaceDirection wanted, SurfaceChose chose)
        {
            int dist = (int)current - (int)wanted;

            // Y
            if (chose == SurfaceChose.Y)
            {
                return dist == 0 ? 0 : 180;
            }

            // XZ
            switch (dist)
            {
                case 1:
                case -3:
                    return 270;
                case 2:
                case -2:
                    return 180;
                case 3:
                case -1:
                    return 90;
                case 0:
                    return 0;
                default:
                    Debug.LogWarning("Problematic Rotation");
                    return 0;
            }
        }

        public static SurfaceDirection LocalToWorld(SurfaceDirection localDir, int rotation)
        {
            if (rotation == 0)
                return localDir;

            switch (localDir)
            {
                case SurfaceDirection.YPos:
                    return SurfaceDirection.YNeg;
                case SurfaceDirection.YNeg:
                    return SurfaceDirection.YPos;
                default:
                    {
                        int i = rotation / 90;
                        return (SurfaceDirection)(((int)localDir + i) % 4);
                    }
            }
        }

        public static SurfaceDirection OppositeOfSD(SurfaceDirection surfaceDirection)
        {
            switch (surfaceDirection)
            {
                case SurfaceDirection.XPos:
                    return SurfaceDirection.XNeg;
                case SurfaceDirection.ZNeg:
                    return SurfaceDirection.ZPos;
                case SurfaceDirection.XNeg:
                    return SurfaceDirection.XPos;
                case SurfaceDirection.ZPos:
                    return SurfaceDirection.ZNeg;
                case SurfaceDirection.YPos:
                    return SurfaceDirection.YNeg;
                case SurfaceDirection.YNeg:
                    return SurfaceDirection.YPos;
                default:
                    Debug.LogWarning("The Surface Direction you want to find the opposite is null!");
                    return surfaceDirection;
            }
        }
    }
}
