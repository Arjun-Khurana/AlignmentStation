using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aerotech.A3200;
using Aerotech.A3200.Exceptions;
using Thorlabs.TLPM_64.Interop;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AlignmentStation
{
    public sealed class Instruments
    {
        private static readonly Lazy<Instruments> lazy = new Lazy<Instruments>(() => new Instruments());

        public static Instruments instance { get { return lazy.Value; } } 

        private Instruments()
        {
        }

        public double GetPowerMeasurement()
        {
            HandleRef Instrument_Handle = new HandleRef();

            TLPM searchDevice = new TLPM(Instrument_Handle.Handle);
            uint count = 0;
            string firstPowermeterFound = "";

            try
            {
                int pInvokeResult = searchDevice.findRsrc(out count);

                if (count > 0)
                {
                    StringBuilder descr = new StringBuilder(1024);

                    searchDevice.getRsrcName(0, descr);

                    firstPowermeterFound = descr.ToString();
                }
            }
            catch { }

            if (count == 0)
            {
                searchDevice.Dispose();

                throw new Exception("No power meter could be found");
            }

            //always use true for ID Query
            TLPM device = new TLPM(firstPowermeterFound, true, false);  //  For valid Ressource_Name see NI-Visa documentation.
            Debug.WriteLine("Power meter found: {0}", firstPowermeterFound);

            double measuredPower = 0;
            short unit = 0;
            
            device.getPowerUnit(out unit);
            device.measPower(out measuredPower);

            Debug.WriteLine("Measured Power: {0}", measuredPower);
            Debug.WriteLine("Measured Power unit: {0}", unit);

            device.Dispose();

            return measuredPower;

        }
    }
}
