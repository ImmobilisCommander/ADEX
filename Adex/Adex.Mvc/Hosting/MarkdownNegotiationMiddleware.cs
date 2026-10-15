using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Adex.Mvc.Hosting
{
    /// <summary>
    /// Négociation de contenu : une requête <c>Accept: text/markdown</c> reçoit la page HTML convertie en Markdown.
    /// </summary>
    public class MarkdownNegotiationMiddleware(RequestDelegate next)
    {
        private const string MarkdownType = "text/markdown";
        // ReverseMarkdown parcourt le DOM HTML et émet l'équivalent Markdown (titres, listes, liens, tableaux...).
        // GithubFlavored active la syntaxe GFM pour les tableaux et le texte barré.
        private static readonly ReverseMarkdown.Converter Converter = new(
            new ReverseMarkdown.Config
            {
                GithubFlavored = true,
            }
        );

        public async Task InvokeAsync(HttpContext context)
        {
            if (!WantsMarkdown(context.Request))
            {
                await next(context);
                return;
            }

            // Le HTML est produit normalement par le pipeline (Razor) dans un tampon mémoire,
            // puis converti après coup : les contrôleurs et vues n'ont rien à connaître du Markdown.
            var original = context.Response.Body;
            await using var buffer = new MemoryStream();
            context.Response.Body = buffer;
            try
            {
                await next(context);
                context.Response.Body = original;
                buffer.Position = 0;

                // Seules les réponses HTML sont converties (JSON, fichiers statiques, redirections : inchangés).
                var contentType = context.Response.ContentType;
                if (
                    contentType is null
                    || !contentType.StartsWith("text/html", System.StringComparison.OrdinalIgnoreCase)
                )
                {
                    await buffer.CopyToAsync(original, context.RequestAborted);
                    return;
                }

                using var reader = new StreamReader(buffer, Encoding.UTF8);
                var html = await reader.ReadToEndAsync(context.RequestAborted);
                var markdown = Converter.Convert(ExtractMain(html)).Trim();
                var bytes = Encoding.UTF8.GetBytes(markdown);

                // Le corps change de taille et de type : on met à jour les en-têtes en conséquence.
                // Estimation du nombre de jetons : environ 4 octets par jeton.
                context.Response.ContentType = $"{MarkdownType}; charset=utf-8";
                context.Response.ContentLength = bytes.Length;
                context.Response.Headers["x-markdown-tokens"] = ((bytes.Length + 3) / 4).ToString();
                context.Response.Headers.Append(HeaderNames.Vary, HeaderNames.Accept);
                await original.WriteAsync(bytes, context.RequestAborted);
            }
            finally
            {
                context.Response.Body = original;
            }
        }

        private static bool WantsMarkdown(HttpRequest request)
        {
            if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
            {
                return false;
            }

            return request.GetTypedHeaders().Accept.Any(a =>
                a.MediaType.Equals(MarkdownType, System.StringComparison.OrdinalIgnoreCase)
                && (a.Quality ?? 1) > 0
            );
        }

        // Ne garde que le contenu de <main> pour écarter navigation, en-tête et pied de page ;
        // sans <main>, la page entière est convertie.
        private static string ExtractMain(string html)
        {
            var start = html.IndexOf("<main", System.StringComparison.OrdinalIgnoreCase);
            var end = html.LastIndexOf("</main>", System.StringComparison.OrdinalIgnoreCase);
            return start >= 0 && end > start ? html[start..(end + 7)] : html;
        }
    }
}
