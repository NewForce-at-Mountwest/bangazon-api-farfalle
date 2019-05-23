# Bangazon Platform API

## To test for each feature: 



### Products:
Product Controller includes full CRUD and testing for full CRUD. Deleting a product should by default ARCHIVE the product by setting IsActive to false. To hard delete a product, the specific item must have a query parameter of HardDelete = true, which will fully delete the item.

ex: (https://localhost:5001/api/product/99?HardDelete=true)

To test, run the Product tests, then use postman to make sure that no new products are listed when you get all. Additionally, you should post a new product, run a soft delete on it, then get it again and make sure that IsActive is set to false.

### Product types:
ProductType should have full CRUD functionality with corresponding tests. Delete on productType will only archive the type, query parameter of HardDelete=true will *ACTUALLY* hard delete.

To test, run tests, make sure they all pass. Check all routes for get single, get all, etc. Post a new type to the server using postman, then, delete it using the base delete. Get the new type you just made, and check that IsActive was set to false. If you end up with the base three types that you started with, it *SHOULD* all be working.

### Customers:


### Orders:


### Payment types:


### Employees:
The employee controller should have functionality to post, update, and get single/all employees. The delete request is included for testing purposes, so that our tests don't fill the database with dummy data. To run a delete, you must include "?PeteyDeletey=True" at the end of the user url.

To test, run employee tests, make sure they all pass. Run client, make sure you can see both single and all employees. Posting to postman should work, and when you get all employees after testing, you should only see the base 3.


### Computers:


### Training programs:


### Departments:


#### Client side app can be found here: https://github.com/NewForce-at-Mountwest/bangazon-farfalle-react-client
