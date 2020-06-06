using Adex.Common;
using Adex.Data.MetaModel;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Adex.Business
{
    internal static class Extensions
    {
        public static int InsertEntity(this SqlConnection connection, Entity obj)
        {
            return connection.ExecuteScalar<int>("insert into Entities (Reference) values (@Reference);SELECT CAST(SCOPE_IDENTITY() as int)", obj); ;
        }

        public static int InsertMetadata(this SqlConnection connection, int entityId, int memberId, string value)
        {
            return connection.ExecuteScalar<int>("insert into Metadatas (Entity_Id, Member_Id, Value) values (@Entity_Id, @Member_Id, @Value);SELECT CAST(SCOPE_IDENTITY() as int)",
                new
                {
                    Entity_Id = entityId,
                    Member_Id = memberId,
                    Value = value
                });
        }

        public static int InsertMember(this SqlConnection connection, Member obj)
        {
            return connection.ExecuteScalar<int>("insert into Members ([Name], Alias) values (@Name, @Alias);SELECT CAST(SCOPE_IDENTITY() as int)", obj);
        }

        public static int InsertLink(this SqlConnection connection, Link obj)
        {
            return connection.ExecuteScalar<int>("insert into Links (Id, From_Id, To_Id, Kind, Date) values (@Id, @From_Id, @To_Id, @Kind, @Date);SELECT CAST(SCOPE_IDENTITY() as int)", obj);
        }
    }
}
