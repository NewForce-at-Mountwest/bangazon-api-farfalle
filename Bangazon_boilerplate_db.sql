DELETE FROM OrderProduct;
DELETE FROM ComputerEmployee;
DELETE FROM EmployeeTraining;
DELETE FROM Employee;
DELETE FROM TrainingProgram;
DELETE FROM Computer;
DELETE FROM Department;
DELETE FROM [Order];
DELETE FROM PaymentType;
DELETE FROM Product;
DELETE FROM ProductType;
DELETE FROM Customer;


ALTER TABLE Employee DROP CONSTRAINT [FK_EmployeeDepartment];
ALTER TABLE ComputerEmployee DROP CONSTRAINT [FK_ComputerEmployee_Employee];
ALTER TABLE ComputerEmployee DROP CONSTRAINT [FK_ComputerEmployee_Computer];
ALTER TABLE EmployeeTraining DROP CONSTRAINT [FK_EmployeeTraining_Employee];
ALTER TABLE EmployeeTraining DROP CONSTRAINT [FK_EmployeeTraining_Training];
ALTER TABLE Product DROP CONSTRAINT [FK_Product_ProductType];
ALTER TABLE Product DROP CONSTRAINT [FK_Product_Customer];
ALTER TABLE PaymentType DROP CONSTRAINT [FK_PaymentType_Customer];
ALTER TABLE [Order] DROP CONSTRAINT [FK_Order_Customer];
ALTER TABLE [Order] DROP CONSTRAINT [FK_Order_Payment];
ALTER TABLE OrderProduct DROP CONSTRAINT [FK_OrderProduct_Product];
ALTER TABLE OrderProduct DROP CONSTRAINT [FK_OrderProduct_Order];


DROP TABLE IF EXISTS OrderProduct;
DROP TABLE IF EXISTS ComputerEmployee;
DROP TABLE IF EXISTS EmployeeTraining;
DROP TABLE IF EXISTS Employee;
DROP TABLE IF EXISTS TrainingProgram;
DROP TABLE IF EXISTS Computer;
DROP TABLE IF EXISTS Department;
DROP TABLE IF EXISTS [Order];
DROP TABLE IF EXISTS PaymentType;
DROP TABLE IF EXISTS Product;
DROP TABLE IF EXISTS ProductType;
DROP TABLE IF EXISTS Customer;


CREATE TABLE Department (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	[Name] VARCHAR(55) NOT NULL,
	Budget 	INTEGER NOT NULL
);

CREATE TABLE Employee (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	FirstName VARCHAR(55) NOT NULL,
	LastName VARCHAR(55) NOT NULL,
	DepartmentId INTEGER NOT NULL,
	IsSuperVisor BIT NOT NULL DEFAULT(0),
    CONSTRAINT FK_EmployeeDepartment FOREIGN KEY(DepartmentId) REFERENCES Department(Id)
);

CREATE TABLE Computer (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	PurchaseDate DATETIME NOT NULL,
	DecomissionDate DATETIME,
	Make VARCHAR(55) NOT NULL,
	Manufacturer VARCHAR(55) NOT NULL
);

CREATE TABLE ComputerEmployee (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	EmployeeId INTEGER NOT NULL,
	ComputerId INTEGER NOT NULL,
	AssignDate DATETIME NOT NULL,
	UnassignDate DATETIME,
    CONSTRAINT FK_ComputerEmployee_Employee FOREIGN KEY(EmployeeId) REFERENCES Employee(Id),
    CONSTRAINT FK_ComputerEmployee_Computer FOREIGN KEY(ComputerId) REFERENCES Computer(Id)
);


CREATE TABLE TrainingProgram (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	[Name] VARCHAR(255) NOT NULL,
	StartDate DATETIME NOT NULL,
	EndDate DATETIME NOT NULL,
	MaxAttendees INTEGER NOT NULL
);

CREATE TABLE EmployeeTraining (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	EmployeeId INTEGER NOT NULL,
	TrainingProgramId INTEGER NOT NULL,
    CONSTRAINT FK_EmployeeTraining_Employee FOREIGN KEY(EmployeeId) REFERENCES Employee(Id),
    CONSTRAINT FK_EmployeeTraining_Training FOREIGN KEY(TrainingProgramId) REFERENCES TrainingProgram(Id)
);

CREATE TABLE ProductType (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	[Name] VARCHAR(55) NOT NULL,
	IsActive BIT NOT NULL DEFAULT(1)
);

CREATE TABLE Customer (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	FirstName VARCHAR(55) NOT NULL,
	LastName VARCHAR(55) NOT NULL
);

CREATE TABLE Product (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	ProductTypeId INTEGER NOT NULL,
	CustomerId INTEGER NOT NULL,
	Price INTEGER NOT NULL,
	Title VARCHAR(255) NOT NULL,
	[Description] VARCHAR(255) NOT NULL,
	Quantity INTEGER NOT NULL,
    CONSTRAINT FK_Product_ProductType FOREIGN KEY(ProductTypeId) REFERENCES ProductType(Id),
    CONSTRAINT FK_Product_Customer FOREIGN KEY(CustomerId) REFERENCES Customer(Id),
	IsActive BIT NOT NULL DEFAULT(1)
);


CREATE TABLE PaymentType (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	AcctNumber INTEGER NOT NULL,
	[Name] VARCHAR(55) NOT NULL,
	CustomerId INTEGER NOT NULL,
    CONSTRAINT FK_PaymentType_Customer FOREIGN KEY(CustomerId) REFERENCES Customer(Id),
	IsActive BIT NOT NULL DEFAULT(1)
);

CREATE TABLE [Order] (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	CustomerId INTEGER NOT NULL,
	PaymentTypeId INTEGER,
    CONSTRAINT FK_Order_Customer FOREIGN KEY(CustomerId) REFERENCES Customer(Id),
    CONSTRAINT FK_Order_Payment FOREIGN KEY(PaymentTypeId) REFERENCES PaymentType(Id)
);

CREATE TABLE OrderProduct (
	Id INTEGER NOT NULL PRIMARY KEY IDENTITY,
	OrderId INTEGER NOT NULL,
	ProductId INTEGER NOT NULL,
    CONSTRAINT FK_OrderProduct_Product FOREIGN KEY(ProductId) REFERENCES Product(Id),
    CONSTRAINT FK_OrderProduct_Order FOREIGN KEY(OrderId) REFERENCES [Order](Id)
);


INSERT INTO Department ([Name], Budget) VALUES ('Accounting', 32718);
INSERT INTO Department ([Name], Budget) VALUES  ('Manufacturing', 43700);
INSERT INTO Department ([Name], Budget) VALUES ('Shipping', 19350);

INSERT INTO Employee (FirstName, LastName, DepartmentId, IsSuperVisor) VALUES ('Bob', 'Ross', 1, 1);
INSERT INTO Employee (FirstName, LastName, DepartmentId, IsSuperVisor) VALUES ('Jacob', 'Turner', 2, 0);
INSERT INTO Employee (FirstName, LastName, DepartmentId, IsSuperVisor) VALUES ('Dwayne', 'Johnson', 3, 1);

INSERT INTO Computer (PurchaseDate, DecomissionDate, Make, Manufacturer) VALUES ('12/07/2018', '3/04/2019', 'Laptop', 'DELL');
INSERT INTO Computer (PurchaseDate, DecomissionDate, Make, Manufacturer) VALUES ('01/05/2017', null, 'Laptop', 'IBM');
INSERT INTO Computer (PurchaseDate, DecomissionDate, Make, Manufacturer) VALUES ('12/17/2016', '05/09/2019', 'Laptop', 'DELL');

INSERT  INTO ComputerEmployee (EmployeeId, ComputerId, AssignDate, UnassignDate) VALUES (1, 2, '01/05/2017', null);
INSERT  INTO ComputerEmployee (EmployeeId, ComputerId, AssignDate, UnassignDate) VALUES (2, 3, '02/13/2018', '07/19/2019');
INSERT  INTO ComputerEmployee (EmployeeId, ComputerId, AssignDate, UnassignDate) VALUES (3, 2, '07/04/2018', null);

INSERT INTO TrainingProgram ([Name], StartDate, EndDate, MaxAttendees) VALUES ('How to Treat Customers', '2/5/19', '6/12/19', 25)
INSERT INTO TrainingProgram ([Name], StartDate, EndDate, MaxAttendees) VALUES ('How to Tie a Tie', '1/1/19', '12/30/19', 100)
INSERT INTO TrainingProgram ([Name], StartDate, EndDate, MaxAttendees) VALUES ('When to Say Hello', '4/12/19', '4/13/19', 5)

INSERT INTO EmployeeTraining (EmployeeId, TrainingProgramId) VALUES (1,3)
INSERT INTO EmployeeTraining (EmployeeId, TrainingProgramId) VALUES (2,2)
INSERT INTO EmployeeTraining (EmployeeId, TrainingProgramId) VALUES (3,1)

INSERT INTO Customer (FirstName, LastName) VALUES('Sebass', 'LeClass');
INSERT INTO Customer (FirstName, LastName) VALUES ('TheDude', 'Abides');
INSERT INTO Customer(FirstName, LastName) VALUES ('Liono', 'Thundercat');

INSERT INTO PaymentType (AcctNumber, [Name], CustomerId) Values (8675309, 'Visa', 1);
INSERT INTO PaymentType(AcctNumber, [Name], CustomerId) Values (1234567, 'Mastercard', 2)
INSERT INTO PaymentType (AcctNumber, [Name], CustomerId) Values (0932109320, 'Taco Bell Gift Card', 1)

INSERT INTO [Order] (PaymentTypeId, CustomerId) Values (1,2)
INSERT INTO [Order] (PaymentTypeId, CustomerId) Values (2,1)
INSERT INTO [Order] (PaymentTypeId, CustomerId) Values (3,3)

INSERT into ProductType ([Name]) VALUES ('Coffee Cup');
INSERT into ProductType ([Name]) VALUES ('Hat');
INSERT into ProductType ([Name]) VALUES ('Soup Bowl');

INSERT into Product (ProductTypeId, CustomerId, Price, Title, [Description], Quantity) VALUES (1, 1, 10, 'Your Favorite Mug', 'insulated travel mug', 10);
INSERT into Product (ProductTypeId, CustomerId, Price, Title, [Description], Quantity) VALUES (2, 2, 5, 'Your Favorite Hat', 'Mesh-back trucker hat', 20);
INSERT into Product (ProductTypeId, CustomerId, Price, Title, [Description], Quantity) VALUES (3, 3, 3, 'Your Favorite Bowl', 'No-spill bowl', 15);


INSERT into OrderProduct (OrderId, ProductId) VALUES (1,2);
INSERT into OrderProduct (OrderId, ProductId) VALUES (2,3);
INSERT into OrderProduct (OrderId, ProductId) VALUES (3,1);