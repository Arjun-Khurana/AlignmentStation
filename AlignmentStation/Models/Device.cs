using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlignmentStation.Models
{
    public abstract class Device
    {
        public string Part_Number { get; set; }
        public int Id { get; set; }
    }

    public class TOSADevice : Device
    {
        double P_Min_TO { get; set; }
        double P_Min_FC { get; set; }
        double P_FC_Shift_Max { get; set; }
    }

    public class TOSAOutput
    {
        int Id { get; set; }
        string Part_Number { get; set; }
        string Job_Number { get; set; }
        int Unit_Number { get; set; }
        string Operator { get; set; }
        DateTime Timestamp { get; set; }
        int Repeat_Number { get; set; }
        double I_Align { get; set; }
        double P_TO { get; set; }
        double P_FC { get; set; }
        double POPCT { get; set; }
        double POPCT_Shift { get; set; }
    }

    public class ROSADevice : Device
    {
        double VPD_RSSI { get; set; }
    }

    public class ROSAOutput
    {
        int Id { get; set; }
        string Part_Number { get; set; }
        string Job_Number { get; set; }
        int Unit_Number { get; set; }
        string Operator { get; set; }
        DateTime Timestamp { get; set; }
        int Repeat_Number { get; set; }
        double P_Optical { get; set; }
        double I_RSSI { get; set; }
        double I_VPD { get; set; }
        double POPCT { get; set; }
        double POPCT_SHIFT { get; set; }
    }
}
