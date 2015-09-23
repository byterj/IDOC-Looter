using System.Collections.Generic;

namespace FAIL
{
    public class Rail
    {
        #region Vars

        private string _name;
        private uint _runebookID;
        private int _runeNumber;
        private List<Location> _path;

        #endregion Vars

        #region Properties

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public uint RunebookID
        {
            get { return _runebookID; }
            set { _runebookID = value; }
        }

        public int RuneNumber
        {
            get { return _runeNumber; }
            set { _runeNumber = value; }
        }

        public List<Location> Path
        {
            get { return _path; }
            set { _path = value; }
        }

        #endregion Properties

        #region Constructs

        public Rail()
        {
            //parameterless constructor for XML serialization
        }

        public Rail(string Name, uint RunebookID, int RuneNumber)
        {
            this.Name = Name;
            this.RunebookID = RunebookID;
            this.RuneNumber = RuneNumber;
            this.Path = new List<Location>();
        }

        #endregion Constructs
    }
}