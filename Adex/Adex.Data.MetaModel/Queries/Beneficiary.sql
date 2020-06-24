select
	c.Name, b.Value
from
	Metadatas b
	inner join Members c on c.Id = b.Member_Id
where
	b.Entity_Id = (select Id from Entities where Reference = 'NROJFJET')