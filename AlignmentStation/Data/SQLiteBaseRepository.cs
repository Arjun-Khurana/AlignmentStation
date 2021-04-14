using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace AlignmentStation.Data
{
    public class SQLiteBaseRepository 
    {
        public static string DbFile
        {
            get { return Environment.CurrentDirectory + "\\DataFile.sqlite";  }
        }

        public static SQLiteConnection SimpleDbConnection()
        {
            return new SQLiteConnection("Data Source=" + DbFile); 
        }
    }
}
