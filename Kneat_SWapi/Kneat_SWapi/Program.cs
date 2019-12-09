using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Kneat_Challenge
{
    class Program
    {
        private static List<Starships> StarshipsData = new List<Starships>();
        private static List<ResultData> Results = new List<ResultData>();

        private const string end_point = "starships?page=";
        private const string root_url = "https://swapi.co/api/";
        private const int hours_per_day = 24;
        private const int hours_per_week = 168;
        private const int hours_per_month = 720;
        private const int hours_per_year = 8760;

        private static int Distance = 0;

        static void Main(string[] args)
        {
            try
            {
                ReadStarshipData("1");

                if (StarshipsData != null && StarshipsData.Count > 0)
                {
                    Console.Clear();
                    RequestTravelDistance();
                }
            }
            catch (Exception ex) { Console.WriteLine("Error: " + ex.Message); }
            finally { RequestTravelDistance(); }
        }

        /// <summary>
        /// Returns all the starship data from swapi.co
        /// </summary>
        /// <param name="page_no"></param>
        static void ReadStarshipData(string page_no)
        {
            try
            {
                string data = "";
                string response = API.CreateObject(root_url + end_point + page_no, data);
                Starship_Results _starship_results = Newtonsoft.Json.JsonConvert.DeserializeObject<Starship_Results>(response.ToString());

                foreach (Starships Starship in _starship_results.results)
                {
                    StarshipsData.Add(Starship);
                }

                if (_starship_results.next != null)
                {
                    string page = HttpUtility.ParseQueryString(new System.Uri(_starship_results.next).Query).Get("page");
                    ReadStarshipData(page);
                }
            }
            catch (Exception ex) { throw ex; }
        }

        /// <summary>
        /// Writes all the calculated values to the console window for the inputted distance
        /// </summary>
        private static void OutputResults()
        {
            try
            {
                Console.Clear();
                Console.WriteLine("Your results for a travel distance of " + Distance + " are:");
                Console.WriteLine("");
                foreach (ResultData rd in Results)
                {
                    Console.WriteLine(rd.name + ": " + (rd.SupplyStops == -1 ? "Cannot Calculate" : rd.SupplyStops.ToString("###,###,###")));
                }
                Console.WriteLine("");
            }
            catch (Exception ex) { throw ex; }
        }

        /// <summary>
        /// Requests the desired travel distance, from the user, for the application to use for it's calculations
        /// </summary>
        private static void RequestTravelDistance()
        {
            try
            {
                Console.Write("Enter total distance to travel: ");
                string val = Console.ReadLine();

                if (val.ToLower() != "exit")
                {
                    if (int.TryParse(val, out Distance))
                    {
                        CalculateReturnValues(Distance);

                        if (Results.Count > 0)
                        {
                            OutputResults();

                            RequestTravelDistance();
                        }
                    }
                    else
                    {
                        Console.WriteLine(val + " is not a valid distance value");
                        Console.WriteLine("");
                        RequestTravelDistance();
                    }
                }
                else { System.Environment.Exit(1); }
            }
            catch (Exception ex) { throw ex; }
        }

        /// <summary>
        /// Builds the results for later output to the console window
        /// </summary>
        /// <param name="Distance">The distance that the starship needs to travel</param>
        private static void CalculateReturnValues(long Distance)
        {
            string ShipName = "";
            try
            {
                if (Results.Count > 0)
                {
                    Results.Clear();
                }

                foreach (Starships starship in StarshipsData)
                {
                    ResultData rd = new ResultData
                    {
                        name = starship.name,
                        SupplyStops = (starship.MGLT == "unknown" || starship.consumables == "unknown" ? -1 : ReturnSupplyStopCount(Distance, Int64.Parse(starship.MGLT), ConvertToHours(starship.consumables)))
                    };

                    Results.Add(rd);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ShipName);
                throw ex;
            }
        }

        /// <summary>
        /// Calculates the number of supply stops required for each of the starships
        /// </summary>
        /// <param name="Distance">The distance that the starship needs to travel</param>
        /// <param name="MGLT">The Maximum number of Megalights this starship can travel in a standard hour. A "Megalight" is a standard unit of distance and has never been defined before within the Star Wars universe. This figure is only really useful for measuring the difference in speed of starships. We can assume it is similar to AU, the distance between our Sun (Sol) and Earth.</param>
        /// <param name="ConsumablesHours">The maximum length of time (in hours) that this starship can provide consumables for its entire crew without having to resupply</param>
        /// <returns>The number of times the starship will need to re-supply to enable it to get to it's destination</returns>
        private static Int64 ReturnSupplyStopCount(long Distance, long MGLT, int ConsumablesHours)
        {
            Int64 StopCount = 0;

            try
            {
                double Result = (Distance / MGLT) / ConsumablesHours;
                StopCount = Int64.Parse(Math.Ceiling(Result).ToString());
            }
            catch (Exception ex) { throw ex; }

            return (StopCount);
        }

        /// <summary>
        /// Converts the consumables string into a usable nummber of hours
        /// </summary>
        /// <param name="Consumables">The maximum length of time that this starship can provide consumables for its entire crew without having to resupply</param>
        /// <returns>The number of hours that the starship can provide consumables for its entire crew without having to resupply</returns>
        private static int ConvertToHours(string Consumables)
        {
            int Hours = 0;

            try
            {
                if (Consumables != "unknown")
                {
                    if (Consumables.ToLower().Contains("hour")) { Hours = int.Parse(Consumables.ToLower().Replace(" hours", "").Replace(" hour", "")); }
                    else if (Consumables.ToLower().Contains("day")) { Hours = int.Parse(Consumables.ToLower().Replace(" days", "").Replace(" day", "")) * hours_per_day; }
                    else if (Consumables.ToLower().Contains("week")) { Hours = int.Parse(Consumables.ToLower().Replace(" weeks", "").Replace(" week", "")) * hours_per_week; }
                    else if (Consumables.ToLower().Contains("month")) { Hours = int.Parse(Consumables.ToLower().Replace(" months", "").Replace(" month", "")) * hours_per_month; }
                    else if (Consumables.ToLower().Contains("year")) { Hours = int.Parse(Consumables.ToLower().Replace(" years", "").Replace(" year", "")) * hours_per_year; }
                }
                else { Hours = 0; }
            }
            catch (Exception ex) { throw ex; }

            return (Hours);
        }

    }

    class Starship_Results
    {
        public int count { get; set; }
        public string next { get; set; }
        public string previous { get; set; }
        public List<Starships> results { get; set; }
    }

    class Starships
    {
        public string name { get; set; }
        public string model { get; set; }
        public string manufacturer { get; set; }
        public string cost_in_credits { get; set; }
        public string length { get; set; }
        public string max_atmosphering_speed { get; set; }
        public string crew { get; set; }
        public string passengers { get; set; }
        public string cargo_capacity { get; set; }
        public string consumables { get; set; }
        public string hyperdrive_rating { get; set; }
        public string MGLT { get; set; }
        public string starship_class { get; set; }
        public List<string> pilots { get; set; }
        public List<string> films { get; set; }
        public string created { get; set; }
        public string edited { get; set; }
        public string url { get; set; }
    }

    class ResultData
    {
        public string name { get; set; }
        public Int64 SupplyStops { get; set; }
    }


}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Kneat_SWapi
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//        }
//    }
//}
