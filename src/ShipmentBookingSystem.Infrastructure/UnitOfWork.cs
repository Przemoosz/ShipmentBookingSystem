using ShipmentBookingSystem.Application.Interfaces;
using ShipmentBookingSystem.Infrastructure.Repository;
using System.Data;

namespace ShipmentBookingSystem.Infrastructure;

internal sealed class UnitOfWork : IUnitOfWork
{
    private IDbTransaction _transaction;

    public IShipmentRepository ShipmentRepository { get; init; }

    public UnitOfWork(IDbConnection dbConnection)
    {
        _transaction = dbConnection.BeginTransaction();
        ShipmentRepository = new ShipmentRepository(dbConnection, _transaction);
    }

    public void SaveChanges()
    {
        _transaction.Commit();
    }

    public void RollbackChanges()
    {
        _transaction.Rollback();
    }

    public void Dispose()
    {
        _transaction.Dispose();
    }
}
