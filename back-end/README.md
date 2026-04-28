# DocApi - Document Management API

## 📋 Description

DocApi est une API REST développée en .NET 8 pour la gestion de documents avec authentification JWT. L'application suit une architecture en couches (Clean Architecture) avec séparation claire des responsabilités.

## 🏗️ Architecture

### Structure du Projet

```
DocApi/
├── Controllers/           # Couche de présentation (API endpoints)
├── Services/             # Couche de logique métier
│   └── Interfaces/       # Contrats des services
├── Repositories/         # Couche d'accès aux données
│   └── Interfaces/       # Contrats des repositories
├── Domain/              # Entités du domaine
│   └── Entities/        # Modèles de données
├── DTOs/                # Data Transfer Objects
├── Infrastructure/      # Configuration et services techniques
├── Common/              # Classes utilitaires et exceptions
├── Database/            # Scripts SQL et migrations
└── Properties/          # Configuration de lancement
```

### Architecture en Couches

#### 1. **Controllers** (Couche Présentation)
- **Responsabilité** : Gestion des requêtes HTTP, validation des entrées, formatage des réponses
- **Dépendances** : Services uniquement
- **Exemple** : `AuthController`, `DocumentController`

#### 2. **Services** (Couche Logique Métier)
- **Responsabilité** : Implémentation de la logique métier, orchestration des opérations
- **Dépendances** : Repositories, DTOs, Entities
- **Exemple** : `AuthService`, `DocumentService`

#### 3. **Repositories** (Couche Accès aux Données)
- **Responsabilité** : Accès aux données, requêtes SQL avec Dapper
- **Dépendances** : Infrastructure, Entities
- **Exemple** : `UserRepository`, `DocumentRepository`

#### 4. **Infrastructure** (Couche Technique)
- **Responsabilité** : Configuration technique, connexions DB, services externes
- **Exemple** : `DbConnectionFactory`

## 🛠️ Technologies Utilisées

- **.NET 8** - Framework principal
- **ASP.NET Core** - API REST
- **MySQL** - Base de données
- **Dapper** - ORM léger pour l'accès aux données
- **JWT Bearer** - Authentification et autorisation
- **BCrypt.Net** - Hachage des mots de passe
- **Swagger/OpenAPI** - Documentation API
- **Dependency Injection** - Inversion de contrôle

## 🔐 Sécurité

### Authentification JWT
- **Algorithme** : HMAC SHA-256
- **Durée de vie** : 60 minutes (configurable)
- **Claims inclus** : UserId, Username, Role, JTI, IAT

### Gestion des Mots de Passe
- **Hachage** : BCrypt avec salt automatique
- **Validation** : Vérification sécurisée avec BCrypt.Verify

### Autorisation
- **Basée sur les rôles** : Admin, User
- **Middleware** : ASP.NET Core Authorization
- **Endpoints protégés** : Attribut `[Authorize]`

## 📊 Base de Données

### Schéma Principal

#### Table Users
```sql
- Id (INT, PK, AUTO_INCREMENT)
- Username (VARCHAR(50), UNIQUE)
- Email (VARCHAR(100), UNIQUE)
- PasswordHash (VARCHAR(255))
- Role (VARCHAR(20))
- CreatedAt (DATETIME)
- IsActive (BOOLEAN)
```

#### Table TypeDocument
```sql
- Id (INT, PK, AUTO_INCREMENT)
- Name (VARCHAR(100), UNIQUE)
- Description (TEXT)
- CreatedAt (DATETIME)
- UpdatedAt (DATETIME)
- CreatedByUserId (INT, FK)
- UpdatedByUserId (INT, FK)
```

#### Table Document
```sql
- Id (INT, PK, AUTO_INCREMENT)
- Title (VARCHAR(255))
- FilePath (VARCHAR(500))
- CreatedAt (DATETIME)
- UpdatedAt (DATETIME)
- TypeDocumentId (INT, FK)
- CreatedByUserId (INT, FK)
- UpdatedByUserId (INT, FK)
```

## 🎯 Style de Codage

### Conventions de Nommage

#### Classes et Interfaces
```csharp
// Classes : PascalCase
public class UserService { }

// Interfaces : PascalCase avec préfixe 'I'
public interface IUserService { }

// Contrôleurs : PascalCase avec suffixe 'Controller'
public class AuthController : ControllerBase { }
```

#### Méthodes et Propriétés
```csharp
// Méthodes : PascalCase avec verbe d'action
public async Task<User> GetByIdAsync(int id) { }
public async Task<bool> CreateAsync(User user) { }

// Propriétés : PascalCase
public string Username { get; set; }
public DateTime CreatedAt { get; set; }
```

#### Variables et Paramètres
```csharp
// Variables locales : camelCase
var connectionString = "...";
var userRepository = new UserRepository();

// Paramètres : camelCase
public async Task<User> GetByIdAsync(int userId) { }
```

#### Champs Privés
```csharp
// Champs privés : camelCase avec underscore
private readonly IUserRepository _userRepository;
private readonly JwtSettings _jwtSettings;
```

### Patterns Utilisés

#### 1. **Repository Pattern**
```csharp
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<int> CreateAsync(User user);
    Task<bool> UpdateAsync(User user);
    Task<bool> DeleteAsync(int id);
}
```

#### 2. **Dependency Injection**
```csharp
// Configuration dans Program.cs
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Injection dans le constructeur
public class AuthService
{
    private readonly IUserRepository _userRepository;
    
    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
}
```

#### 3. **Factory Pattern**
```csharp
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class DbConnectionFactory : IDbConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }
}
```

### Gestion des Erreurs

#### Exceptions Personnalisées
```csharp
public class ServiceException : Exception
{
    public ServiceException(string message) : base(message) { }
}

public class NotFoundException : ServiceException
{
    public NotFoundException(string message) : base(message) { }
}
```

#### Gestion dans les Contrôleurs
```csharp
[HttpPost("login")]
public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
{
    try
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }
    catch (ServiceException ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}
```

### Validation des Données

#### DTOs avec DataAnnotations
```csharp
public class LoginRequest
{
    [Required]
    public required string Username { get; set; }
    
    [Required]
    public required string Password { get; set; }
}

public class RegisterRequest
{
    [Required]
    [MinLength(3)]
    public required string Username { get; set; }
    
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
    
    [Required]
    [MinLength(6)]
    public required string Password { get; set; }
}
```

## 🚀 Configuration et Déploiement

### Variables d'Environnement

#### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=host;Port=port;Database=db;User=user;Password=pass;"
  },
  "JwtSettings": {
    "SecretKey": "YourSecretKey32CharactersMinimum!",
    "Issuer": "DocApi",
    "Audience": "DocApiUsers",
    "ExpirationInMinutes": 60
  }
}
```

### Commandes de Démarrage

```bash
# Restaurer les packages
dotnet restore

# Compiler l'application
dotnet build

# Lancer l'application
dotnet run

# Lancer sur un port spécifique
dotnet run --urls "http://localhost:5186"
```

## 📚 API Endpoints

### Authentification
- `POST /api/auth/login` - Connexion utilisateur
- `POST /api/auth/register` - Inscription utilisateur
- `GET /api/auth/profile` - Profil utilisateur (protégé)

### Documents
- `GET /api/document` - Liste des documents (protégé)
- `POST /api/document` - Créer un document (protégé)
- `GET /api/document/{id}` - Détails d'un document (protégé)
- `PUT /api/document/{id}` - Modifier un document (protégé)
- `DELETE /api/document/{id}` - Supprimer un document (protégé)

### Types de Documents
- `GET /api/typedocument` - Liste des types (protégé)
- `POST /api/typedocument` - Créer un type (protégé)

### Administration
- `POST /api/admin/update-password-hashes` - Mettre à jour les hashes
- `GET /api/admin/verify-users` - Vérifier les utilisateurs
- `GET /api/admin/test-auth` - Tester l'autorisation (protégé)

## 🧪 Tests et Debugging

### Utilisateurs de Test
- **Admin** : `admin` / `admin123`
- **User** : `user1` / `user123`
- **Manager** : `manager` / `manager123`

### Swagger UI
Accédez à la documentation interactive : `http://localhost:5186/swagger`

### Logs de Debug
L'application inclut des logs de debug pour :
- Génération des tokens JWT
- Validation des tokens
- Erreurs d'authentification

## 📝 Bonnes Pratiques

### Code Quality
- **Async/Await** : Toutes les opérations I/O sont asynchrones
- **Using Statements** : Gestion automatique des ressources
- **Nullable Reference Types** : Activé pour éviter les NullReferenceException
- **Separation of Concerns** : Chaque couche a une responsabilité claire

### Sécurité
- **Validation des entrées** : DataAnnotations sur tous les DTOs
- **Hachage sécurisé** : BCrypt pour les mots de passe
- **Tokens JWT** : Expiration et validation appropriées
- **HTTPS** : Redirection automatique en production

### Performance
- **Connection Pooling** : Gestion optimisée des connexions DB
- **Dapper** : ORM léger pour de meilleures performances
- **Dependency Injection** : Scoped lifetime pour les services

## 🔧 Maintenance

### Scripts de Base de Données
- `Database/CreateDatabase.sql` - Script complet de création de la base de données avec données de test

### Monitoring
- Logs structurés avec les niveaux appropriés
- Gestion des exceptions centralisée
- Métriques de performance disponibles

---

**Développé avec ❤️ en .NET 8**