namespace FAIL
{
    class Settings
    {
        private uint _homeRunebookID;
        private int _homeRuneNumber;

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

        public Settings()
        {

        }

    }
}
