// <copyright file="ICsvLoader.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using System;
using System.Collections.Generic;

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
        void LoadReferences();

        /// <summary>
        /// Loads interest bonds providers
        /// </summary>
        /// <param name="path">Path to the source data</param>
        void LoadProviders(string path);

        /// <summary>
        /// Loads interests bonds. Adds beneficiaries and eventually adds missing providers.
        /// </summary>
        /// <param name="path">Path to the source data</param>
        void LoadLinks(string path);

        /// <summary>
        /// Save data loaded
        /// </summary>
        void Save();

        /// <summary>
        /// Loads interest bonds and filter results with text to search passed as parameter
        /// </summary>
        /// <param name="txt">Text to search</param>
        /// <param name="take">Number of records to return</param>
        /// <returns></returns>
        GraphDataSet LinksToJson(string txt, int? take);
    }
}