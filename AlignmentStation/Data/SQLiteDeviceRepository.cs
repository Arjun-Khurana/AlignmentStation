using AlignmentStation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Dapper;

namespace AlignmentStation.Data
{
    public class SQLiteDeviceRepository : SQLiteBaseRepository, IDeviceRepository
    {
        public SQLiteDeviceRepository()
        {
            Console.WriteLine("Checking if database exists");
            if (!File.Exists(DbFile))
            {
                Console.WriteLine("DB does not exist");
                CreateDatabase();
            }
        }

        public ROSADevice GetROSADevice(int id)
        {
            if (!File.Exists(DbFile)) return null;

            using (var conn = SimpleDbConnection())
            {
                conn.Open();
                ROSADevice device = conn.Query<ROSADevice>(
                    @"SELECT id, part_number, vpd_rssi,
                    FROM ROSADevice
                    where id = @id", 
                    new { id }).FirstOrDefault();

                return device;
            }
        }

        public List<TOSADevice> GetAllTOSADevices()
        {
            if (!File.Exists(DbFile)) return null;

            using (var conn = SimpleDbConnection())
            {
                conn.Open();

                var devices = conn.Query<TOSADevice>(@"select * from TOSADevice").ToList();

                return devices;
            }
        }

        public TOSADevice GetTOSADevice(int id)
        {
            if (!File.Exists(DbFile)) return null;

            using (var conn = SimpleDbConnection())
            {
                conn.Open();

                TOSADevice device = conn.Query<TOSADevice>(
                    @"SELECT Id, Part_Number, P_Min_TO, P_Min_FC, P_FC_Shift_Max
                    FROM TOSADevice
                    where Id = @id", new { id }).FirstOrDefault();

                return device;
            }
        }

        public void SaveROSAOutput(ROSAOutput output)
        {
            throw new NotImplementedException();
        }

        public void SaveTOSAOutput(TOSAOutput output)
        {
            throw new NotImplementedException();
        }

        private static void CreateDatabase()
        {
            using (var conn = SimpleDbConnection())
            {
                conn.Open();

                conn.Execute(
                    @"create table TOSADevice 
                    (
                        Id integer primary key autoincrement,
                        Part_Number varchar(255) not null,
                        P_Min_TO double not null,
                        P_Min_FC double not null,
                        P_FC_Shift_Max double not null
                    )");


                conn.Execute(@"insert into TOSADevice (Part_Number, P_Min_TO, P_Min_FC, P_FC_Shift_Max)
                                values  ('p-10', 1.0, 2.8, 3.6),
                                        ('p-11', 2.0, 354.7, 32.6);");
                
                conn.Execute(
                    @"create table TOSAOutput
                    (
                        Id integer primary key autoincrement,
                        Part_Number varchar(255) not null,
                        Job_Number varchar(255) not null,
                        Unit_Number integer not null,
                        Operator varchar(255) not null,
                        Timestamp datetime not null,
                        Repeat_Number integer not null,
                        I_Align double not null,
                        P_TO double not null,
                        P_TC double not null,
                        POPCF double not null,
                        POPCT_Shift double not null
                    )");

                conn.Execute(
                    @"create table ROSADevice 
                    (
                        Id integer primary key autoincrement,
                        Part_Number varchar(255) not null,
                        VPD_RSSI double not null
                    )");

                conn.Execute(@"insert into ROSADevice (Part_Number, VPD_RSSI)
                                values  ('r-10', 1.0),
                                        ('r-11', 32.6);");
                
                conn.Execute(
                    @"create table ROSAOutput 
                    (
                        Id integer primary key autoincrement,
                        Part_Number varchar(255) not null,
                        Job_Number varchar(255) not null,
                        Unit_Number integer not null,
                        Operator varchar(255) not null,
                        Timestamp datetime not null,
                        Repeat_Number integer not null,
                        P_Optical double not null,
                        I_RSSI double not null,
                        I_VPD double not null,
                        POPCT double not null,
                        POPCT_Shift double not null
                    )");

            }
        }
    }
}
