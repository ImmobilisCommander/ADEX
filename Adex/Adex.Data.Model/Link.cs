// <copyright file="Link.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using System;
using System.ComponentModel.DataAnnotations.Schema;
using Adex.Common;

namespace Adex.Data.Model
{
    [Table("Links")]
    public class Link : Entity, ILink
    {
        public Guid From_Id
        {
            get { return From?.Id ?? _fromId; }
            set { _fromId = value; }
        }

        public Guid To_Id
        {
            get { return To?.Id ?? _toId; }
            set { _toId = value; }
        }

        private Guid _fromId;
        private Guid _toId;

        public Entity From { get; set; }

        public Entity To { get; set; }

        public string Kind { get; set; }

        public DateTime Date { get; set; }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}
