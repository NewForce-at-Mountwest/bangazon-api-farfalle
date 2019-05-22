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
                        
                        //Checks that each item in the list is in the future,
                        //then checks to see if that item is in the list before adding it or employees to it
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
                        //if item in the list is in the past - checks to see if it is already in the list - adds it if not, adds the employees to the existing one if it is
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

                    TrainingProgram program = null;

                    if (reader.Read())
                    {
                        program = new TrainingProgram
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                            EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                            MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees"))
                        };

                    }
                    reader.Close();
                    return Ok(program);
                }
            }
        }







        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TrainingProgram trainingProgram)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO TrainingProgram (Name, StartDate, EndDate, MaxAttendees) OUTPUT INSERTED.Id VALUES (@Name, @StartDate, @EndDate, @MaxAttendees)";
                    cmd.Parameters.Add(new SqlParameter("@Name", trainingProgram.Name));
                    cmd.Parameters.Add(new SqlParameter("@StartDate", trainingProgram.StartDate));
                    cmd.Parameters.Add(new SqlParameter("@EndDate", trainingProgram.EndDate));
                    cmd.Parameters.Add(new SqlParameter("@MaxAttendees", trainingProgram.MaxAttendees));


                    int newId = (int)cmd.ExecuteScalar();
                    trainingProgram.Id = newId;
                    return CreatedAtRoute("GetTrainingProgram", new { id = newId }, trainingProgram);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] TrainingProgram program)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE TrainingProgram SET Name = @Name, StartDate = @StartDate, EndDate = @EndDate, MaxAttendees = @MaxAttendees WHERE Id = @id";


                        cmd.Parameters.Add(new SqlParameter("@Name", program.Name));
                        cmd.Parameters.Add(new SqlParameter("@StartDate", program.StartDate));
                        cmd.Parameters.Add(new SqlParameter("@EndDate", program.EndDate));
                        cmd.Parameters.Add(new SqlParameter("@MaxAttendees", program.MaxAttendees));
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
                if (!ProgramExists(id))
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
                        cmd.CommandText = @"DELETE FROM TrainingProgram WHERE ID = @id";
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
                if (!ProgramExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        private bool ProgramExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name, StartDate, EndDate, MaxAttendees FROM TrainingProgram WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();

                }
            }
        }

    }

}





