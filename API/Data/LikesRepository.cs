using System;
using System.Collections.Generic;
using System.Linq;
using API.Extensions;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using API.Helpers;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;
        public LikesRepository(DataContext context)
        {
            _context = context;
        }
        public async Task<UserLike> GetUserLike(int sourceUserId,int likedUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId,likedUserId);
        }
        
        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users
                    .Include(a => a.LikedUsers)
                    .FirstOrDefaultAsync(a=>a.Id == userId);
        }
        
        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            var users = _context.Users.OrderBy(a=>a.UserName).AsQueryable();

            var likes = _context.Likes.AsQueryable();

            if(likesParams.Predicate == "liked")
            {
                likes = likes.Where(like=> like.SourceUserId == likesParams.UserId);
                users = likes.Select(like=> like.LikedUser);
            }

            if(likesParams.Predicate == "likedBy")
            {
                likes = likes.Where(like=> like.LikedUserId == likesParams.UserId);
                users = likes.Select(like=> like.SourceUser);
            }

            var likedUsers = users.Select(user => new LikeDto{
                Username = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(a=>a.IsMain).Url,
                City = user.City,
                Id = user.Id
            });

            return await PagedList<LikeDto>.CreateAsync(likedUsers,likesParams.PageNumber,
                likesParams.PageSize);
        }
    }
}