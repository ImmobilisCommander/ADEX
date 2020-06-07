select * 
from 
	Entities e
	inner join Companies c on c.Id = e.Id

select * 
from 
	Entities e
	inner join Persons p on p.id = e.Id

select ec.*, c.*, l.* , ep.*, p.*
from 
	Entities ec
	inner join Companies c on c.Id = ec.Id
	inner join Links l on l.From_Id = ec.Id
	inner join Entities ep on ep.Id = l.To_Id
	inner join Persons p on p.Id = ep.Id

select *
from
	Links l
	inner join Entities e on e.Id = l.From_Id
where
	e.Reference like 'ZJFVYRIK'

/*
delete from FinancialLinks
delete from Links
delete from Persons
delete from Companies
delete from Entities
*/