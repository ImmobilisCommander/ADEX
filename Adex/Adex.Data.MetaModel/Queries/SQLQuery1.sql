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


select *
from
	(
	select a.Reference as Company, am.Value as Designation, b.Reference as Beneficiary, bm1.Value + ' ' + bm2.Value as SocialDenomination, count(*) as NumberOfLinks, SUM(CONVERT(decimal, lm.Value)) as Amount
	from
		Entities a
		inner join Metadatas am on am.Entity_Id = a.Id
		inner join Members amb on amb.Id = am.Member_Id and amb.Name = 'denomination_sociale'

		inner join Links l on l.From_Id = a.Id
		inner join Metadatas lm on lm.Entity_Id = l.Id
		inner join Members lmb on lmb.Id = lm.Member_Id and lmb.Name like '%_montant_ttc'

		inner join Entities b on b.Id = l.To_Id
		inner join Metadatas bm1 on bm1.Entity_Id = b.Id
		inner join Members bmb1 on bmb1.Id = bm1.Member_Id and bmb1.Name = 'benef_nom'
		inner join Metadatas bm2 on bm2.Entity_Id = b.Id
		inner join Members bmb2 on bmb2.Id = bm2.Member_Id and bmb2.Name = 'benef_prenom'
	Group by
		a.Reference, am.Value, b.Reference, bm1.Value, bm2.Value
	Having
		SUM(CONVERT(decimal, lm.Value)) > 3000 --count(*) > 10	

	union all

	select a.Reference, am.Value, b.Reference, bm1.Value, count(*), SUM(CONVERT(decimal, lm.Value))
	from
		Entities a
		inner join Metadatas am on am.Entity_Id = a.Id
		inner join Members amb on amb.Id = am.Member_Id and amb.Name = 'denomination_sociale'

		inner join Links l on l.From_Id = a.Id
		inner join Metadatas lm on lm.Entity_Id = l.Id
		inner join Members lmb on lmb.Id = lm.Member_Id and lmb.Name like '%_montant_ttc'

		inner join Entities b on b.Id = l.To_Id
		inner join Metadatas bm1 on bm1.Entity_Id = b.Id
		inner join Members bmb1 on bmb1.Id = bm1.Member_Id and bmb1.Name = 'denomination_sociale'
	Group by
		a.Reference, am.Value, b.Reference, bm1.Value
	Having
		SUM(CONVERT(decimal, lm.Value)) > 3000 --count(*) > 10	
	) as tbl
order by Amount desc

select count(*) from Entities
select count(*) from Links
select count(*) from Members
select count(*) from Metadatas
select * from Links

select le.Reference, l.Kind, l.Date, lm.Value as Amount
from 
	Entities e
	inner join Metadatas m on m.Entity_Id = e.Id
	inner join Members b on b.Id = m.Member_Id and b.Name = 'benef_nom'

	inner join Links l on l.To_Id = e.Id
	inner join Metadatas lm on lm.Entity_Id = l.Id
	inner join Members lb on lb.Id = lm.Member_Id and lb.Name like '%_montant_ttc'
	inner join Entities le on le.Id = l.Id
where 
	e.Id = 4143126

/*
truncate table Metadatas
delete from Members
truncate table Links
delete from Entities
*/

select a.Reference as Company, am.Value as Designation, p.Reference as Beneficiary, pm1.Value + ' ' + pm2.Value as SocialDenomination, b.NumberOfLinks, b.Amount
from
	(
	select
		l.From_Id, l.To_Id, count(*) as NumberOfLinks, SUM(CONVERT(decimal, lm.Value)) as Amount
	from
		Links l
		inner join Metadatas lm on lm.Entity_Id = l.Id
		inner join Members lmb on lmb.Id = lm.Member_Id and lmb.Name like '%_montant_ttc'
	group by
		l.From_Id, l.To_Id
	having
		SUM(CONVERT(decimal, lm.Value)) > 30000
	) b

	inner join Entities a on a.Id = b.From_Id
	inner join Metadatas am on am.Entity_Id = a.Id
	inner join Members amb on amb.Id = am.Member_Id and amb.Name = 'denomination_sociale'

	inner join Entities p on p.Id = b.To_Id
	inner join Metadatas pm1 on pm1.Entity_Id = p.Id
	inner join Members pmb1 on pmb1.Id = pm1.Member_Id and pmb1.Name = 'benef_nom'
	inner join Metadatas pm2 on pm2.Entity_Id = p.Id
	inner join Members pmb2 on pmb2.Id = pm2.Member_Id and pmb2.Name = 'benef_prenom'
