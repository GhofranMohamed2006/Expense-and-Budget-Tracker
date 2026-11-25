namespace Pennywise.BLL.Exceptions;

public class NotFoundException(string message) : Exception(message);       // -> 404
public class ConflictException(string message) : Exception(message);       // -> 409
public class BusinessRuleException(string message) : Exception(message);   // -> 400
