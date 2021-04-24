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
        // I VCSEL min to be ok in step 1 - pass in before measure
        double I_Align { get; set; }        
        
        // Tolerance for current drop across arroyo
        double I_Align_Tol { get; set; }

        // Min power to be ok in step 1
        double P_Min_TO { get; set; }       
        
        // Min power to be ok after alignment
        double P_Min_FC { get; set; }       
        
        // Max voltage to be ok in step 1
        double V_Max { get; set; }          
        
        // Ratio of power after alignment / power before,
        // at end of alignment process, see how good alignment went
        double POPCT_Min { get; set; }      
        
        // After UV light, measure power again, this is max allowed different
        // ( pre UV - post UV ) / pre UV - normalized into percent
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
