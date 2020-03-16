if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'Employees')
begin
	create table Employees
	(
		empid int not null constraint Pk_Employees primary key clustered,
		mgrid int constraint Fk_Employees_Employees references Employees(empid),
		empname varchar(25) not null,
		salary money not null,
		check (empid <> mgrid)
	);

	insert into Employees (empid, mgrid, empname, salary) values 
		( 1, null,      'David', $10000.00),
		( 2,    1,      'Eitan',  $7000.00),
		( 3,    1,        'Ina',  $7500.00),
		( 4,    2,     'Seraph',  $5000.00),
		( 5,    2,       'Jiru',  $5500.00),
		( 6,    2,      'Steve',  $4500.00),
		( 7,    3,      'Aaron',  $5000.00),
		( 8,    5,     'Lilach',  $3500.00),
		( 9,    7,       'Rita',  $3000.00),
		(10,    5,       'Sean',  $3000.00),
		(11,    7,    'Gabriel',  $3000.00),
		(12,    9,     'Emilia',  $2000.00),
		(13,    9,    'Michael',  $2000.00),
		(14,    9,       'Didi',  $1500.00);

	create unique index idx_unc_mgrid_empid on Employees(mgrid, empid);
end

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'Parts')
begin
	create table Parts
	(
		partid int not null constraint Pk_Parts primary key clustered,
		partname varchar(25) not null
	);

	insert into Parts(partid, partname) values 
		( 1, 'Black Tea'	  ),
		( 2, 'White Tea'	  ),
		( 3, 'Latte'		  ),
		( 4, 'Espresso'		  ),
		( 5, 'Double Espresso'),
		( 6, 'Cup Cover'	  ),
		( 7, 'Regular Cup'	  ),
		( 8, 'Stirrer'		  ),
		( 9, 'Espresso Cup'	  ),
		(10, 'Tea Shot'		  ),
		(11, 'Milk'			  ),
		(12, 'Coffe Shot'	  ),
		(13, 'Tea Leaves'	  ),
		(14, 'Water'		  ),
		(15, 'Sugar Bag'	  ),
		(16, 'Ground Coffee'  ),
		(17, 'Black Tea'	  );
end


if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'BOM')
begin
	create table BOM
	(
		partid int not null constraint FK_BOM_Parts_pid references Parts,
		assemblyid int constraint FK_BOM_Parts_aid references Parts,
		unit varchar(3) not null,
		qty decimal(8,2) not null,
		constraint UNQ_BOM_pid_aid UNIQUE(partid, assemblyid),
		constraint CK_BOM_diffids CHECK (partid <> assemblyid)
	);

	insert into BOM(partid, assemblyid, unit, qty) values 
		( 1, NULL, 'EA',   1.00),
		( 2, NULL, 'EA',   1.00),
		( 3, NULL, 'EA',   1.00),
		( 4, NULL, 'EA',   1.00),
		( 5, NULL, 'EA',   1.00),
		( 6,    1, 'EA',   1.00),
		( 7,    1, 'EA',   1.00),
		(10,    1, 'EA',   1.00),
		(14,    1, 'ml', 230.00),
		( 6,    2, 'EA',   1.00),
		( 7,    2, 'EA',   1.00),
		(10,    2, 'EA',   1.00),
		(14,    2, 'ml', 205.00),
		(11,    2, 'ml',  25.00),
		( 6,    3, 'EA',   1.00),
		( 7,    3, 'EA',   1.00),
		(11,    3, 'ml', 225.00),
		(12,    3, 'EA',   1.00),
		( 9,    4, 'EA',   1.00),
		(12,    4, 'EA',   1.00),
		( 9,    5, 'EA',   1.00),
		(12,    5, 'EA',   2.00),
		(13,   10,  'g',   5.00),
		(14,   10, 'ml',  20.00),
		(14,   12, 'ml',  20.00),
		(16,   12,  'g',  15.00),
		(17,   16,  'g',  15.00);
end


if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'Cities')
begin
	create table Cities
	(
		cityid char(3) not null constraint PK_Cities primary key,
		city varchar(30) not null,
		region varchar(30),
		country varchar(30) not null
	);

	insert into Cities(cityid, city, region, country) values
		('ATL', 'Atlanta', 'GA', 'USA'),
		('ORD', 'Chicago', 'IL', 'USA'),
		('DEN', 'Denver', 'CO', 'USA'),
		('IAH', 'Houston', 'TX', 'USA'),
		('MCI', 'Kansas City', 'MO', 'USA'),
		('LAX', 'Los Angeles', 'CA', 'USA'),
		('MIA', 'Miami', 'FL', 'USA'),
		('MSP', 'Minneapolis', 'MN', 'USA'),
		('JFK', 'New York', 'NY', 'USA'),
		('SEA', 'Seattle', 'WA', 'USA'),
		('SFO', 'San Francisco', 'CA', 'USA'),
		('ANC', 'Anchorage', 'AK', 'USA'),
		('FAI', 'Fairbanks', 'AK', 'USA');
end

if not exists (select 1 from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'Roads')
begin
	create table Roads
	(
		city1 char(3) not null constraint FK_Roads_Cities_city1 references Cities,
		city2 char(3) not null constraint FK_Roads_Cities_city2 references Cities,
		distance int not null,
		constraint PK_Roads primary key (city1, city2),
		constraint CK_Roads_CityDiff check (city1 <> city2),
		constraint CK_Roads_Positive_Distance check (distance > 0)
	);

	insert into Roads(city1, city2, distance) values
		('ANC', 'FAI', 359),
		('ATL', 'ORD', 715),
		('ATL', 'IAH', 800),
		('ATL', 'MCI', 805),
		('ATL', 'MIA', 665),
		('ATL', 'JFK', 865),
		('DEN', 'IAH', 1120),
		('DEN', 'MCI', 600),
		('DEN', 'LAX', 1025),
		('DEN', 'MSP', 915),
		('DEN', 'SEA', 1335),
		('DEN', 'SFO', 1270),		
		('IAH', 'MCI', 795),
		('IAH', 'LAX', 1550),
		('IAH', 'MIA', 1190),
		('JFK', 'ORD', 795),
		('LAX', 'SFO', 385),
		('MCI', 'ORD', 525),
		('MCI', 'MSP', 440),
		('MSP', 'ORD', 410),
		('MSP', 'SEA', 2015),
		('SEA', 'SFO', 815);
end
go

create function SubordinatesIterative(@root as int) returns @Subordinates table
(
	empid int not null primary key nonclustered,
	[level] int not null,
	unique clustered ([level], empid)
)
as
begin
	declare @level int = 0;

	insert into @Subordinates(empid, [level])
		select empid, @level from Employees where empid = @root;
		
	while @@ROWCOUNT > 0
	begin
		set @level = @level + 1;

		insert into @Subordinates(empid, [level])
			select c.empid, @level
			from @Subordinates as p
				inner join Employees c 
					on p.empid = c.mgrid 
						and p.[level] = @level - 1;
	end		

	return;
end
go

select * from SubordinatesIterative(3) s;
go

create function SubordinatesRecursive(@root as int) returns @Subordinates table
(
	empid int not null primary key nonclustered,
	[level] int not null,
	unique clustered ([level], empid)
)
as
begin
	with subordinates
	as
	(
		select p.empid, 0 as [level]
		from Employees p
		where p.empid = @root

		union all

		select c.empid, p.[level] + 1
		from subordinates as p
			inner join Employees c on p.empid = c.mgrid
	)
	insert into @Subordinates
		select empid, [level] from subordinates
	return;
end
go

select * from SubordinatesRecursive(3) s;
go 