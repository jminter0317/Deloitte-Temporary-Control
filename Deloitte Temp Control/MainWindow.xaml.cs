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
using System.Windows.Threading;
using MiRLibWpf;

namespace Deloitte_Temp_Control
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //these dictionaries hold the names of the positions as a string. They have a key, so they can be accessed by the random number generator
        public static Dictionary<int, string> docks = new Dictionary<int, string>();
        public static Dictionary<int, string> holdingPositions = new Dictionary<int, string>();
        public static Dictionary<int, string> chargers = new Dictionary<int, string>();
        public static Dictionary<MissionType, string> missions = new Dictionary<MissionType, string>();
        public static Dictionary<string, int> robNames = new Dictionary<string, int>();
        //robots does not need to be randomly accessed, so it is not a dictionary
        public static List<Robot> robots = new List<Robot>();

        //enum mission types
        public enum MissionType
        {
            Charging = 1,
            Holding = 2,
            Line = 3
        }

        //user parameters set in the UI
        //amount of seconds between starting robot motion tasks
        public static int Interval = 30;
        //amount of time the robot will wait at the holding position and at the conveyer
        public static int WaitTime = 15;
        //minimum battery the robot must have before going to the holding position and the line
        public static int MinimumBattery = 25;

        public MainWindow()
        {
            InitializeComponent();

            //create the dictionaries of positions and their keys
            PopulateDictionaries();
            //create the list of robots
            robots = PopulateRobots();

            Task iMonitor = new Task(() => InactiveRobotMonitor());
            iMonitor.Start();

            Task aMonitor = new Task(() => ActiveRobotMonitor());
            aMonitor.Start();
        }
        public static void InactiveRobotMonitor()
        {
            while(true)
            {
                List<int> inactiveRobots = new List<int>();
                for (int i = 0; i < robots.Count; i++)
                {
                    if (!robots[i].run)
                    {
                        inactiveRobots.Add(i);
                    }
                }

                foreach(int i in inactiveRobots)
                {
                    try
                    {
                        //try to get the info from the robot
                        RobStatus status = robots[i].connection.GetRobStatus();
                        robots[i].RobotName = status.robName;
                        robots[i].BatteryPercentage = (double)status.battery;
                        robots[i].MissionText = robots[i].MissionText;
                        robots[i].MissionQueue = robots[i].connection.getMissionsInQueue().Length;

                        if (!robots[i].lineTask && !robots[i].chargingTask && !robots[i].runOnce)
                        {
                            Task ChargingTask = new Task(() => RobotChargeTask(i));
                            ChargingTask.Start();
                            robots[i].runOnce = true;
                        }
                    }
                    catch
                    {
                        continue;
                    }

                    Thread.Sleep(Interval * 1000);

                }      
            }
        }

        public static void ActiveRobotMonitor()
        {
            while (true)
            {
                List<int> ActiveRobots = new List<int>();
                for (int i = 0; i < robots.Count; i++)
                {
                    if (robots[i].run)
                    {
                        ActiveRobots.Add(i);
                    }
                }

                foreach(int i in ActiveRobots)
                {
                    try
                    {
                        //try to get the info from the robot
                        RobStatus status = robots[i].connection.GetRobStatus();
                        robots[i].RobotName = status.robName;
                        robots[i].BatteryPercentage = (double)status.battery;
                        robots[i].MissionText = robots[i].MissionText;
                        robots[i].MissionQueue = robots[i].connection.getMissionsInQueue().Length;

                        if (robots[i].BatteryPercentage > MinimumBattery && robots[i].run)
                        {
                            robots[i].runOnce = false;
                            if (!robots[i].lineTask && !robots[i].chargingTask)
                            {
                                Task lineTask = new Task(() => RobotLineTask(i));
                                lineTask.Start();
                            }

                        }
                        else 
                        {
                            if (!robots[i].lineTask && !robots[i].chargingTask && !robots[i].runOnce)
                            {
                                Task ChargingTask = new Task(() => RobotChargeTask(i));
                                ChargingTask.Start();
                                robots[i].runOnce = true;
                            }
                        }


                    }
                    catch
                    {
                        //if we cannot get info from the robot, just move on to the next one
                        continue;
                    }

                    Thread.Sleep(Interval * 1000);
                }
            }
        }

        public static List<Robot> PopulateRobots()
        {
            //172.17.60.201
            //172.17.60.202
            //172.17.60.203
            //172.17.60.204
            //172.17.60.205

            //create robots with their IP Address
            List<Robot> robots = new List<Robot>();
            robots.Add(new Robot("172.17.60.201"));
            robots.Add(new Robot("172.17.60.202"));
            robots.Add(new Robot("172.17.60.203"));
            robots.Add(new Robot("172.17.60.204"));
            robots.Add(new Robot("172.17.60.205"));

            return robots;
        }

        public static void PopulateDictionaries()
        {
            //dock names
            docks.Add(1, "Dock A");
            docks.Add(2, "Dock B");
            docks.Add(3, "Dock C");
            docks.Add(4, "Dock D");
            docks.Add(5, "Dock E");
            docks.Add(6, "Dock F");
            docks.Add(7, "Dock G");
            docks.Add(8, "Dock H");
            docks.Add(9, "Dock I");
            docks.Add(10, "Dock J");

            //position names
            holdingPositions.Add(1, "LineUp1");
            holdingPositions.Add(2, "LineUp2");
            holdingPositions.Add(3, "LineUp3");
            holdingPositions.Add(4, "LineUp4");
            holdingPositions.Add(5, "LineUp5");

            //charger names
            chargers.Add(1, "ChargingStation1");
            chargers.Add(2, "ChargingStation3");
            chargers.Add(3, "ChargingStation2");
            chargers.Add(4, "ChargingStation4");
            chargers.Add(5, "ChargingStation5");

            missions.Add(MissionType.Charging, "Charge");
            missions.Add(MissionType.Holding, "Holding");
            missions.Add(MissionType.Line, "Line");

            robNames.Add("MiR_201803066", 1);
            robNames.Add("MiR_201803057", 2);
            robNames.Add("MiR_201803068", 3);
            robNames.Add("MiR_201803056", 4);
            robNames.Add("MiR_201803064", 5);
        }

        public static void RobotLineTask(int index)
        {
            robots[index].lineTask = true;

            //go to holding position if not already at one
            if (robots[index].Holding == 0)
            {
                GoToHoldingPosition(index);
            }

            Thread.Sleep(WaitTime * 1000);

            GoToDock(index);

            Thread.Sleep(WaitTime * 1000);

            GoToHoldingPosition(index);

            robots[index].lineTask = false;
        }

        public static void RobotChargeTask(int index)
        {
            robots[index].chargingTask = true;

            //get the name of the robot's charger
            int chargerKey = 0;
            robNames.TryGetValue(robots[index].RobotName, out chargerKey);

            string chargerName = "";
            string missionName = "";
            chargers.TryGetValue(chargerKey, out chargerName);
            missions.TryGetValue(MissionType.Charging, out missionName);

            //append the mission to the robot to send it to the charger
            string[,] args = { { "Charger" , chargerName } };

            //take that dock so no one else can
            robots[index].Dock = 0;
            robots[index].Holding = 0;
            robots[index].Charger = chargerKey;
            robots[index].connection.AppendMissionToQueue(missionName, true, true, args, true);

            robots[index].chargingTask = false;
        }
        //loop every set amount of time
        //if a robot is doing nothing and the battery percentage is above the slider amount
        public static void GoToHoldingPosition(int index)
        {
            int holdingPosition = 1;

            //pick a holding position that someone is not at
            while (holdingPosition <= holdingPositions.Count)
            {
                if (robots.Exists(x => x.Holding == holdingPosition))
                {
                    ++holdingPosition;
                }
                else
                {
                    break;
                }
            }

            //take that position so no one else can
            robots[index].Holding = holdingPosition;
            robots[index].Charger = 0;
            robots[index].Dock = 0;

            string holdingName = "";
            string holdingMission = "";
            //get the names of the missions and positions that we will go to
            holdingPositions.TryGetValue(holdingPosition, out holdingName);
            missions.TryGetValue(MissionType.Holding, out holdingMission);

            //append the mission to the  queue
            string[,] args = { { "Position" , holdingName } };
            robots[index].connection.AppendMissionToQueue(holdingMission, true, true, args, true);

            WaitForQueueZero(index);
        }
        public static void GoToDock(int index)
        {
            Random random = new Random();
            int dock = random.Next(1, docks.Count);

            //pick a dock that someone is not at
            while (robots.Exists(x => x.Dock == dock))
            {
                dock = random.Next(1, docks.Count);
            }

            //take that dock so no one else can
            robots[index].Dock = dock;
            robots[index].Holding = 0;
            robots[index].Charger = 0;

            string dockName = "";
            string dockMission = "";

            docks.TryGetValue(dock, out dockName);
            missions.TryGetValue(MissionType.Line, out dockMission);

            //append the mission to the  queue
            string[,] args = { { "Position" , dockName } };
            robots[index].connection.AppendMissionToQueue(dockMission, true, true, args, true);

            WaitForQueueZero(index);
        }

        public static void WaitForQueueZero(int index)
        {
            int missionQueue = 1;
            while (missionQueue != 0)
            {
                missionQueue = robots[index].connection.getMissionsInQueue().Length;
                robots[index].MissionQueue = missionQueue;
                Thread.Sleep(1000);
            }
        }

        private void RobotOneRun_Checked(object sender, RoutedEventArgs e)
        {
            robots[0].run = (bool)RobotOneRun.IsChecked;
        }

        private void RobotTwoRun_Checked(object sender, RoutedEventArgs e)
        {
            robots[1].run = (bool)RobotTwoRun.IsChecked;
        }

        private void RobotThreeRun_Checked(object sender, RoutedEventArgs e)
        {
            robots[2].run = (bool)RobotThreeRun.IsChecked;
        }

        private void RobotFourRun_Checked(object sender, RoutedEventArgs e)
        {
            robots[3].run = (bool)RobotFourRun.IsChecked;
        }

        private void RobotFiveRun_Checked(object sender, RoutedEventArgs e)
        {
            robots[4].run = (bool)RobotFiveRun.IsChecked;
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RobotOneB.Content = "Battery: " + (Convert.ToInt32(robots[0].BatteryPercentage)).ToString();
            RobotTwoB.Content = "Battery: " + (Convert.ToInt32(robots[1].BatteryPercentage)).ToString();
            RobotThreeB.Content = "Battery: " + (Convert.ToInt32(robots[2].BatteryPercentage)).ToString();
            RobotFourB.Content = "Battery: " + (Convert.ToInt32(robots[3].BatteryPercentage)).ToString();
            RobotFiveB.Content = "Battery: " + (Convert.ToInt32(robots[4].BatteryPercentage)).ToString();
        }
    }
}
