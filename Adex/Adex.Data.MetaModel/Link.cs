﻿// <copyright file="Link.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using Adex.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.Data.MetaModel
{
    [Table("Links")]
    public class Link : Entity, ILink
    {
        [NotMapped]
        public int From_Id { get { return From.Id; } }

        [NotMapped]
        public int To_Id { get { return To.Id; } }

        public Entity From { get; set; }

        public Entity To { get; set; }

        [Index(IsClustered = false, IsUnique = false)]
        [MaxLength(1000)]
        public string Kind { get; set; }

        public DateTime Date { get; set; }

        public override string ToString()
        {
            return Reference;
        }
    }
}
