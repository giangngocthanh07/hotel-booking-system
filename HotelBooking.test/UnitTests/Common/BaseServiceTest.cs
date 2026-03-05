using AutoFixture;
using Moq;
using System.Linq.Expressions;

public abstract class BaseServiceTest
{
    // Protected to allow derived classes (e.g., UserServiceTest) access
    protected readonly Fixture _fixture;
    protected readonly Mock<IUnitOfWork> _mockUnitOfWork;

    public BaseServiceTest()
    {
        _fixture = new Fixture();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        // Default setup for Unit of Work (required by ~99% of tests)
        // Simulates SaveChangesAsync returning 1 (successful persistence)
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
    }

    // ==========================================
    // 1. GENERIC HELPER: MOCK FIND OPERATION
    // ==========================================
    // TRepo: Repository type (IUserRepository, IRoomRepository, etc.)
    // TEntity: Entity type (User, Room, etc.)
    protected void MockRepo_Find_Returns<TRepo, TEntity>(Mock<TRepo> mockRepo, TEntity? returnResult)
        where TRepo : class, IRepository<TEntity> // Ensures Repo inherits from base interface
        where TEntity : class
    {
        mockRepo.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<TEntity, bool>>>()))
                .ReturnsAsync(returnResult);
    }

    // ==========================================
    // 2. GENERIC HELPER: MOCK ADD OPERATION FAILURE
    // ==========================================
    // Simulates an exception during the Add operation (e.g., database crash)
    protected void MockRepo_Add_ThrowsException<TRepo, TEntity>(Mock<TRepo> mockRepo)
        where TRepo : class, IRepository<TEntity>
        where TEntity : class
    {
        mockRepo.Setup(x => x.AddAsync(It.IsAny<TEntity>()))
                .ThrowsAsync(new Exception("Database Error Simulation"));
    }

    #region VERIFY HELPERS
    // ==========================================
    // 3. GENERIC HELPER: VERIFY AddAsync CALLS
    // ==========================================

    // Verifies the AddAsync method call for any Repository
    protected void Verify_Repo_AddAsync<TRepo, TEntity>(Mock<TRepo> mockRepo, int times = 1) 
        where TRepo : class, IRepository<TEntity>
        where TEntity : class
    {
        mockRepo.Verify(x => x.AddAsync(It.IsAny<TEntity>()), Times.Exactly(times));
    }

    protected void Verify_Repo_Never_AddAsync<TRepo, TEntity>(Mock<TRepo> mockRepo)
        where TRepo : class, IRepository<TEntity>
        where TEntity : class
    {
        mockRepo.Verify(x => x.AddAsync(It.IsAny<TEntity>()), Times.Never);
    }

    // ==========================================
    // 4. GENERIC HELPER: VERIFY UpdateAsync CALLS
    // ==========================================
    protected void Verify_Repo_UpdateAsync<TRepo, TEntity>(Mock<TRepo> mockRepo, int times = 1)
        where TRepo : class, IRepository<TEntity>
        where TEntity : class
    {
        mockRepo.Verify(x => x.UpdateAsync(It.IsAny<TEntity>()), Times.Exactly(times));
    }

    protected void Verify_Repo_Never_UpdateAsync<TRepo, TEntity>(Mock<TRepo> mockRepo)
        where TRepo : class, IRepository<TEntity>
        where TEntity : class
    {
        mockRepo.Verify(x => x.UpdateAsync(It.IsAny<TEntity>()), Times.Never);
    }

    // ==========================================
    // 5. GENERIC HELPER: VERIFY SaveChangesAsync CALLS
    // ==========================================
    
    // Verifies that database changes were saved
    protected void Verify_Saved(int times = 1)
    {
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Exactly(times));
    }

    // Verifies that NO database changes were saved (used for failure cases)
    protected void Verify_Never_Saved()
    {
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    // ==========================================
    // 6. GENERIC HELPER: VERIFY SingleOrDefaultAsync CALLS
    // ==========================================
    protected void Verify_Repo_SingleOrDefaultAsync<TRepo, TEntity>(Mock<TRepo> mockRepo, int times = 1)
        where TRepo : class, IRepository<TEntity>
        where TEntity : class
    {
        mockRepo.Verify(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<TEntity, bool>>>()), Times.Exactly(times));
    }

    protected void Verify_Repo_Never_SingleOrDefaultAsync<TRepo, TEntity>(Mock<TRepo> mockRepo)
        where TRepo : class, IRepository<TEntity>
        where TEntity : class
    {
        mockRepo.Verify(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<TEntity, bool>>>()), Times.Never);
    }

    #endregion
}