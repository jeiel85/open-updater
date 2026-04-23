using System.Net;
using System.Text;

namespace AutoUpdate.Shared.Services
{
    /// <summary>
    /// 다운로드 소스 유형
    /// </summary>
    public enum DownloadSourceType
    {
        FTP,
        GitHub,
        URL
    }

    /// <summary>
    /// 다운로드 결과
    /// </summary>
    public class DownloadResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public long FileSize { get; set; }
        public long DownloadedSize { get; set; }
    }

    /// <summary>
    /// 다운로드 진행 상태
    /// </summary>
    public class DownloadProgress
    {
        public long TotalSize { get; set; }
        public long DownloadedSize { get; set; }
        public string CurrentFile { get; set; } = string.Empty;
        public int PercentComplete => TotalSize > 0 ? (int)(DownloadedSize * 100 / TotalSize) : 0;
    }

    /// <summary>
    /// FTP 다운로드 서비스
    /// </summary>
    public class FtpDownloadService
    {
        private readonly string _server;
        private readonly string _username;
        private readonly string _password;
        private readonly int _port;
        private readonly bool _usePassive;
        private CancellationTokenSource? _cts;

        public event EventHandler<DownloadProgress>? ProgressChanged;
        public event EventHandler<DownloadResult>? DownloadCompleted;
        public event EventHandler<string>? StatusChanged;

        public FtpDownloadService(string server, string username, string password, int port = 21, bool usePassive = true)
        {
            _server = server;
            _username = username;
            _password = password;
            _port = port;
            _usePassive = usePassive;
        }

        /// <summary>
        /// FTP 서버 연결 테스트
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var url = $"ftp://{_server}:{_port}/";
                var request = (FtpWebRequest)WebRequest.Create(url);
                request.Credentials = new NetworkCredential(_username, _password);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.UsePassive = _usePassive;

                using var response = await request.GetResponseAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 파일 리스트 가져오기
        /// </summary>
        public async Task<List<string>> GetFileListAsync(string remotePath)
        {
            var result = new List<string>();

            try
            {
                var url = $"ftp://{_server}:{_port}/{remotePath}";
                var request = (FtpWebRequest)WebRequest.Create(url);
                request.Credentials = new NetworkCredential(_username, _password);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                request.UsePassive = _usePassive;

                using var response = await request.GetResponseAsync();
                using var reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("euc-kr"));

                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var parts = line.Split(new[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 9)
                    {
                        var name = parts[8];
                        if (line.StartsWith("d"))
                        {
                            var subFiles = await GetFileListAsync($"{remotePath}/{name}");
                            result.AddRange(subFiles);
                        }
                        else
                        {
                            result.Add($"{remotePath}/{name}|{remotePath}|{name}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("FtpDownloadService", ex);
            }

            return result;
        }

        /// <summary>
        /// 파일 다운로드
        /// </summary>
        public async Task<DownloadResult> DownloadFileAsync(string localPath, string remotePath, CancellationTokenSource? cts = null)
        {
            _cts = cts ?? new CancellationTokenSource();
            var result = new DownloadResult();

            try
            {
                var dir = Path.GetDirectoryName(localPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var url = $"ftp://{_server}/{remotePath}";
                var request = (FtpWebRequest)WebRequest.Create(url);
                request.Credentials = new NetworkCredential(_username, _password);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.UseBinary = true;
                request.UsePassive = _usePassive;

                var sizeRequest = (FtpWebRequest)WebRequest.Create(url);
                sizeRequest.Credentials = new NetworkCredential(_username, _password);
                sizeRequest.Method = WebRequestMethods.Ftp.GetFileSize;
                sizeRequest.UseBinary = true;
                sizeRequest.UsePassive = _usePassive;

                using (var sizeResponse = await sizeRequest.GetResponseAsync())
                {
                    result.FileSize = sizeResponse.ContentLength;
                }

                using var response = (FtpWebResponse)await request.GetResponseAsync();
                using var ftpStream = response.GetResponseStream();
                using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write);

                var buffer = new byte[8192];
                long totalRead = 0;
                int read;

                while ((read = await ftpStream.ReadAsync(buffer, 0, buffer.Length, _cts.Token)) > 0)
                {
                    if (_cts.Token.IsCancellationRequested)
                    {
                        result.Success = false;
                        result.ErrorMessage = "사용자가 취소했습니다.";
                        return result;
                    }

                    await fileStream.WriteAsync(buffer.AsMemory(0, read), _cts.Token);
                    totalRead += read;

                    ProgressChanged?.Invoke(this, new DownloadProgress
                    {
                        TotalSize = result.FileSize,
                        DownloadedSize = totalRead,
                        CurrentFile = remotePath
                    });
                }

                result.Success = true;
                result.DownloadedSize = totalRead;
            }
            catch (OperationCanceledException)
            {
                result.Success = false;
                result.ErrorMessage = "다운로드가 취소되었습니다.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                Logger.Error("FtpDownloadService", ex);
            }

            DownloadCompleted?.Invoke(this, result);
            return result;
        }

        /// <summary>
        /// 다운로드 취소
        /// </summary>
        public void Cancel()
        {
            _cts?.Cancel();
        }
    }

    /// <summary>
    /// GitHub 릴리즈 정보
    /// </summary>
    public class GitHubReleaseInfo
    {
        public string TagName { get; set; } = string.Empty;
        public string AssetsUrl { get; set; } = string.Empty;
        public string HtmlUrl { get; set; } = string.Empty;
        public List<GitHubAsset> Assets { get; set; } = new();
    }

    public class GitHubAsset
    {
        public string Name { get; set; } = string.Empty;
        public string BrowserDownloadUrl { get; set; } = string.Empty;
        public long Size { get; set; }
    }

    /// <summary>
    /// GitHub 다운로드 서비스
    /// </summary>
    public class GitHubDownloadService
    {
        private readonly string _owner;
        private readonly string _repo;
        private readonly string _tag;
        private readonly HttpClient _client;
        private CancellationTokenSource? _cts;

        public event EventHandler<DownloadProgress>? ProgressChanged;
        public event EventHandler<DownloadResult>? DownloadCompleted;

        public GitHubDownloadService(string owner, string repo, string tag = "latest")
        {
            _owner = owner;
            _repo = repo;
            _tag = tag;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("User-Agent", "OpenUpdater");
        }

        /// <summary>
        /// 최신 릴리즈 정보 가져오기
        /// </summary>
        public async Task<GitHubReleaseInfo?> GetLatestReleaseInfoAsync()
        {
            try
            {
                var url = _tag == "latest"
                    ? $"https://api.github.com/repos/{_owner}/{_repo}/releases/latest"
                    : $"https://api.github.com/repos/{_owner}/{_repo}/releases/tags/{_tag}";

                var response = await _client.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                return System.Text.Json.JsonSerializer.Deserialize<GitHubReleaseInfo>(json, new System.Text.Json.JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
            }
            catch (Exception ex)
            {
                Logger.Error("GitHubDownloadService", ex);
                return null;
            }
        }

        /// <summary>
        /// 파일 다운로드
        /// </summary>
        public async Task<DownloadResult> DownloadFileAsync(string localPath, string downloadUrl, CancellationTokenSource? cts = null)
        {
            _cts = cts ?? new CancellationTokenSource();
            var result = new DownloadResult();

            try
            {
                var dir = Path.GetDirectoryName(localPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using var response = await _client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, _cts.Token);
                response.EnsureSuccessStatusCode();

                result.FileSize = response.Content.Headers.ContentLength ?? 0;

                using var contentStream = await response.Content.ReadAsStreamAsync(_cts.Token);
                using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write);

                var buffer = new byte[8192];
                long totalRead = 0;
                int read;

                while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length, _cts.Token)) > 0)
                {
                    if (_cts.Token.IsCancellationRequested)
                    {
                        result.Success = false;
                        result.ErrorMessage = "사용자가 취소했습니다.";
                        return result;
                    }

                    await fileStream.WriteAsync(buffer.AsMemory(0, read), _cts.Token);
                    totalRead += read;

                    ProgressChanged?.Invoke(this, new DownloadProgress
                    {
                        TotalSize = result.FileSize,
                        DownloadedSize = totalRead,
                        CurrentFile = downloadUrl
                    });
                }

                result.Success = true;
                result.DownloadedSize = totalRead;
            }
            catch (OperationCanceledException)
            {
                result.Success = false;
                result.ErrorMessage = "다운로드가 취소되었습니다.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                Logger.Error("GitHubDownloadService", ex);
            }

            DownloadCompleted?.Invoke(this, result);
            return result;
        }

        public void Cancel()
        {
            _cts?.Cancel();
        }
    }

    /// <summary>
    /// 일반 URL 다운로드 서비스
    /// </summary>
    public class UrlDownloadService
    {
        private readonly HttpClient _client;
        private CancellationTokenSource? _cts;

        public event EventHandler<DownloadProgress>? ProgressChanged;
        public event EventHandler<DownloadResult>? DownloadCompleted;

        public UrlDownloadService()
        {
            _client = new HttpClient();
        }

        /// <summary>
        /// 파일 다운로드
        /// </summary>
        public async Task<DownloadResult> DownloadFileAsync(string localPath, string url, CancellationTokenSource? cts = null)
        {
            _cts = cts ?? new CancellationTokenSource();
            var result = new DownloadResult();

            try
            {
                var dir = Path.GetDirectoryName(localPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using var response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, _cts.Token);
                response.EnsureSuccessStatusCode();

                result.FileSize = response.Content.Headers.ContentLength ?? 0;

                using var contentStream = await response.Content.ReadAsStreamAsync(_cts.Token);
                using var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write);

                var buffer = new byte[8192];
                long totalRead = 0;
                int read;

                while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length, _cts.Token)) > 0)
                {
                    if (_cts.Token.IsCancellationRequested)
                    {
                        result.Success = false;
                        result.ErrorMessage = "사용자가 취소했습니다.";
                        return result;
                    }

                    await fileStream.WriteAsync(buffer.AsMemory(0, read), _cts.Token);
                    totalRead += read;

                    ProgressChanged?.Invoke(this, new DownloadProgress
                    {
                        TotalSize = result.FileSize,
                        DownloadedSize = totalRead,
                        CurrentFile = url
                    });
                }

                result.Success = true;
                result.DownloadedSize = totalRead;
            }
            catch (OperationCanceledException)
            {
                result.Success = false;
                result.ErrorMessage = "다운로드가 취소되었습니다.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                Logger.Error("UrlDownloadService", ex);
            }

            DownloadCompleted?.Invoke(this, result);
            return result;
        }

        public void Cancel()
        {
            _cts?.Cancel();
        }
    }
}