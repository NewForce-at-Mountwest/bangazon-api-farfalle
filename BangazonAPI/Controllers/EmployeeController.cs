using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public EmployeeController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(string include, string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string command = $@"SELECT e.Id AS 'Employee Id', e.FirstName, e.LastName, e.DepartmentId, e.IsSuperVisor,
                        d.Name AS 'Department', c.Make, c.Manufacturer, c.PurchaseDate, c.DecomissionDate
                        FROM Employee e JOIN Department d ON e.DepartmentId = d.Id
						JOIN ComputerEmployee ce ON e.Id = ce.EmployeeId
                        JOIN Computer c ON ce.ComputerId=c.Id";

                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Employee> employees = new List<Employee>();

                    while (reader.Read())
                    {

                        Employee employee = new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Employee Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("IsSuperVisor"))
                        };

                    }
                    reader.Close();

                    return Ok(employees);
                }
            }
        }


        [HttpGet("{id}", Name = "GetEmployee")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, EmployeeName, EmployeeLanguage
                        FROM Employee
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Employee Employee = null;

                    if (reader.Read())
                    {
                        Employee = new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("EmployeeName")),
                            LastName = reader.GetString(reader.GetOrdinal("EmployeeLanguage"))
                        };
                    }
                    reader.Close();

                    return Ok(Employee);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Employee Employee)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Employee (EmployeeName, EmployeeLanguage)
                                        OUTPUT INSERTED.Id
                                        VALUES (@EmployeeName, @EmployeeLanguage)";
                    cmd.Parameters.Add(new SqlParameter("@EmployeeName", Employee.EmployeeName));
                    cmd.Parameters.Add(new SqlParameter("@EmployeeLanguage", Employee.EmployeeLanguage));

                    int newId = (int)cmd.ExecuteScalar();
                    Employee.Id = newId;
                    return CreatedAtRoute("GetEmployee", new { id = newId }, Employee);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Employee Employee)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Employee
                                            SET EmployeeName = @EmployeeName,
                                                EmployeeLanguage = @EmployeeLanguage
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@EmployeeName", Employee.EmployeeName));
                        cmd.Parameters.Add(new SqlParameter("@EmployeeLanguage", Employee.EmployeeLanguage));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!EmployeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Employee WHERE id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!EmployeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool EmployeeExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, EmployeeName, EmployeeLanguage
                        FROM Employee
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
