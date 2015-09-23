using ScriptSDK;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows.Forms;

namespace FAIL
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FAIL());
        }

        public static T Cast<T>(this UOEntity obj) where T : UOEntity
        {
            return Activator.CreateInstance(typeof(T), obj.Serial) as T;
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> collection)
        {
            DataTable dt = new DataTable("DataTable");
            Type t = typeof(T);
            PropertyInfo[] pia = t.GetProperties();

            //Inspect the properties and create the columns in the DataTable
            foreach (PropertyInfo pi in pia)
            {
                if (pi.Name == "Added" || pi.Name == "Checked")
                    continue;

                if (pi.Name == "Location")
                {
                    dt.Columns.Add(pi.Name, typeof(string));
                    continue;
                }

                Type ColumnType = pi.PropertyType;
                if ((ColumnType.IsGenericType))
                {
                    ColumnType = ColumnType.GetGenericArguments()[0];
                }
                dt.Columns.Add(pi.Name, ColumnType);
            }

            //Populate the data table
            foreach (T item in collection)
            {
                DataRow dr = dt.NewRow();
                dr.BeginEdit();
                foreach (PropertyInfo pi in pia)
                {
                    if (pi.Name == "Added" || pi.Name == "Checked")
                        continue;

                    if (pi.Name == "Location")
                    {
                        Type st = typeof(Location);
                        Location _location = (Location)pi.GetValue(item, null);
                        string _string = _location.X.ToString() + ", " + _location.Y.ToString();
                        dr[pi.Name] = _string;
                        continue;
                    }

                    if (pi.GetValue(item, null) != null)
                    {
                        dr[pi.Name] = pi.GetValue(item, null);
                    }
                }
                dr.EndEdit();
                dt.Rows.Add(dr);
            }
            return dt;
        }
    }
}