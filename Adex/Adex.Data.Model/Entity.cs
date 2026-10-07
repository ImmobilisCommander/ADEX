// <copyright file="Entity.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Adex.Common;

namespace Adex.Data.Model
{
    [Table("Entities")]
    public class Entity : IEntity
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// External identifier
        /// </summary>
        [MaxLength(200)]
        [Required]
        public string Reference { get; set; }

        public ICollection<EntityAttribute> Attributes { get; set; } =
            new List<EntityAttribute>();
    }
}
