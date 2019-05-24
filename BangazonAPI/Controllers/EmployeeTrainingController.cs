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



{
    [Route("api/[controller]")]
[ApiController]
public class EmployeeTrainingController : ControllerBase
{

    private readonly IConfiguration _config;
    public EmployeeTrainingController(IConfiguration config)
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



        [HttpPost]
        public async Task<IActionResult> Post([FromBody] EmployeeTraining EmployeeTraining)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO EmployeeTraining (EmployeeId, TrainingProgramId) OUTPUT INSERTED.Id VALUES (@EmployeeId, @TrainingProgramId)";

                    cmd.Parameters.Add(new SqlParameter("@EmployeeId", EmployeeTraining.EmployeeId));
                    cmd.Parameters.Add(new SqlParameter("@TrainingProgramId", EmployeeTraining.TrainingProgramId));

                    int newId = (int)cmd.ExecuteScalar();
                    EmployeeTraining.Id = newId;
                    return CreatedAtRoute("GetTrainingProgram", new { id = newId }, EmployeeTraining);
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
                        cmd.CommandText = @"DELETE FROM EmployeeTraining WHERE ID = @id";
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
                    cmd.CommandText = @"SELECT Id, EmployeeId, TrainingProgramId, FROM EmployeeTraining WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();

                }
            }
        }

    }

}





