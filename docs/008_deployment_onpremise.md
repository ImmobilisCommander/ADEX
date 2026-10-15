# Déploiement on premise (Windows + Cloudflare Tunnel)

## Principe

Le MVC est le seul composant publié. `cloudflared` (service Windows, tunnel géré en ligne par jeton) relaie `https://adex.laboratoires.dev` vers le MVC en HTTPS sur la machine, avec un certificat auto-signé. Le chiffrement est donc conservé de bout en bout. L'API et PostgreSQL ne sont joignables que depuis la machine.

| Composant | Écoute | Exposé |
|---|---|---|
| MVC | `https://localhost:5001` | Oui, via le tunnel |
| API | `https://localhost:5002` | Non |
| Serveur MCP | `https://localhost:5003` | Non |
| PostgreSQL | `localhost` | Non |

Dans Cloudflare (Zero Trust → Tunnels → route publiée) : type `HTTPS`, URL `localhost:5001`, **No TLS Verify** activé (certificat auto-signé).

## Disposition de publication

```
<racine>\api\    Adex.WebApi (publié)
<racine>\web\    Adex.Mvc (publié)
<racine>\data\   declarations.csv
```

Les chemins `Import:DataDirectory` et `Csv:FilePath` pointent en production vers le chemin absolu `D:\publish\adex\data` (indépendant du dossier de travail du service). Si la racine de publication change, adapter `appsettings.Production.json` de l'API.

## Configuration

Le socle commun reste dans `appsettings.json` (Serilog, journaux sous `D:\Logs`, TTL du tableau de bord, cache CSV). Chaque environnement ne porte que ses différences :

| Paramètre | Development | Production |
|---|---|---|
| Écoute (`Urls`) | profils `launchSettings.json` | `https://localhost:5001` (MVC), `https://localhost:5002` (API), `https://localhost:5003` (MCP) |
| `AdexApi:BaseAddress` (MVC, MCP) | `https://localhost:44330/` (MVC) ; `https://localhost:7002/` (MCP) | `https://localhost:5002/` |
| `Import:DataDirectory`, `Csv:FilePath` | `D:\publish\adex\data` | `D:\publish\adex\data` |
| `AllowedHosts` | `*` | MVC : `adex.laboratoires.dev;localhost` ; API : `127.0.0.1;localhost` |
| Niveau `Adex.Application.Csv` | `Debug` | `Information` |

Lancer avec `ASPNETCORE_ENVIRONMENT=Production`.

- Le MVC applique `UseForwardedHeaders` (`X-Forwarded-For/Proto/Host`, proxy de confiance : loopback uniquement).
- La redirection HTTPS de l'API est désactivée en production. Swagger reste limité au développement.
- La configuration se limite à `appsettings.json`, `appsettings.Development.json` et `appsettings.Production.json`. Aucune variable d'environnement machine n'est requise (elles seraient partagées entre toutes les applications de la machine). La chaîne PostgreSQL et `Import:ApiKey` sont identiques dans les deux environnements : ils restent dans `appsettings.json` et ne se surchargent dans `appsettings.Production.json` que lorsqu'ils divergeront.
- L'import reste désactivé sans `Import:ApiKey`. Remplacer `dev-import-key` par une clé forte avant exposition. Éviter l'utilisateur `postgres` : créer un rôle dédié aux droits limités.

## Certificat HTTPS

Kestrel ne charge le certificat de développement qu'en `Development`. En production, fournir un certificat par défaut, par exemple en exportant le certificat de développement (valable pour `localhost`, un an) :

```powershell
New-Item -ItemType Directory C:\Adex\Certs -Force
dotnet dev-certs https -ep C:\Adex\Certs\adex.pfx
```

Le chemin se déclare dans l'`appsettings.Production.json` déployé de chaque application (ou celui du dépôt, s'il est identique pour toutes les machines) :

```json
{ "Kestrel": { "Certificates": { "Default": { "Path": "C:\\Adex\\Certs\\adex.pfx" } } } }
```

Sans option `-p`, le PFX n'a pas de mot de passe : limiter l'accès au dossier au compte qui exécute les services. Avec un mot de passe, ajouter `"Password"` à la même section. Pour que l'appel MVC → API soit accepté, le certificat doit être approuvé sur la machine (`dotnet dev-certs https --trust`, ou import dans les autorités racines). Il expire : le renouveler avant l'échéance.

## Page de maintenance (PC éteint)

Si le PC ou `cloudflared` est hors ligne, Cloudflare affiche son erreur 5xx (530, 1033, 502). `deploy/maintenance.html` (autonome, CSS en ligne, thème clair/sombre automatique, rechargement toutes les minutes) est la page de remplacement prévue.

- **Pages d'erreur personnalisées Cloudflare** (Custom Pages → 5xx) : la personnalisation des erreurs 5xx n'est probablement pas disponible sur le plan gratuit ; à vérifier sur le compte.
- **Alternative gratuite : un Worker** sur la route du site. Il appelle l'origine ; en cas d'échec ou de réponse 5xx, il renvoie le contenu de `maintenance.html` avec le statut 503 et `Retry-After`.

## Exploitation

- Publier avec `dotnet publish -c Release` selon la disposition ci-dessus, puis exécuter chaque hôte comme service Windows à démarrage automatique et reprise sur échec.
- L'API applique les migrations `Adex` au démarrage : sauvegarder la base avant chaque mise à jour (voir [Migrations PostgreSQL](007_postgresql_migrations.md)).
- Vérifier que `https://adex.laboratoires.dev/api/import` renvoie 404 et qu'aucun port entrant n'est ouvert sur la box.
- Prévoir une sauvegarde `pg_dump` régulière avec copie hors du poste, et la tester.
