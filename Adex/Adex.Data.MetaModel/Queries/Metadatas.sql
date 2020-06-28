select distinct
	mb.Name, m.Value
from
	Metadatas m
	inner join Members mb on mb.Id = m.Member_Id
where
	m.Value like '%lacombe%'

select
	e.Id, e.Reference, mb.Name, m.Value
from
	Metadatas m
	inner join Members mb on mb.Id = m.Member_Id-- and (mb.Name = 'denomination_sociale' or mb.Name = 'benef_prenom' or mb.Name = 'benef_nom')
	inner join (
		select
			m.Entity_Id
		from
			Metadatas m
		where
			m.Value = 'lacombe'
	) tbl on tbl.Entity_Id = m.Entity_Id
	inner join Entities e on e.Id = m.Entity_Id
