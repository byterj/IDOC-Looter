using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using ScriptSDK;
using ScriptSDK.Mobiles;
using ScriptSDK.Engines;
using ScriptDotNet2;
using ScriptDotNet2.Data;
using ScriptAPI;
using ScriptSDK.Attributes;

namespace FAIL
{
    public partial class FAIL : Form
    {

        #region Vars
        private bool BuildingRail, BuildingRailPause, FAILFormClosing, Searching;
        private string RailFilePath, HouseFilePath, SettingsFilePath;
        private Rail SelectedRail, CurrentRail;
        private List<Rail> SelectedRails = new List<Rail>();
        private List<uint> Runebooks = new List<uint>();
        private Object thisLock = new Object();
        private DataSet DataSetHouses = new DataSet();
        [XmlArray]
        private List<Rail> Rails = new List<Rail>();
        [XmlArray]
        List<House> Houses = new List<House>();
        private Settings Settings = new Settings();
        #endregion

        #region Form Functions
        public FAIL()
        {
            InitializeComponent();

            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                System.Deployment.Application.ApplicationDeployment ad =
                    System.Deployment.Application.ApplicationDeployment.CurrentDeployment;
                this.Text = String.Format("FAIL (Fully Automated IDOC Looter) {0}", ad.CurrentVersion);
            }

            RailFilePath = Directory.GetCurrentDirectory() + "\\rails.xml";
            HouseFilePath = Directory.GetCurrentDirectory() + "\\houses.xml";
            SettingsFilePath = Directory.GetCurrentDirectory() + "\\settings.xml";

        }
        private void FAIL_Load(object sender, EventArgs e)
        {
            try
            {
                if (!Profile.IsConnected)
                    MessageBox.Show("Please connect a profile in Stealth and try again.");

                var Player = PlayerMobile.GetPlayer();
                Player.Backpack.Use();
                /*
                LoadRails();
                LoadHouses();
                LoadSettings();
                */
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message.ToString());
            }

        }
        private void FAIL_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (BuildingRail)
            {
                workerBuildRail.CancelAsync();
                this.Enabled = false;
                FAILFormClosing = true;
                e.Cancel = true;
            }

            //base.OnFormClosing(e);
        }
        private void FAIL_FormClosed(object sender, FormClosedEventArgs e)
        {
        }
        private void btnAddRunebook_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show("Select runebook to add...");
                Item _itemRunebook = Target.RequestTarget();

                Runebooks.Add(_itemRunebook.ID);

                listRunebooks.DataSource = null;
                listRunebooks.DataSource = Runebooks;
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message.ToString());
            }
        }
        private void btnRemoveRunebook_Click(object sender, EventArgs e)
        {
            try
            {
                if (listRunebooks.SelectedIndex == -1)
                    MessageBox.Show("There was no runebook selected to remove.");
                else
                {
                    var _runebookID = Convert.ToUInt32(listRunebooks.SelectedItem.ToString());

                    if (Rails.Any(x => x.RunebookID == _runebookID))
                        MessageBox.Show("Runebook is being used in an existed rail, remove the rail first.");
                    else
                    {
                        Runebooks.RemoveAt(listRunebooks.SelectedIndex);

                        listRunebooks.DataSource = null;
                        listRunebooks.DataSource = Runebooks;
                    }
                }
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message.ToString());
            }
        }
        private void btnAddRail_Click(object sender, EventArgs e)
        {
            try
            {
                if (listRunebooks.SelectedIndex == -1)
                    MessageBox.Show("Select a runebook this rail be referenced to, and try again.");
                else if (Convert.ToInt32(txtRuneNumber.Text) < 0 || Convert.ToInt32(txtRuneNumber.Text) > 16)
                    MessageBox.Show("Rune number can't be less than 0 or greater than 16.");
                else if (txtRailName == null)
                    MessageBox.Show("Please enter a name for the rail.");
                else
                {
                    Rail _rail = new Rail(txtRailName.Text, Convert.ToUInt32(listRunebooks.SelectedValue.ToString()), Convert.ToUInt16(txtRuneNumber.Text));
                    Rails.Add(_rail);

                    List<string> _railNames = new List<string>();

                    foreach (Rail _r in Rails)
                    {
                        _railNames.Add(_r.Name);
                    }

                    listRails.DataSource = null;
                    listRails.DataSource = _railNames;
                }
            }
            catch (Exception x)
            {
                MessageBox.Show(x.StackTrace.ToString());
            }
        }
        private void btnRemoveRail_Click(object sender, EventArgs e)
        {

            if (listRails.SelectedIndex == -1)
                MessageBox.Show("Select rails to remove.");
            else
            {
                try
                {
                    SelectedRails.Clear();

                    for (int x = 0; x < listRails.Items.Count; x++)
                    {
                        if (listRails.GetSelected(x) == true)
                            Rails.Remove(Rails[x]);
                    }
                    
                    List<string> _railNames = new List<string>();

                    foreach (Rail _r in Rails)
                    {
                        _railNames.Add(_r.Name);
                    }

                    listRails.DataSource = null;
                    listRails.DataSource = _railNames;
                }
                catch (Exception x)
                {
                    MessageBox.Show(x.Message.ToString());
                }
            }

        }
        private void btnStartRail_Click(object sender, EventArgs e)
        {

            if (listRails.SelectedIndex == -1)
                MessageBox.Show("Select a rail to start building.");
            else
            {
                try
                {
                    SelectedRail = Rails[listRails.SelectedIndex];
                    BuildingRail = true;
                    BuildingRailPause = false;
                    workerBuildRail.RunWorkerAsync();
                }
                catch (Exception x)
                {
                    MessageBox.Show(x.Message.ToString());
                }
            }

                
        }
        private void btnStopRail_Click(object sender, EventArgs e)
        {
            BuildingRail = false;
            workerBuildRail.CancelAsync();
        }
        private void btnSaveRails_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(RailFilePath))
                    CreateXMLFile(RailFilePath);

                txtStatus.AppendLine("Saving rail settings...");
                XmlSerializer _serializer = new XmlSerializer(typeof(List<Rail>));
                using (TextWriter _writer = new StreamWriter(RailFilePath))
                {
                    _serializer.Serialize(_writer, Rails);
                }
                txtStatus.AppendLine("Rail settings saved!");
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message.ToString());
            }
        }
        private void btnLoadRails_Click(object sender, EventArgs e)
        {
            try
            {
                XmlSerializer _deserializer = new XmlSerializer(typeof(List<Rail>));
                TextReader _reader = new StreamReader(RailFilePath);
                object obj = _deserializer.Deserialize(_reader);
                Rails = (List<Rail>)obj;
                _reader.Close();

                List<string> _railNames = new List<string>();

                foreach (Rail _r in Rails)
                {
                    _railNames.Add(_r.Name);
                    Runebooks.Add(_r.RunebookID);
                }

                Runebooks = Runebooks.Distinct().ToList();

                listRunebooks.DataSource = null;
                listRunebooks.DataSource = Runebooks;

                listRails.DataSource = null;
                listRails.DataSource = _railNames;
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message.ToString());
            }
        }
        private void btnResetRails_Click(object sender, EventArgs e)
        {

        }
        private void btnStartSearch_Click(object sender, EventArgs e)
        {
            if (listRails.SelectedIndex == -1)
                MessageBox.Show("Select rails to start searching.");
            else
            {
                try
                {
                    SelectedRails.Clear();

                    for (int x = 0; x < listRails.Items.Count; x++)
                    {
                        if (listRails.GetSelected(x) == true)
                            SelectedRails.Add(Rails[x]);
                    }

                    Searching = true;
                    workerSearch.RunWorkerAsync();
                    workerCheckHouses.RunWorkerAsync();
                }
                catch (Exception x)
                {
                    MessageBox.Show(x.Message.ToString());
                }
            }
        }
        private void btnStopSearch_Click(object sender, EventArgs e)
        {
            Searching = false;
            workerSearch.CancelAsync();
            workerCheckHouses.CancelAsync();
        }
        private void btnSaveHouses_Click(object sender, EventArgs e)
        {
            SaveHouses();
        }
        private void btnLoadHouses_Click(object sender, EventArgs e)
        {
            try
            {
                XmlSerializer _deserializer = new XmlSerializer(typeof(List<House>));
                TextReader _reader = new StreamReader(HouseFilePath);
                object obj = _deserializer.Deserialize(_reader);
                Houses = (List<House>)obj;
                _reader.Close();

                List<string> _houseNames = new List<string>();

                foreach (House _house in Houses)
                {
                    _houseNames.Add(_house.Tooltip);
                }

                UpdateGridView();

                txtHouseStatus.AppendLine("Houses loaded!");
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message.ToString());
            }
        }
        private void btnDebugGetID_Click(object sender, EventArgs e)
        {
            Item _result = Target.RequestTarget();
            MessageBox.Show(_result.ID.ToString() + " " + _result.Type.ToString());

            List<ClilocItemRec> _properties = Stealth.Default.GetClilocRec(_result.ID);

            txtDebugStatus.AppendLine(_properties.Count.ToString());

            foreach (ClilocItemRec _property in _properties)
            {
                txtDebugStatus.AppendLine(_property.ClilocID.ToString());
                foreach (String _param in _property.Params)
                {
                    txtDebugStatus.AppendLine(_param.ToString());
                }
            }

            txtDebugStatus.AppendLine(_result.Tooltip);


        }
        private void btnDebugSearchItems_Click(object sender, EventArgs e)
        {
            try
            {
            ScriptLogger.Initialize();
            ScriptLogger.LogToStealth = true;
            Scanner.Range = 20;
            Scanner.VerticalRange = 20;

            var Player = PlayerMobile.GetPlayer();
            var results = Scanner.Find<HouseSigns>(0x0, false);
            List<HouseSigns> list = results.Select(x => x.Cast<HouseSigns>()).ToList();

            string _textToSend = "";


            txtDebugStatus.AppendLine(list.Count.ToString());
            foreach (ScriptSDK.Items.Item _item in list)
            {
                string[] _text = _item.Tooltip.Split('|');
                txtDebugStatus.AppendLine("DEBUG");
                foreach (String _char in _text)
                {
                        txtDebugStatus.AppendLine(_char);
                }
                txtDebugStatus.AppendLine("DEBUG");

                    for (int x = 0; x < _text.Count(); x++)
                    {
                        if (_text[x].Remove(6) == "Name: ")
                            _textToSend = _text[x].Remove(0, 6) + " ";
                        else if (_text[x].Remove(7) == "Owner: ")
                            _textToSend = _text[x].Remove(0, 7) + " ";
                        if (_text[x].Length > 11)
                            if (_text[x].Remove(11) == "Condition: ")
                                _textToSend = _text[x].Remove(0, 11) + " ";
                            else
                                _textToSend = "Refreshed";
                    }


                txtDebugStatus.AppendLine(_textToSend);
                txtDebugStatus.AppendLine(_item.Tooltip);
                }

            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message.ToString());
            }

        }
        private void btnSetSafeRunebook_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show("Select runebook...");
                Item _itemRunebook = Target.RequestTarget();

                Settings.HomeRunebookID = _itemRunebook.ID;

            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message.ToString());
            }
        }
        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                    CreateXMLFile(SettingsFilePath);

                Settings.HomeRuneNumber = Convert.ToInt32(txtSafeRuneNumber.Text);
                txtStatus.AppendLine("Saving general settings...");
                XmlSerializer _serializer = new XmlSerializer(typeof(Settings));
                using (TextWriter _writer = new StreamWriter(SettingsFilePath))
                {
                    _serializer.Serialize(_writer, Settings);
                }
                txtStatus.AppendLine("General settings saved!");
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message.ToString());
            }

        }
        private void btnLoadSettings_Click(object sender, EventArgs e)
        {
            try
            {
                XmlSerializer _deserializer = new XmlSerializer(typeof(Settings));
                TextReader _reader = new StreamReader(SettingsFilePath);
                object obj = _deserializer.Deserialize(_reader);
                Settings = (Settings)obj;
                _reader.Close();

                txtHouseStatus.AppendLine("General settings loaded!");
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message.ToString());
            }
        }
        private void btnRefreshDataGrid_Click(object sender, EventArgs e)
        {
            UpdateGridView();
        }
        private void btnPauseBuildRail_Click(object sender, EventArgs e)
        {
            BuildingRailPause = true;
            txtStatus.AppendLine("Paused building rail...");
        }
        private void btnContinueBuildRail_Click(object sender, EventArgs e)
        {
            BuildingRailPause = false;
            txtStatus.AppendLine("Resumed building rail...");
        }
        #endregion

        #region Worker Functions
        private void workerSearch_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                while (Searching)
                {
                    if (workerSearch.CancellationPending)
                        break;

                    foreach (Rail _rail in SelectedRails)
                    {
                        if (workerSearch.CancellationPending)
                            break;

                        lock (thisLock)
                        {
                            CurrentRail = _rail;

                            Item _runebook = new Item(_rail.RunebookID);

                            GumpClass _runebookGump = OpenRunebook(_runebook);

                            if (_runebookGump == null)
                            {
                                workerSearch.ReportProgress(0, "Couldn't find runebook!");
                                Searching = false;
                                workerSearch.CancelAsync();
                            }

                            Thread.Sleep(1000);

                            int _button = 49 + _rail.RuneNumber;

                            workerSearch.ReportProgress(0, "Recalling to start spot...");

                            _runebookGump.ClickButton(_button);

                            Thread.Sleep(5000);

                            int _pathLocations = _rail.Path.Count();
                            string _textToReport = _pathLocations.ToString() + " locations found!";

                            workerSearch.ReportProgress(0, _textToReport);
                            workerSearch.ReportProgress(0, "Starting search...");
                        }

                        int _n = 0;
                        foreach (Location _location in _rail.Path)
                        {
                            if (workerSearch.CancellationPending)
                                break;

                            lock (thisLock)
                            {
                                _n++;
                                workerSearch.ReportProgress(0, "Moving to location (" + _n.ToString() + ")");

                                if (workerSearch.CancellationPending)
                                    break;


                                while (Self.X != _location.X || Self.Y != _location.Y)
                                {
                                    if (workerSearch.CancellationPending)
                                        break;

                                    Stealth.Default.MoveXY((ushort)_location.X, (ushort)_location.Y, false, 0, true);
                                }
                            }
                        }
                    }
                    
                    Item _homeRunebook = new Item(Settings.HomeRunebookID);

                    GumpClass _homeRunebookGump = OpenRunebook(_homeRunebook);

                    if (_homeRunebookGump == null)
                    {
                        workerSearch.ReportProgress(0, "Couldn't find runebook!");
                        Searching = false;
                        workerSearch.CancelAsync();
                    }

                    Thread.Sleep(1000);

                    int _homeButton = 49 + Settings.HomeRuneNumber;

                    workerSearch.ReportProgress(0, "Recalling to start spot...");

                    _homeRunebookGump.ClickButton(_homeButton);

                    Thread.Sleep(5000);
                    

                    workerSearch.ReportProgress(0, "Search finished.");
                    Searching = false;
                    workerSearch.CancelAsync();
                    workerCheckHouses.CancelAsync();
                }
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message.ToString());
            }
        }
        private void workerSearch_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            txtStatus.AppendLine(e.UserState.ToString());
        }
        private void workerSearch_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            txtStatus.AppendLine("Stopped searching!");
        }
        private void workerCheckHouses_DoWork(object sender, DoWorkEventArgs e)
        {

            Scanner.Range = 20;
            Scanner.VerticalRange = 20;

            try
            {
                while (Searching)
                {
                    lock (thisLock)
                    {
                        if (workerCheckHouses.CancellationPending)
                            break;
                        
                        var _results = Scanner.Find<HouseSigns>(0x0, false);
                        List<HouseSigns> _resultsList = _results.Select(x => x.Cast<HouseSigns>()).ToList();

                        foreach (ScriptSDK.Items.Item _item in _resultsList)
                        {
                            if (!Houses.Any(x => x.ID == _item.Serial.Value))
                            {
                                workerCheckHouses.ReportProgress(0, "New house added!");
                                House _house = new House(_item.Serial.Value, _item.Tooltip, CurrentRail.Name);
                                Houses.Add(_house);
                            }
                            else
                            {
                                workerCheckHouses.ReportProgress(0, "Updating house...");
                                House _house = Houses.Find(x => x.ID == _item.Serial.Value);
                                _house.Checked = DateTime.Now;
                                _house.Tooltip = _item.Tooltip;

                                string[] _text = _item.Tooltip.Split('|');

                                for (int x = 0; x < _text.Count(); x++)
                                {
                                    if (_text[x].Length > 11)
                                        if (_text[x].Remove(11) == "Condition: ")
                                            _house.Condition = _text[x].Remove(0, 11);
                                        else
                                            _house.Condition = "Refreshed";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message.ToString());
            }
        }
        private void workerCheckHouses_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            txtHouseStatus.AppendLine(e.UserState.ToString());
        }
        private void workerCheckHouses_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            txtHouseStatus.AppendLine("Check Houses Stopped!");
            SaveHouses();
        }
        private void workerBuildRail_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            txtStatus.AppendLine(e.UserState.ToString());
        }
        private void workerBuildRail_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            txtStatus.AppendLine("Rail building ended!");
        }
        private void workerBuildRail_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                workerBuildRail.ReportProgress(0, "Building rail, start running around!");

                int _n = 0;

                while (BuildingRail)
                {
                    if (BuildingRailPause)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    _n++;
                    if (workerBuildRail.CancellationPending)
                        break;

                    workerBuildRail.ReportProgress(0, "Updating path (" + _n + ")");

                    int _x, _y;
                    _x = Self.X;
                    _y = Self.Y;
                    
                    Location _location = new Location(_x, _y);

                    SelectedRail.Path.Add(_location);
                    Thread.Sleep(2500);
                }
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message.ToString());
            }
        }
        #endregion

        #region Methods
        private void CreateXMLFile(string Filename)
        {
            XmlDocument _doc = new XmlDocument();
            XmlNode _docNode = _doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            _doc.AppendChild(_docNode);
            XmlNode _rootNode = _doc.CreateElement("ArrayOfRail");
            _doc.AppendChild(_rootNode);
            StreamWriter _outStream = File.CreateText(Filename);
            _doc.Save(_outStream);
            _outStream.Close();
        }
        private GumpClass OpenRunebook(Item Runebook)
        {
            uint _nextGumpCount = GumpClass.GumpCount + 1, _attempts = 0;

            while (GumpClass.GumpCount <= _nextGumpCount && ++_attempts < 5)
            {
                if (GumpClass.GumpCount > 0)
                    if (GumpClass.LastGump().ID == Runebook.ID)
                        return GumpClass.LastGump();

                Runebook.Use();
                Script.Wait(750);
            }

            return null; // return default(GumpClass);
        }
        private void UpdateGridView()
        {
            try
            {
                DataSetHouses = new DataSet();
                DataSetHouses.Tables.Add(Houses.ToDataTable());

                DataSetHouses.Locale = CultureInfo.InvariantCulture;

                dataGridView1.AutoGenerateColumns = true;
                dataGridView1.DataSource = DataSetHouses;
                dataGridView1.DataMember = "DataTable";
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message.ToString());
            }

        }
        private void SaveHouses()
        {
            try
            {
                if (!File.Exists(HouseFilePath))
                    CreateXMLFile(HouseFilePath);

                txtStatus.AppendLine("Saving houses...");
                XmlSerializer _serializer = new XmlSerializer(typeof(List<House>));
                using (TextWriter _writer = new StreamWriter(HouseFilePath))
                {
                    _serializer.Serialize(_writer, Houses);
                }
                txtStatus.AppendLine("Houses saved!");
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message.ToString());
            }
        }
        #endregion

    }

    #region Objects
    /*
    public class Rails : List<Rail>
    {
        private Rail _rail;
        public Rail Rail
        {
            get { return _rail; }
            set { _rail = value; }
        }
        public Rails()
        {

        }
        public Rails(IEnumerable<Rail> list)
            : base(list)
        {
        }
        public Rails(Rail Rail)
        {
            this.Rail = Rail;
        }
    }*/
    public class Rail
    {
        private string _name;
        private uint _runebookID;
        private int _runeNumber;
        private List<Location> _path;

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
        public Rail()
        {
            //parameterless constructor for xml serialization
        }
        public Rail(string Name, uint RunebookID, int RuneNumber)
        {
            this.Name = Name;
            this.RunebookID = RunebookID;
            this.RuneNumber = RuneNumber;
            this.Path = new List<Location>();
        }
    }
    public class Location
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
            //parameterless constructor for xml serialization
        }
        public Location(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }
    public static class WinFormsExtensions
    {
        public static void AppendLine(this TextBox source, string value)
        {
            if (source.Text.Length == 0)
                source.Text = value;
            else
                source.AppendText("\r\n" + value);
        }
    }
    public class House
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
        public House(uint ID, string Tooltip, string Rail)
        {
            this.ID = ID;                
            this.Tooltip = Tooltip;
            this.Location = new Location(Self.X, Self.Y);
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

    [QuerySearch(new ushort[] {
       0x0B96, 0x0BA4, 0x0BA6, 0x0BA8, 0x0BAA,
        0x0BAC, 0x0BAE, 0x0BB0, 0x0BB2, 0x0BB4,
        0x0BB6, 0x0BB8, 0x0BBA, 0x0BBC, 0x0BBE,
        0x0BC0, 0x0BC2, 0x0BC4, 0x0BC6, 0x0BC8,
        0x0BCA, 0x0BCC, 0x0BCE, 0x0BD0, 0x0BD2,
        0x0BD4, 0x0BD6, 0x0BD8, 0x0BDA, 0x0BDC,
        0x0BDE, 0x0BE0, 0x0BE2, 0x0BE4, 0x0BE6,
        0x0BE8, 0x0BEA, 0x0BEC, 0x0BEE, 0x0BF0,
        0x0BF2, 0x0BF4, 0x0BF6, 0x0BF8, 0x0BFA,
        0x0BFC, 0x0BFE, 0x0C00, 0x0C02, 0x0C04,
        0x0C06, 0x0C08, 0x0C0A, 0x0C0C, 0x0C0E,
        0x0C44 })]
    public class HouseSigns : ScriptSDK.Items.Item
    {
        public HouseSigns(Serial serial)
            : base(serial)
        {

        }
    }
    public class Settings
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
    #endregion

}
