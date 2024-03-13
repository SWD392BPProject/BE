using KidProjectServer.Config;
using KidProjectServer.Entities;
using KidProjectServer.Models;
using KidProjectServer.Services;
using KidProjectServer.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using static System.Reflection.Metadata.BlobBuilder;

namespace KidProjectServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IFeedbackService _feedbackService;
        private readonly IStatisticService _statisticService;

        public FeedbackController(IConfiguration configuration, IFeedbackService feedbackService, IStatisticService statisticService)
        {
            _feedbackService = feedbackService;
            _statisticService = statisticService;
            _configuration = configuration;
        }

        [HttpPost("reply")]
        public async Task<ActionResult<IEnumerable<Feedback>>> CreateReplyFeedback([FromForm] FeedbackFormValues feedbackDto)
        {
            Feedback feedback = await _feedbackService.GetFeedbackByID(feedbackDto.FeedbackID??0);
            if (feedback == null)
            {
                return Ok(ResponseHandle<Feedback>.Error("Not found feedback"));
            }
            Feedback feedbackReply = await _feedbackService.CreateReplyFeedback(feedback, feedbackDto);
            return Ok(ResponseHandle<Feedback>.Success(feedbackReply)); 
        }


        [HttpPost]
        public async Task<ActionResult<IEnumerable<Feedback>>> CreateOrUpdateFeedback([FromForm] FeedbackFormValues feedbackDto)
        {
            Feedback feedback = await _feedbackService.GetFeedbackByUserIDAndBookingID(feedbackDto.UserID??0, feedbackDto.BookingID??0);
            if(feedback == null)
            {
                feedback = await _feedbackService.CreateFeedback(feedbackDto);
                await _statisticService.AddStatisticCountRating();
            }
            else
            {
                feedback = await _feedbackService.UpdateFeedback(feedback ,feedbackDto);
            }
            await _feedbackService.UpdateAvgRating(feedback.PartyID??0);

            return Ok(ResponseHandle<Feedback>.Success(feedback));
        }

        [HttpGet("byUserIDAndBooking/{userId}/{bookingId}")]
        public async Task<ActionResult<IEnumerable<Feedback>>> GetFeedbackByID(int userId,int bookingId)
        {
            Feedback feedback = await _feedbackService.GetFeedbackByUserIDAndBookingID(userId, bookingId);
            return Ok(ResponseHandle<Feedback>.Success(feedback));
        }

        [HttpGet("byPartyId/{partyId}")]
        public async Task<ActionResult<IEnumerable<FeedbackDto>>> GetFeedbackByPartyID(int partyId)
        {
            FeedbackDto[] feedbacks = await _feedbackService.GetFeedbackByPartyID(partyId);
            return Ok(ResponseArrayHandle<FeedbackDto>.Success(feedbacks));
        }

        [HttpGet("byReplyID/{replyId}")]
        public async Task<ActionResult<IEnumerable<FeedbackDto>>> GetFeedbackByReplyID(int replyId)
        {
            FeedbackDto[] feedbacks = await _feedbackService.GetFeedbackByReplyID(replyId);
            return Ok(ResponseArrayHandle<FeedbackDto>.Success(feedbacks));
        }
    }
}