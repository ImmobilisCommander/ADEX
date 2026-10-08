// <copyright file="IEntity.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using System;

namespace Adex.Common
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }
}
