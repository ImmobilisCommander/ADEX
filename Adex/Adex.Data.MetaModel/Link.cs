// <copyright file="Link.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Adex.Common;

namespace Adex.Data.MetaModel
{
    [Table("Links")]
    public class Link : Entity, ILink
    {
        public int From_Id
        {
            get { return From?.Id ?? _fromId; }
            set { _fromId = value; }
        }

        public int To_Id
        {
            get { return To?.Id ?? _toId; }
            set { _toId = value; }
        }

        private int _fromId;
        private int _toId;

        public Entity From { get; set; }

        public Entity To { get; set; }

        [MaxLength(1000)]
        public string Kind { get; set; }

        public DateTime Date { get; set; }

        public override string ToString()
        {
            return Reference;
        }
    }
}
