using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aerotech.A3200;
using Aerotech.A3200.Exceptions;
using Aerotech.A3200.Status;
using Thorlabs.TLPM_64.Interop;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NationalInstruments.Visa;
using Ivi.Visa;

namespace AlignmentStation
{
    public sealed class Instruments
    {
        private static readonly Lazy<Instruments> lazy = new Lazy<Instruments>(() => new Instruments());
        public static Instruments instance { get { return lazy.Value; } } 

        private Controller c;
        private SerialSession arroyo;
        private UsbSession powerMeter;

        private Instruments()
        {
            try
            {
                c = Controller.Connect();
            }
            catch(A3200Exception ex)
            {
                Console.WriteLine("Error connecting to controller");
            }

            using (var rm = new ResourceManager())
            {
                string myResource = "";
                try
                {
                    IEnumerable<string> resources = rm.Find("ASRL?*INSTR");
                    foreach (string s in resources)
                    {
                        Debug.WriteLine(s);
                        myResource = s;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("fuck u bloody");
                }

                SerialSession session = (SerialSession)rm.Open(myResource);
                session.BaudRate = 38400;

                session.RawIO.Write("*IDN?\n");
                string r = session.RawIO.ReadString();
                Debug.WriteLine(r);

                arroyo = session;

                try
                {
                    IEnumerable<String> resources = rm.Find("USB?*");
                    foreach (string s in resources)
                    {
                        Debug.WriteLine(s);
                        myResource = s;
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("fokk");
                }

                UsbSession session2 = (UsbSession)rm.Open(myResource);
                session2.SendEndEnabled = true;
                session2.TerminationCharacterEnabled = false;

                session2.RawIO.Write("*IDN?\n");
                var t = session2.ReadStatusByte();

                r = session2.RawIO.ReadString();
                Debug.WriteLine(r);

                powerMeter = session2;
            }
        }

        public void CallArroyo()
        {
            arroyo.RawIO.Write("LAS:LDV?\n");
            var r = arroyo.RawIO.ReadString();

            Debug.WriteLine(r + "v");
        }

        public void CalibrateAxes()
        {
            c.Commands.Axes["Y"].Motion.Enable();
            c.Commands.Axes["X"].Motion.Enable();
            c.Commands.Axes["Z"].Motion.Enable();

            try
            {
                c.Commands.Motion.Linear("Y", -100);
            }
            catch(A3200Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                c.Parameters.Axes["Y"].Limits.LimitDebounceDistance.Value = 0;
                c.Commands.Axes["Y"].Motion.FaultAck();
            }

            try
            {
                c.Commands.Motion.Linear("Z", 100);
            }
            catch(A3200Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                c.Parameters.Axes["Z"].Limits.LimitDebounceDistance.Value = 0;
                c.Commands.Axes["Z"].Motion.FaultAck();
            }

            try
            {
                c.Commands.Motion.Linear("X", 100);
            }
            catch(A3200Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                c.Parameters.Axes["X"].Limits.LimitDebounceDistance.Value = 0;
                c.Commands.Axes["X"].Motion.FaultAck();
            }

            c.Commands.Motion.Linear("X", -12.106);
            c.Commands.Motion.Linear("Z", 0.42);
            c.Commands.Motion.Linear("Y", 15.692);
        }

        public void FindCentroid()
        {
            c.Commands.IO.AnalogInput(0, 0);

            c.Commands.Motion.Fiber.Centroid3D();
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
            Debug.WriteLine(firstPowermeterFound);

            TLPM device = new(firstPowermeFormattedIOterFound, true, true);  //  For valid Ressource_Name see NI-Visa documentation.
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

        public void OpenArroyo()
        {

        }
    }
}
