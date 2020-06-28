// <copyright file="Metadata.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.Data.MetaModel
{
    [Table("Metadatas")]
    public class Metadata
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Entity Entity { get; set; }

        [Required]
        public Member Member { get; set; }

        [Index(IsClustered = false, IsUnique = false)]
        [MaxLength(1000)]
        public string Value { get; set; }
    }
}
