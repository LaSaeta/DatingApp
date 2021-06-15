using API.DTOs;
using API.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Interfaces
{
    public interface IPhotoRepository
    {
        Task<IEnumerable<PhotoForApprovalDTO>> GetUnapprovedPhotos();
        Task<Photo> GetPhotoById(int photoId);
        void RemovePhoto(Photo photo);
    }
}
