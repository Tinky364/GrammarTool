using Grammar.Helper;

namespace Grammar.Core
{
    public struct ProperTileObject
    {
        public ProperTileObject(ObjTile prefab, SurfaceDirection properSurface, int prefabIndex, SurfaceDirection placeNextToSD)
        {
            this.prefab = prefab;
            this.properSurface = properSurface;
            this.prefabIndex = prefabIndex;
            this.placeNextToSD = placeNextToSD;
        }

        public ObjTile prefab;
        public SurfaceDirection properSurface;
        public int prefabIndex;
        public SurfaceDirection placeNextToSD;
    }
}
