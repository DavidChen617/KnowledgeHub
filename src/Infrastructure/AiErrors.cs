using ShareKernal;

namespace Infrastructure;

public static class AiErrors
{
    public static readonly Error ChainExhausted = new(
        "AI.ChainExhausted",
        "All AI service providers in the chain failed.",
        ErrorType.ServiceUnavailable);
}
