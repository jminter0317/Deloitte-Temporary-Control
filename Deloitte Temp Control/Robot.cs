using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MiRLibWpf;

namespace Deloitte_Temp_Control
{
	public class Robot
	{
		//172.17.60.201
		//172.17.60.202
		//172.17.60.203
		//172.17.60.204
		//172.17.60.205
		public Robot()
        {
			RobotName = "";
			IPAddress = "";
			MissionText = "";
			BatteryPercentage = 0;
			MissionQueue = 0;
			Holding = 0;
			Dock = 0;
			Charger = 0;
			lineTask = false;
			chargingTask = false;
			this.runOnce = false;
        }
		public Robot(string _IPAddress)
		{
			this.RobotName = "";
			this.MissionText = "";
			this.BatteryPercentage = 0;
			this.MissionQueue = 0;
			this.Holding = 0;
			this.Dock = 0;
			this.Charger = 0;
			this.IPAddress = _IPAddress;
			this.run = false;
			this.connection = new MiRConnection("distributor", "distributor", this.IPAddress);
			this.lineTask = false;
			this.runOnce = false;
			this.chargingTask = false;
		}
		public MiRLibWpf.MiRConnection connection;
		public string RobotName { get; set; }
		public string MissionText { get; set; }
		public double BatteryPercentage { get; set; }
		public int MissionQueue { get; set; }
		public int Holding { get; set; }
		public int Dock { get; set; }
		public int Charger { get; set; }
		public string IPAddress { get; set; }
		public bool run { get; set; }
		public bool runOnce { get; set; }
		public bool lineTask { get; set; }
		public bool chargingTask { get; set; }
	}
}