using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Adex.Mvc.Sitemap
{
    public static class SitemapXmlBuilder
    {
        public static string BuildXml(
            string baseUrl,
            IEnumerable<string> staticPaths,
            IEnumerable<Guid> contributorIds,
            IEnumerable<Guid> beneficiaryIds
        )
        {
            var sb = new StringBuilder(1024 * 512);
            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = false,
                OmitXmlDeclaration = false,
            };

            using (var writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

                foreach (var path in staticPaths)
                {
                    writer.WriteStartElement("url");
                    writer.WriteElementString("loc", $"{baseUrl}{path}");
                    writer.WriteEndElement();
                }

                foreach (var id in contributorIds)
                {
                    writer.WriteStartElement("url");
                    writer.WriteElementString("loc", $"{baseUrl}/{id}");
                    writer.WriteEndElement();
                }

                foreach (var id in beneficiaryIds)
                {
                    writer.WriteStartElement("url");
                    writer.WriteElementString("loc", $"{baseUrl}/{id}");
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            return sb.ToString();
        }
    }
}
