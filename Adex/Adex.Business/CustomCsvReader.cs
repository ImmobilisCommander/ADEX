using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Adex.Business
{
    /// <summary>
    /// Custom implementation of <see cref="CsvReader"/>
    /// </summary>
    internal class CustomCsvReader : CsvReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sr">Stream of the file</param>
        /// <param name="inf">Culture of the file</param>
        public CustomCsvReader(StreamReader sr, CsvConfiguration configuration)
            : base(sr, configuration)
        {
        }

        /// <summary>
        /// Get the filed by index if index greater than or equal to 0
        /// </summary>
        /// <param name="index">Index of the field to get</param>
        /// <returns></returns>
        public override string GetField(int index)
        {
            if (index >= 0)
            {
                return base.GetField(index);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get the filed by index if index greater than or equal to 0 and convert value to int
        /// </summary>
        /// <param name="index">Index of the field to get</param>
        /// <returns></returns>
        public int GetFieldAsInt(int index)
        {
            if (index >= 0)
            {
                var temp = base.GetField(index);
                return string.IsNullOrEmpty(temp) ? 0 : Convert.ToInt32(temp);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Get the filed by index if index greater than or equal to 0 and convert value to double
        /// </summary>
        /// <param name="index">Index of the field to get</param>
        /// <param name="cu">Culture of the value of the field</param>
        /// <returns></returns>
        public double GetFieldAsDouble(int index, CultureInfo cu)
        {
            if (index >= 0)
            {
                var temp = base.GetField(index);
                return string.IsNullOrEmpty(temp) ? 0 : Convert.ToDouble(temp, cu);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Get the filed by name
        /// </summary>
        /// <param name="names">Array of name to test</param>
        /// <returns></returns>
        public string GetField(string[] names)
        {
            string temp = null;
            foreach (var name in names)
            {
                if (base.TryGetField(name, out temp))
                {
                    break;
                }
            }
            return string.IsNullOrEmpty(temp) ? null : temp;
        }
    }
}
