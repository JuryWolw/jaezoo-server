using JaezooServer.Models;
using Microsoft.AspNetCore.Mvc;

namespace JaezooServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private static List<User> users = new();

        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (user == null || string.IsNullOrEmpty(user.Login) || string.IsNullOrEmpty(user.Password))
            {
                return BadRequest("Некорректные данные пользователя");
            }

            if (users.Any(u => u.Login == user.Login))
            {
                return BadRequest("Пользователь уже существует");
            }

            users.Add(user);
            return Ok(new { Message = "Регистрация успешна" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User user)
        {
            if (user == null || string.IsNullOrEmpty(user.Login) || string.IsNullOrEmpty(user.Password))
            {
                return BadRequest("Некорректные данные пользователя");
            }

            var existingUser = users.FirstOrDefault(u => u.Login == user.Login && u.Password == user.Password);
            if (existingUser == null)
            {
                return Unauthorized("Неверный логин или пароль");
            }

            return Ok(new { Login = existingUser.Login });
        }
    }
}