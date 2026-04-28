using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using DocApi.Domain;
using DocApi.Repositories.Interfaces;
using MySql.Data.MySqlClient;


namespace DocApi.Repositories
{
    public class PointControleRepository : IPointControleRepository
    {
        private readonly string _connectionString;

        public PointControleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found");
        }

        private IDbConnection CreateConnection() => new MySqlConnection(_connectionString);

        public async Task<IEnumerable<PointControle>> GetByOrganisationIdAsync(Guid organisationId)
        {
            const string sql = @"
                SELECT * FROM pointscontrole 
                WHERE OrganisationId = @OrganisationId 
                ORDER BY DateCreation DESC";

            using var connection = CreateConnection();
            return await connection.QueryAsync<PointControle>(sql, new { OrganisationId = organisationId });
        }

        public async Task<PointControle?> GetByIdAsync(Guid id)
        {
            const string sql = "SELECT * FROM pointscontrole WHERE Id = @Id";
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<PointControle>(sql, new { Id = id });
        }

        public async Task<Guid> CreateAsync(PointControle pointControle)
        {
            const string sql = @"
                INSERT INTO pointscontrole (Id, OrganisationId, ProcessusId, Nom, Description, Type, Frequence, ResponsableId, Actif, DateCreation)
                VALUES (@Id, @OrganisationId, @ProcessusId, @Nom, @Description, @Type, @Frequence, @ResponsableId, @Actif, @DateCreation)";

            using var connection = CreateConnection();
            await connection.ExecuteAsync(sql, pointControle);
            return pointControle.Id;
        }

        public async Task<bool> UpdateAsync(PointControle pointControle)
        {
            const string sql = @"
                UPDATE pointscontrole 
                SET Nom = @Nom, 
                    Description = @Description, 
                    ProcessusId = @ProcessusId, 
                    Type = @Type, 
                    Frequence = @Frequence, 
                    ResponsableId = @ResponsableId, 
                    Actif = @Actif,
                    DateModification = @DateModification
                WHERE Id = @Id";

            using var connection = CreateConnection();
            var affected = await connection.ExecuteAsync(sql, pointControle);
            return affected > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            const string sql = "DELETE FROM pointscontrole WHERE Id = @Id";
            using var connection = CreateConnection();
            var affected = await connection.ExecuteAsync(sql, new { Id = id });
            return affected > 0;
        }

        public async Task<IEnumerable<EvaluationPointControle>> GetEvaluationsByPointControleIdAsync(Guid pointControleId)
        {
            const string sql = @"
                SELECT * FROM evaluationspointcontrole 
                WHERE PointControleId = @PointControleId 
                ORDER BY DateEvaluation DESC";

            using var connection = CreateConnection();
            return await connection.QueryAsync<EvaluationPointControle>(sql, new { PointControleId = pointControleId });
        }

        public async Task<Guid> AddEvaluationAsync(EvaluationPointControle evaluation)
        {
            const string sql = @"
                INSERT INTO evaluationspointcontrole (Id, PointControleId, DateEvaluation, Conforme, Commentaire, EvalueParId, DateCreation)
                VALUES (@Id, @PointControleId, @DateEvaluation, @Conforme, @Commentaire, @EvalueParId, @DateCreation)";

            using var connection = CreateConnection();
            await connection.ExecuteAsync(sql, evaluation);
            return evaluation.Id;
        }
    }
}