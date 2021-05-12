﻿using System;
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

        public double alignmentPowerCalibration = 500.0;

        private Instruments()
        {
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

                session2.RawIO.Write("SENS:CORR 27\n");

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

            try
            {
                aerotechController.Commands.Motion.Linear("Z", -100);
            }
            catch (A3200Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                aerotechController.Parameters.Axes["Z"].Limits.LimitDebounceDistance.Value = 0;
                aerotechController.Commands.Axes["Z"].Motion.FaultAck();
            }

            try
            {
                aerotechController.Commands.Motion.Linear("Y", 100);
            }
            catch (A3200Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                aerotechController.Parameters.Axes["Y"].Limits.LimitDebounceDistance.Value = 0;
                aerotechController.Commands.Axes["Y"].Motion.FaultAck();
            }

            try
            {
                aerotechController.Commands.Motion.Linear("X", 100);
            }
            catch (A3200Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                aerotechController.Parameters.Axes["X"].Limits.LimitDebounceDistance.Value = 0;
                aerotechController.Commands.Axes["X"].Motion.FaultAck();
            }

            aerotechController.Reset();

            aerotechController.Commands.Axes["Y"].Motion.Enable();
            aerotechController.Commands.Axes["X"].Motion.Enable();
            aerotechController.Commands.Axes["Z"].Motion.Enable();

            aerotechController.Commands.Motion.Linear("X", -10.3059);
            aerotechController.Commands.Motion.Linear("Y", -13.5771);
            // aerotechController.Commands.Motion.Linear("Z", 18.9341);
            aerotechController.Commands.Motion.Linear("Z", 17.6605);
        }

        public void FindFirstLight()
        {
            this.SetArroyoLaserOn();

            Thread.Sleep(3200);

            aerotechController.Commands.Execute("WAIT MODE INPOS");

            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.SpiralFine.SFAxis1.Value = 0;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.SpiralFine.SFAxis2.Value = 2;

            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.SpiralFine.SFMotionType.Value = 0;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.SpiralFine.SFEndRadius.Value = 0.2;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.SpiralFine.SFNumSpirals.Value = 20;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.SpiralFine.SFSegmentLength.Value = 0.05;
            aerotechController.Commands.Motion.Fiber.SpiralFine();
        }

        public void FindCentroid()
        {
            double firstPower = this.GetThorlabsPower();
            double edgeValue = firstPower * 0.75; 
            Debug.Print("Using first light power: {0}", firstPower);
            Debug.Print("Using edge value: {0}", edgeValue);

            aerotechController.Commands.Execute("WAIT MODE INPOS");
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CInputMode.Value = 0;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CEdgeValue.Value = edgeValue;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CScanIncrement.Value = 0.00025;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CMaxDisplacement1.Value = 1;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CMaxDisplacement2.Value = 1;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CMaxDisplacement3.Value = 1;
            aerotechController.Parameters.Tasks[TaskId.TLibrary].Fiber.Centroid.CReturnToCenter.Value = 1;

            aerotechController.Commands.Motion.Fiber.Centroid3D();
        }

        public double GetThorlabsPower()
        {
            powerMeter.RawIO.Write("MEAS:POW?\n");
            return Double.Parse(powerMeter.RawIO.ReadString());
        }

        public void OpenRelay(int relayNum)
        {
            string strCmdText;
            strCmdText = $"QAAMZ open {relayNum}";
            Process.Start("relay.exe", strCmdText);
            Thread.Sleep(250);
        }

        public void CloseRelay(int relayNum)
        {
            string strCmdText;
            strCmdText = $"QAAMZ close {relayNum}";
            Process.Start("relay.exe", strCmdText);
            Thread.Sleep(250);
        }
    }
}
