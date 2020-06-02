select e.Id, e.Reference, mb.Id, mb.Name, m.Id, m.Value
from 
	Entities e
	inner join Metadatas m on m.Entity_Id = e.Id
	inner join Members mb on mb.Id = m.Member_Id

select *
from
	Entities a
	inner join Links l on l.From_Id = a.Id
	inner join Entities b on b.Id = l.To_Id

/*
truncate table Metadatas
delete from Entities
*/