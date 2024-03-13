using KidProjectServer.Controllers;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using KidProjectServer.Repositories;
using KidProjectServer.Util;
using System.Drawing;

namespace KidProjectServer.Services
{
    public interface IFeedbackService
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

    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;

        public FeedbackService(IFeedbackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }

        public async Task<Feedback> CreateReplyFeedback(Feedback feedback, FeedbackFormValues feedbackDto)
        {
            return await _feedbackRepository.CreateReplyFeedback(feedback, feedbackDto);
        }

        public async Task<Feedback> GetFeedbackByID(int id)
        {
            return await _feedbackRepository.GetFeedbackByID(id);
        }

        public async Task<Feedback> GetFeedbackByUserIDAndBookingID(int userID, int bookingID)
        {
            return await _feedbackRepository.GetFeedbackByUserIDAndBookingID(userID, bookingID);
        }
        public async Task<Feedback> CreateFeedback(FeedbackFormValues feedbackDto)
        {
            return await _feedbackRepository.CreateFeedback(feedbackDto);
        }

        public async Task<Feedback> UpdateFeedback(Feedback feedback, FeedbackFormValues feedbackDto)
        {
            return await _feedbackRepository.UpdateFeedback(feedback, feedbackDto);
        }

        public async Task UpdateAvgRating(int partyID)
        {
            await _feedbackRepository.UpdateAvgRating(partyID);
        }

        public async Task<FeedbackDto[]> GetFeedbackByPartyID(int partyID)
        {
            return await _feedbackRepository.GetFeedbackByPartyID(partyID);
        }

        public async Task<FeedbackDto[]> GetFeedbackByReplyID(int replyID)
        {
            return await _feedbackRepository.GetFeedbackByReplyID(replyID);
        }
    }
}
