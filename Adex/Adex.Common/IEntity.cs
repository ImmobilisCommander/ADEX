// <copyright file="IEntity.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

namespace Adex.Common
{
    public interface IEntity
    {
        int Id { get; set; }

        string Reference { get; set; }
    }
}
