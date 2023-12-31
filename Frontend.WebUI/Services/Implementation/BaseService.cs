﻿using Frontend.WebUI.Models.DTOs;
using Frontend.WebUI.Services.Interface;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using static Frontend.WebUI.Utility.Constants;

namespace Frontend.WebUI.Services.Implementation
{
	public class BaseService : IBaseService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ITokenProvider _token;

		public BaseService(IHttpClientFactory httpClientFactory, ITokenProvider token)
		{
			_httpClientFactory = httpClientFactory;
			_token = token;
		}
		public async Task<ResponseDTO?> SendAsync(RequestDTO requestDTO, bool withBearer = true)
		{
			try
			{
				HttpClient client = _httpClientFactory.CreateClient("API");
				HttpRequestMessage message = new HttpRequestMessage();

				// ưu tiên nhận dữ liệu dưới định dạng JSON nếu có sẵn
				// client vẫn sẽ nhận được dữ liệu nếu dữ liệu trả về khác với json
				if (requestDTO.ContentType == ContentType.MultipartFormData)
				{
					message.Headers.Add("Accept", "*/*");
				}
				else
				{
					message.Headers.Add("Accept", "application/json");
				}


				if (withBearer)
				{
					var token = _token.GetToken();
					message.Headers.Add("Authorization", $"Bearer {token}");
				}

				message.RequestUri = new Uri(requestDTO.Url);

				if (requestDTO.ContentType == ContentType.MultipartFormData)
				{
					var content = new MultipartFormDataContent();
					foreach (var item in requestDTO.Data.GetType().GetProperties())
					{
						var value = item.GetValue(requestDTO.Data);
						// chi upload file neu file co kieu iformfile
						if (value is FormFile)
						{
							var file = (FormFile)value;
							if (file is not null)
							{
								content.Add(new StreamContent(file.OpenReadStream()), item.Name, file.FileName);
							}
						}
						else
						{
							content.Add(new StringContent(value == null ? "" : value.ToString()), item.Name);
						}
					}
					message.Content = content;
				}
				else if (requestDTO.Data != null)
				{
					message.Content = new StringContent(JsonConvert.SerializeObject(requestDTO.Data), Encoding.UTF8, "application/json");
				}

				HttpResponseMessage? responseMessage = null;
				switch (requestDTO.ApiType)
				{
					case ApiType.POST:
						message.Method = HttpMethod.Post;
						break;
					case ApiType.DELETE:
						message.Method = HttpMethod.Delete;
						break;
					case ApiType.PUT:
						message.Method = HttpMethod.Put;
						break;
					default:
						message.Method = HttpMethod.Get;
						break;
				}
				responseMessage = await client.SendAsync(message);
				switch (responseMessage.StatusCode)
				{
					case HttpStatusCode.NotFound:
						return new() { IsSuccess = false, Message = "Not Found" };
					case HttpStatusCode.Forbidden:
						return new() { IsSuccess = false, Message = "Access Denied" };
					case HttpStatusCode.Unauthorized:
						return new() { IsSuccess = false, Message = "Unauthorized" };
					case HttpStatusCode.InternalServerError:
						return new() { IsSuccess = false, Message = "Internal Server Error" };
					default:
						var apiContent = await responseMessage.Content.ReadAsStringAsync();
						var apiResponseDto = JsonConvert.DeserializeObject<ResponseDTO>(apiContent);
						return apiResponseDto;
				}
			}
			catch (Exception ex)
			{
				var dto = new ResponseDTO
				{
					Message = ex.Message.ToString(),
					IsSuccess = false
				};
				return dto;
			}
		}
	}
}
