using API.Interfaces;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using API.DTOs;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using API.Helpers;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UserRepository(DataContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users
            .Include(p=>p.Photos)
            .ToListAsync();
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
             .Include(p=>p.Photos)
            .SingleOrDefaultAsync(x=> x.UserName == username);
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = _context.Users.AsQueryable();
            query = query.Where(a=>a.UserName != userParams.CurrentUsername);            
            query = query.Where(a=>a.Gender == userParams.Gender);
            var midDob = DateTime.Today.AddYears(-userParams.MaxAge -1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
            query = query.Where(a=>a.DateOfBirth >= midDob && a.DateOfBirth <= maxDob);   
            query = userParams.OrderBy switch
            {
                "Created" => query.OrderByDescending(x=>x.Created),
                _ => query.OrderByDescending(u=>u.LastActive)
            };
            return await API.Helpers.PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(
                _mapper.ConfigurationProvider).AsNoTracking(),
                     userParams.PageNumber,userParams.PageSize);
        }   

        public async Task<MemberDto> GetMemberAsync(string username)
        {
                return await _context.Users
                    .Where(x=> x.UserName == username)
                    .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                    .SingleOrDefaultAsync();
        }

        public async Task<string> GetUserGender(string username)
        {
            return await _context.Users.Where(a=>a.UserName == username).Select(x=>x.Gender).FirstOrDefaultAsync();
        }
    }
}