namespace UserManagement.Application.Interfaces;

public interface IUnitOfWork
{
    Task SaveChangesAsync();
}