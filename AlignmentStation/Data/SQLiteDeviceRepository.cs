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

        public CalibrationLocation GetCalibrationLocation(int Id)
        {
            if (!File.Exists(DbFile)) return null;

            using (var conn = SimpleDbConnection())
            {
                conn.Open();
                CalibrationLocation units = conn.Query<CalibrationLocation>(
                                    @"SELECT X, Y, Z, Id
                        from CalibrationLocation where 
                        Id == @Id", new { Id }).FirstOrDefault();

                return units;
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

                int? num = conn.Query<int?>(
                    @"select max(Unit_Number) from TOSAOutput where Job_Number = @jobNumber", new { jobNumber }).FirstOrDefault();

                return (int)(num == null ? 0 : num);
            }
        }
        
        public int GetMaxROSAUnitNumber(string jobNumber)
        {
            using (var conn = SimpleDbConnection())
            {
                conn.Open();

                int? num = conn.Query<int?>(
                    @"select max(Unit_Number) from ROSAOutput where Job_Number = @jobNumber",
                    new { jobNumber }).FirstOrDefault();

                return (int)(num == null ? 0 : num);
            }
        }

        public double? GetLatestROSAFiberPower(string jobNumber)
        {
            using (var conn = SimpleDbConnection())
            {
                conn.Open();

                (DateTime? timestamp, double? power) n = conn.Query<(DateTime?, double?)>(
                    @"select Timestamp, Fiber_Power from ROSAOutput where Job_Number = @jobNumber order by Timestamp desc limit 1",
                    new { jobNumber }).FirstOrDefault();

                if (n.timestamp != null && n.power != null)
                {
                    DateTime now = DateTime.Now;
                    if (n.timestamp  > now.AddHours(-12) && n.timestamp <= now)
                    {
                        return n.power;
                    }
                }

                return null;
            }
        }

        public void SaveROSADevice(ROSADevice device)
        {
            using (var conn = SimpleDbConnection())
            {
                conn.Open();
                conn.Execute(
                @"INSERT INTO ROSADevice
                    ( Part_Number, VPD_RSSI, Resp_Min )
                    values 
                    ( @part_number, @vpd_rssi, @resp_min )", 
                new {
                    part_number = device.Part_Number,
                    vpd_rssi = device.VPD_RSSI,
                    resp_Min = device.Resp_Min
                });
            }
        }

        public void SaveTOSADevice(TOSADevice device)
        {
            using (var conn = SimpleDbConnection())
            {
                conn.Open();
                conn.Execute(
                @"INSERT INTO TOSADevice 
                    ( 
                        Part_Number, 
                        I_Align,
                        I_Align_Tol,
                        P_Min_TO,
                        P_Min_FC, 
                        V_Max, 
                        POPCT_Min, 
                        P_FC_Shift_Max
                    )
                    values 
                    ( 
                        @part_number, 
                        @i_align,
                        @i_align_tol, 
                        @p_min_to,
                        @p_min_fc, 
                        @v_max,
                        @popct_min, 
                        @p_fc_shift_max
                        )", 
                new {
                    part_number = device.Part_Number,
                    i_align = device.I_Align,
                    i_align_tol = device.I_Align_Tol,
                    p_min_to = device.P_Min_TO,
                    p_min_fc = device.P_Min_FC,
                    v_max = device.V_Max,
                    popct_min = device.POPCT_Min,
                    p_fc_shift_max = device.P_FC_Shift_Max
                });
            }
        }

        public void SaveROSAOutput(ROSAOutput output)
        {
            using (var conn = SimpleDbConnection())
            {
                conn.Open();
                conn.Execute(
                @"INSERT INTO ROSAOutput 
                    ( 
                        Part_Number,
                        Job_Number,
                        Unit_Number,
                        Operator,
                        Timestamp,
                        Resp_Shift,
                        Resp,
                        Resp_Post_Cure,
                        Fiber_Power,
                        Passed
                    )
                    values 
                    ( 
                        @part_number, 
                        @job_number,
                        @unit_number, 
                        @op,
                        @timestamp, 
                        @resp_shift,
                        @resp,
                        @resp_post_cure,
                        @fiber_power,
                        @passed
                        )", 
                new {
                    part_number = output.Part_Number, 
                    job_number = output.Job_Number,
                    unit_number = output.Unit_Number, 
                    op = output.Operator,
                    timestamp = DateTime.Now,
                    resp_shift = output.Resp_Shift,
                    resp = output.Resp,
                    resp_post_cure = output.Resp_Post_Cure,
                    fiber_power = output.Fiber_Power,
                    passed = output.Passed
                });
            }
        }

        public void SaveTOSAOutput(TOSAOutput output)
        {
            using (var conn = SimpleDbConnection())
            {
                conn.Open();
                conn.Execute(
                @"INSERT INTO TOSAOutput 
                    ( 
                        Part_Number,
                        Job_Number,
                        Unit_Number,
                        Operator,
                        Timestamp,
                        I_Align,
                        P_TO,
                        P_FC,
                        POPCT,
                        POPCT_Post_Cure,
                        POPCT_Shift,
                        Passed
                    )
                    values 
                    ( 
                        @part_number, 
                        @job_number,
                        @unit_number, 
                        @op,
                        @timestamp, 
                        @i_align, 
                        @p_to,
                        @p_fc,
                        @popct,
                        @popct_post_cure,
                        @popct_shift,
                        @passed
                        )",
                new {
                    part_number = output.Part_Number,
                    job_number = output.Job_Number,
                    unit_number = output.Unit_Number,
                    op = output.Operator,
                    timestamp = DateTime.Now,
                    i_align = output.I_Align,
                    p_to = output.P_TO,
                    p_fc = output.P_FC,
                    popct = output.POPCT,
                    popct_post_cure = output.POPCT_Post_Cure,
                    popct_shift = output.POPCT_Shift,
                    passed = output.Passed
                }) ;
            }
        }
        
        public ReferenceUnits GetTOSAReferenceUnits(TOSAOutput output)
        {
            var part = output.Part_Number;

            using (var conn = SimpleDbConnection())
            {
                conn.Open();

                ReferenceUnits units = conn.Query<ReferenceUnits>(
                    @"SELECT X, Y, Z, Part_Number
                        from TOSAReferenceUnits where 
                        Part_Number = @part ;", new { part }).FirstOrDefault();

                return units;
            }
        }

        public ReferenceUnits GetROSAReferenceUnits(ROSAOutput output)
        {
            var part = output.Part_Number;

            using (var conn = SimpleDbConnection())
            {
                conn.Open();

                ReferenceUnits units = conn.Query<ReferenceUnits>(
                    @"SELECT X, Y, Z, Part_Number
                        from ROSAReferenceUnits where 
                        Part_Number = @part ;", new { part }).FirstOrDefault();

                return units;
            }
        }

        public void SaveTOSAReferenceUnits(ReferenceUnits units)
        {
            using (var conn = SimpleDbConnection())
            {
                conn.Open();

                conn.Execute(@"
                    INSERT OR REPLACE INTO TOSAReferenceUnits
                    (X, Y, Z, Part_Number)
                    values 
                    (@X, @Y, @Z, @Part_Number);
                ",
                new
                {
                    X = units.X,
                    Y = units.Y,
                    Z = units.Z,
                    Part_Number = units.Part_Number
                });
            }
        }

        public void SaveROSAReferenceUnits(ReferenceUnits units)
        {
            using (var conn = SimpleDbConnection())
            {
                conn.Open();

                conn.Execute(@"
                    INSERT OR REPLACE INTO ROSAReferenceUnits
                    (X, Y, Z, Part_Number)
                    values 
                    (@X, @Y, @Z, @Part_Number);
                ",
                new
                {
                    X = units.X,
                    Y = units.Y,
                    Z = units.Z,
                    Part_Number = units.Part_Number
                });
            }
        }

        private static void CreateDatabase()
        {
            using (var conn = SimpleDbConnection())
            {
                conn.Open();

                conn.Execute(
                    @"create table ROSAReferenceUnits (
                        X double not null,
                        Y double not null,
                        Z double not null,
                        Part_Number varchar(255) not null, 
                        primary key (Part_Number)
                        )
                    ");

                conn.Execute(
                    @"create table TOSAReferenceUnits (
                        X double not null,
                        Y double not null,
                        Z double not null,
                        Part_Number varchar(255) not null, 
                        primary key (Part_Number)
                       )
                    ");

                conn.Execute(
                   @"create table CalibrationLocation (
                        X double not null,
                        Y double not null,
                        Z double not null,
                        Id integer primary key
                       )
                    ");

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
                        I_Align double not null,
                        P_TO double not null,
                        P_FC double not null,
                        POPCT double not null,
                        POPCT_Post_Cure double not null,
                        POPCT_Shift double not null,
                        Passed boolean not null
                    )");

                conn.Execute(
                    @"create table ROSADevice 
                    (
                        Id integer primary key autoincrement,
                        Part_Number varchar(255) not null,
                        VPD_RSSI varchar(4) not null,
                        Resp_Min double not null
                    )");

                conn.Execute(@"insert into ROSADevice (Part_Number, VPD_RSSI, Resp_Min)
                                values  ('r-10', 'vpd', 0.2),
                                        ('r-11', 'rssi', 0.2);");
                
                conn.Execute(
                    @"create table ROSAOutput 
                    (
                        Id integer primary key autoincrement,
                        Part_Number varchar(255) not null,
                        Job_Number varchar(255) not null,
                        Unit_Number integer not null,
                        Operator varchar(255) not null,
                        Timestamp datetime not null,
                        Resp double not null,
                        Resp_Post_Cure double not null,
                        Resp_Shift double not null,
                        Fiber_Power double not null,
                        Passed boolean not null
                    )");

            }
        }
    }
}
