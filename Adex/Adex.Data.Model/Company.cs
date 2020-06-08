// <copyright file="Company.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.Data.Model
{
    [Table("Companies")]
    public class Company : Entity
    {
        /// <summary>
        /// Functional designation of the record
        /// </summary>
        [MaxLength(200)]
        public virtual string Designation { get; set; }

        public override string ToString()
        {
            return $"{Reference} {Designation}";
        }
    }
}
