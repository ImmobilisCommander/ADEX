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
	inner join Links l on l.FromId = ec.Id
	inner join Entities ep on ep.Id = l.ToId
	inner join Persons p on p.Id = ep.Id
	