// <copyright file="Entity.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using System;
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
        public Guid Id { get; set; }

        public EntityAttribute Attributes { get; set; }
    }
}
