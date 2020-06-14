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

        public static void InsertLink(this SqlConnection connection, Link obj)
        {
            connection.Execute("insert into Links (Id, From_Id, To_Id, Kind, Date) values (@Id, @From_Id, @To_Id, @Kind, @Date)", obj);
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

        public static string GetHashCodeBenef(this string[] csv)
        {
            var benef_adresse1 = csv.TryGetValue(csv.GetFieldIndex(CsvColumnsName.BenefAdresse1))?.Trim();
            var benef_adresse2 = csv.TryGetValue(csv.GetFieldIndex(CsvColumnsName.BenefAdresse2))?.Trim();
            var benef_adresse3 = csv.TryGetValue(csv.GetFieldIndex(CsvColumnsName.BenefAdresse3))?.Trim();
            var benef_adresse4 = csv.TryGetValue(csv.GetFieldIndex(CsvColumnsName.BenefAdresse4))?.Trim();
            var benef_categorie_code = csv.TryGetValue(csv.GetFieldIndex(CsvColumnsName.BenefCategorieCode))?.Trim();
            var benef_codepostal = csv.TryGetValue(csv.GetFieldIndex(CsvColumnsName.BenefCodepostal))?.Trim();
            var benef_denomination_sociale = csv.TryGetValue(csv.GetFieldIndex(CsvColumnsName.BenefDenominationSociale))?.Trim();
            var benef_etablissement = csv.TryGetValue(csv.GetFieldIndex(CsvColumnsName.BenefEtablissement))?.Trim();
            var benef_etablissement_codepostal = csv.TryGetValue(csv.GetFieldIndex(CsvColumnsName.BenefEtablissementCodepostal))?.Trim();
            var benef_etablissement_ville = csv.TryGetValue(csv.GetFieldIndex(CsvColumnsName.BenefEtablissementVille))?.Trim();
            var benef_identifiant_type_code = csv.TryGetValue(csv.GetFieldIndex(CsvColumnsName.BenefIdentifiantTypeCode))?.Trim();
            var benef_identifiant_valeur = csv.TryGetValue(csv.GetFieldIndex(CsvColumnsName.BenefIdentifiantValeur))?.Trim();
            var benef_nom = csv.TryGetValue(csv.GetFieldIndex(CsvColumnsName.BenefNom))?.Trim();
            var benef_objet_social = csv.TryGetValue(csv.GetFieldIndex(CsvColumnsName.BenefObjetSocial))?.Trim();
            var benef_pays_code = csv.TryGetValue(csv.GetFieldIndex(CsvColumnsName.BenefPaysCode))?.Trim();
            var benef_prenom = csv.TryGetValue(csv.GetFieldIndex(CsvColumnsName.BenefPrenom))?.Trim();
            var benef_qualite_code = csv.TryGetValue(csv.GetFieldIndex(CsvColumnsName.BenefQualiteCode))?.Trim();
            var benef_specialite_code = csv.TryGetValue(csv.GetFieldIndex(CsvColumnsName.BenefSpecialiteCode))?.Trim();
            var benef_specialite_libelle = csv.TryGetValue(csv.GetFieldIndex(CsvColumnsName.BenefSpecialiteLibelle))?.Trim();
            var benef_titre_code = csv.TryGetValue(csv.GetFieldIndex(CsvColumnsName.BenefTitreCode))?.Trim();
            var benef_titre_libelle = csv.TryGetValue(csv.GetFieldIndex(CsvColumnsName.BenefTitreLibelle))?.Trim();
            var benef_ville = csv.TryGetValue(csv.GetFieldIndex(CsvColumnsName.BenefVille))?.Trim();

            var source = $"{benef_adresse1}{benef_adresse2}{benef_adresse3}{benef_adresse4}{benef_categorie_code}{benef_codepostal}{benef_denomination_sociale}{benef_etablissement}{benef_etablissement_codepostal}{benef_etablissement_ville}{benef_identifiant_type_code}{benef_identifiant_valeur}{benef_nom}{benef_objet_social}{benef_pays_code}{benef_prenom}{benef_qualite_code}{benef_specialite_code}{benef_specialite_libelle}{benef_titre_code}{benef_titre_libelle}{benef_ville}";

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

        public static int GetFieldIndex(this string[] array, string txt)
        {
            return Array.IndexOf(array, txt);
        }

        private static readonly char[] _trimChars = new char[] { '\"' };

        public static string TryGetValue(this string[] array, int index)
        {
            string retour = null;
            if (index >= 0 && array.Length > index)
            {
                return array[index]?.Trim(_trimChars);
            }
            return retour;
        }
    }
}
