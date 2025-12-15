# Guide de Publication des Releases - Windows Cleaner

Ce guide explique comment publier une nouvelle version de Windows Cleaner sur GitHub pour que le système de mise à jour automatique fonctionne correctement.

## 📋 Prérequis

- [ ] Compte GitHub avec accès au dépôt
- [ ] Version stable prête à publier
- [ ] Binaires compilés (exe, zip, msi)
- [ ] Notes de version rédigées
- [ ] Tests effectués sur la version

## 🚀 Processus de Publication

### 1. Préparer la Version

#### A. Mettre à jour le numéro de version

Modifiez ces fichiers :

**WindowsCleaner.csproj**
```xml
<Version>1.0.9</Version>
<FileVersion>1.0.9.0</FileVersion>
<InformationalVersion>1.0.9</InformationalVersion>
```

**MainForm.cs**
```csharp
var updateManager = new UpdateManager("christwadel65-ux", "Windows-Cleaner", "1.0.9");
```

**app.manifest**
```xml
<assemblyIdentity version="1.0.9.0" ... />
```

#### B. Compiler les binaires

```powershell
# Release x64
dotnet build -c Release -r win-x64

# Ou via Visual Studio : Build > Build Solution (Release mode)
```

#### C. Créer les packages

**Option 1 : Installateur (recommandé)**
```powershell
# Utiliser Inno Setup
iscc build/windows-cleaner.iss
```

**Option 2 : Archive ZIP**
```powershell
# Compresser le dossier release
Compress-Archive -Path "bin\Release\net10.0-windows\*" -DestinationPath "windows-cleaner-v1.0.9-portable.zip"
```

### 2. Créer le Tag Git

```bash
# Créer et pousser le tag
git tag -a v1.0.9 -m "Release version 1.0.9"
git push origin v1.0.9
```

**Format du tag : `v{MAJOR}.{MINOR}.{PATCH}`**

### 3. Créer la Release sur GitHub

#### A. Via l'Interface Web

1. Allez sur https://github.com/votre-username/Windows-Cleaner/releases
2. Cliquez sur **"Draft a new release"**
3. Remplissez les champs :

**Tag version :** `v1.0.9`  
**Release title :** `Windows Cleaner v1.0.9`  
**Description :**

```markdown
## ✨ Nouveautés

- Système de mise à jour automatique depuis GitHub
- Vérification au démarrage et manuelle
- Notification dans la barre de statut

## 🔧 Améliorations

- Optimisation des performances
- Interface utilisateur améliorée

## 🐛 Corrections

- Fix bug critique XYZ
- Correction fuite mémoire

## 📦 Fichiers

- `windows-cleaner-v1.0.9-setup.exe` : Installateur Windows (recommandé)
- `windows-cleaner-v1.0.9-portable.zip` : Version portable sans installation

## 📊 Statistiques

- Taille installateur : 2.5 MB
- Taille portable : 3.8 MB
- Framework : .NET 10.0-windows

## ⚙️ Installation

### Avec l'installateur
1. Téléchargez `windows-cleaner-v1.0.9-setup.exe`
2. Exécutez le fichier (droits admin requis)
3. Suivez les instructions

### Version portable
1. Téléchargez `windows-cleaner-v1.0.9-portable.zip`
2. Extrayez le contenu
3. Lancez `windows-cleaner.exe`

## 🔗 Liens

- [Documentation](https://github.com/votre-username/Windows-Cleaner/tree/main/docs)
- [Changelog complet](https://github.com/votre-username/Windows-Cleaner/blob/main/CHANGELOG.md)
- [Issues](https://github.com/votre-username/Windows-Cleaner/issues)
```

4. **Attachez les fichiers** :
   - Glissez-déposez `windows-cleaner-v1.0.9-setup.exe`
   - Glissez-déposez `windows-cleaner-v1.0.9-portable.zip`

5. Cochez/décochez selon le cas :
   - [ ] **Set as a pre-release** (version bêta/test)
   - [x] **Set as the latest release** (version stable)

6. Cliquez sur **"Publish release"**

#### B. Via GitHub CLI

```bash
# Créer la release avec fichiers
gh release create v1.0.9 \
  --title "Windows Cleaner v1.0.9" \
  --notes-file release-notes.md \
  windows-cleaner-v1.0.9-setup.exe \
  windows-cleaner-v1.0.9-portable.zip
```

### 4. Vérifier la Publication

Après publication, vérifiez que :

✅ Le tag `v1.0.9` est visible  
✅ La release apparaît dans l'onglet "Releases"  
✅ Les fichiers sont téléchargeables  
✅ L'API GitHub retourne la bonne version :

```bash
curl https://api.github.com/repos/votre-username/Windows-Cleaner/releases/latest
```

**Réponse attendue :**
```json
{
  "tag_name": "v1.0.9",
  "name": "Windows Cleaner v1.0.9",
  ...
}
```

### 5. Tester la Mise à Jour

1. Lancez Windows Cleaner v1.0.8
2. Attendez quelques secondes (vérification automatique)
3. Vérifiez la barre de statut : `✨ Nouvelle version disponible : 1.0.9`
4. Cliquez sur **Aide > 🔄 Vérifier les mises à jour**
5. Confirmez que le dialogue affiche la v1.0.9
6. Cliquez sur "Oui" et vérifiez que la page s'ouvre

## 📝 Checklist Complète

### Avant la Publication

- [ ] Code stable et testé
- [ ] Tous les tests passent
- [ ] Documentation à jour
- [ ] CHANGELOG.md mis à jour
- [ ] Numéros de version cohérents dans tous les fichiers
- [ ] Binaires compilés en mode Release
- [ ] Packages créés (exe, zip)
- [ ] Notes de version rédigées

### Pendant la Publication

- [ ] Tag Git créé avec bon format (v1.0.9)
- [ ] Tag poussé sur GitHub
- [ ] Release GitHub créée
- [ ] Fichiers attachés à la release
- [ ] Description complète avec markdown
- [ ] Coché "Set as the latest release"

### Après la Publication

- [ ] Release visible sur GitHub
- [ ] Fichiers téléchargeables
- [ ] API GitHub retourne la bonne version
- [ ] Test de mise à jour réussi depuis version précédente
- [ ] Annonce aux utilisateurs (si applicable)

## 🔄 Workflow Automatisé (GitHub Actions)

Pour automatiser la publication, créez `.github/workflows/release.yml` :

```yaml
name: Release

on:
  push:
    tags:
      - 'v*'

jobs:
  build-and-release:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '10.0.x'
    
    - name: Build Release
      run: dotnet build -c Release
    
    - name: Create Setup
      run: iscc build/windows-cleaner.iss
    
    - name: Create ZIP
      run: Compress-Archive -Path "bin\Release\net10.0-windows\*" -DestinationPath "windows-cleaner-portable.zip"
    
    - name: Create Release
      uses: softprops/action-gh-release@v1
      with:
        files: |
          Output/windows-cleaner-setup.exe
          windows-cleaner-portable.zip
        body_path: release-notes.md
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

**Utilisation :**
```bash
git tag -a v1.0.9 -m "Release 1.0.9"
git push origin v1.0.9
# GitHub Actions créera automatiquement la release
```

## 🎯 Bonnes Pratiques

### Versionnage Sémantique

**Format : MAJOR.MINOR.PATCH**

- **MAJOR (1.x.x)** : Breaking changes
  - Modifications incompatibles de l'API
  - Changements majeurs d'architecture
  - Exemple : 1.0.9 → 2.0.0

- **MINOR (x.1.x)** : Nouvelles fonctionnalités
  - Ajouts compatibles
  - Nouvelles fonctionnalités
  - Exemple : 1.0.9 → 1.1.0

- **PATCH (x.x.1)** : Corrections
  - Bugs fixes
  - Corrections de sécurité
  - Exemple : 1.0.9 → 1.0.10

### Nommage des Fichiers

✅ **BON :**
- `windows-cleaner-v1.0.9-setup.exe`
- `windows-cleaner-v1.0.9-portable.zip`
- `windows-cleaner-v1.0.9-x64.msi`

❌ **MAUVAIS :**
- `setup.exe` (trop vague)
- `WindowsCleaner.zip` (pas de version)
- `WC_1.0.9.exe` (abréviation obscure)

### Notes de Version

Incluez toujours :
- ✅ Liste des nouveautés
- ✅ Corrections de bugs
- ✅ Améliorations de performance
- ✅ Instructions d'installation
- ✅ Breaking changes (si applicable)
- ✅ Liens vers documentation

## 🚨 Que Faire en Cas d'Erreur

### Release publiée avec mauvaise version

```bash
# Supprimer la release et le tag
gh release delete v1.0.9 --yes
git tag -d v1.0.9
git push origin :refs/tags/v1.0.9

# Recréer avec bon numéro
git tag -a v1.0.10 -m "Correct version"
git push origin v1.0.10
```

### Fichiers manquants dans la release

1. Allez sur la page de la release
2. Cliquez sur "Edit release"
3. Ajoutez les fichiers manquants
4. Cliquez sur "Update release"

### Mauvaises notes de version

1. Allez sur la page de la release
2. Cliquez sur "Edit release"
3. Modifiez la description
4. Cliquez sur "Update release"

## 📞 Support

Pour toute question sur le processus de release :

- 📖 Consultez la [documentation GitHub](https://docs.github.com/en/repositories/releasing-projects-on-github)
- 💬 Ouvrez une discussion sur GitHub Discussions
- 🐛 Signalez un problème sur GitHub Issues

---

**Version du document :** 1.0  
**Dernière mise à jour :** 15 décembre 2025
