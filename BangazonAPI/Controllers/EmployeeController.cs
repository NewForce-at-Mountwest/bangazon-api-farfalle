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
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string command = $@"SELECT e.Id AS 'Employee Id', e.FirstName, e.LastName, e.IsSuperVisor, e.DepartmentId,
                        d.Id AS 'Department Id', d.Name AS 'Department', d.Budget ,c.Id AS 'Computer Id', 
						c.Make, c.Manufacturer, c.PurchaseDate, c.DecomissionDate
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
                            CurrentDepartment = new Department()
                            
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Department Id")),
                                Name = reader.GetString(reader.GetOrdinal("Department")),
                                Budget = reader.GetInt32(reader.GetOrdinal("Budget"))
                            },
                            IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("IsSuperVisor")),


                            CurrentComputer = new Computer()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Computer Id")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                                PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate"))
                               
                            }
                            
                    };



                        if (!reader.IsDBNull(reader.GetOrdinal("DecomissionDate")))
                        {
                            employee.CurrentComputer.DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"));

                    };



                        employees.Add(employee);


                        
                    }
                    reader.Close();
                    return Ok(employees);
                       
                    
                }
            }
        }
        


        [HttpGet("{id}", Name = "GetExercise")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.Id AS 'Employee Id', e.FirstName, e.LastName, e.IsSuperVisor, e.DepartmentId,
                        d.Id AS 'Department Id', d.Name AS 'Department', d.Budget ,c.Id AS 'Computer Id', 
						c.Make, c.Manufacturer, c.PurchaseDate, c.DecomissionDate
                        FROM Employee e JOIN Department d ON e.DepartmentId = d.Id
						JOIN ComputerEmployee ce ON e.Id = ce.EmployeeId
                        JOIN Computer c ON ce.ComputerId=c.Id WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Employee employee = null;

                    if (reader.Read())
                    {
                        employee = new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("IsSuperVisor"))

                        };
                    }
                    reader.Close();

                    return Ok(employee);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Employee employee)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Employee (FirstName, LastName, IsSuperVisor, DepartmentId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@FirstName, @LastName, @IsSuperVisor, @DepartmentId)";
                    cmd.Parameters.Add(new SqlParameter("@FirstName", employee.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@LastName", employee.LastName));
                    cmd.Parameters.Add(new SqlParameter("@IsSuperVisor", employee.IsSuperVisor));
                    cmd.Parameters.Add(new SqlParameter("@DepartmentId", employee.DepartmentId));

                    int newId = (int)cmd.ExecuteScalar();
                    employee.Id = newId;
                    return CreatedAtRoute("GetEmployee", new { id = newId }, employee);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Employee employee)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = 
                            @"UPDATE Employee 
                            SET FirstName = @FirstName,
                            LastName = @LastName,
                            IsSuperVisor = @IsSuperVisor,
                            DepartmentId = @DepartmentId
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@FirstName", employee.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@LastName", employee.LastName));
                        cmd.Parameters.Add(new SqlParameter("@IsSuperVisor", employee.IsSuperVisor));
                        cmd.Parameters.Add(new SqlParameter("@DepartmentId", employee.DepartmentId));
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
                        cmd.CommandText = @"DELETE FROM Exercise WHERE id = @id";
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
                        SELECT Id, FirstName, LastName, IsSuperVisor, DepartmentId
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

