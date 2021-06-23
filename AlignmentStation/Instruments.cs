using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aerotech.A3200;
using Aerotech.A3200.Exceptions;
using Aerotech.A3200.Status;
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

        private Controller aerotechController;
        private SerialSession arroyo;
        private UsbSession powerMeter;

        private string RelaySerial = "QAAMZ";

        public double powerAttenuation = 24.0;
        public double alignmentPowerCalibration = 0;

        private Instruments()
        {
            alignmentPowerCalibration = Math.Pow(10, powerAttenuation / 10);

            try
            {
                aerotechController = Controller.Connect();
            }
            catch(A3200Exception ex)
            {
                Console.WriteLine("Error connecting to controller");
            }

            aerotechController.Commands.Axes["Y"].Motion.Enable();
            aerotechController.Commands.Axes["X"].Motion.Enable();
            aerotechController.Commands.Axes["Z"].Motion.Enable();

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
                    Debug.WriteLine("Couldn't find Arroyo");

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
                    Debug.WriteLine("Couldn't find thorlabs");
                }

                UsbSession session2 = (UsbSession)rm.Open(myResource);
                session2.SendEndEnabled = true;
                session2.TerminationCharacterEnabled = false;

                session2.RawIO.Write("*IDN?\n");
                var t = session2.ReadStatusByte();

                r = session2.RawIO.ReadString();

                session2.RawIO.Write($"SENS:CORR {powerAttenuation}\n");

                powerMeter = session2;
            }
        }

        public void SetPowerMeterWavelength(int wavelength)
        {
            this.powerMeter.RawIO.Write($"SENS:CORR:WAV {wavelength}\n");
        }

        public void SetArroyoLaserOn()
        {
            arroyo.RawIO.Write("LAS:OUT 1\n");
            Thread.Sleep(5000);
        }

        public void SetArroyoLaserOff()
        {
            arroyo.RawIO.Write("LAS:OUT 0\n");
        }

        public void SetArroyoVoltage(double voltage)
        {
            arroyo.RawIO.Write("LAS:MODE:LDV\n");
            arroyo.RawIO.Write("LAS:LDV " + voltage + "\n");
            Thread.Sleep(1000);
        }

        public void SetArroyoCurrent(double current)
        {
            // set control mode to current
            arroyo.RawIO.Write("LAS:MODE:ILBW\n");
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
            aerotechController.Commands.Axes["Y"].Motion.Enable();
            aerotechController.Commands.Axes["X"].Motion.Enable();
            aerotechController.Commands.Axes["Z"].Motion.Enable();

            aerotechController.Commands.Axes.Select("X", "Y", "Z").Motion.Home();

            aerotechController.Commands.Motion.Linear("X", 1.9964);
            aerotechController.Commands.Motion.Linear("Y", -2.1939);
            aerotechController.Commands.Motion.Linear("Z", 4.7878);
        }

        public void FindFirstLight(bool moveBack = true)
        {
            this.SetArroyoLaserOn();

            if (moveBack)
            {
                aerotechController.Commands.Axes["Z"].Motion.Enable();
                aerotechController.Commands.Motion.Linear("Z", -0.1);
            }

            aerotechController.Commands.Execute("WAIT MODE INPOS");

            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.SpiralFine.SFAxis1.Value = 0;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.SpiralFine.SFAxis2.Value = 2;

            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.SpiralFine.SFMotionType.Value = 0;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.SpiralFine.SFEndRadius.Value = 0.1;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.SpiralFine.SFNumSpirals.Value = 20;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.SpiralFine.SFSegmentLength.Value = 0.025;
            aerotechController.Commands.Motion.Fiber.SpiralFine();
        }

        public void FindCentroid(double edgeValue, double stepSize)
        {
            Debug.Print("Using edge value: {0}", edgeValue);

            aerotechController.Commands.Execute("WAIT MODE INPOS");
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CInputMode.Value = 0;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CEdgeValue.Value = edgeValue;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CScanIncrement.Value = stepSize;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CMaxDisplacement1.Value = 0.2;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CMaxDisplacement2.Value = 0.2;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CMaxDisplacement3.Value = 1.0;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CReturnToCenter.Value = 1;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CAxis1.Value = 0;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CAxis2.Value = 2;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CAxis3.Value = 1;

            aerotechController.Commands.Motion.Fiber.Centroid3D();
        }

        public void FindCentroidHillClimb(double edgeValue, double stepSize)
        {
            Debug.Print("Using edge value: {0}", edgeValue);

            aerotechController.Commands.Execute("WAIT MODE INPOS");
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CInputMode.Value = 0;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CEdgeValue.Value = edgeValue;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CScanIncrement.Value = stepSize;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CMaxDisplacement1.Value = 0.2;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CMaxDisplacement2.Value = 0.2;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CMaxDisplacement3.Value = 0.2;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CReturnToCenter.Value = 0;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CAxis1.Value = 0;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CAxis2.Value = 2;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CAxis3.Value = 1;

            aerotechController.Commands.Motion.Fiber.Centroid2D();
            Debug.Print("Completed Centroid 2d; Running Hill Climb");

            aerotechController.Commands.Execute("WAIT MODE INPOS");
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.HillClimb.HCAxis.Value = 1;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.HillClimb.HCInputMode.Value = 0;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.HillClimb.HCScanIncrement.Value = 0.0025;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.HillClimb.HCMaxDisplacement.Value = 0.4;

            aerotechController.Commands.Motion.Fiber.HillClimb();
            Debug.Print("Completed hill climb");
        }

        public double GetThorlabsPower()
        {
            powerMeter.RawIO.Write("MEAS:POW?\n");
            return Double.Parse(powerMeter.RawIO.ReadString());
        }

        public void OpenRelay(int relayNum)
        {
            string strCmdText;
            strCmdText = $"{RelaySerial} open {relayNum}";
            Process.Start("relay.exe", strCmdText);
            Thread.Sleep(250);
        }

        public void CloseRelay(int relayNum)
        {
            string strCmdText;
            strCmdText = $"{RelaySerial} close {relayNum}";
            Process.Start("relay.exe", strCmdText);
            Thread.Sleep(250);
        }

        public double GetAerotechAnalogVoltage()
        {
            var v = aerotechController.Commands.IO.AnalogInput(0, "X");
            Debug.Print($"Aerotech voltage: {v}");

            return v;
        }

        public void AerotechAbort()
        {
            aerotechController.Tasks[TaskId.TLibrary].Program.Stop();
        }

        public void PrintAerotechPosition()
        {
            // TODO: print / return the aerotech position 
        }
    }
}
