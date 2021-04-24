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
using System.Threading;

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
                        Debug.WriteLine("Found resource", s);
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

                powerMeter = session2;
            }
        }

        public void SetArroyoLaserOn()
        {
            arroyo.RawIO.Write("LAS:OUT 1\n");
            Thread.Sleep(1000);
        }

        public void SetArroyoLaserOff()
        {
            arroyo.RawIO.Write("LAS:OUT 0\n");
        }

        public void SetArroyoCurrent(double current)
        {
            arroyo.RawIO.Write("LAS:LDI " + current + "\n");
            Thread.Sleep(1000);
        }

        public double GetArroyoVoltage()
        {
            arroyo.RawIO.Write("LAS:LDV?\n");
            var r = arroyo.RawIO.ReadString();

            Debug.WriteLine(r + "v");

            return Double.Parse(r);
        }

        public double GetArroyoCurrent()
        {
            arroyo.RawIO.Write("LAS:LDI?\n");
            var r = arroyo.RawIO.ReadString();

            Debug.WriteLine(r + "A");

            return Double.Parse(r);
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
            c.Commands.Motion.Fiber.Centroid3D();
        }

        public double GetThorlabsPower()
        {
            powerMeter.RawIO.Write("MEAS:POW?\n");
            return Double.Parse(powerMeter.RawIO.ReadString());
        }
    }
}
