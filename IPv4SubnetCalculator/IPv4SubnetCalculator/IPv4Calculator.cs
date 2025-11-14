using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

public struct SubnetInfo
{
    public string name;
    public int hosCount;
    public int predictedAssignedRange;
}

namespace IPv4SubnetCalculator
{
    public class IPv4Calculator
    {
        //The network id that we start with.
        uint baseId;

        //The network prefix we start with.
        int basePrefix;

        //The number of subnets that we need.
        int numberOfSubnets;

        //List of info for each subnet.
        List<SubnetInfo> subnets = new List<SubnetInfo>();

        int possibleHostCount = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        public IPv4Calculator()
        {

        }

        /// <summary>
        /// Enters the base network information.
        /// </summary>
        public void EnterNetworkInfo()
        {
            //Asks the user to enter base network id, and gets the input from the console.
            //Console.Write("Enter base network ID, and prefix (such as 192.168.182.0/24)... ");
            string input = "";


            //Splits the input into ip address, and subnet prefix
            string[] parts = { };

            //Stores the base network prefix
            basePrefix = -1;

            string baseNetowkrIpAddress = "";

            while(!IsEnteredBaseNetworkValid(baseNetowkrIpAddress, basePrefix))
            {
                Console.Clear();

                if(parts.Length > 0)
                {
                    if (!IsEnteredIpValid(parts[0]))
                    {
                        Console.WriteLine("Entered IP-address was invalid.");
                    }

                    if (!IsValidPrefix(basePrefix))
                    {
                        Console.WriteLine("Entered prefix was invalid.");
                    }
                }

                //Asks the user to enter base network id, and gets the input from the console.
                Console.Write("Enter base network ID, and prefix (such as 192.168.182.0/24)... ");
                input = Console.ReadLine();

                //Removes possible white space.
                input.Replace(" ", "");

                //Splits the input into ip address, and subnet prefix
                parts = input.Split('/');


                baseNetowkrIpAddress = parts[0];

                if (parts.Length >= 2)
                {
                    //Stores the base network prefix
                    basePrefix = int.Parse(parts[1].Replace(" ", ""));
                }
            }

            //Stores the base network id.
            baseId = IpToInt(IPAddress.Parse(parts[0].Replace(" ", "")));

    
            //Clears screen, and writes the base network information, as well as possible host count.
            possibleHostCount = (int)Math.Pow(2, (32 - basePrefix)) - 2;

            Console.Clear();
            PrintBaseNetworkInfo();
            Console.WriteLine($"\nPossible number of hosts: {possibleHostCount}.\n");
        }

        /// <summary>
        /// Enters information about how many subnets is needed, and how many hosts.
        /// </summary>
        public void EnterSubnetInfo()
        {
            //Empties list, in case it is not.
            subnets.Clear();

            //Asks user to enter the number of needed subnets.
            Console.Write("\nEnter number of subnets... ");
            numberOfSubnets = int.Parse(Console.ReadLine());

            //Runs for every needed subnet.
            for (int i = 0; i < numberOfSubnets; i++)
            {
                //Cleans up screen.
                PrintSubnetSetUpInfo(i);

                if(possibleHostCount == 0)
                {
                    Console.WriteLine("No more possible hosts available on the network.");
                    Console.WriteLine("Press any button to go to calculation.");
                    Console.ReadKey();
                    break;
                }

                //Temporarily stores subnet information, until it's ready to be added to the list.
                SubnetInfo info;

                //Ask user to enter name of the new subnet.
                Console.Write("Enter subnet name... ");
                info.name = Console.ReadLine();
                info.hosCount = 0;
                info.predictedAssignedRange = 0;


                do
                {
                    if(!IsHostWishPossible(info.hosCount))
                    {
                        Console.Clear();
                        PrintSubnetSetUpInfo(i);

                        Console.WriteLine("Enters numbers of hosts in subnet no possible.");
                        Console.WriteLine($"Only {possibleHostCount} host(s) availabel on the while network.\n");
                        Console.Write($"Enter subnet name... {info.name}\n");
                    }

                    

                    //Asks user to enter the number of hosts, for the new subnet.
                    Console.Write("Enter number of hosts... ");
                    info.hosCount = int.Parse(Console.ReadLine());
                    info.predictedAssignedRange = PredictNeededIpRange(info.hosCount);
                } while (!IsHostWishPossible(info.hosCount));

                //Adds the information to the list.
                subnets.Add(info);

                possibleHostCount -= PredictNeededIpRange(info.hosCount);
            }
        }

        /// <summary>
        /// Clalculates the subnets.
        /// </summary>
        public void CalculateSubnets()
        {
            //Cleans up screen, and prints the information the user entered.
            Console.Clear();
            PrintBaseNetworkInfo();
            Console.WriteLine("\nEntered subnets:");
            Console.WriteLine($"  {"Name:",-20}{"Number of hosts:"}");


            foreach (SubnetInfo si in subnets)
            {
                Console.WriteLine($"  {si.name,-20}{si.hosCount}");
            }

            Console.WriteLine("\n==========================================================\n");

            //Sports the subnet by which one has the largest host count.
            subnets.Sort((a, b) => b.hosCount.CompareTo(a.hosCount));

            //The network that is currently being subnetted.
            //Set to the base network, as the first subnet will have that Id.
            uint currentNetwork = baseId;

            //The subnet list works as a list for all needed subnets.
            //Runs through all needed subnets, entered by the user.
            foreach (SubnetInfo si in subnets)
            {
                //Stores host count.
                int hostsNeeded = si.hosCount;

                //Calculates how many hostbits is needed.
                int hostBitsNeeded = (int)Math.Ceiling(Math.Log2(hostsNeeded + 2));

                //Calculates the prefix for the new subnet.
                int newPrefix = 32 - hostBitsNeeded;

                //Calculates the size of the new subnet, including Id and Broadcast.
                int subnetSize = (int)Math.Pow(2, hostBitsNeeded);

                //Gets the id for the new subnet,
                uint netowkrId = currentNetwork;

                //Calculates the broadcast address.
                //-1 because the size includes the Id
                uint broadcastAddress = currentNetwork + (uint)subnetSize - 1;

                //Gets the first usable address.
                uint firstHost = netowkrId + 1;

                //Gets the last usable address.
                uint lastHost = broadcastAddress - 1;

                //Prints subnet information into console.
                Console.WriteLine(si.name);
                PrintAligned("  Required amount of hosts", hostsNeeded.ToString(), "");
                PrintAligned("  Network ID", $"{IntToString(netowkrId)}/{newPrefix}", $"{IntToBinary(netowkrId)}");
                PrintAligned("  Broadcast address", IntToString(broadcastAddress), $"{IntToBinary(broadcastAddress)}");
                PrintAligned("  Usable addresses", $"{IntToString(firstHost)} - {IntToString(lastHost)}", $"{IntToBinary(firstHost)} - {IntToBinary(lastHost)}");
                PrintAligned("  Subnet mask", IntToString(PrefixToMask(newPrefix)), $"{IntToBinary(PrefixToMask(newPrefix))}");

                Console.WriteLine();

                //Sets the next network that are being subnetted.
                currentNetwork += (uint)subnetSize;
            }
        }



        /// <summary>
        /// Converts an ip address to a uint32.
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        uint IpToInt(IPAddress ip)
        {
            //Gets the address as a byte array.
            byte[] bytes = ip.GetAddressBytes();
            Array.Reverse(bytes);

            //Converts to uint32, and returns it.
            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        /// Gets the full subnet mask, from subnet prefix.
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        uint PrefixToMask(int prefix)
        {
            //If the prefix is 0...
            if (prefix == 0)
            {
                //Return a 0 subnet mask.
                return 0;
            }
            else
            {
                //Gets the subnet mask through bitshifting the prefix, and returns it.
                return uint.MaxValue << (32 - prefix);
            }
        }

        /// <summary>
        /// Converts a uint to string.
        /// </summary>
        /// <param name="Uint"></param>
        /// <returns></returns>
        string IntToString(uint Uint)
        {
            //Gets a byte arrya from the uint.
            byte[] bytes = BitConverter.GetBytes(Uint);
            Array.Reverse(bytes);

            //Converts to string, and reutrns it.
            return new IPAddress(bytes).ToString();
        }

        /// <summary>
        /// Converts a unint to string of binary value.
        /// </summary>
        /// <param name="Int"></param>
        /// <returns></returns>
        string IntToBinary(uint Int)
        {
            //Gets byte arrya.
            byte[] bytes = BitConverter.GetBytes(Int);
            Array.Reverse(bytes);

            //Returns the string, by converting the array to base 2, for binary, and ensures the value is 8 characters long, inserting '0' into empty spots.
            return string.Join(".", bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
        }

        /// <summary>
        /// Prints the base network information in the console.
        /// </summary>
        private void PrintBaseNetworkInfo()
        {
            Console.WriteLine($"{"Base network:",-21}{IntToString(baseId)}/{basePrefix}");
            Console.WriteLine($"{"Base subnet mask:",-21}{IntToString(PrefixToMask(basePrefix))} ({IntToBinary(PrefixToMask(basePrefix))})");
        }

        /// <summary>
        /// Prints subnet information in alignment.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="ipString"></param>
        /// <param name="binaryString"></param>
        private void PrintAligned(string label, string ipString, string binaryString)
        {
            //Check if it should print binary information.
            if (binaryString.Replace(" ", "") != "")
            {

                Console.WriteLine($"{label,-30} : {ipString,-35} ({binaryString})");
            }
            else
            {
                Console.WriteLine($"{label,-30} : {ipString,-35}");
            }
        }

        private bool IsEnteredBaseNetworkValid(string ip, int prefix)
        {
            if(!IsEnteredIpValid(ip) || !IsValidPrefix(prefix))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if a given subnet prefix is valid.
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        private bool IsValidPrefix(int prefix)
        {
            if(prefix >= 0 && prefix <= 30)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if a give IP address is valid.
        /// </summary>
        /// <param name="enteredIp"></param>
        /// <returns></returns>
        private bool IsEnteredIpValid(string enteredIp)
        {
            string[] stringParts = enteredIp.Split('.');
            try
            {
                foreach (string part in stringParts)
                {
                    int i = int.Parse(part);

                    if (i < 0 || i > 255)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether or not a subnet can fit in the network, based on the amount of hosts it needs.
        /// </summary>
        /// <param name="neededNumberOfHosts"></param>
        /// <returns></returns>
        private bool IsHostWishPossible(int neededNumberOfHosts)
        {
            if (possibleHostCount - PredictNeededIpRange(neededNumberOfHosts) < 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Prints the header with information, when entering subnet information.
        /// </summary>
        /// <param name="i"></param>
        private void PrintSubnetSetUpInfo(int i)
        {
            Console.Clear();
            PrintBaseNetworkInfo();

            Console.WriteLine($"\nPossible hosts remaining: {possibleHostCount}");

            Console.WriteLine($"\n\nEnter info for subnet ({i + 1}/{numberOfSubnets})...");

            for (int y = 0; y < subnets.Count(); y++)
            {
                Console.WriteLine($"  {y + 1}. {subnets[y].name,-10}{subnets[y].hosCount, -10}(Predicted IP range: {subnets[y].predictedAssignedRange})");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Predicts the number of IP address a subnet is going to need, based in binary table.
        /// The output includes Network id address and Broadcast address.
        /// </summary>
        /// <param name="numberOfHosts"></param>
        /// <returns></returns>
        private int PredictNeededIpRange(int numberOfHosts)
        {
            int hostBitsNeeded = (int)Math.Ceiling(Math.Log2(numberOfHosts + 2));
            int newPrefix = 32 - hostBitsNeeded;
            int subnetSize = (int)Math.Pow(2, hostBitsNeeded);

            return subnetSize;
        }
    }
}

