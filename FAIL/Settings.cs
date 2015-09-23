namespace FAIL
{
    public class Settings
    {
        #region Vars

        private uint _homeRunebookID;
        private int _homeRuneNumber;

        #endregion Vars

        #region Properties

        public uint HomeRunebookID
        {
            get { return _homeRunebookID; }
            set { _homeRunebookID = value; }
        }

        public int HomeRuneNumber
        {
            get { return _homeRuneNumber; }
            set { _homeRuneNumber = value; }
        }

        #endregion Properties

        #region Constructs

        public Settings()
        {
        }

        #endregion Constructs
    }
}