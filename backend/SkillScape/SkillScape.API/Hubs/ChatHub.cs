using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;
using System.Security.Claims;

namespace SkillScape.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ApplicationDbContext _context;

    public ChatHub(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Group name helper
    private string GetChatRoomName(string userA, string userB)
    {
        var sortedIds = new[] { userA, userB }.OrderBy(id => id).ToArray();
        return $"{sortedIds[0]}-{sortedIds[1]}";
    }

    public async Task JoinChat(string otherUserId)
    {
        var currentUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null) return;

        var roomName = GetChatRoomName(currentUserId, otherUserId);
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
    }

    public async Task LeaveChat(string otherUserId)
    {
        var currentUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null) return;

        var roomName = GetChatRoomName(currentUserId, otherUserId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
    }

    public async Task SendMessage(string receiverId, string content, string? imageUrl = null)
    {
        var logFile = @"c:\Users\Priyansh\Downloads\skill-navigator-main (1)\skill-navigator-main\backend\SkillScape\SkillScape.API\chat_debug_log.txt";
        
        var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        System.IO.File.AppendAllText(logFile, $"\n[{DateTime.UtcNow}] SendMessage Triggered! sender={senderId}, receiver={receiverId}, content={content}");

        if (senderId == null) 
        {
            System.IO.File.AppendAllText(logFile, " - FAILED: senderId is null");
            return;
        }

        try
        {
            // Persist to DB
            var message = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                ImageUrl = imageUrl,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
            System.IO.File.AppendAllText(logFile, " - DB Save Success");

            // Broadcast to the room
            var roomName = GetChatRoomName(senderId, receiverId);
            await Clients.Group(roomName).SendAsync("ReceiveMessage", message);
            System.IO.File.AppendAllText(logFile, " - Broadcast Success");
        }
        catch (Exception ex)
        {
            var errorLog = $"SendMessage Error: {ex.Message}\nStack: {ex.StackTrace}";
            if (ex.InnerException != null)
            {
                errorLog += $"\nInner: {ex.InnerException.Message}";
            }
            System.IO.File.WriteAllText("chat_error_log.txt", errorLog);
            throw;
        }
        
        // Also fire a general notification if the user is online but not in the specific chat room
        // Assuming we map ConnectionIds to UserIds in a concurrent dictionary (simplified approach here is group by ID)
        await Clients.Group(receiverId).SendAsync("ReceiveNotification", new 
        {
            Title = "New Message",
            Message = content,
            ImageUrl = imageUrl,
            Type = "chat",
            SenderId = senderId
        });
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            // Join a personal group for global notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
        await base.OnConnectedAsync();
    }
}
