using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public PhotoRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Photo> GetPhotoById(int photoId)
        {
            return await _context.Photos
                                 .IgnoreQueryFilters()
                                 .FirstOrDefaultAsync(photo => photo.Id == photoId);
        }

        public async Task<IEnumerable<PhotoForApprovalDTO>> GetUnapprovedPhotos()
        {
            return await _context.Photos
                                 .Where(photo => !photo.IsApproved)
                                 .IgnoreQueryFilters()
                                 .ProjectTo<PhotoForApprovalDTO>(_mapper.ConfigurationProvider)
                                 .ToListAsync();
        }

        public void RemovePhoto(Photo photo)
        {
            _context.Photos.Remove(photo);
        }
    }
}
