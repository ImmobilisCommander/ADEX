// <copyright file="ICsvLoader.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Adex.Common
{
    public interface ICsvLoader
    {
        /// <summary>
        /// Envent to transmit a message
        /// </summary>
        event EventHandler<MessageEventArgs> OnMessage;

        /// <summary>
        /// Load reference data from entities table
        /// </summary>
        Task LoadReferencesAsync(CancellationToken cancellationToken);

        Task LoadProvidersAsync(string path, CancellationToken cancellationToken);

        Task LoadLinksAsync(string path, CancellationToken cancellationToken);

        /// <summary>
        /// Save data loaded
        /// </summary>
        Task SaveAsync(CancellationToken cancellationToken);
    }
}
