namespace ShipmentBookingSystem.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
	IShipmentRepository ShipmentRepository { get; }
    void SaveChanges();
    void RollbackChanges();
}