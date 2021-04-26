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

        public List<ROSADevice> GetAllROSADevices()
        {
            if (!File.Exists(DbFile)) return null;

            using (var conn = SimpleDbConnection())
            {
                conn.Open();

                var devices = conn.Query<ROSADevice>(@"select * from ROSADevice").ToList();

                return devices;
            }
        }

        public ROSADevice GetROSADevice(int id)
        {
            if (!File.Exists(DbFile)) return null;

            using (var conn = SimpleDbConnection())
            {
                conn.Open();
                ROSADevice device = conn.Query<ROSADevice>(
                    @"SELECT id, 
                        part_number, 
                        vpd_rssi,
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
                    @"SELECT Id, 
                        Part_Number, 
                        I_Align, 
                        P_Min_TO, 
                        P_Min_FC, 
                        V_Max, 
                        POPCT_Min, 
                        P_FC_Shift_Max
                    FROM TOSADevice
                    where Id = @id", new { id }).FirstOrDefault();

                return device;
            }
        }
        
        public int GetMaxTOSAUnitNumber(string jobNumber)
        {
            using (var conn = SimpleDbConnection())
            {
                conn.Open();

                int num = conn.Query<int>(
                    @"select max(Unit_Number) from TOSAOutput where Job_Number = @jobNumber", new { jobNumber }).FirstOrDefault();

                return num;
            }
        }
        
        public int GetMaxROSAUnitNumber(string jobNumber)
        {
            using (var conn = SimpleDbConnection())
            {
                conn.Open();

                int num = conn.Query<int>(
                    @"select max(Unit_Number) from ROSAOutput where Job_Number = @jobNumber", new { jobNumber }).FirstOrDefault();

                return num;
            }
        }

        public void SaveROSADevice(ROSADevice device)
        {
            throw new NotImplementedException();
        }

        public void SaveTOSADevice(TOSADevice device)
        {
            throw new NotImplementedException();
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
                        I_Align double not null,
                        I_Align_Tol double not null,
                        P_Min_TO double not null,
                        P_Min_FC double not null,
                        V_Max double not null,
                        POPCT_Min double not null,
                        P_FC_Shift_Max double not null
                    )");


                conn.Execute(@"insert into TOSADevice (
                                Part_Number, 
                                I_Align,
                                I_Align_Tol,
                                P_Min_TO, 
                                P_Min_FC, 
                                V_Max, 
                                POPCT_Min, 
                                P_FC_Shift_Max)
                                values  
                                    ('Device 1', 6.0, 0.1, 1, 0.5, 3, 0.4, 0.1)");
                
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
