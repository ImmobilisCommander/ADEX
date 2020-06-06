select e.Id, e.Reference, mb.Id, mb.Name, mb.Alias, m.Id, m.Value
from 
	Entities e
	inner join Metadatas m on m.Entity_Id = e.Id
	inner join Members mb on mb.Id = m.Member_Id

select a.Reference, ma.Value, b.Reference, mb.Value
from
	Entities a
	inner join Metadatas ma on ma.Entity_Id = a.Id
	inner join Members mba on mba.Id = ma.Member_Id and mba.Name = 'denomination_sociale'
	inner join Links l on l.From_Id = a.Id
	inner join Entities b on b.Id = l.To_Id
	inner join Metadatas mb on mb.Entity_Id = b.Id
	inner join Members mbb on mbb.Id = mb.Member_Id and mbb.Name = 'denomination_sociale'

select * from Members

/*
truncate table Metadatas
delete from Links
delete from Entities
*/