# 🎉 Windows Cleaner v2.0.0 - Release Notes

**Date de sortie** : 15 décembre 2025

## 🚀 Version Majeure - Refonte Complète

Windows Cleaner passe en version 2.0.0 avec des fonctionnalités majeures qui transforment l'application en un outil encore plus puissant pour les développeurs et les utilisateurs avancés.

---

## ✨ Nouvelles Fonctionnalités

### 1. 🔄 Système de Mise à Jour Automatique

L'application vérifie maintenant automatiquement les nouvelles versions disponibles sur GitHub !

**Fonctionnalités :**
- ✅ Vérification automatique au démarrage
- ✅ Menu "Aide > 🔄 Vérifier les mises à jour"
- ✅ Notification dans la barre de statut
- ✅ Dialogue avec notes de version complètes
- ✅ Ouverture directe de la page GitHub Release
- ✅ Comparaison intelligente des versions (sémantique)
- ✅ Pas de téléchargement forcé - vous gardez le contrôle

**Comment ça marche :**
1. Lancez l'application
2. Si une nouvelle version existe, vous verrez : `✨ Nouvelle version disponible : X.Y.Z`
3. Cliquez sur le message ou allez dans **Aide > 🔄 Vérifier les mises à jour**
4. Lisez les notes de version
5. Cliquez sur "Oui" pour ouvrir la page de téléchargement

---

### 2. 💻 Interface de Nettoyage Développeur

**Nouveau groupe dans l'interface** avec 10 options spécialisées pour les développeurs !

#### Options Disponibles

| Option | Description | Gain d'espace typique |
|--------|-------------|----------------------|
| 📦 **VS Code** | Cache Visual Studio Code | 100-500 MB |
| 📦 **NuGet** | Packages NuGet mis en cache | 500 MB - 2 GB |
| 📦 **Maven** | Repository Maven local | 1-5 GB |
| 📦 **npm** | Cache npm global | 500 MB - 2 GB |
| 🐳 **Docker** | Images et conteneurs inutilisés | 5-20 GB |
| 📁 **node_modules** | Vieux dossiers (> 30 jours) | 1-10 GB |
| 🔨 **Visual Studio** | obj, bin, .vs | 500 MB - 5 GB |
| 🐍 **Python** | __pycache__, *.pyc | 100-500 MB |
| 📂 **Git** | Garbage collection repos | 100-1000 MB |
| 🎮 **Jeux** | Caches Steam/Epic | 1-5 GB |

**Intégration complète :**
- ✅ Profil "Nettoyage Développeur" enrichi
- ✅ Boutons "Tout" et "Rien" incluent ces options
- ✅ Statistiques détaillées dans les rapports HTML
- ✅ Sauvegarde automatique des sessions

**Gain potentiel total : 10-50 GB selon votre environnement de développement !**

---

### 3. 📊 Statistiques SSD Améliorées

Vos optimisations SSD sont maintenant **trackées et visibles** dans les statistiques !

**Améliorations :**
- ✅ Compteur "Optimisations TRIM" fonctionnel
- ✅ Compteur "Vérifications SMART" fonctionnel
- ✅ Rapport SMART détaillé sauvegardé
- ✅ Détection multi-niveaux des disques (compatibilité maximale)
- ✅ Informations enrichies : modèle, statut, interface, taille, partitions

**Exemple de rapport SMART :**
```
=== DISQUES PHYSIQUES ===

Disque: Samsung SSD 970 EVO Plus
Statut: OK
Interface: NVMe
Taille: 500 GB
Partitions: 3

=== VOLUMES ===

Lecteur: C:
Type: NTFS
Santé: Healthy
Taille: 465.75 GB (Libre: 123.45 GB)
```

**Accès :** Menu **Outils > ⚡ Optimiser le système** puis **Outils > 📈 Voir les statistiques**

---

## 🔧 Améliorations Techniques

### Interface Utilisateur
- **Fenêtre agrandie** : 1220x850 pixels pour le nouveau groupe développeur
- **Layout optimisé** : 20+ options de nettoyage bien organisées
- **Groupes distincts** : Standard, Avancées, Développeur, Logs
- **Responsive** : Journal des opérations s'adapte automatiquement

### Profils de Nettoyage
- **Profil Développeur** : Toutes les options de cache activées par défaut
- **Profil Complet** : Inclut maintenant l'optimisation SSD
- **Sauvegarde complète** : Toutes les nouvelles options sauvegardées dans les profils personnalisés

### Détection des Disques
- **Méthode robuste** : Essaie Win32_DiskDrive puis Get-Volume en fallback
- **Sans admin** : Fonctionne même sans droits administrateur complets
- **Rapports enrichis** : Informations détaillées sur chaque disque et volume

---

## 📝 Documentation

### Nouveaux Guides
- **[UPDATE_GUIDE.md](UPDATE_GUIDE.md)** - Guide complet du système de mise à jour
- **[RELEASE_GUIDE.md](RELEASE_GUIDE.md)** - Comment publier une release sur GitHub
- **[prepare_release.ps1](../scripts/prepare_release.ps1)** - Script d'automatisation

### Mise à Jour
- **[CHANGELOG.md](../CHANGELOG.md)** - Historique complet des versions
- **[README.md](../README.md)** - Documentation principale mise à jour

---

## 🐛 Corrections de Bugs

- ✅ **Fix** : Statistiques SMART affichaient toujours 0
- ✅ **Fix** : Détection des disques échouait sur certains systèmes
- ✅ **Fix** : Boutons Tout/Rien n'incluaient pas les options développeur
- ✅ **Fix** : Profils ne sauvegardaient pas les options de cache
- ✅ **Fix** : Méthode StatisticsManager incorrecte (RecordCleaningSession vs SaveStatistics)

---

## ⚙️ Changements Breaking (Version Majeure)

### Pourquoi 2.0.0 ?

Cette version est marquée comme **majeure** car :

1. **Interface agrandie** : Nécessite résolution minimale 1220x850
2. **Nouvelles propriétés** : CleaningStatistics étendu (non rétrocompatible)
3. **20+ options** : Beaucoup plus d'options de nettoyage
4. **Architecture** : Ajout de UpdateManager et refonte des profils

### Migration depuis 1.x

- ✅ **Profils personnalisés** : Automatiquement migrés
- ✅ **Statistiques** : Anciennes stats préservées
- ⚠️ **Interface** : Peut nécessiter un écran plus grand
- ℹ️ **Paramètres** : Sauvegardés et restaurés automatiquement

---

## 📊 Statistiques du Développement

- **Lignes de code ajoutées** : 1,200+
- **Nouveaux fichiers** : 4 (UpdateManager.cs, 3 docs)
- **Fichiers modifiés** : 8
- **Temps de développement** : 1 journée intensive
- **Tests** : Compilation 0 erreur, tests manuels OK

---

## 🚀 Installation

### Depuis GitHub Release

1. Téléchargez `WindowsCleaner-Setup-2.0.0.exe` depuis la [page des releases](https://github.com/christwadel65-ux/Windows-Cleaner/releases)
2. Exécutez l'installateur
3. Suivez les instructions

### Version Portable

1. Téléchargez `WindowsCleaner-Portable-2.0.0.zip`
2. Extrayez dans un dossier
3. Lancez `windows-cleaner.exe`

### Prérequis

- **Windows** : 10 ou 11 (x64)
- **.NET Runtime** : 10.0 (inclus dans l'installateur)
- **Résolution** : Minimum 1220x850

---

## 🎯 Comment Utiliser les Nouvelles Fonctionnalités

### Mise à Jour Automatique

```
1. Lancez l'application
2. Vérification automatique en arrière-plan
3. Si maj disponible → Notification dans la barre de statut
4. Aide > Vérifier les mises à jour → Dialogue complet
5. Cliquez "Oui" → Page GitHub s'ouvre
```

### Nettoyage Développeur

```
1. Sélectionnez le profil "Nettoyage Développeur"
   OU
   Cochez manuellement les options dans le groupe "💻 Nettoyage Développeur"
   
2. Cliquez "🔍 Simuler" pour tester (recommandé)
   
3. Vérifiez le journal des opérations
   
4. Si OK, cliquez "🧹 Nettoyer"
   
5. Consultez les statistiques : Outils > 📈 Voir les statistiques
```

### Optimisation SSD avec Statistiques

```
1. Outils > ⚡ Optimiser le système
   
2. Confirmez l'exécution
   
3. Opérations : TRIM SSD, SMART, Compaction Registre, Nettoyage Mémoire
   
4. Session sauvegardée automatiquement
   
5. Outils > 📈 Voir les statistiques
   → Optimisations TRIM : 1 session(s)
   → Vérifications SMART : 1 session(s)
   → Dernier Rapport SMART visible
```

---

## 🙏 Remerciements

Merci à tous ceux qui ont testé les versions précédentes et fourni des retours précieux !

---

## 📞 Support

- **Issues** : [GitHub Issues](https://github.com/christwadel65-ux/Windows-Cleaner/issues)
- **Documentation** : [docs/](.)
- **Changelog** : [CHANGELOG.md](../CHANGELOG.md)

---

## 📜 Licence

Ce projet est sous licence MIT. Voir [LICENSE](../LICENSE) pour plus de détails.

---

**Profitez de Windows Cleaner 2.0.0 ! 🎉**
