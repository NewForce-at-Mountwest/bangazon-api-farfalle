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
        public async Task<IActionResult> GetAllDepartments(string include)
        {

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string commandText = $"SELECT d.Id as 'DepartmentId', d.[Name] AS 'Department Name', d.Budget, e.id as 'EmployeeId', e.FirstName as 'Employee FirstName', e.LastName as 'Employee lastName', e.IsSuperVisor FROM Department d Full JOIN Employee e on d.id = e.departmentId";

                    

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
                        employee = new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                            FirstName = reader.GetString(reader.GetOrdinal("Employee FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("Employee LastName")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("isSuperVisor"))
                        };

                        if (departments.Any(d => d.Id == department.Id))
                        {
                            Department departmentOnList = departments.Where(d => d.Id == department.Id).FirstOrDefault();
                            if (include == "employees")
                            {                               

                                if (!departmentOnList.Employees.Any(e => e.Id == employee.Id))
                                {
                                    departmentOnList.Employees.Add(employee);

                                }
                            }
                           

                        }
                        else
                        {

                            if (include == "employees") {
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



        //        //GET: Code for getting a single department (active or not)
        //        [HttpGet("{id}", Name = "Department")]
        //        public async Task<IActionResult> GetSingleDepartment([FromRoute] int id)
        //        {
        //            using (SqlConnection conn = Connection)
        //            {
        //                conn.Open();
        //                using (SqlCommand cmd = conn.CreateCommand())
        //                {
        //                    cmd.CommandText = $"SELECT Id, AcctNumber, [Name], CustomerId, IsActive from Department WHERE Id=@id";
        //                    cmd.Parameters.Add(new SqlParameter("@id", id));
        //                    SqlDataReader reader = cmd.ExecuteReader();

        //                    Department departmentToDisplay = null;

        //                    while (reader.Read())
        //                    {

        //                        {
        //                            departmentToDisplay = new Department
        //                            {
        //                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                                AcctNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber")),
        //                                Name = reader.GetString(reader.GetOrdinal("Name")),
        //                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
        //                                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))

        //                            };
        //                        };
        //                    };


        //                    reader.Close();

        //                    return Ok(departmentToDisplay);
        //                }
        //            }
        //        }

        //        //  POST: Code for creating a department
        //        [HttpPost]
        //        public async Task<IActionResult> PostDepartment([FromBody] Department department)
        //        {
        //            using (SqlConnection conn = Connection)
        //            {
        //                conn.Open();
        //                using (SqlCommand cmd = conn.CreateCommand())
        //                {
        //                    cmd.CommandText = @"INSERT INTO Department (AcctNumber, [Name], CustomerId, IsActive)
        //                                                    OUTPUT INSERTED.Id
        //                                                    VALUES (@AcctNumber, @Name, @CustomerId, @IsActive)";
        //                    cmd.Parameters.Add(new SqlParameter("@AcctNumber", department.AcctNumber));
        //                    cmd.Parameters.Add(new SqlParameter("@Name", department.Name));
        //                    cmd.Parameters.Add(new SqlParameter("@CustomerId", department.CustomerId));
        //                    cmd.Parameters.Add(new SqlParameter("@IsActive", department.IsActive));



        //                    int newId = (int)cmd.ExecuteScalar();
        //                    department.Id = newId;
        //                    return CreatedAtRoute("Department", new { id = newId }, department);
        //                }
        //            }
        //        }

        //        // PUT: Code for editing a department
        //        [HttpPut("{id}")]
        //        public async Task<IActionResult> PutDepartment([FromRoute] int id, [FromBody] Department department)
        //        {
        //            try
        //            {
        //                using (SqlConnection conn = Connection)
        //                {
        //                    conn.Open();
        //                    using (SqlCommand cmd = conn.CreateCommand())
        //                    {
        //                        cmd.CommandText = @"UPDATE Department
        //                                                        SET AcctNumber = @AcctNumber,
        //                                                        Name = @Name,
        //                                                        Budget=@Budget"


        //                        cmd.Parameters.Add(new SqlParameter("@Name", department.Name));
        //                        cmd.Parameters.Add(new SqlParameter("@Budget", department.Budget));
        //                        cmd.Parameters.Add(new SqlParameter("@id", id));

        //                        int rowsAffected = cmd.ExecuteNonQuery();
        //                        if (rowsAffected > 0)
        //                        {
        //                            return new StatusCodeResult(StatusCodes.Status204NoContent);
        //                        }
        //                        throw new Exception("No rows affected");
        //                    }
        //                }
        //            }
        //            catch (Exception)
        //            {
        //                if (!DepartmentExists(id))
        //                {
        //                    return NotFound();
        //                }
        //                else
        //                {
        //                    throw;
        //                }
        //            }
        //        }



        //        private bool DepartmentExists(int id)
        //        {
        //            using (SqlConnection conn = Connection)
        //            {
        //                conn.Open();
        //                using (SqlCommand cmd = conn.CreateCommand())
        //                {
        //                    cmd.CommandText = @"SELECT Id, name
        //                                    FROM Department
        //                                    WHERE Id = @id";
        //                    cmd.Parameters.Add(new SqlParameter("@id", id));

        //                    SqlDataReader reader = cmd.ExecuteReader();
        //                    return reader.Read();
        //                }
        //            }
        //        }


    }
}



