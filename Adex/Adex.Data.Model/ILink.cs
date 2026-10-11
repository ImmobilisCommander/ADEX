// <copyright file="ILink.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using System;

namespace Adex.Data.Model
{
    public interface ILink
    {
        Guid From_Id { get; }

        Guid To_Id { get; }

        string Kind { get; set; }

        DateTime Date { get; set; }
    }
}
