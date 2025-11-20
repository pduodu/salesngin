namespace salesngin.Hubs
{
    [Authorize] // Optionally, you can secure the hub with authorization.
    public class NotificationHub : Hub
    {
        private readonly Dictionary<string, string> userConnections = new Dictionary<string, string>();
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserConnectionService _userConnectionService;
        public NotificationHub(UserManager<ApplicationUser> userManager, IUserConnectionService userConnectionService)
        {
            _userManager = userManager;
            _userConnectionService = userConnectionService;
        }


        // Method to add a user to a specific group based on their role
        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // Method to send an order to the Kitchen
        [Authorize(Roles = ApplicationRoles.Staff)]
        public async Task SendOrderToKitchen(string message)
        {
            // Send the order to the 'Kitchen' group
            await Clients.Groups(ApplicationRoles.Staff).SendAsync("ReceiveOrder", message);
        }

        // Method to notify Staff that an order is complete
        [Authorize(Roles = ApplicationRoles.Staff)]
        public async Task NotifySales(string message)
        {
            // Send the notification to the 'Staff' group
            await Clients.Groups(ApplicationRoles.Staff).SendAsync("OrderComplete", message);
        }

        public async Task RefreshChatBox(string message)
        {
            // Send the notification to the 'Staff' group
            await Clients.All.SendAsync("RefreshChatbox", message);
            //await Clients.Groups(ApplicationRoles.Kitchen).SendAsync("OrderComplete", message);
            //await Clients.Groups(ApplicationRoles.Staff).SendAsync("OrderComplete", message);
        }

        public async Task RefreshStatusBar(string message)
        {
            await Clients.All.SendAsync("RefreshStatusBar", message);
        }

        // Send a message to a specific user by user ID
        public async Task SendMessageToUser(string userId, string message)
        {
            if (userConnections.TryGetValue(userId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
            }
        }

        public async Task SendMessageToUsers(IReadOnlyList<string> userIds, string message)
        {
            if (userIds.Count > 0)
            {
                foreach (var userId in userIds)
                {
                    var connectionId = _userConnectionService.GetConnection(userId);
                    if (!string.IsNullOrEmpty(connectionId))
                    {
                        await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
                    }
                }
            }
        }

        public override async Task OnConnectedAsync()
        {
            // When a user connects, add them to their respective role-based group
            var user = await _userManager.GetUserAsync(Context.User);
            if (user != null)
            {
                var userId = user.Id.ToString();
                // Get the connection ID for the connected user.
                var connectionId = Context.ConnectionId;
                // Update the mapping with the user's connection ID.
                //userConnections[userId] = connectionId;
                _userConnectionService.AddConnection(userId, connectionId);

                if (await _userManager.IsInRoleAsync(user, ApplicationRoles.Staff))
                {
                    await AddToGroup(ApplicationRoles.Staff);
                }
                else if (await _userManager.IsInRoleAsync(user, ApplicationRoles.Staff))
                {
                    await AddToGroup(ApplicationRoles.Staff);
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // When a user disconnects, remove them from their role-based group
            var user = await _userManager.GetUserAsync(Context.User);
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, ApplicationRoles.Staff))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, ApplicationRoles.Staff);
                }
                else if (await _userManager.IsInRoleAsync(user, ApplicationRoles.Staff))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, ApplicationRoles.Staff);
                }

                // Find the user ID associated with the disconnected connection ID.
                var userId = user.Id.ToString();
                var connectionId = Context.ConnectionId;
                _userConnectionService.RemoveConnection(userId);

                //var userId = userConnections.FirstOrDefault(x => x.Value == connectionId).Key;

                //if (!string.IsNullOrEmpty(userId))
                ////if (userId > 0)
                //{
                //    // Remove the entry from the userConnections dictionary.
                //    userConnections.Remove(userId);
                //}

            }
            await base.OnDisconnectedAsync(exception);
        }


    }
}
