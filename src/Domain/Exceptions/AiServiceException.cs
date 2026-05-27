using Domain.Shared;

namespace Domain.Exceptions;

public class AiServiceException(string message) : DomainException(message);

public class AiRateLimitException(string message) : AiServiceException(message);
