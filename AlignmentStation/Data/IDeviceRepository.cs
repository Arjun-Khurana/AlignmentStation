using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlignmentStation.Models;
using System.Data.SQLite;

namespace AlignmentStation.Data
{
    public interface IDeviceRepository
    {
        ROSADevice GetROSADevice(int id);
        TOSADevice GetTOSADevice(int id);
        void SaveROSAOutput(ROSAOutput output);
        void SaveTOSAOutput(TOSAOutput output);
        ReferenceUnits GetTOSAReferenceUnits(TOSAOutput output);
        ReferenceUnits GetROSAReferenceUnits(ROSAOutput output);
        void SaveTOSAReferenceUnits(ReferenceUnits units);
        void SaveROSAReferenceUnits(ReferenceUnits units);
        CalibrationLocation GetCalibrationLocation(int Id);
    }

}
