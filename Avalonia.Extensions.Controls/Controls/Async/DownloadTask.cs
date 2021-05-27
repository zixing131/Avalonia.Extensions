﻿using System;
using System.IO;
using System.Net.Http;

namespace Avalonia.Extensions.Controls
{
    public sealed class DownloadTask
    {
        private bool Loading = false;
        private HttpClient HttpClient { get; }
        public DownloadTask()
        {
            HttpClient = Core.Instance.GetClient();
        }
        public async void Create(string url, Action<Result> callBack)
        {
            if (!Loading)
            {
                Loading = true;
                try
                {
                    if (!string.IsNullOrEmpty(url))
                    {
                        HttpResponseMessage hr = await HttpClient.GetAsync(url);
                        hr.EnsureSuccessStatusCode();
                        var stream = await hr.Content.ReadAsStreamAsync();
                        callBack?.Invoke(new Result(stream));
                    }
                }
                catch (Exception ex)
                {
                    callBack?.Invoke(new Result(ex.Message));
                }
                Loading = false;
            }
        }
        public sealed class Result
        {
            public Result(Stream stream)
            {
                Success = true;
                Stream = stream;
            }
            public Result(string message)
            {
                Success = false;
                Message = message;
            }
            public bool Success { get; set; }
            public string Message { get; set; }
            public Stream Stream { get; set; }
        }
    }
}