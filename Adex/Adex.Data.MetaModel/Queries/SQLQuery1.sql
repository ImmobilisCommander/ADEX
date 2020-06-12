/*
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
*/


select a.Id, a.Reference as Company_Code, ma.Id, ma.Value as Social_Denomination, le.Reference as LinkReference, l.Kind, l.Date, b.Reference, mb.Value as Lastname, mc.Value as Firstname
from 
	Entities a
	inner join Metadatas ma on ma.Entity_Id = a.Id
	inner join Members mba on mba.Id = ma.Member_Id and mba.Name = 'denomination_sociale'

	inner join Links l on l.From_Id = a.Id
	inner join Entities le on le.Id = l.Id

	inner join Entities b on b.Id = l.To_Id

	inner join Metadatas mb on mb.Entity_Id =l.To_Id
	inner join Members mbbx on mbbx.Id = mb.Member_Id and mbbx.Name = 'benef_nom'

	inner join Metadatas mc on mc.Entity_Id = l.To_Id
	inner join Members mbcx on mbcx.Id = mc.Member_Id and mbcx.Name = 'benef_prenom'
--where
--	a.Reference = 'OPFMIKBW'
order by a.Reference


select top 100 *
from
	(
	select a.Reference as Company, b.Reference as Beneficiary, bm1.Value + ' ' + bm2.Value as SocialDenomination, count(*) as NumberOfLinks
	from
		Entities a
		inner join Links l on l.From_Id = a.Id

		inner join Entities b on b.Id = l.To_Id
		inner join Metadatas bm1 on bm1.Entity_Id = b.Id
		inner join Members bmb1 on bmb1.Id = bm1.Member_Id and bmb1.Name = 'benef_nom'
		inner join Metadatas bm2 on bm2.Entity_Id = b.Id
		inner join Members bmb2 on bmb2.Id = bm2.Member_Id and bmb2.Name = 'benef_prenom'
	Group by
		a.Reference, b.Reference, bm1.Value, bm2.Value
	Having
		count(*) > 10

	union all

	select a.Reference, b.Reference, bm1.Value, count(*)
	from
		Entities a
		inner join Links l on l.From_Id = a.Id

		inner join Entities b on b.Id = l.To_Id
		inner join Metadatas bm1 on bm1.Entity_Id = b.Id
		inner join Members bmb1 on bmb1.Id = bm1.Member_Id and bmb1.Name = 'denomination_sociale'
	Group by
		a.Reference, b.Reference, bm1.Value
	Having
		count(*) > 10	
	) as tbl
order by NumberOfLinks desc

select count(*) from Entities
select count(*) from Links
select count(*) from Members
select count(*) from Metadatas
select * from Links
select * from Members order by [Name] asc
select top 1000 * 
from 
	Entities e
	inner join Metadatas m on m.Entity_Id = e.Id

where e.Reference = 'NROJFJET'

/*
truncate table Metadatas
delete from Members
truncate table Links
delete from Entities
*/