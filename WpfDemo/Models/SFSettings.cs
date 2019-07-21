﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace WpfDemo
{
    public class BLEGravitySensorConfig
    {
        /// <summary>
        /// device id of BLE gravity sensors
        /// </summary>
        public string Id;

        /// <summary>
        /// User friendly name
        /// </summary>
        public string Name;

        /// <summary>
        /// MAC address;
        /// </summary>
        public string MACAddress;

        /// <summary>
        /// Baseline vector point to the top of monitor
        /// </summary>
        public Vector3? Baseline;

        /// <summary>
        /// Rotary axis
        /// </summary>
        public Vector3? Normal;


        /// <summary>
        /// Id of binding monitor
        /// </summary>
        public string BindedMonitor;

    }
    public class SFSettings
    {
        public List<BLEGravitySensorConfig> sensors;

        public SFSettings()
        {
            sensors = new List<BLEGravitySensorConfig>();
        }
    }
}