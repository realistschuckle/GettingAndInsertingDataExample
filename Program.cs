using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DataExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please enter the first letter of the last name");
            Console.WriteLine("that you would like to query.");
            string firstLetter = Console.ReadLine();
            firstLetter += "%";

            Console.WriteLine("Please enter the month and year that you'd");
            Console.WriteLine("like to query.");

            int month = -1;
            while (month == -1)
            {
                Console.Write("MONTH (1-12): ");
                string monthInput = Console.ReadLine();
                try
                {
                    month = int.Parse(monthInput);
                }
                catch (FormatException)
                {
                    Console.WriteLine("That ain't a number, stoopid.");
                }
            }

            int year = -1;
            while (year == -1)
            {
                Console.Write("YEAR: ");
                string yearInput = Console.ReadLine();
                try
                {
                    year = int.Parse(yearInput);
                }
                catch (FormatException)
                {
                    Console.WriteLine("That ain't a number, stoopid.");
                }
                finally
                {
                    Console.WriteLine("This runs regardless of an error or not.");
                }
            }

            DateTime lowerBound = new DateTime(year, month, 1);

            DateTime upperBound = lowerBound.AddMonths(1);
            upperBound = upperBound.AddDays(-1);

            string myConnection = ConfigurationManager
                .ConnectionStrings["ServerConnectUm"]
                .ConnectionString;

            using (SqlConnection connection = new SqlConnection(myConnection))
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"
                        SELECT FirstName
                             , LastName
                             , ModifiedDate
                          FROM Person.Person
                         WHERE LastName LIKE @firstLetterWildcard
                           AND ModifiedDate BETWEEN @lowerBoundDate 
                                                AND @upperBoundDate
                      ORDER BY ModifiedDate
                             , LastName
                             , FirstName
                    ";
                    command.Parameters.AddWithValue("@firstLetterWildcard", firstLetter);
                    command.Parameters.AddWithValue("@lowerBoundDate", lowerBound);
                    command.Parameters.AddWithValue("@upperBoundDate", upperBound);

                    using (SqlDataReader reader = command.ExecuteReader())
                    { 
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                string firstName = reader.GetString(0);
                                string lastName = (string)reader.GetValue(1);
                                DateTime modifiedDate = Convert.ToDateTime(reader["ModifiedDate"]);

                                string modifiedFormatted = modifiedDate.ToString("MM/dd/yyyy");

                                Console.WriteLine($"{lastName + ",",-12} {firstName,-12} : {modifiedFormatted}");
                            }
                        }
                    }
                }
                Console.ReadLine();

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"
                        INSERT INTO Sales.Currency(CurrencyCode, Name)
                        VALUES ('BIT', 'Bitcoin');
                    ";
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (SqlException e)
                    {
                        Console.WriteLine($"ERROR: {e.Message}");
                    }
                }
            }
        }
    }
}
