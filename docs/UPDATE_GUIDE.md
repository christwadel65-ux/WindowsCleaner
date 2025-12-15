# Guide de Mise à Jour - Windows Cleaner

## 🔄 Système de Mise à Jour Automatique

Windows Cleaner intègre un système de mise à jour automatique qui vérifie les nouvelles versions disponibles sur GitHub.

## 📋 Fonctionnalités

### 1. Vérification Automatique au Démarrage

Chaque fois que vous lancez Windows Cleaner :
- ✅ L'application vérifie automatiquement les nouvelles versions sur GitHub
- ✅ Si une mise à jour est disponible, une notification apparaît dans la barre de statut
- ✅ La vérification se fait en arrière-plan sans bloquer l'application
- ✅ Aucune action requise de votre part

**Notification exemple :**
```
✨ Nouvelle version disponible : 1.0.9 (Cliquez sur Aide > Vérifier les mises à jour)
```

### 2. Vérification Manuelle

Pour vérifier manuellement les mises à jour :

1. Cliquez sur **Aide** dans la barre de menu
2. Sélectionnez **🔄 Vérifier les mises à jour**
3. Un dialogue s'affiche avec :
   - Version actuelle installée
   - Nouvelle version disponible
   - Date de publication
   - Notes de version (changelog)
   - Bouton pour télécharger

**Exemple de dialogue :**
```
Une nouvelle version est disponible !

Version actuelle : 1.0.8
Nouvelle version : 1.0.9
Date de publication : 15/12/2025

Notes de version :
- Correction de bugs critiques
- Amélioration des performances
- Nouvelles fonctionnalités...

Voulez-vous ouvrir la page de téléchargement ?
[Oui] [Non]
```

### 3. Téléchargement et Installation

1. Cliquez sur **Oui** dans le dialogue de mise à jour
2. Votre navigateur s'ouvre sur la page GitHub Release
3. Téléchargez le fichier d'installation (.exe, .zip ou .msi)
4. Fermez Windows Cleaner
5. Exécutez le fichier téléchargé
6. Suivez les instructions d'installation

## 🔧 Configuration Technique

### Dépôt GitHub

Par défaut, le système vérifie les mises à jour sur :
- **Propriétaire :** `christwadel65-ux`
- **Dépôt :** `Windows-Cleaner`
- **API GitHub :** https://api.github.com/repos/christwadel65-ux/Windows-Cleaner/releases/latest

### Modification du Dépôt

Si vous forkez le projet, modifiez les paramètres dans [MainForm.cs](../src/WindowsCleaner/UI/MainForm.cs) :

```csharp
private async Task CheckForUpdatesAsync(bool silent)
{
    // Changez ces valeurs selon votre dépôt
    var updateManager = new UpdateManager(
        "votre-username",      // Propriétaire GitHub
        "votre-repo",          // Nom du dépôt
        "1.0.8"               // Version actuelle
    );
    // ...
}
```

## 📊 Comparaison des Versions

Le système utilise le **versionnage sémantique** (Semantic Versioning) :

### Format : `MAJOR.MINOR.PATCH`

- **MAJOR** : Changements incompatibles (breaking changes)
- **MINOR** : Nouvelles fonctionnalités compatibles
- **PATCH** : Corrections de bugs

### Exemples de Comparaison

```
1.0.9 > 1.0.8   ✅ Mise à jour disponible
1.1.0 > 1.0.8   ✅ Mise à jour disponible
2.0.0 > 1.0.8   ✅ Mise à jour majeure disponible
1.0.8 = 1.0.8   ℹ️ Vous avez la dernière version
1.0.7 < 1.0.8   ℹ️ Version de développement
```

## 🔐 Sécurité

### Connexion HTTPS

- ✅ Toutes les communications utilisent **HTTPS**
- ✅ API GitHub officielle (api.github.com)
- ✅ Vérification SSL/TLS automatique

### Vérification des Fichiers

Après téléchargement, il est recommandé de :
1. Vérifier la signature numérique du fichier
2. Comparer le hash MD5/SHA256 si fourni
3. Télécharger uniquement depuis les releases officielles GitHub

## 🛠️ Dépannage

### Erreur "Impossible de vérifier les mises à jour"

**Causes possibles :**
- Pas de connexion Internet
- Pare-feu bloquant l'accès à GitHub
- Dépôt GitHub temporairement indisponible
- Limite de taux API GitHub atteinte

**Solutions :**
1. Vérifiez votre connexion Internet
2. Réessayez dans quelques minutes
3. Vérifiez manuellement sur GitHub : https://github.com/christwadel65-ux/Windows-Cleaner/releases

### La vérification automatique ne fonctionne pas

**Vérifications :**
- L'application doit avoir accès à Internet
- Vérifiez les logs dans `Fichier > Lire les logs`
- Recherchez les messages contenant "mise à jour"

**Désactivation temporaire :**
Si vous souhaitez désactiver la vérification automatique, commentez cette ligne dans le constructeur :

```csharp
// _ = CheckForUpdatesAsync(silent: true);
```

### Timeout de connexion

Si la vérification prend trop de temps :
- Le timeout par défaut est de **30 secondes**
- Modifiable dans `UpdateManager.cs` :

```csharp
_httpClient.Timeout = TimeSpan.FromSeconds(60); // 60 secondes
```

## 📝 Logs

Les opérations de mise à jour sont enregistrées dans les logs :

```
🔍 Vérification des mises à jour...
✨ Nouvelle version disponible : 1.0.9 (actuelle: 1.0.8)
🌐 Page de release ouverte : https://github.com/...
```

Pour consulter les logs :
1. Menu **Fichier > 📖 Lire les logs**
2. Recherchez les messages avec 🔍, ✨, 🌐

## 🎯 Bonnes Pratiques

### Pour les Utilisateurs

1. ✅ Laissez la vérification automatique activée
2. ✅ Mettez à jour régulièrement pour bénéficier des corrections
3. ✅ Lisez les notes de version avant de mettre à jour
4. ✅ Créez un point de restauration avant l'installation

### Pour les Développeurs

1. ✅ Suivez le versionnage sémantique strict
2. ✅ Créez des releases GitHub avec tags appropriés (v1.0.9)
3. ✅ Incluez des notes de version détaillées
4. ✅ Attachez les binaires (.exe, .zip, .msi) aux releases
5. ✅ Testez les mises à jour avant publication

## 📦 Format des Releases GitHub

### Structure Recommandée

```yaml
Tag: v1.0.9
Nom: Windows Cleaner v1.0.9
Description: |
  ## Nouveautés
  - Système de mise à jour automatique
  - Amélioration des performances
  
  ## Corrections
  - Fix bug critique XYZ
  
Fichiers attachés:
  - windows-cleaner-v1.0.9-setup.exe (Installateur)
  - windows-cleaner-v1.0.9-portable.zip (Version portable)
```

### Fichiers Supportés

Le système recherche automatiquement ces extensions :
- `.exe` - Installateur Windows
- `.zip` - Archive portable
- `.msi` - Package MSI

## 🌐 API GitHub

### Limites de Taux

GitHub API limite les requêtes :
- **Sans authentification** : 60 requêtes/heure
- **Avec authentification** : 5000 requêtes/heure

Pour une utilisation intensive, envisagez d'ajouter un token GitHub.

### Endpoint Utilisé

```
GET https://api.github.com/repos/{owner}/{repo}/releases/latest
```

**Réponse JSON :**
```json
{
  "tag_name": "v1.0.9",
  "name": "Windows Cleaner v1.0.9",
  "body": "Release notes...",
  "html_url": "https://github.com/.../releases/tag/v1.0.9",
  "published_at": "2025-12-15T10:30:00Z",
  "prerelease": false,
  "assets": [
    {
      "name": "windows-cleaner-setup.exe",
      "browser_download_url": "https://github.com/.../windows-cleaner-setup.exe",
      "size": 2048000
    }
  ]
}
```

## 📞 Support

En cas de problème avec les mises à jour :

1. Consultez les logs de l'application
2. Vérifiez la page GitHub Releases manuellement
3. Ouvrez une issue sur GitHub
4. Contactez le support technique

---

**Version du document :** 1.0  
**Dernière mise à jour :** 15 décembre 2025
