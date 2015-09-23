using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace FAIL
{
    class House
    {
        private uint _id;
        private string _name;
        private string _owner;
        private string _condition;
        private string _tooltip;
        private Location _location;
        private string _rail;
        private DateTime _added;
        private DateTime _checked;

        public uint ID
        {
            get { return _id; }
            set { _id = value; }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public string Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }
        public string Condition
        {
            get { return _condition; }
            set { _condition = value; }
        }
        public string Tooltip
        {
            get { return _tooltip; }
            set { _tooltip = value; }
        }
        public Location Location
        {
            get { return _location; }
            set { _location = value; }
        }
        public string Rail
        {
            get { return _rail; }
            set { _rail = value; }
        }
        [XmlIgnore]
        public DateTime Added
        {
            get { return _added; }
            set { _added = value; }
        }
        [XmlElement("Added")]
        public string DateAdded
        {
            get { return this.Added.ToString("yyyy-MM-dd HH:mm:ss"); }
            set { this.Added = DateTime.Parse(value); }
        }
        [XmlIgnore]
        public DateTime Checked
        {
            get { return _checked; }
            set { _checked = value; }
        }
        [XmlElement("Checked")]
        public string DateChecked
        {
            get { return this.Checked.ToString("yyyy-MM-dd HH:mm:ss"); }
            set { this.Checked = DateTime.Parse(value); }
        }

        public House()
        {

        }
        public House(uint ID, string Tooltip, string Rail, int X, int Y)
        {
            this.ID = ID;
            this.Tooltip = Tooltip;
            this.Location = new Location(X, Y);
            this.Rail = Rail;

            string[] _text = Tooltip.Split('|');

            if (Tooltip.Length > 11)
                for (int x = 0; x < _text.Count(); x++)
                {
                    if (_text[x].Remove(6) == "Name: ")
                        this.Name = _text[x].Remove(0, 6);
                    else if (_text[x].Remove(7) == "Owner: ")
                        this.Owner = _text[x].Remove(0, 7);
                    if (_text[x].Length > 11)
                        if (_text[x].Remove(11) == "Condition: ")
                            this.Condition = _text[x].Remove(0, 11);
                        else
                            this.Condition = "Refreshed";
                }
            else
            {
                this.Name = "Unknown";
                this.Owner = "Unknown";
                this.Condition = "Unknown";
            }

            this.Added = DateTime.Now;
            this.Checked = DateTime.Now;
        }
        public void BuildSet()
        {

        }
    }
}
