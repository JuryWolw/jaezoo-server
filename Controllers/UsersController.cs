﻿using JaezooServer.Models;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JaezooServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private static readonly List<User> users = new();

        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (user == null || string.IsNullOrEmpty(user.Login) || string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.Email))
            {
                return BadRequest("Некорректные данные пользователя");
            }

            if (users.Any(u => u.Login == user.Login))
            {
                return BadRequest("Пользователь уже существует");
            }

            user.Id = Guid.NewGuid().ToString();
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            users.Add(user);
            return Ok(new { Message = "Регистрация успешна", user.Id });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Login) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Некорректные данные пользователя");
            }

            var existingUser = users.FirstOrDefault(u => u.Login == request.Login);
            if (existingUser == null || !BCrypt.Net.BCrypt.Verify(request.Password, existingUser.Password))
            {
                return Unauthorized("Неверный логин или пароль");
            }

            return Ok(new { login = existingUser.Login, id = existingUser.Id });
        }

        [HttpGet("search")]
        public IActionResult Search([FromQuery] string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest("Не указан запрос для поиска");
            }

            var matchingUsers = users
                .Where(u => u.Id == query || u.Login.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(u => new { u.Id, u.Login })
                .ToList();

            if (!matchingUsers.Any())
            {
                return NotFound("Пользователи не найдены");
            }

            return Ok(matchingUsers);
        }

        [HttpPost("addFriend")]
        public IActionResult AddFriend([FromBody] FriendRequest request)
        {
            if (request.FromUser == null || request.ToUser == null)
            {
                return BadRequest("Некорректные данные запроса");
            }

            var user = users.FirstOrDefault(u => u.Login == request.FromUser);
            var friend = users.FirstOrDefault(u => u.Login == request.ToUser);
            if (user == null || friend == null)
            {
                return BadRequest("Пользователь не найден");
            }

            if (!user.Friends.Contains(friend.Login))
            {
                user.Friends.Add(friend.Login);
            }

            return Ok(new { Message = $"Пользователь {friend.Login} добавлен в друзья" });
        }

        [HttpPut("updateProfile")]
        public IActionResult UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            if (request.Login == null)
            {
                return BadRequest("Логин обязателен");
            }

            var user = users.FirstOrDefault(u => u.Login == request.Login);
            if (user == null)
            {
                return BadRequest("Пользователь не найден");
            }

            user.Email = request.Email ?? user.Email;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
            user.Description = request.Description ?? user.Description;
            user.Status = request.Status ?? user.Status;
            user.AvatarPath = request.AvatarPath ?? user.AvatarPath;

            return Ok(new { Message = "Профиль обновлен" });
        }
    }

    public class LoginRequest
    {
        public required string Login { get; set; }
        public required string Password { get; set; }
    }

    public class FriendRequest
    {
        public required string FromUser { get; set; }
        public required string ToUser { get; set; }
    }

    public class UpdateProfileRequest
    {
        public required string Login { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? AvatarPath { get; set; }
    }
}