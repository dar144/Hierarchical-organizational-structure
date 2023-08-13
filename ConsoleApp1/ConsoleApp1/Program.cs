using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Reflection.Metadata;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ConsoleApp
{
    class Program
    {
    static string connectionString = process.env.CONNECTION_STRING;
        public static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("----------------------------------");
                Console.WriteLine();
                Console.WriteLine("Wybierz opcję:");
                Console.WriteLine("1. Utwórz tabelę w bazie danych.");
                Console.WriteLine("2. Dodaj pracownika.");
                Console.WriteLine("3. Usuń pracownika.");
                Console.WriteLine("4. Utwórz raport.");
                Console.WriteLine("5. Wyjdź z aplikacji.");
                Console.Write("Opcja:");

                int option;
                if (!int.TryParse(Console.ReadLine(), out option))
                {
                    Console.WriteLine("Nieprawidłowa opcja." +
                        " Spróbuj ponownie.");
                    continue;
                }

                Console.WriteLine();
                Console.WriteLine("----------------------------------");
                Console.WriteLine();

                switch (option)
                {
                    case 1:
                        CreateTable();
                        break;
                    case 2:
                        AddEmployee();
                        break;
                    case 3:
                        RemoveEmployee();
                        break;
                    case 4:
                        CreateReport();
                        break;
                    case 5:
                        return;
                    default:
                        Console.WriteLine("Nieprawidłowa opcja." +
                            " Spróbuj ponownie.");
                        break;
                }
            }
        }

        public static void CreateTable()
        {
            using (SqlConnection connection 
                = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "CREATE TABLE" +
                        " Teams (teamId INT PRIMARY KEY, " +
                        "teamName VARCHAR(50), employees XML)";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.ExecuteNonQuery();
                    Console.WriteLine("Tabela została utworzona.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Błąd podczas tworzenia tabeli: " +
                        "Tabela już istnieje");
                }
            }
        }


    public static void AddEmployee()
    {
        Console.WriteLine("Podaj Id zespołu:");
        int teamId;
        if (!int.TryParse(Console.ReadLine(), out teamId) 
            || teamId <= 0)
        {
            Console.WriteLine("Nieprawidłowe Id zespołu.");
            return;
        }

        bool isNewTeam = false;
        string existingEmployeesXml;
        int parentEmployeeId = 0;

        using (SqlConnection connection 
            = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Teams WHERE teamId = @teamId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@teamId", teamId);
                int existingTeamCount = (int)command.ExecuteScalar();

                if (existingTeamCount == 0)
                {
                    isNewTeam = true;

                    Console.WriteLine("Podaj nazwę zespołu:");
                    string teamName = Console.ReadLine();

                    query = "INSERT INTO Teams " +
                        "(teamId, teamName, employees) " +
                        "VALUES (@teamId, @teamName, " +
                        "'<Employees></Employees>')";
                    command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@teamName",
                        teamName);
                    command.ExecuteNonQuery();
                } else
                {
                    Console.WriteLine("Podaj Id " +
                        "pracownika nadrzędnego:");
                    if (!int.TryParse(Console.ReadLine(),
                        out parentEmployeeId) 
                        || parentEmployeeId <= 0)
                    {
                        Console.WriteLine("Nieprawidłowe" +
                            " Id pracownika nadrzędnego.");
                        return;
                    }
                }

                string existingEmployeesQuery = "SELECT employees" +
                    " FROM Teams WHERE teamId = @teamId";
                SqlCommand cmd = new SqlCommand(existingEmployeesQuery,
                    connection);
                cmd.Parameters.AddWithValue("@teamId", teamId);
                existingEmployeesXml = (string)cmd.ExecuteScalar();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd podczas dodawania" +
                    " pracownika: " + ex.Message);
                return;
            }
        }


        XDocument parentEmployeesXml = new XDocument();
        if (existingEmployeesXml == "<Employees />")
            parentEmployeesXml = XDocument.Parse("<Employees></Employees>");
        else
            parentEmployeesXml = XDocument.Parse(existingEmployeesXml);

        XElement employeeRoot = new XElement("Employee");

        Console.WriteLine("Podaj dane pracownika:");
        Console.WriteLine("Imię:");
        string firstName = Console.ReadLine();
        int maxEmployeeId = 0;
        var employees = parentEmployeesXml.Descendants("Employee");
        if (employees.Any())
        {
            maxEmployeeId = employees.Max(e => (int)e.Attribute("EmployeeId"));
        }

        employeeRoot.SetAttributeValue("EmployeeId", (maxEmployeeId + 1).ToString());
        employeeRoot.SetAttributeValue("ParentId", parentEmployeeId.ToString());
        employeeRoot.Add(new XElement("FirstName", firstName));

        Console.WriteLine("Nazwisko:");
        string lastName = Console.ReadLine();
        employeeRoot.Add(new XElement("LastName", lastName));

        Console.WriteLine("Stanowisko:");
        string position = Console.ReadLine();
        employeeRoot.Add(new XElement("Position", position));


        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();

                string query = "UPDATE Teams SET employees = @employeeData WHERE teamId = @teamId";
                if (isNewTeam)
                {
                    parentEmployeesXml.Root.Add(employeeRoot);
                            
                }
                else
                {
                    XElement parentEmployee = 
                        parentEmployeesXml.XPathSelectElement($"//Employee[@EmployeeId='{parentEmployeeId}']");

                    parentEmployee.Add(employeeRoot);
                }

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@teamId", teamId);
                command.Parameters.AddWithValue("@employeeData", parentEmployeesXml.ToString());
                command.ExecuteNonQuery();
                Console.WriteLine("Pracownik został dodany.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd podczas dodawania pracownika: " + ex.Message);
            }
        }
    }

        public static void RemoveEmployee()
        {
            Console.WriteLine("Podaj Id zespołu:");
            int teamId;
            if (!int.TryParse(Console.ReadLine(), out teamId) || teamId <= 0)
            {
                Console.WriteLine("Nieprawidłowe Id zespołu.");
                return;
            }

            Console.WriteLine("Podaj Id pracownika do usunięcia:");
            int employeeId;
            if (!int.TryParse(Console.ReadLine(), out employeeId) || employeeId <= 0)
            {
                Console.WriteLine("Nieprawidłowe Id pracownika.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT employees FROM Teams WHERE teamId = @teamId";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@teamId", teamId);
                    string existingEmployeesXml = (string)command.ExecuteScalar();

                    XDocument employeesXml = XDocument.Parse(existingEmployeesXml);

                    XElement employeeToRemove 
                        = employeesXml.XPathSelectElement($"//Employee[@EmployeeId='{employeeId}']");
                    if (employeeToRemove == null)
                    {
                        Console.WriteLine("Nie znaleziono pracownika o podanym Id.");
                        return;
                    }

                    employeeToRemove.Remove();

                    if(employeesXml.ToString() == "<Employees />")
                    {
                        query = "DELETE FROM Teams WHERE teamId = @teamId";
                        command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@teamId", teamId);
                        command.ExecuteNonQuery();

                    } else
                    {
                        query = "UPDATE Teams SET employees = @employeeData WHERE teamId = @teamId";
                        command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@teamId", teamId);
                        command.Parameters.AddWithValue("@employeeData", employeesXml.ToString());
                        command.ExecuteNonQuery();

                        string existingEmployeesQuery = "SELECT employees FROM Teams WHERE teamId = @teamId";
                        SqlCommand cmd = new SqlCommand(existingEmployeesQuery, connection);
                        cmd.Parameters.AddWithValue("@teamId", teamId);
                        existingEmployeesXml = (string)cmd.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Błąd podczas usuwania pracownika: " + ex.Message);
                }
            }
        }

        public static void CreateReport()
        {
            Console.WriteLine("Podaj Id zespołu:");
            int teamId;
            if (!int.TryParse(Console.ReadLine(), out teamId) || teamId <= 0)
            {
                Console.WriteLine("Nieprawidłowe Id zespołu.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT employees FROM Teams WHERE teamId = @teamId";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@teamId", teamId);
                    string employeesXml = (string)command.ExecuteScalar();

                    XDocument employeesXDoc = XDocument.Parse(employeesXml);

                    Console.WriteLine("Raport pracowników:");
                    Console.WriteLine("=======================================");

                    foreach (XElement employeeElement in employeesXDoc.Descendants("Employee"))
                    {
                        int employeeId = (int)employeeElement.Attribute("EmployeeId");
                        string firstName = (string)employeeElement.Element("FirstName");
                        string lastName = (string)employeeElement.Element("LastName");
                        string position = (string)employeeElement.Element("Position");
                        int? parentId = (int?)employeeElement.Attribute("ParentId");

                        Console.WriteLine($"ID: {employeeId}");
                        Console.WriteLine($"ID Pracownika Nadrzędnego: " +
                            $"{(parentId.HasValue ? parentId.ToString() : "Brak")}");
                        Console.WriteLine($"Imię: {firstName}");
                        Console.WriteLine($"Nazwisko: {lastName}");
                        Console.WriteLine($"Stanowisko: {position}");

                        Console.WriteLine("---------------------------------------");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Błąd podczas generowania raportu: Nie ma takiego zespołu." );
                }
            }
        }
    }
}
