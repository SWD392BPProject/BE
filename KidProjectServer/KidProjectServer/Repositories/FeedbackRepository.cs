using KidProjectServer.Config;
using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using Microsoft.EntityFrameworkCore;

namespace KidProjectServer.Repositories
{
    public interface IFeedbackRepository
    {
        Task<Feedback> GetFeedbackByID(int id);
        Task<Feedback> GetFeedbackByUserIDAndBookingID(int userID, int bookingID);
        Task<Feedback> CreateReplyFeedback(Feedback feedback, FeedbackFormValues feedbackDto);
        Task<Feedback> CreateFeedback(FeedbackFormValues feedbackDto);
        Task<Feedback> UpdateFeedback(Feedback feedback, FeedbackFormValues feedbackDto);
        Task UpdateAvgRating(int partyID);
        Task<FeedbackDto[]> GetFeedbackByPartyID(int partyID);
        Task<FeedbackDto[]> GetFeedbackByReplyID(int replyID);
    }

    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly DBConnection _context;

        public FeedbackRepository(DBConnection context)
        {
            _context = context;
        }

        public async Task<Feedback> CreateFeedback(FeedbackFormValues feedbackDto)
        {
            Feedback feedback = new Feedback
            {
                BookingID = feedbackDto.BookingID,
                UserID = feedbackDto.UserID,
                Rating = feedbackDto.Rating,
                PartyID = feedbackDto.PartyID,
                Comment = feedbackDto.Comment,
                CreateDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow,
                Type = Constants.TYPE_FEEDBACK,
                Status = Constants.STATUS_ACTIVE
            };
            await _context.Feedbacks.AddAsync(feedback);
            await _context.SaveChangesAsync();
            feedback.FeedbackReplyID = feedback.FeedbackID;
            await _context.SaveChangesAsync();
            return feedback;
        }

        public async Task<Feedback> CreateReplyFeedback(Feedback feedback, FeedbackFormValues feedbackDto)
        {
            Feedback feedbackReply = new Feedback
            {
                FeedbackReplyID = feedback.FeedbackID,
                UserID = feedbackDto.UserID,
                PartyID = feedback.PartyID,
                BookingID = feedback.BookingID,
                Comment = feedbackDto.Comment,
                ReplyComment = feedback.Comment,
                CreateDate = DateTime.UtcNow,
                LastUpdateDate = DateTime.UtcNow,
                Type = Constants.TYPE_REPLY,
                Status = Constants.STATUS_ACTIVE
            };

            await _context.Feedbacks.AddAsync(feedbackReply);
            await _context.SaveChangesAsync();
            return feedbackReply;
        }

        public async Task<Feedback> GetFeedbackByID(int id)
        {
            return await _context.Feedbacks.Where(p => p.FeedbackID == id).FirstOrDefaultAsync();
        }

        public async Task<FeedbackDto[]> GetFeedbackByReplyID(int replyID)
        {
            var query = from feedbacks in _context.Feedbacks
                        join bookings in _context.Bookings on feedbacks.BookingID equals bookings.BookingID
                        join users in _context.Users on feedbacks.UserID equals users.UserID
                        where feedbacks.FeedbackReplyID == replyID
                        select new FeedbackDto
                        {
                            FeedbackID = feedbacks.FeedbackID,
                            FeedbackReplyID = feedbacks.FeedbackReplyID,
                            BookingID = feedbacks.BookingID,
                            Image = users.Image,
                            Rating = feedbacks.Rating,
                            Comment = feedbacks.Comment,
                            ReplyComment = feedbacks.ReplyComment,
                            Type = feedbacks.Type,
                            CreateDate = feedbacks.CreateDate
                        };
            FeedbackDto[] feedbacks1 = await query.ToArrayAsync();
            return feedbacks1;
        }

        public async Task<FeedbackDto[]> GetFeedbackByPartyID(int partyID)
        {
            var query = from feedbacks in _context.Feedbacks
                        join bookings in _context.Bookings on feedbacks.BookingID equals bookings.BookingID
                        join users in _context.Users on feedbacks.UserID equals users.UserID
                        where bookings.PartyID == partyID
                        select new FeedbackDto
                        {
                            FeedbackID = feedbacks.FeedbackID,
                            FeedbackReplyID = feedbacks.FeedbackReplyID,
                            BookingID = feedbacks.BookingID,
                            Image = users.Image,
                            Rating = feedbacks.Rating,
                            Comment = feedbacks.Comment,
                            ReplyComment = feedbacks.ReplyComment,
                            Type = feedbacks.Type,
                            CreateDate = feedbacks.CreateDate
                        };
            FeedbackDto[] feedbacks1 = await query.OrderBy(p => p.FeedbackReplyID).ToArrayAsync();
            return feedbacks1;
        }

        public async Task<Feedback> GetFeedbackByUserIDAndBookingID(int userID, int bookingID)
        {
            return await _context.Feedbacks.Where(p => p.UserID == userID && p.BookingID == bookingID).FirstOrDefaultAsync();
        }

        public async Task UpdateAvgRating(int partyID)
        {
            Feedback[] feedbacks = await _context.Feedbacks.Where(p => p.PartyID == partyID).ToArrayAsync();
            Party party = await _context.Parties.Where(p => p.PartyID == partyID).FirstOrDefaultAsync();
            int totalRating = 0;
            foreach (Feedback feed in feedbacks)
            {
                totalRating += feed.Rating ?? 0;
            }
            int ratingAvg = totalRating / feedbacks.Length;
            party.Rating = ratingAvg;
            await _context.SaveChangesAsync();
        }

        public async Task<Feedback> UpdateFeedback(Feedback feedback, FeedbackFormValues feedbackDto)
        {
            feedback.Comment = feedbackDto.Comment;
            feedback.Rating = feedbackDto.Rating;
            await _context.SaveChangesAsync();
            return feedback;
        }
    }
}
