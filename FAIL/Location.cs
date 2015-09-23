namespace FAIL
{
    public class Location
    {
        #region Vars

        private int _x, _y;

        #endregion Vars

        #region Properties

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

        #endregion Properties

        #region Constructs

        public Location()
        {
            //parameterless constructor for XML serialization
        }

        public Location(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        #endregion Constructs
    }
}