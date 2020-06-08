// <copyright file="ILink.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using System;

namespace Adex.Common
{
    public interface ILink
    {
        int From_Id { get; }

        int To_Id { get; }

        string Kind { get; set; }

        DateTime Date { get; set; }
    }
}
