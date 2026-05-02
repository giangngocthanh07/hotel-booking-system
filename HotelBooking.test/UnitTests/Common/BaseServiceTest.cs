using AutoFixture;
using Moq;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

public abstract class BaseServiceTest<T>
{
    // Protected to allow derived classes (e.g., UserServiceTest) access
    protected readonly Fixture _fixture;
    protected readonly Mock<IUnitOfWork> _mockUnitOfWork;
    protected readonly Mock<ILogger<T>> _mockLogger;


    public BaseServiceTest()
    {
        _fixture = new Fixture();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<T>>();

        // Default setup for Unit of Work (required by ~99% of tests)
        // Simulates SaveChangesAsync returning 1 (successful persistence)
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Simulate transaction methods (if used in service)
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.RollBackTransactionAsync()).Returns(Task.CompletedTask);
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
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));
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

    // ===========================================
    // 4. GENERIC HELPER: VERIFY UpdateAsync CALLS
    // ===========================================
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

    // ==========================================
    // 7. GENERIC HELPER: VERIFY AnyAsync CALLS
    // ==========================================

    protected void Verify_Repo_AnyAsync<TRepo, TEntity>(Mock<TRepo> mockRepo, int times = 1)
        where TRepo : class, IRepository<TEntity>
        where TEntity : class
    {
        mockRepo.Verify(x => x.AnyAsync(It.IsAny<Expression<Func<TEntity, bool>>>()), Times.Exactly(times));
    }

    protected void Verify_Repo_Never_AnyAsync<TRepo, TEntity>(Mock<TRepo> mockRepo)
        where TRepo : class, IRepository<TEntity>
        where TEntity : class
    {
        mockRepo.Verify(x => x.AnyAsync(It.IsAny<Expression<Func<TEntity, bool>>>()), Times.Never);
    }

    //
    #region Verify Log

    protected void VerifyLogErrorOnce(int times = 1)
    {
        _mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(level => level == LogLevel.Warning || level == LogLevel.Error), // 1. Level
                It.IsAny<EventId>(),                                                           // 2. EventId
                It.Is<It.IsAnyType>((v, t) => true),                                           // 3. State 
                It.IsAny<Exception>(),                                                         // 4. Exception
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)                  // 5. Formatter
            ),
            Times.Exactly(times)
        );
    }
    #endregion

    #endregion
}

