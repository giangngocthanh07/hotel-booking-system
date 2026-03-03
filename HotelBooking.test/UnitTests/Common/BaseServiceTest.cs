using AutoFixture;
using Moq;
using System.Linq.Expressions;

public abstract class BaseServiceTest
{
    // Để protected để các class con (UserServiceTest) dùng được
    protected readonly Fixture _fixture;
    protected readonly Mock<IUnitOfWork> _mockUnitOfWork;

    public BaseServiceTest()
    {
        _fixture = new Fixture();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        // Setup mặc định cho UoW (99% các test đều cần cái này)
        // Giả lập SaveChangesAsync trả về 1 (lưu thành công)
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
    }

    // ==========================================
    // 1. HELPER GENERIC: GIẢ LẬP TÌM KIẾM (Find)
    // ==========================================
    // TRepo: Kiểu Repository (IUserRepository, IRoomRepository...)
    // TEntity: Kiểu Entity (User, Room...)
    protected void MockRepo_Find_Returns<TRepo, TEntity>(Mock<TRepo> mockRepo, TEntity? returnResult)
        where TRepo : class, IRepository<TEntity> // Bắt buộc Repo phải có hàm của cha
        where TEntity : class
    {
        mockRepo.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<TEntity, bool>>>()))
                .ReturnsAsync(returnResult);
    }

    // ==========================================
    // 2. HELPER GENERIC: GIẢ LẬP THÊM MỚI (Add) - BỊ LỖI
    // ==========================================
    // Helper này dùng để giả lập lỗi khi Add (ví dụ DB sập)
    protected void MockRepo_Add_ThrowsException<TRepo, TEntity>(Mock<TRepo> mockRepo)
        where TRepo : class, IRepository<TEntity>
        where TEntity : class
    {
        mockRepo.Setup(x => x.AddAsync(It.IsAny<TEntity>()))
                .ThrowsAsync(new Exception("Database Error Simulation"));
    }

    #region VERIFY HELPERS
    // ==========================================
    // 3. HELPER GENERIC: VERIFY LẦN GỌI HÀM AddAsync
    // ==========================================

    // Helper chuyên dùng để Verify hàm AddAsync của bất kỳ Repository nào
    // TRepo: Kiểu Mock (vd: IUserRepository)
    // TEntity: Kiểu Entity (vd: User)
    protected void Verify_Repo_AddAsync<TRepo, TEntity>(Mock<TRepo> mockRepo, int times = 1) // Mặc định là 1 lần
        where TRepo : class, IRepository<TEntity>
        where TEntity : class
    {
        mockRepo.Verify(x => x.AddAsync(It.IsAny<TEntity>()), Times.Exactly(times));
    }

    protected void Verify_Repo_Never_AddAsync<TRepo, TEntity>(Mock<TRepo> mockRepo)
        where TRepo : class, IRepository<TEntity> // Đảm bảo Repo này có hàm AddAsync
        where TEntity : class
    {
        mockRepo.Verify(x => x.AddAsync(It.IsAny<TEntity>()), Times.Never);
    }

    // ==========================================
    // 4. HELPER GENERIC: VERIFY LẦN GỌI HÀM UpdateAsync
    // ==========================================
    protected void Verify_Repo_UpdateAsync<TRepo, TEntity>(Mock<TRepo> mockRepo, int times = 1) // Mặc định là 1 lần
        where TRepo : class, IRepository<TEntity>
        where TEntity : class
    {
        mockRepo.Verify(x => x.UpdateAsync(It.IsAny<TEntity>()), Times.Exactly(times));
    }

    protected void Verify_Repo_Never_UpdateAsync<TRepo, TEntity>(Mock<TRepo> mockRepo) // Mặc định là 1 lần
        where TRepo : class, IRepository<TEntity>
        where TEntity : class
    {
        mockRepo.Verify(x => x.UpdateAsync(It.IsAny<TEntity>()), Times.Never);
    }
    // ==========================================
    // 5. HELPER GENERIC: VERIFY LẦN GỌI HÀM SaveChangesAsync
    // ==========================================
    // 1. Verify đã lưu DB (Mặc định 1 lần)
    protected void Verify_Saved(int times = 1)
    {
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Exactly(times));
    }

    // 2. Verify KHÔNG lưu DB (Dùng cho case lỗi)
    protected void Verify_Never_Saved()
    {
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    // ==========================================
    // 6. HELPER GENERIC: VERIFY LẦN GỌI HÀM SingleOrDefaultAsync
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