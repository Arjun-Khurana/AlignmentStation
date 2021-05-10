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

    public abstract class TestOutput
    {
        public int Id { get; set; }
        public string Part_Number { get; set; }
        public string Job_Number { get; set; }
        public int Unit_Number { get; set; }
        public string Operator { get; set; }
        public DateTime Timestamp { get; set; }
        public int Repeat_Number { get; set; }
    }

    public class TOSADevice : Device
    {
        // I VCSEL min to be ok in step 1 - pass in before measure
        public double I_Align { get; set; }        
        
        // Tolerance for current drop across arroyo
        public double I_Align_Tol { get; set; }

        // Min power to be ok in step 1
        public double P_Min_TO { get; set; }       
        
        // Min power to be ok after alignment
        public double P_Min_FC { get; set; }       
        
        // Max voltage to be ok in step 1
        public double V_Max { get; set; }          
        
        // Ratio of power after alignment / power before,
        // at end of alignment process, see how good alignment went
        public double POPCT_Min { get; set; }      
        
        // After UV light, measure power again, this is max allowed different
        // ( pre UV - post UV ) / pre UV - normalized into percent
        public double P_FC_Shift_Max { get; set; } 
    }

    public class TOSAOutput : TestOutput
    {
        public double I_Align { get; set; }
        public double P_TO { get; set; }
        public double P_FC { get; set; }
        public double POPCT { get; set; }
        public double POPCT_Shift { get; set; }
    }

    public class ROSADevice : Device
    {
        public double VPD_RSSI { get; set; }
    }

    public class ROSAOutput : TestOutput
    {
        public double P_Optical { get; set; }
        public double I_RSSI { get; set; }
        public double I_VPD { get; set; }
        public double POPCT { get; set; }
        public double POPCT_Shift { get; set; }
    }
}
