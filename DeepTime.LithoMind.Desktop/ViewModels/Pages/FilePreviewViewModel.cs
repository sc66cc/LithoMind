using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeepTime.LithoMind.Desktop.ViewModels.Base;

namespace DeepTime.LithoMind.Desktop.ViewModels.Pages
{
	/// <summary>
	/// 预览内容类型
	/// </summary>
	public enum PreviewType
	{
		/// <summary>
		/// 无内容/空白
		/// </summary>
		None,

		/// <summary>
		/// 文本内容
		/// </summary>
		Text,

		/// <summary>
		/// 图片内容
		/// </summary>
		Image,

		/// <summary>
		/// 文件基本信息
		/// </summary>
		FileInfo,

		/// <summary>
		/// 不支持的格式 - 显示文件名
		/// </summary>
		Unsupported,

		/// <summary>
		/// 错误状态
		/// </summary>
		Error
	}

	/// <summary>
	/// 文件预览视图模型
	/// 支持多种文件格式的预览功能
	/// </summary>
	public partial class FilePreviewViewModel : PageViewModelBase
	{
		/// <summary>
		/// 当前预览类型
		/// </summary>
		[ObservableProperty]
		private PreviewType _currentPreviewType = PreviewType.None;

		/// <summary>
		/// 文本预览内容
		/// </summary>
		[ObservableProperty]
		private string _textContent = string.Empty;

		/// <summary>
		/// 图片预览内容
		/// </summary>
		[ObservableProperty]
		private Bitmap? _imageContent;

		/// <summary>
		/// 当前预览的文件名
		/// </summary>
		[ObservableProperty]
		private string _currentFileName = string.Empty;

		/// <summary>
		/// 当前预览的文件路径
		/// </summary>
		[ObservableProperty]
		private string _currentFilePath = string.Empty;

		/// <summary>
		/// 文件大小信息
		/// </summary>
		[ObservableProperty]
		private string _fileSize = string.Empty;

		/// <summary>
		/// 文件类型描述
		/// </summary>
		[ObservableProperty]
		private string _fileTypeDescription = string.Empty;

		/// <summary>
		/// 文件修改时间
		/// </summary>
		[ObservableProperty]
		private string _fileModifiedTime = string.Empty;

		/// <summary>
		/// 错误信息
		/// </summary>
		[ObservableProperty]
		private string _errorMessage = string.Empty;

		/// <summary>
		/// 是否正在加载
		/// </summary>
		[ObservableProperty]
		private bool _isLoading;

		/// <summary>
		/// 欢迎提示文本
		/// </summary>
		[ObservableProperty]
		private string _welcomeText = "文件预览区域";

		/// <summary>
		/// 文本预览最大行数
		/// </summary>
		private const int MaxTextLines = 200;

		/// <summary>
		/// 支持的图片格式
		/// </summary>
		private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".bmp" };

		/// <summary>
		/// 支持的文本格式
		/// </summary>
		private static readonly string[] TextExtensions = { ".txt", ".log", ".csv", ".las", ".xml", ".json" };

		/// <summary>
		/// 已知但不直接预览的格式
		/// </summary>
		private static readonly string[] KnownExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".sgy", ".segy", ".lmproj" };

		public FilePreviewViewModel()
		{
			Id = "FilePreview";
			Title = "数据预览";
			IconKey = "";
			Order = 3;
		}

		/// <summary>
		/// 预览文件 - 支持ProjectNode（工程结构目录）
		/// </summary>
		public async Task PreviewFileAsync(ProjectNode fileNode)
		{
			if (fileNode == null || fileNode.IsDirectory)
			{
				ClearPreview();
				return;
			}

			IsLoading = true;
			CurrentFileName = fileNode.Name;
			CurrentFilePath = fileNode.FullPath;
			FileSize = fileNode.FileSize;
			FileTypeDescription = fileNode.TypeDescription;

			try
			{
				var extension = fileNode.FileExtension.ToLowerInvariant();

				// 判断文件类型并进行相应预览
				if (IsTextFile(extension))
				{
					await PreviewTextFileAsync(fileNode);
				}
				else if (IsImageFile(extension))
				{
					await PreviewImageFileAsync(fileNode);
				}
				else if (IsKnownFile(extension))
				{
					ShowFileInfo(fileNode);
				}
				else
				{
					ShowUnsupportedFile(fileNode);
				}
			}
			catch (Exception ex)
			{
				ShowError(ex.Message);
			}
			finally
			{
				IsLoading = false;
			}
		}

		/// <summary>
		/// 预览本地文件 - 支持FileSystemNode（本地文件目录）
		/// </summary>
		public async Task PreviewLocalFileAsync(FileSystemNode fileNode)
		{
			if (fileNode == null || fileNode.IsDirectory)
			{
				ClearPreview();
				return;
			}

			IsLoading = true;
			CurrentFileName = fileNode.Name;
			CurrentFilePath = fileNode.FullPath;
			FileSize = fileNode.SizeDisplay;
			
			// 获取文件扩展名
			var extension = Path.GetExtension(fileNode.FullPath).ToLowerInvariant();
			FileTypeDescription = GetFileTypeDescription(extension);

			try
			{
				// 判断文件类型并进行相应预览
				if (IsTextFile(extension))
				{
					await PreviewRealTextFileAsync(fileNode.FullPath);
				}
				else if (IsImageFile(extension))
				{
					await PreviewRealImageFileAsync(fileNode.FullPath);
				}
				else if (IsKnownFile(extension))
				{
					ShowRealFileInfo(fileNode.FullPath);
				}
				else
				{
					CurrentPreviewType = PreviewType.Unsupported;
				}
			}
			catch (Exception ex)
			{
				ShowError(ex.Message);
			}
			finally
			{
				IsLoading = false;
			}
		}

		/// <summary>
		/// 获取文件类型描述
		/// </summary>
		private static string GetFileTypeDescription(string extension)
		{
			return extension.ToLowerInvariant() switch
			{
				".las" => "测井曲线文件",
				".sgy" or ".segy" => "地震数据文件",
				".txt" => "文本文件",
				".log" => "日志文件",
				".csv" => "CSV数据文件",
				".xml" => "XML文件",
				".json" => "JSON文件",
				".pdf" => "PDF文档",
				".doc" or ".docx" => "Word文档",
				".xls" or ".xlsx" => "Excel表格",
				".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" => "图片文件",
				".lmproj" => "LithoMind项目文件",
				_ => "未知文件类型"
			};
		}

		/// <summary>
		/// 预览真实文本文件 - 读取前200行
		/// </summary>
		private async Task PreviewRealTextFileAsync(string filePath)
		{
			try
			{
				if (!File.Exists(filePath))
				{
					ShowError("文件不存在");
					return;
				}

				var sb = new StringBuilder();
				int lineCount = 0;

				using (var reader = new StreamReader(filePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
				{
					string? line;
					while ((line = await reader.ReadLineAsync()) != null && lineCount < MaxTextLines)
					{
						sb.AppendLine(line);
						lineCount++;
					}

					if (!reader.EndOfStream)
					{
						sb.AppendLine();
						sb.AppendLine($"... (仅显示前 {MaxTextLines} 行) ...");
					}
				}

				TextContent = sb.ToString();
				CurrentPreviewType = PreviewType.Text;
			}
			catch (Exception ex)
			{
				ShowError($"无法读取文本文件: {ex.Message}");
			}
		}

		/// <summary>
		/// 预览真实图片文件
		/// </summary>
		private async Task PreviewRealImageFileAsync(string filePath)
		{
			try
			{
				if (!File.Exists(filePath))
				{
					ShowError("图片文件不存在");
					return;
				}

				await Task.Run(() =>
				{
					// 加载真实图片
					ImageContent = new Bitmap(filePath);
				});

				CurrentPreviewType = PreviewType.Image;
			}
			catch (Exception ex)
			{
				ShowError($"无法加载图片: {ex.Message}");
			}
		}

		/// <summary>
		/// 显示真实文件信息
		/// </summary>
		private void ShowRealFileInfo(string filePath)
		{
			try
			{
				var fileInfo = new System.IO.FileInfo(filePath);
				FileModifiedTime = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
				CurrentPreviewType = PreviewType.FileInfo;
			}
			catch
			{
				FileModifiedTime = "无法获取";
				CurrentPreviewType = PreviewType.FileInfo;
			}
		}

		/// <summary>
		/// 预览文本文件
		/// </summary>
		private async Task PreviewTextFileAsync(ProjectNode fileNode)
		{
			try
			{
				// 模拟读取文件内容（原型阶段使用模拟数据）
				var content = await GenerateTextContentAsync(fileNode);
				TextContent = content;
				CurrentPreviewType = PreviewType.Text;
			}
			catch (Exception ex)
			{
				ShowError($"无法读取文本文件: {ex.Message}");
			}
		}

		/// <summary>
		/// 生成模拟文本内容（原型阶段）
		/// </summary>
		private async Task<string> GenerateTextContentAsync(ProjectNode fileNode)
		{
			await Task.Delay(100); // 模拟加载延迟

			var sb = new StringBuilder();
			sb.AppendLine($"========================================");
			sb.AppendLine($"文件名: {fileNode.Name}");
			sb.AppendLine($"文件路径: {fileNode.FullPath}");
			sb.AppendLine($"文件大小: {fileNode.FileSize}");
			sb.AppendLine($"文件类型: {fileNode.TypeDescription}");
			sb.AppendLine($"========================================");
			sb.AppendLine();

			// 根据文件类型生成不同的模拟内容
			var extension = fileNode.FileExtension.ToLowerInvariant();

			if (extension == ".las")
			{
				sb.AppendLine("~VERSION INFORMATION");
				sb.AppendLine(" VERS.                          2.0 : CWLS LOG ASCII STANDARD - VERSION 2.0");
				sb.AppendLine(" WRAP.                          NO  : ONE LINE PER DEPTH STEP");
				sb.AppendLine("~WELL INFORMATION");
				sb.AppendLine(" WELL.                  Well-A1     : WELL NAME");
				sb.AppendLine(" FLD .                  LithoMind   : FIELD");
				sb.AppendLine(" LOC .                  Block-1     : LOCATION");
				sb.AppendLine("~CURVE INFORMATION");
				sb.AppendLine(" DEPT.M                             : DEPTH");
				sb.AppendLine(" GR  .GAPI                          : GAMMA RAY");
				sb.AppendLine(" RHOB.G/C3                          : BULK DENSITY");
				sb.AppendLine("~A  DEPTH     GR      RHOB");
				for (int i = 0; i < 50; i++)
				{
					var depth = 1000 + i * 0.5;
					var gr = 50 + Math.Sin(i * 0.3) * 30;
					var rhob = 2.4 + Math.Cos(i * 0.2) * 0.2;
					sb.AppendLine($"  {depth:F1}   {gr:F2}   {rhob:F3}");
				}
			}
			else if (extension == ".txt" && fileNode.Name.Contains("README"))
			{
				sb.AppendLine("LithoMind 项目说明文件");
				sb.AppendLine("========================");
				sb.AppendLine();
				sb.AppendLine("项目名称: LithoMind演示工程");
				sb.AppendLine("创建时间: 2024-01-15");
				sb.AppendLine("作者: 深时地学团队");
				sb.AppendLine();
				sb.AppendLine("项目说明:");
				sb.AppendLine("本工程用于演示LithoMind岩性智能识别系统的基本功能。");
				sb.AppendLine("包含测井数据、地震数据、地质图件等多种类型的示例文件。");
			}
			else
			{
				sb.AppendLine("这是文件内容的模拟预览。");
				sb.AppendLine("在实际应用中，这里将显示文件的真实内容。");
				sb.AppendLine();
				for (int i = 1; i <= Math.Min(50, MaxTextLines); i++)
				{
					sb.AppendLine($"第 {i} 行: 示例文本内容 - {fileNode.Name}");
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// 预览图片文件
		/// </summary>
		private async Task PreviewImageFileAsync(ProjectNode fileNode)
		{
			try
			{
				await Task.Delay(100); // 模拟加载延迟

				// 原型阶段：显示图片信息，不加载真实图片
				// 在实际应用中，这里会使用 new Bitmap(filePath) 加载图片
				ImageContent = null;
				CurrentPreviewType = PreviewType.Image;
			}
			catch (Exception ex)
			{
				ShowError($"无法加载图片: {ex.Message}");
			}
		}

		/// <summary>
		/// 显示文件基本信息
		/// </summary>
		private void ShowFileInfo(ProjectNode fileNode)
		{
			FileModifiedTime = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
			CurrentPreviewType = PreviewType.FileInfo;
		}

		/// <summary>
		/// 显示不支持的文件格式
		/// </summary>
		private void ShowUnsupportedFile(ProjectNode fileNode)
		{
			CurrentPreviewType = PreviewType.Unsupported;
		}

		/// <summary>
		/// 显示错误信息
		/// </summary>
		private void ShowError(string message)
		{
			ErrorMessage = message;
			CurrentPreviewType = PreviewType.Error;
		}

		/// <summary>
		/// 清除预览内容
		/// </summary>
		[RelayCommand]
		public void ClearPreview()
		{
			CurrentPreviewType = PreviewType.None;
			TextContent = string.Empty;
			ImageContent = null;
			CurrentFileName = string.Empty;
			CurrentFilePath = string.Empty;
			FileSize = string.Empty;
			FileTypeDescription = string.Empty;
			FileModifiedTime = string.Empty;
			ErrorMessage = string.Empty;
		}

		/// <summary>
		/// 判断是否为文本文件
		/// </summary>
		private static bool IsTextFile(string extension)
		{
			return Array.Exists(TextExtensions, ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// 判断是否为图片文件
		/// </summary>
		private static bool IsImageFile(string extension)
		{
			return Array.Exists(ImageExtensions, ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// 判断是否为已知文件格式
		/// </summary>
		private static bool IsKnownFile(string extension)
		{
			return Array.Exists(KnownExtensions, ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
		}
	}
}
