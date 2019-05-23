//**********************************************************************************************//
// This test should run to test the getAll, getSingle, Post, And Put, as well as query additions
// on the EmployeeTraining Controller.
// Created By Sydney Wait
//
//*********************************************************************************************//

using Newtonsoft.Json;
using BangazonAPI.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace TestBangazonAPI
{
    public class TestEmployeeTraining
    {


        // Create a new employeeTraining in the db and make sure we get a 200 OK status code back

        public async Task<EmployeeTraining> createEmployeeTraining(HttpClient client)
        {
            EmployeeTraining employeeTraining = new EmployeeTraining
            {

                EmployeeId = 1,
                TrainingProgramId = 1

            };
            string employeeTrainingAsJSON = JsonConvert.SerializeObject(employeeTraining);


            HttpResponseMessage response = await client.PostAsync(
                "api/employeeTraining",
                new StringContent(employeeTrainingAsJSON, Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            EmployeeTraining newEmployeeTraining = JsonConvert.DeserializeObject<EmployeeTraining>(responseBody);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return newEmployeeTraining;

        }

        // Delete a employeeTraining join table in the database and make sure we get a no content status code back
        public async Task deleteEmployeeTraining(EmployeeTraining employeeTraining, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/employeeTraining/{employeeTraining.Id}");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }





        [Fact]
        public async Task Test_Create_EmployeeTraining()
        {
            using (var client = new APIClientProvider().Client)
            {

                // Create a new EmployeeTraining
                EmployeeTraining newEmployeeTraining = await createEmployeeTraining(client);

                // Make sure the info checks out
                Assert.Equal(1, newEmployeeTraining.EmployeeId);



                // Clean up after ourselves - delete EmployeeTraining!
                deleteEmployeeTraining(newEmployeeTraining, client);
            }
        }

    }
}



       