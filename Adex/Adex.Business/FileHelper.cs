// <copyright file="FileHelper.cs" company="julien_lefevre@outlook.fr">
//   Copyright (c) 2020 All Rights Reserved
//   <author>Julien LEFEVRE</author>
// </copyright>

using System.IO;
using System.Text;

namespace Adex.Business
{
    public static class FileHelper
    {
        /// <summary>
        /// Rewrite any file to UTF-8 format
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destinationDirectory"></param>
        /// <param name="nbOfLines"></param>
        public static void ReWriteToUTF8(string sourceFile, string destinationDirectory, int? nbOfLines = null)
        {
            using (var r = new StreamReader(sourceFile, true))
            {
                var newFile = Path.Combine(destinationDirectory, Path.GetFileName(sourceFile));
                if (File.Exists(newFile))
                {
                    File.Delete(newFile);
                }

                using (var w = new StreamWriter(newFile, false, Encoding.UTF8))
                {
                    if (nbOfLines == null)
                    {
                        while (!r.EndOfStream)
                        {
                            w.Write($"{r.ReadLine()}\n");
                        }
                    }
                    else
                    {
                        for (int i = 0; i < nbOfLines; i++)
                        {
                            w.Write($"{r.ReadLine()}\n");
                        }
                    }
                }
            }
        }
    }
}
