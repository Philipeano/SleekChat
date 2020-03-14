using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleekChat.Core.Entities;
using SleekChat.Data.Contracts;
using SleekChat.Data.Helpers;

namespace SleekChat.Api.Controllers
{
    [Authorize]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly ICurrentUser currentUser;
        private readonly INotificationData notificationData;
        private readonly IUserData userData;
        private readonly ValidationHelper validator;
        private readonly FormatHelper formatter;
        private readonly HttpHelper httpHelper;
        private KeyValuePair<bool, string> validationResult;

        public NotificationsController(IUserData userData, INotificationData notificationData, ICurrentUser currentUser)
        {
            this.currentUser = currentUser;
            this.notificationData = notificationData;
            this.userData = userData;
            validator = new ValidationHelper();
            formatter = new FormatHelper();
            httpHelper = new HttpHelper();
        }


        // GET: api/notifications?recipientId
        [HttpGet("api/notifications")]
        public ActionResult GetAll([FromQuery(Name = "recipientId")] string recipientId = "")
        {
            // If recipient id was not specified, return ALL notifications
            if (!Request.Query.ContainsKey("recipientId"))
                return Ok(formatter.Render(notificationData.GetAllNotifications(), "Notifications", Operation.Retrieved));

            // Validate specified recipient id
            validationResult = validator.IsBlank("recipient id", recipientId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("recipient id", recipientId);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Guid reqRecipientId = Guid.Parse(recipientId);

            User recipient = userData.GetUserById(reqRecipientId);
            return recipient == null
                ? NotFound(formatter.Render(validator.Result("The specified recipient id does not match any existing user.")))
                : (ActionResult)Ok(formatter.Render(notificationData.GetNotificationsForAUser(reqRecipientId), "Notifications", Operation.Retrieved));
        }


        // GET: api/notifications/id
        [HttpGet("api/notifications/{id}")]
        public ActionResult GetById([FromRoute] string id)
        {
            // Validate specified notification id
            validationResult = validator.IsBlank("notification id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("notification id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Guid reqNotificationId = Guid.Parse(id);

            Notification notification = notificationData.GetNotificationById(reqNotificationId);
            return notification == null
                ? NotFound(formatter.Render(validator.Result("There is no notification with the specified id.")))
                : (ActionResult)Ok(formatter.Render(notification, "Notification", Operation.Retrieved));
        }


        // PATCH: api/notifications/id?newStatus
        [HttpPatch("api/notifications/{id}")]
        public ActionResult Patch([FromRoute] string id, [FromQuery(Name = "newStatus")] string status)
        {
            // Validate specified notification id
            validationResult = validator.IsBlank("notification id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("notification id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Guid reqNotificationId = Guid.Parse(id);
            Notification notification = notificationData.GetNotificationById(reqNotificationId);
            if (notification == null)
                return NotFound(formatter.Render(validator.Result("No notification exists with the specified id.")));

            Guid userId = currentUser.GetUserId();

            // Check if current user is the actual recipient of this notification
            if (!notificationData.IsNotificationRecipient(reqNotificationId, userId))
            {
                formatter.RenderJson(validator.Result("You are not the actual recipient of this notification."), out string responseTxt);
                httpHelper.Forbid(Response, responseTxt);
                return null;
            }

            // If newStatus is not supplied, abort the operation
            if (!Request.Query.ContainsKey("newStatus"))
                return BadRequest(formatter.Render(validator.Result("Required parameter 'new status' was not provided.")));

            validationResult = validator.IsBlank("new status", status);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            _ = notificationData.UpdateNotification(reqNotificationId, DataHelper.GetStatus(status), out Notification updatedNotification);
            return Ok(formatter.Render(updatedNotification, "Notification", Operation.Updated));
        }


        // DELETE: api/notifications/id
        [HttpDelete("api/notifications/{id}")]
        public ActionResult Delete([FromRoute] string id)
        {
            // Validate specified notification id
            validationResult = validator.IsBlank("notification id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            validationResult = validator.IsValidGuid("notification id", id);
            if (validationResult.Key == false)
                return BadRequest(formatter.Render(validationResult));

            Guid reqNotificationId = Guid.Parse(id);
            Notification notification = notificationData.GetNotificationById(reqNotificationId);
            if (notification == null)
                return NotFound(formatter.Render(validator.Result("No notification exists with the specified id.")));

            Guid userId = currentUser.GetUserId();

            // Allow deletion only if current user is the actual recipient of the notification
            if (notificationData.IsNotificationRecipient(reqNotificationId, userId))
            {
                notificationData.DeleteNotification(reqNotificationId);
                return Ok(formatter.Render(null, "Notification", Operation.Deleted));
            }

            formatter.RenderJson(validator.Result("You are not the actual recipient of this notification."), out string responseTxt);
            httpHelper.Forbid(Response, responseTxt);
            return null;
        }
    }
}