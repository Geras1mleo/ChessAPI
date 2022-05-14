global using System.ComponentModel.DataAnnotations;
global using System.Text.Json.Serialization;
global using Microsoft.AspNetCore.Mvc;

global using Chess;
global using MediatR;
global using Newtonsoft.Json;

global using ChessServices;
global using ChessServices.DTOs;
global using ChessServices.Models;
global using ChessServices.Exceptions;
global using ChessServices.Notifications;
global using ChessServices.Commands.Lobby;
global using ChessServices.Commands.Game;
global using ChessServices.Queries.Lobby;
global using ChessServices.Queries.Game;
