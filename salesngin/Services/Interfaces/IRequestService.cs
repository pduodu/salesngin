namespace salesngin.Services.Interfaces;

public interface IRequestService
{
    Task<RequestViewModel> GetCreateRequestPageAsync(int requestId, string itemType = null, CancellationToken ct = default);
    Task<RequestViewModel> GetRequestDetailsPageAsync(int requestId, CancellationToken ct = default);

    Task<OperationResult<int>> PostCreateRequestPageAsync(RequestViewModel input, ApplicationUser user, CancellationToken ct = default);
    Task<OperationResult> CreateRequestAsync(Request request, CancellationToken ct = default);
    Task<OperationResult> UpdateRequestAsync(Request request, CancellationToken ct = default);
    Task<OperationResult> PostRequestStatusUpdatePageAsync(RequestViewModel input, CancellationToken ct = default);
    Task<OperationResult> DeleteRequestAsync(int requestId, CancellationToken ct = default);
    
    Task<Request> GetRequestByIdAsync(int requestId, CancellationToken ct = default);
    Task<List<Request>> GetUserRequestsAsync(int userId, CancellationToken ct = default);
    Task<List<Request>> GetRequestsAsync(CancellationToken ct = default);
}