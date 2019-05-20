using Newtonsoft.Json;
using BangazonAPI.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using TestBangazonAPI;

namespace TestBangazonAPI
{

    public class TestProducts
    {


        public async Task<Product> createBanana(HttpClient client)
        {
            //make a new product
            Product Banana = new Product
            {        
                ProductTypeId = 2,
                CustomerId = 1,
                Price = 5,
                Title = "Banana Bandana",
                Description = "It's a bandana that looks like a banana...somehow.",
                Quantity = 100,
                IsActive = true
            };
            //turn the product into json
            string BananaAsJSON = JsonConvert.SerializeObject(Banana);

            //post the banana
            HttpResponseMessage response = await client.PostAsync(
                "api/Product",
                new StringContent(BananaAsJSON, Encoding.UTF8, "application/json")
            );
            //make sure we were successfull
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            //convert back into c#
            Product newBanana = JsonConvert.DeserializeObject<Product>(responseBody);

            //make sure our status code is gud
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return newBanana;

        }

        // Delete banana in the database and make sure we get a no content status code back
        public async Task deleteBanana(Product Banana, HttpClient client)
        {
            //this makes sure there aren't a bunch of bananas chilling in our database by setting the hard delete bool to be true, thus ACTUALLY deleting the product vs. setting isActive = false
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/Product/{Banana.Id}?HardDelete=true");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }


        [Fact]
        public async Task Test_Get_All_Products()
        {
            // Use the http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // get all our products and wait for a response object
                HttpResponseMessage response = await client.GetAsync("api/Product");

                // Make sure that a response comes back at all
                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a list of Product instances
                List<Product> ProductList = JsonConvert.DeserializeObject<List<Product>>(responseBody);

                // Did we get back a 200 OK status code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // Are there any products in the list?
                Assert.True(ProductList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_Product()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new product
                Product newBanana = await createBanana(client);

                // Then we're going to try to get that product from the database
                HttpResponseMessage response = await client.GetAsync($"api/Product/{newBanana.Id}");

                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON back into C#
                Product Product = JsonConvert.DeserializeObject<Product>(responseBody);

                // did we get back the right stuff? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Banana Bandana", newBanana.Title);

                // delete Banana
                deleteBanana(newBanana, client);
            }
        }

        [Fact]
        public async Task Test_Get_NonExitant_Product_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a Product that doesn't exist
                HttpResponseMessage response = await client.GetAsync("api/Product/999999999");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }


        [Fact]
        public async Task Test_Create_And_Delete_Product()
        {
            using (var client = new APIClientProvider().Client)
            {

                // Create a new Banana
                Product newBanana = await createBanana(client);

                // Make sure Banana is indeed a banana
                Assert.Equal("Banana Bandana", newBanana.Title);


                // delete the filthy hat
                deleteBanana(newBanana, client);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistent_Product_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Attempt to delete a nonexistant product
                HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/Product/600000");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_Product()
        {

            // Changing Banana Bandana's title to be more fitting
            string newTitle = "RING RING RING RING BANANA PHONE";

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new instance of Banana
                Product newBanana = await createBanana(client);

                // Set a new title
                newBanana.Title = newTitle;

                // Convert it to JSON
                string modifiedBananaAsJSON = JsonConvert.SerializeObject(newBanana);

                // PUT the banana
                HttpResponseMessage response = await client.PutAsync(
                    $"api/Product/{newBanana.Id}",
                    new StringContent(modifiedBananaAsJSON, Encoding.UTF8, "application/json")
                );


                response.EnsureSuccessStatusCode();

                // Convert the response to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Check that there's a no content status code
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            
                HttpResponseMessage getBanana = await client.GetAsync($"api/Product/{newBanana.Id}");
                getBanana.EnsureSuccessStatusCode();

                string getBananaBody = await getBanana.Content.ReadAsStringAsync();
                Product modifiedBanana = JsonConvert.DeserializeObject<Product>(getBananaBody);

                Assert.Equal(HttpStatusCode.OK, getBanana.StatusCode);

           
                Assert.Equal(newTitle, modifiedBanana.Title);

              // DELETE THE YELLOW
                deleteBanana(modifiedBanana, client);
            }
        }
    }
}
