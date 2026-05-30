namespace Infrastructure.Exceptions;

public class AiServiceException(string message) : Exception(message);

public class AiRateLimitException(string message) : AiServiceException(message);
