using Newtonsoft.Json;
using BangazonAPI.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using System;

namespace TestBangazonAPI
{

    //Unit Testing for the Training Program Controller
    //    Authored by Sable Bowen

    public class TestTrainingProgram
    {



        public async Task<TrainingProgram> createProgram(HttpClient client)
        {
            TrainingProgram program = new TrainingProgram
            {
                Name = "Computer Programming Program",
                StartDate = new DateTime(2018, 06, 14),
                EndDate = new DateTime(2100, 07, 20),
                MaxAttendees = 14
            };
            string programAsJSON= JsonConvert.SerializeObject(program);


            HttpResponseMessage response = await client.PostAsync(
                "api/trainingprogram",
                new StringContent(programAsJSON, Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            TrainingProgram newProgram = JsonConvert.DeserializeObject<TrainingProgram>(responseBody);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return newProgram;

        }

        // Delete a student in the database and make sure we get a no content status code back
        public async Task deleteProgram(TrainingProgram program, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/trainingprogram/{program.Id}");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }


        [Fact]
        public async Task Test_Get_All_Program()
        {
            // Use the http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // Call the route to get all our students; wait for a response object
                HttpResponseMessage response = await client.GetAsync("api/trainingprogram");

                // Make sure that a response comes back at all
                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a list of student instances
                List<TrainingProgram> programList = JsonConvert.DeserializeObject<List<TrainingProgram>>(responseBody);

                // Did we get back a 200 OK status code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // Are there any students in the list?
                Assert.True(programList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_All_Future_Programs()
        {
            // Use the http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // Call the route to get all our students; wait for a response object
                HttpResponseMessage response = await client.GetAsync("api/trainingprogram/?completed=false");

                // Make sure that a response comes back at all
                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

               
                List<TrainingProgram> programList = JsonConvert.DeserializeObject<List<TrainingProgram>>(responseBody);

                // Did we get back a 200 OK status code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                Assert.True(programList.Count > 0);

                //Checks that each item in the list is in the future
                DateTime now = DateTime.Now;
                Assert.True(!programList.Any(i => DateTime.Compare(i.EndDate, now) < 0));
            }
        }
                


           
        [Fact]
        public async Task Test_Get_Single_Program()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new student
                TrainingProgram newProgram = await createProgram(client);

                // Try to get that student from the database
                HttpResponseMessage response = await client.GetAsync($"api/trainingprogram/{newProgram.Id}");

                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Customer customer = JsonConvert.DeserializeObject<Customer>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Computer Programming Program", newProgram.Name);
                Assert.Equal(14, newProgram.MaxAttendees);


                // Clean up after ourselves- delete david!
                deleteProgram(newProgram, client);
            }
        }


        [Fact]
        public async Task Test_Get_NonExistant_Program_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a student with an enormously huge Id
                HttpResponseMessage response = await client.GetAsync("api/trainingprogram/999999999");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }


        [Fact]
        public async Task Test_Create_And_Delete_Program()
        {
            using (var client = new APIClientProvider().Client)
            {

                // Create a new David
                TrainingProgram newProgram = await createProgram(client);

                // Make sure his info checks out
                Assert.Equal("Computer Programming Program", newProgram.Name);
                Assert.Equal(14, newProgram.MaxAttendees);

             
                deleteProgram(newProgram, client);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistent_Program_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Try to delete an Id that shouldn't exist in the DB
                HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/trainingprogram/600000");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_Program()
        {

            
            string newProgramName = "How to Code";

            using (HttpClient client = new APIClientProvider().Client)
            {

               
                TrainingProgram newProgram = await createProgram(client);

                
                newProgram.Name = newProgramName;

                // Convert them to JSON
                string modifiedProgramAsJSON = JsonConvert.SerializeObject(newProgram);

                // Make a PUT request with the new info
                HttpResponseMessage response = await client.PutAsync(
                    $"api/trainingprogram/{newProgram.Id}",
                    new StringContent(modifiedProgramAsJSON, Encoding.UTF8, "application/json")
                );


                response.EnsureSuccessStatusCode();

                // Convert the response to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // We should have gotten a no content status code
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                // Try to GET the student we just edited
                HttpResponseMessage getProgram = await client.GetAsync($"api/trainingprogram/{newProgram.Id}");
                getProgram.EnsureSuccessStatusCode();

                string getProgramBody = await getProgram.Content.ReadAsStringAsync();
                TrainingProgram modifiedProgram = JsonConvert.DeserializeObject<TrainingProgram>(getProgramBody);

                Assert.Equal(HttpStatusCode.OK, getProgram.StatusCode);

                // Make sure his name was in fact updated
                Assert.Equal(newProgramName, modifiedProgram.Name);

                // Clean up after ourselves- delete him
                deleteProgram(modifiedProgram, client);
            }
        }
    }
}


