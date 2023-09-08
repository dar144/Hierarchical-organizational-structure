# Hierarchical organizational structure

The aim of the project is to develop an API solution that will enable the creation of modifying and deleting elements of the hierarchical structure and generating reports on its basis.

The API provides the following functionality:

1. Create a table: Creates a table called `Teams` in the database with `teamId` columns
<p align="center">
  <img width="250" alt="create" src="https://github.com/dar144/Hierarchical-organizational-structure/assets/90532518/842c08ca-c318-4e88-8ad6-8a4e1fc17f19">
</p>

2. Add Employee: Adds an employee to the specified team in the database. Requires
providing teamId, employee data and possibly parentId of the parent employee.
<p align="center">
<img width="250" alt="add1" src="https://github.com/dar144/Hierarchical-organizational-structure/assets/90532518/661cd906-3e61-4dce-b0c8-564b6d14c08a">
</p>
<p align="center">
<img width="250" alt="add2" src="https://github.com/dar144/Hierarchical-organizational-structure/assets/90532518/0de9494e-44ea-4b9a-8f10-779482dbc693">
</p>

3. Remove Employee: Removes an employee from the specified team in the database. Requires
providing teamId and employeeId of the employee.
<p align="center">
<img width="250" alt="delete" src="https://github.com/dar144/Hierarchical-organizational-structure/assets/90532518/dffcc837-f928-4db6-b0fd-e5dca108be08">
</p>

4. Create Report: Generates a report for the specified team in the database.
Requires application teamId.
<p align="center">
  <img width="250" alt="report" src="https://github.com/dar144/Hierarchical-organizational-structure/assets/90532518/f7f21222-2576-400f-8a01-5dea6c315365">
</p>

## Technology Used

* `XML` for storing the hierarchical structure in the database.

* `C#` for console application implementation, which query the database, manipulate data, and return appropriate data results.

* `Microsoft SQL Server` for database management.

## Usage

To easely run c this code, you need to use Microsoft Visual Studio.


## References

[SQL Server technical documentation](https://nodejs.org/en/docs](https://learn.microsoft.com/en-us/sql/sql-server/?view=sql-server-ver16)https://learn.microsoft.com/en-us/sql/sql-server/?view=sql-server-ver16)

[C# documentation](https://getbootstrap.com/docs/3.4/css/](https://learn.microsoft.com/en-us/dotnet/csharp/)https://learn.microsoft.com/en-us/dotnet/csharp/)
