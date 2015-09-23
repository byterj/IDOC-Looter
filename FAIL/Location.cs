namespace FAIL
{
    class Location
    {
        private int _x, _y;
        public int X
        {
            get { return _x; }
            set { _x = value; }
        }
        public int Y
        {
            get { return _y; }
            set { _y = value; }
        }
        public Location()
        {
            //parameterless constructor for XML serialization
        }
        public Location(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }
}
