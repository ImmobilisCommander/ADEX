ALTER INDEX IX_Reference ON Entities DISABLE;
ALTER INDEX IX_From_Id ON Links DISABLE;
ALTER INDEX IX_Id ON Links DISABLE;
ALTER INDEX IX_To_Id ON Links DISABLE;
ALTER INDEX IX_Entity_Id ON Metadatas DISABLE;
ALTER INDEX IX_Member_Id ON Metadatas DISABLE;

ALTER INDEX IX_Reference ON Entities REBUILD;
ALTER INDEX IX_From_Id ON Links REBUILD;
ALTER INDEX IX_Id ON Links REBUILD;
ALTER INDEX IX_To_Id ON Links REBUILD;
ALTER INDEX IX_Entity_Id ON Metadatas REBUILD;
ALTER INDEX IX_Member_Id ON Metadatas REBUILD;


select a.Id, a.Reference, ma.Id, ma.Value as PropertyValue, mb.Id, mba.Name as PropertyName, l.Kind, l.Date, b.Reference, mb.Value as PropertyValue
from 
	Entities a
	inner join Metadatas ma on ma.Entity_Id = a.Id
	inner join Members mba on mba.Id = ma.Member_Id and mba.Name = 'denomination_sociale'
	inner join Links l on l.From_Id = a.Id
	inner join Entities b on b.Id = l.To_Id
	inner join Metadatas mb on mb.Entity_Id = b.Id
	inner join Members mbb on mbb.Id = mb.Member_Id and mbb.Name = 'benef_nom'
where
	a.Reference = 'NROJFJET'



select a.Reference as Company, b.Reference as Beneficiary, count(*) as Nb
from
	Entities a
	inner join Links l on l.From_Id = a.Id
	inner join Entities b on b.Id = l.To_Id
Group by
	a.Reference, b.Reference
Having
	count(*) > 1
order by count(*) desc

select count(*) from Entities
select count(*) from Links
select * from Links
select * from Members order by [Name] asc

/*
truncate table Metadatas
delete from Members
truncate table Links
delete from Entities
*/