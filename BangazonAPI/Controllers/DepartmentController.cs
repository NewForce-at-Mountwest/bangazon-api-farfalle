//**********************************************************************************************//
// This Controller gives the client access to getAll, getSingle, Post, and Put
// on the Department Resource.  It supports query functionality and include statements.
// Created by Sydney Wait
//*********************************************************************************************//



using BangazonAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;


namespace BangazonAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DepartmentController(IConfiguration config)
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


        //GET:Code for getting a list of Departments which are ACTIVE in the system
        [HttpGet]
        public async Task<IActionResult> GetAllDepartments(string _include, string _filter, int _gt)
        {

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string commandText = $"SELECT d.Id as 'DepartmentId', d.[Name] AS 'Department Name', d.Budget, e.id as 'EmployeeId', e.FirstName as 'Employee FirstName', e.LastName as 'Employee lastName', e.IsSuperVisor FROM Department d Full JOIN Employee e on d.id = e.departmentId";

                    if (_filter == "budget")
                    {
                        commandText+= $" WHERE d.budget >= '{_gt}'"; 

                    }                   

                    cmd.CommandText = commandText;



                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Department> departments = new List<Department>();
                    Department department = null;
                    List<Employee> employees = new List<Employee>();
                    Employee employee = null;


                    while (reader.Read())
                    {
                        department = new Department
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            Name = reader.GetString(reader.GetOrdinal("Department Name")),
                            Budget = reader.GetInt32(reader.GetOrdinal("Budget"))

                        };

                        //Check first to see if this line on the table has an employee in it so the code doesn't throw an error
                        if (!reader.IsDBNull(reader.GetOrdinal("EmployeeId"))) {
                            employee = new Employee
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                                FirstName = reader.GetString(reader.GetOrdinal("Employee FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("Employee LastName")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("isSuperVisor"))
                            };
                         
                            
                        }
                        else { employee = null; }

                        if (departments.Any(d => d.Id == department.Id))
                        {
                            Department departmentOnList = departments.Where(d => d.Id == department.Id).FirstOrDefault();
                            if (_include == "employees")
                            {                               

                                if (!departmentOnList.Employees.Any(e => e.Id == employee.Id))
                                {
                                        departmentOnList.Employees.Add(employee);
                                }
                            }                           

                        }
                        else
                        {
                            
                            if (_include == "employees") {
                                department.Employees.Add(employee);
                                }

                                departments.Add(department);
                        }

                    }

                        reader.Close();
                    

                    return Ok(departments);
                }
            }
        }



        //GET: Code for getting a single department (active or not)
        [HttpGet("{id}", Name = "Department")]

        public async Task<IActionResult> GetSingleDepartment([FromRoute] int id, string _include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT d.Id as 'DepartmentId', d.[Name] AS 'Department Name', d.Budget, e.id as 'EmployeeId', e.FirstName as 'Employee FirstName', e.LastName as 'Employee lastName', e.IsSuperVisor FROM Department d Full JOIN Employee e on d.id = e.departmentId WHERE d.Id=@id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();
                    Employee employee = null;
                    Department departmentToDisplay = null;

                    int counter = 0;

                    while (reader.Read())
                    {

                        if (counter < 1)
                        {
                            departmentToDisplay = new Department
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                Name = reader.GetString(reader.GetOrdinal("Department Name")),
                                Budget = reader.GetInt32(reader.GetOrdinal("Budget"))
                            };
                            counter++;
                        }


                        if (_include == "employees")
                        {
                            //Check to see that the current employee is not null
                            if (!reader.IsDBNull(reader.GetOrdinal("EmployeeId")))
                            {
                                employee = new Employee
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("Employee FirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("Employee LastName")),
                                    DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                    IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("isSuperVisor"))
                                };
                                departmentToDisplay.Employees.Add(employee);

                            }
                        }
                            
                        
                    };


                    reader.Close();

                    return Ok(departmentToDisplay);
                }
            }
        }

        //  POST: Code for creating a department
        [HttpPost]
        public async Task<IActionResult> PostDepartment([FromBody] Department department)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Department (Name, Budget)
                                                            OUTPUT INSERTED.Id
                                                            VALUES (@Name, @Budget)";
                    cmd.Parameters.Add(new SqlParameter("@Name", department.Name));
                    cmd.Parameters.Add(new SqlParameter("@Budget", department.Budget));
                   
                    int newId = (int)cmd.ExecuteScalar();
                    department.Id = newId;
                    return CreatedAtRoute("Department", new { id = newId }, department);
                }
            }
        }

        // PUT: Code for editing a department
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDepartment([FromRoute] int id, [FromBody] Department department)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Department
                                           SET name = @Name,
                                           Budget = @Budget
                                           WHERE id = @id";


                        cmd.Parameters.Add(new SqlParameter("@Name", department.Name));
                        cmd.Parameters.Add(new SqlParameter("@Budget", department.Budget));
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
                if (!DepartmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE: Code for deleting a payment type--soft delete actually changes 'isActive' to 0 (false)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment([FromRoute] int id, string q)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {

                        if (q == "delete_test_item")
                        {
                            cmd.CommandText = @"DELETE Department
                                              WHERE id = @id";
                        }
                       

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
                if (!DepartmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }





        private bool DepartmentExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name
                                            FROM Department
                                            WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }


    }
}



