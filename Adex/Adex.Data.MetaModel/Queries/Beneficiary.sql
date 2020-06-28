select
	c.Name, b.Value
from
	Metadatas b
	inner join Members c on c.Id = b.Member_Id
where
	b.Entity_Id = (select Id from Entities where Reference = 'NROJFJET')

select distinct
	*
from
	Entities e
	inner join Metadatas b on b.Entity_Id = e.Id
	inner join Members c on c.Id = b.Member_Id and c.Name = 'benef_nom'

	inner join Metadatas f on f.Entity_Id = e.Id
	inner join Members d on d.Id = f.Member_Id and d.Name = 'benef_prenom'
where
	b.Value = 'lacombe' and f.Value = 'karine'


select
	e.Reference, em.Value, l.Kind, l.Date, sum(convert(decimal, lm.Value)) as Amount
from
	Entities e
	inner join Metadatas em on em.Entity_Id = e.Id

	inner join Links l on l.From_Id = e.Id
	inner join Metadatas lm on lm.Entity_Id = l.Id

	inner join Entities b on b.Id = l.To_Id

where
	b.Reference = '10001466357'-- and e.Reference = 'MYXUTQWV'
group by
	e.Reference, l.Kind, l.Date