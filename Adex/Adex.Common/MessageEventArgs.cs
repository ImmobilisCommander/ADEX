// <copyright file="MessageEventArgs.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using System;

namespace Adex.Common
{
    public class MessageEventArgs : EventArgs
    {
        public Level Level { get; set; }

        public string Message { get; set; }
    }
}