﻿using Core.Exceptions;
using Core.Models;
using System.Net;
using System.Text.Json;

namespace GastosAPI.Middlewares;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private const string _jsonContentType = "application/json";

    public ExceptionHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BusinessLogicException ex)
        {
            await HandleBusinessLogicException(context, ex);
        }
        catch (NotFoundException notFoundEx)
        {
            await HandleNotFoundException(context, notFoundEx);
        }
        catch (Exception ex)
        {
            await HandleUnexpectedException(context, ex);
        }
    }

    private async Task HandleUnexpectedException(HttpContext context, Exception ex)
    {
        context.Response.ContentType = _jsonContentType;
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        await Console.Out.WriteLineAsync($"Ha ocurrido un error inesperado. Detalles: {ex.StackTrace}");

        var error = new ErrorModel
        {
            Message = ex.Message
        };

        var errorJson = JsonSerializer.Serialize(error);

        await context.Response.WriteAsync(errorJson);
    }

    private async Task HandleNotFoundException(HttpContext context, NotFoundException notFoundEx)
    {
        context.Response.ContentType = _jsonContentType;
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;

        var error = new ErrorModel
        {
            Message = notFoundEx.Message
        };

        var errorJson = JsonSerializer.Serialize(error);

        await context.Response.WriteAsync(errorJson);
    }

    private async Task HandleBusinessLogicException(HttpContext context, BusinessLogicException ex)
    {
        context.Response.ContentType = _jsonContentType;
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        var error = new ErrorModel
        {
            Message = ex.Message
        };
        var errorJson = JsonSerializer.Serialize(error);
        await context.Response.WriteAsync(errorJson);
    }
}
