// <copyright file="FinancialLink.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using System.ComponentModel.DataAnnotations.Schema;

namespace Adex.Data.Model
{
    [Table("FinancialLinks")]
    public class FinancialLink : Link
    {
        public decimal Amount { get; set; }
    }
}
