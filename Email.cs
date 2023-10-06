// SmtpClient isn't recomended for new development since it doesn't support modern protocols
// It was used in this project because it fulfills our needs and avoids external dependencies

namespace Email;


internal readonly record struct Host(string Name, ushort Port);
internal readonly record struct Credential(string Username, string Password);
