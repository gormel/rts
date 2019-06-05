namespace Assets.Core.Map
{
    class MapData : IMapData
    {
        public int Width { get; }
        public int Length { get; }

        public float[,] mHeights;
        public MapObject[,] mObjects;

        public MapData(int width, int length, float[,] data, MapObject[,] objects)
        {
            Width = width;
            Length = length;
            mHeights = data;
            mObjects = objects;
        }

        public float GetHeightAt(int x, int y)
        {
            return mHeights[x, y];
        }

        public MapObject GetMapObjectAt(int x, int y)
        {
            return mObjects[x, y];
        }
    }
}