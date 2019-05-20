using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Controllers

    //Controller for training programs API management. Get/push/post/delete functionality. Employees should be displayed.
    //Authored by Sable Bowen

{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingProgramController : ControllerBase
    {
        
            private readonly IConfiguration _config;
        public TrainingProgramController(IConfiguration config)
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
        public async Task<IActionResult> Get(string completed)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {

                    //SqlCommands dependant on query strings


                    cmd.CommandText = @"SELECT tp.Id, tp.Name, tp.StartDate, tp.EndDate, tp.MaxAttendees, e.Id AS EmployeeId, e.FirstName, e.LastName, e.DepartmentId, e.IsSupervisor  FROM TrainingProgram tp JOIN EmployeeTraining et on tp.Id = et.TrainingProgramId JOIN Employee e on e.Id = et.EmployeeId";



                    SqlDataReader reader = cmd.ExecuteReader();
                    List<TrainingProgram> trainingPrograms = new List<TrainingProgram>();

                    while (reader.Read())
                    {
                        TrainingProgram trainingProgram = new TrainingProgram
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                            EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                            MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees")),
                            Employees = new List<Employee>()
                        };

                        Employee employee = new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("IsSuperVisor"))
                        };


                        DateTime now = DateTime.Now;
                        

                        if (completed == "false")
                        {
                            if (!(DateTime.Compare(trainingProgram.EndDate, now) < 0))
                            {
                                if (trainingPrograms.Any(x => x.Id == trainingProgram.Id)) {
                                    {
                                        TrainingProgram program = trainingPrograms.Where(x => x.Id == trainingProgram.Id).FirstOrDefault();
                                        program.Employees.Add(employee);

                                    } 

                                }
                                else
                                {
                                    trainingProgram.Employees.Add(employee);
                                    trainingPrograms.Add(trainingProgram);
                                }
                            }
                        } else
                        {
                            if (trainingPrograms.Any(x => x.Id == trainingProgram.Id))
                            {

                                TrainingProgram program = trainingPrograms.Where(x => x.Id == trainingProgram.Id).FirstOrDefault();
                                program.Employees.Add(employee);

                            } else
                            {
                                trainingProgram.Employees.Add(employee);
                                trainingPrograms.Add(trainingProgram);
                            }
                        }


                    }

                    reader.Close();
                    return Ok(trainingPrograms);
                }
            }

        }




        [HttpGet("{id}", Name = "GetTrainingProgram")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name, StartDate, EndDate, MaxAttendees FROM TrainingProgram WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Customer customer = null;

                    if (reader.Read())
                    {
                        customer = new Customer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName"))
                        };

                    }
                    reader.Close();
                    return Ok(customer);
                }
            }
        }







        // POST: api/TrainingProgram
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/TrainingProgram/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
