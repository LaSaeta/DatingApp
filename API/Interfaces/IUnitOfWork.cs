using System.Threading.Tasks;

namespace API.Interfaces
{
    public interface IUnitOfWork
    {
        ILikesRepository LikesRepository { get; }
        IMessageRepository MessageRepository { get; }
        IUserRepository UserRepository { get; }
        IPhotoRepository PhotoRepository { get; }
        Task<bool> Complete();
        bool HasChanges();
    }
}
