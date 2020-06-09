// <copyright file="Extensions.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using Adex.Data.MetaModel;
using CsvHelper;
using Dapper;
using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
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

        public static string GetFullErrorMessage(this Exception x)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{x.GetType().FullName}: {x.Message}");

            if (x.InnerException != null)
            {
                sb.AppendLine(GetFullErrorMessage(x.InnerException));
            }

            return sb.ToString();
        }

        public static string GetHashCodeBenef(this CustomCsvReader csv)
        {
            var benef_adresse1 = csv.GetField(csv.GetFieldIndex(CsvColumnsName.BenefAdresse1))?.Trim();
            var benef_adresse2 = csv.GetField(csv.GetFieldIndex(CsvColumnsName.BenefAdresse2))?.Trim();
            var benef_adresse3 = csv.GetField(csv.GetFieldIndex(CsvColumnsName.BenefAdresse3))?.Trim();
            var benef_adresse4 = csv.GetField(csv.GetFieldIndex(CsvColumnsName.BenefAdresse4))?.Trim();
            var benef_categorie_code = csv.GetField(csv.GetFieldIndex(CsvColumnsName.BenefCategorieCode))?.Trim();
            var benef_codepostal = csv.GetField(csv.GetFieldIndex(CsvColumnsName.BenefCodepostal))?.Trim();
            var benef_denomination_sociale = csv.GetField(csv.GetFieldIndex(CsvColumnsName.BenefDenominationSociale))?.Trim();
            var benef_etablissement = csv.GetField(csv.GetFieldIndex(CsvColumnsName.BenefEtablissement))?.Trim();
            var benef_etablissement_codepostal = csv.GetField(csv.GetFieldIndex(CsvColumnsName.BenefEtablissementCodepostal))?.Trim();
            var benef_etablissement_ville = csv.GetField(csv.GetFieldIndex(CsvColumnsName.BenefEtablissementVille))?.Trim();
            var benef_identifiant_type_code = csv.GetField(csv.GetFieldIndex(CsvColumnsName.BenefIdentifiantTypeCode))?.Trim();
            var benef_identifiant_valeur = csv.GetField(csv.GetFieldIndex(CsvColumnsName.BenefIdentifiantValeur))?.Trim();
            var benef_nom = csv.GetField(csv.GetFieldIndex(CsvColumnsName.BenefNom))?.Trim();
            var benef_objet_social = csv.GetField(csv.GetFieldIndex(CsvColumnsName.BenefObjetSocial))?.Trim();
            var benef_pays_code = csv.GetField(csv.GetFieldIndex(CsvColumnsName.BenefPaysCode))?.Trim();
            var benef_prenom = csv.GetField(csv.GetFieldIndex(CsvColumnsName.BenefPrenom))?.Trim();
            var benef_qualite_code = csv.GetField(csv.GetFieldIndex(CsvColumnsName.BenefQualiteCode))?.Trim();
            var benef_specialite_code = csv.GetField(csv.GetFieldIndex(CsvColumnsName.BenefSpecialiteCode))?.Trim();
            var benef_speicalite_libelle = csv.GetField(csv.GetFieldIndex(CsvColumnsName.BenefSpeicaliteLibelle))?.Trim();
            var benef_titre_code = csv.GetField(csv.GetFieldIndex(CsvColumnsName.BenefTitreCode))?.Trim();
            var benef_titre_libelle = csv.GetField(csv.GetFieldIndex(CsvColumnsName.BenefTitreLibelle))?.Trim();
            var benef_ville = csv.GetField(csv.GetFieldIndex(CsvColumnsName.BenefVille))?.Trim();

            var source = $"{benef_adresse1}{benef_adresse2}{benef_adresse3}{benef_adresse4}{benef_categorie_code}{benef_codepostal}{benef_denomination_sociale}{benef_etablissement}{benef_etablissement_codepostal}{benef_etablissement_ville}{benef_identifiant_type_code}{benef_identifiant_valeur}{benef_nom}{benef_objet_social}{benef_pays_code}{benef_prenom}{benef_qualite_code}{benef_specialite_code}{benef_speicalite_libelle}{benef_titre_code}{benef_titre_libelle}{benef_ville}";

            var sBuilder = new StringBuilder();
            using (var md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(source));

                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
            }

            return sBuilder.ToString();
        }
    }
}
