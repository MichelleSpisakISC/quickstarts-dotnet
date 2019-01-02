﻿/*
* PURPOSE: Makes a connection to an instance of InterSystems IRIS Data Platform wth ADO.NET
* to perform following activities:
*      1. View top 10 stock
*      2. Create the portfolio table in InterSystems IRIS Data Platform to store your personal stock portfolio information
*      3. Add several portfolio items
*      4. Update Portfolio item
*      5. Delete a portfolio item
*      6. Simulate adding stocks to your stock portfolio and see how you would have done.
*/

using System;
using System.Collections.Generic;
using InterSystems.Data.IRISClient;

namespace myApp
{
    class adonetplaystocks
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // Initialize dictionary to store connection details from config.txt
            IDictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary = generateConfig("..\\..\\..\\config.txt");

            // Retrieve connection information from configuration file
            string ip = dictionary["ip"];
            int port = Convert.ToInt32(dictionary["port"]);
            string Namespace = dictionary["namespace"];
            string username = dictionary["username"];
            string password = dictionary["password"];

            try
            {
                // Using IRISADOConnection to connect
                IRISADOConnection connect = new IRISADOConnection();

                // Create connection string
                connect.ConnectionString = "Server = " + ip + "; Port = " + port + "; Namespace =  " + Namespace + "; Password = " + password + "; User ID = " + username;
                connect.Open();
                Console.WriteLine("Connected to InterSystems IRIS.");

                // Starting interactive prompt
                bool always = true;
                while (always)
                {
                    Console.WriteLine("1. View top 10");
                    Console.WriteLine("2. Create Portfolio table");
                    Console.WriteLine("3. Add to Portfolio");
                    Console.WriteLine("4. Update Portfolio");
                    Console.WriteLine("5. Delete from Portfolio");
                    Console.WriteLine("6. View Portfolio");
                    Console.WriteLine("7. Quit");
                    Console.WriteLine("What would you like to do? ");

                    String option = Console.ReadLine();
                    switch (option)
                    {

                        // Task 2
                        case "1":
                            Task2(connect);
                            break;

                        // Task 3
                        case "2":
                            Task3(connect);
                            break;

                        // Task 4
                        case "3":
                            Task4(connect);
                            break;

                        // Task 5
                        case "4":
                            Task5(connect);
                            break;

                        // Task 6
                        case "5":
                            Task6(connect);
                            break;

                        // Task 7
                        case "6":
                            Task7(connect);
                            break;

                        case "7":
                            Console.WriteLine("Exited.");
                            always = false;
                            break;

                        default:
                            Console.WriteLine("Invalid option. Try again!");
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Interactive prompt failed:\n" + e);
            }
        }

        // Task 2: View top 10 stocks for selected date
        // Note: Choose 2016/08/12 for date
        public static void Task2(IRISADOConnection connect)
        {
            Console.WriteLine("On which date? (YYYY/MM/DD) ");
            String queryDate = Console.ReadLine();
            FindTopOnDate(connect, queryDate);
        }

        // Task 3: Create Portfolio Table
        // Note: We recommend finishing this task first before moving to the Task 4, Task 5 and Task 6
        public static void Task3(IRISADOConnection connect)
        {
            Console.WriteLine("Creating table...");
            CreatePortfolioTable(connect);
        }

        // Task 4: Add item to Portfolio table
        // Note: We recommend choosing stock name using list of stocks generated by Task 2 
        public static void Task4(IRISADOConnection connect)
        {
            Console.WriteLine("Name: ");
            String name = Console.ReadLine();

            Console.WriteLine("Date: ");
            String tDate = Console.ReadLine();

            Console.WriteLine("Price: ");
            String price = Console.ReadLine();

            Console.WriteLine("Number of shares: ");
            String share = Console.ReadLine();
            int shares;
            Int32.TryParse(share, out shares);

            AddPortfolioItem(connect, name, tDate, price, shares);
        }

        // Task 5: Update item in Portfolio table
        public static void Task5(IRISADOConnection connect)
        {
            Console.WriteLine("Which stock would you like to update? ");
            String stockName = Console.ReadLine();

            Console.WriteLine("New Price: ");
            String updatePrice = Console.ReadLine();

            Console.WriteLine("New Date: ");
            String updateDate = Console.ReadLine();

            Console.WriteLine("New number of shares: ");
            String upShare = Console.ReadLine();
            int updateShares;
            Int32.TryParse(upShare, out updateShares);

            UpdateStock(connect, stockName, updatePrice, updateDate, updateShares);
        }

        // Task 6: Delete item from Portfolio table
        public static void Task6(IRISADOConnection connect)
        {
            Console.WriteLine("Which stock would you like to remove? ");
            String removeName = Console.ReadLine();
            DeleteStock(connect, removeName);
        }

        // Task 7: View your Portfolio to see how much you gain/loss
        // Note: Choose option 3 to add 2 or 3 stocks to your portfolio (using names from top 10 and 2016-08-12 as date); then
        //       choose option 6 using date 2017-08-10 to view your % Gain or Loss after a year.
        public static void Task7(IRISADOConnection connect)
        {
            Console.WriteLine("Selling on which date? ");
            String sellDate = Console.ReadLine();
            PortfolioProfile(connect, sellDate);
        }

        // Find top 10 stocks on a particular date
        public static void FindTopOnDate(IRISADOConnection dbconnection, String onDate)
        {
            try
            {
                String sql = "SELECT distinct top 10 TransDate,Name,StockClose,StockOpen,High,Low,Volume FROM Demo.Stock WHERE TransDate=? ORDER BY stockclose desc";
                IRISCommand cmd = new IRISCommand(sql, dbconnection);
                cmd.Parameters.AddWithValue("TransDate", Convert.ToDateTime(onDate));
                IRISDataReader reader = cmd.ExecuteReader();
                Console.WriteLine("Date\t\tName\tOpening Price\tDaily High\tDaily Low\tClosing Price\tVolume");
                while (reader.Read())
                {
                    DateTime date = (DateTime)reader[reader.GetOrdinal("TransDate")];
                    decimal open = (decimal)reader[reader.GetOrdinal("StockOpen")];
                    decimal high = (decimal)reader[reader.GetOrdinal("High")];
                    decimal low = (decimal)reader[reader.GetOrdinal("Low")];
                    decimal close = (decimal)reader[reader.GetOrdinal("StockClose")];
                    int volume = (int)reader[reader.GetOrdinal("Volume")];
                    String name = (string)reader[reader.GetOrdinal("Name")];
                    Console.WriteLine(date.ToString("MM/dd/yyyy") + "\t" + name + "\t" + open + "\t" + high + "\t" + low + "\t" + close + "\t" + volume);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        // Create Portfolio Table
        public static void CreatePortfolioTable(IRISADOConnection dbconnection)
        {
            String createTable = "CREATE TABLE Demo.Portfolio(Name varchar(50) unique, PurchaseDate date, PurchasePrice numeric(10,4), Shares int, DateTimeUpdated DateTime)";
            try
            {
                IRISCommand cmd = new IRISCommand(createTable, dbconnection);
                cmd.ExecuteNonQuery();
                Console.WriteLine("Created Demo.Portfolio table successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Table not created and likely already exists.");
            }
        }

        // Add item to Portfolio Table
        public static void AddPortfolioItem(IRISADOConnection dbconnection, String name, String purchaseDate, String price, int shares)
        {
            DateTime t = DateTime.Now;
            try
            {
                String sql = "INSERT INTO Demo.Portfolio (Name, PurchaseDate, PurchasePrice, Shares, DateTimeUpdated) VALUES (?,?,?,?,?)";
                IRISCommand cmd = new IRISCommand(sql, dbconnection);
                cmd.Parameters.AddWithValue("Name", name);
                cmd.Parameters.AddWithValue("PurchaseDate", Convert.ToDateTime(purchaseDate));
                cmd.Parameters.AddWithValue("PurchasePrice", price);
                cmd.Parameters.AddWithValue("Shares", shares);
                cmd.Parameters.AddWithValue("DateTimeUpdated", t);
                cmd.ExecuteNonQuery();
                Console.WriteLine("Added new line item for stock " + name + ".");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error adding portfolio item: " + e);
            }
        }

        // Update item in Portfolio Table
        public static void UpdateStock(IRISADOConnection dbconnection, String stockname, String price, String transDate, int shares)
        {
            DateTime t = DateTime.Now;
            try
            {
                String sql = "UPDATE Demo.Portfolio SET purchaseDate = ?, purchasePrice= ?, shares = ?, DateTimeUpdated= ? WHERE name= ?";
                IRISCommand cmd = new IRISCommand(sql, dbconnection);
                cmd.Parameters.AddWithValue("PurchaseDate", Convert.ToDateTime(transDate));
                cmd.Parameters.AddWithValue("PurchasePrice", price);
                cmd.Parameters.AddWithValue("Shares", shares);
                cmd.Parameters.AddWithValue("DateTimeUpdated", t);
                cmd.Parameters.AddWithValue("Name", stockname);
                int count = cmd.ExecuteNonQuery();

                if (count > 0)
                {
                    Console.WriteLine(stockname + " updated.");
                }
                else
                {
                    Console.WriteLine(stockname + " not found");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error updating " + stockname + " : " + e);
            }
        }

        // Delete item from Portfolio Table
        public static void DeleteStock(IRISADOConnection dbconnection, String stockname)
        {
            try
            {
                String sql = "DELETE FROM Demo.Portfolio WHERE name = ?";
                IRISCommand cmd = new IRISCommand(sql, dbconnection);
                cmd.Parameters.AddWithValue("Name", stockname);
                int count = cmd.ExecuteNonQuery();
                if (count > 0)
                {
                    Console.WriteLine("Deleted " + stockname + " successfully.");
                }
                else
                {
                    Console.WriteLine(stockname + " not found.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error deleting stock: " + e);
            }
        }

        // View Portfolio Table to see % gain or loss
        public static void PortfolioProfile(IRISADOConnection dbconnection, String sellDate)
        {
            decimal cumulStartValue = 0;
            decimal cumulEndValue = 0;
            DateTime t = DateTime.Now;
            try
            {
                String sql = "SELECT pf.Name, pf.PurchasePrice, pf.PurchaseDate, pf.Shares, pf.DateTimeUpdated, st.StockClose FROM Demo.Portfolio as pf JOIN Demo.Stock as st on st.Name = pf.Name WHERE st.TransDate = ?";
                IRISCommand cmd = new IRISCommand(sql, dbconnection);
                cmd.Parameters.AddWithValue("Name", Convert.ToDateTime(sellDate));
                IRISDataReader reader = cmd.ExecuteReader();
                Console.WriteLine("Name" + " " + "Purchase Date" + " " + "Purchase Price" + " " + "Stock Close\tShares\tDatetime Updated\t% Change" + "   " + "Gain or Loss");

                while (reader.Read())
                {
                    String name = (string)reader[reader.GetOrdinal("Name")];
                    DateTime purchaseDate = (DateTime)reader[reader.GetOrdinal("purchaseDate")];
                    decimal purchasePrice = (decimal)reader[reader.GetOrdinal("PurchasePrice")];
                    decimal stockClose = (decimal)reader[reader.GetOrdinal("StockClose")];
                    int shares = (int)reader[reader.GetOrdinal("Shares")];
                    DateTime dateTimeUpdated = (DateTime)reader[reader.GetOrdinal("DateTimeUpdated")];
                    decimal percentChange = (stockClose - purchasePrice) / (purchasePrice) * 100;
                    decimal startValue = purchasePrice * shares;
                    decimal endValue = stockClose * shares;
                    decimal gainOrLoss = Math.Round(endValue - startValue, 2);
                    cumulStartValue += startValue;
                    cumulEndValue += endValue;
                    Console.WriteLine(name + "\t" + purchaseDate.ToString("MM/dd/yyyy") + "\t" + purchasePrice + "\t  " + stockClose + "\t" + shares + "\t"
                            + dateTimeUpdated + "\t" + String.Format("{0:0.##}", percentChange) + "\t     " + gainOrLoss);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error printing portfolio information: " + e);
            }
        }

        // Helper method: Get connection details from config file
        static IDictionary<string, string> generateConfig(string filename)
        {
            // Initial empty dictionary to store connection details
            IDictionary<string, string> dictionary = new Dictionary<string, string>();

            // Iterate over all lines in configuration file
            string[] lines = System.IO.File.ReadAllLines(filename);
            foreach (string line in lines)
            {
                string[] info = line.Replace(" ", String.Empty).Split(':');
                // Check if line contains enough information
                if (info.Length >= 2)
                {
                    dictionary[info[0]] = info[1];
                }
                else
                {
                    Console.WriteLine("Ignoring line: " + line);
                }
            }
            return dictionary;
        }
    }
}